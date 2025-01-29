using System;
using System.Runtime.InteropServices;
using System.Text;
using MoonWorks.Storage;
using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Shaders are used to create graphics pipelines.
	/// Graphics pipelines take a vertex shader and a fragment shader.
	/// </summary>
	public class Shader : SDLGPUResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL.SDL_ReleaseGPUShader;

		public uint NumSamplers { get; init; }
		public uint NumStorageTextures { get; init; }
		public uint NumStorageBuffers { get; init; }
		public uint NumUniformBuffers { get; init; }

		private Shader(GraphicsDevice device) : base(device)
		{
			Name = "Shader";
		}

		/// <summary>
		/// Creates a shader using a specified shader format.
		/// </summary>
		public static unsafe Shader Create(
			GraphicsDevice device,
			TitleStorage storage,
			string filePath,
			string entryPoint,
			in ShaderCreateInfo shaderCreateInfo
		) {
			if (!storage.GetFileSize(filePath, out var size))
			{
				return null;
			}

			var buffer = NativeMemory.Alloc((nuint) size);
			var span = new Span<byte>(buffer, (int) size);
			if (!storage.ReadFile(filePath, span))
			{
				return null;
			}

			var pipeline = Create(device, span, entryPoint, shaderCreateInfo);
			NativeMemory.Free(buffer);
			return pipeline;
		}

		/// <summary>
		/// Creates a shader using a specified shader format.
		/// </summary>
		public static unsafe Shader Create(
			GraphicsDevice device,
			ReadOnlySpan<byte> span,
			string entryPoint,
			in ShaderCreateInfo shaderCreateInfo
		) {
			var entryPointBuffer = InteropUtilities.MarshalString(entryPoint);

			fixed (byte* spanPtr = span)
			{
				INTERNAL_ShaderCreateInfo createInfo;
				createInfo.CodeSize = (nuint) span.Length;
				createInfo.Code = spanPtr;
				createInfo.EntryPoint = entryPointBuffer;
				createInfo.Stage = shaderCreateInfo.Stage;
				createInfo.Format = shaderCreateInfo.Format;
				createInfo.NumSamplers = shaderCreateInfo.NumSamplers;
				createInfo.NumStorageTextures = shaderCreateInfo.NumStorageTextures;
				createInfo.NumStorageBuffers = shaderCreateInfo.NumStorageBuffers;
				createInfo.NumUniformBuffers = shaderCreateInfo.NumUniformBuffers;
				createInfo.Props = shaderCreateInfo.Props;

				var cleanProps = false;
				if (shaderCreateInfo.Name != null)
				{
					if (createInfo.Props == 0)
					{
						createInfo.Props = SDL3.SDL.SDL_CreateProperties();
						cleanProps = true;
					}

					SDL3.SDL.SDL_SetStringProperty(createInfo.Props, SDL3.SDL.SDL_PROP_GPU_SHADER_CREATE_NAME_STRING, shaderCreateInfo.Name);
				}

				var shaderModule = SDL.SDL_CreateGPUShader(
					device.Handle,
					createInfo
				);

				NativeMemory.Free(entryPointBuffer);

				if (shaderModule == nint.Zero)
				{
					Logger.LogError("Failed to compile shader!");
					Logger.LogError(SDL3.SDL.SDL_GetError());
					return null;
				}

				var shader = new Shader(device)
				{
					Handle = shaderModule,
					NumSamplers = shaderCreateInfo.NumSamplers,
					NumStorageTextures = shaderCreateInfo.NumStorageTextures,
					NumStorageBuffers = shaderCreateInfo.NumStorageBuffers,
					NumUniformBuffers = shaderCreateInfo.NumUniformBuffers
				};

				if (cleanProps)
				{
					SDL3.SDL.SDL_DestroyProperties(createInfo.Props);
				}

				return shader;
			}
		}

		/// <summary>
		/// Creates a shader for any backend from SPIRV bytecode.
		/// </summary>
		internal static unsafe Shader CreateFromSPIRV(
			GraphicsDevice device,
			string name, // can be null
			ReadOnlySpan<byte> span,
			string entryPoint,
			ShaderStage shaderStage,
			bool enableDebug
		) {
			var entryPointBuffer = InteropUtilities.MarshalString(entryPoint);
			var nameBuffer = InteropUtilities.MarshalString(name);

			fixed (byte* spanPtr = span)
			{
				SDL_ShaderCross.INTERNAL_SPIRVInfo spirvInfo;
				spirvInfo.Bytecode = spanPtr;
				spirvInfo.BytecodeSize = (nuint) span.Length;
				spirvInfo.EntryPoint = entryPointBuffer;
				spirvInfo.ShaderStage = (SDL_ShaderCross.ShaderStage) shaderStage;
				spirvInfo.EnableDebug = enableDebug;
				spirvInfo.Name = nameBuffer;
				spirvInfo.Props = 0;

				var shaderModule = SDL_ShaderCross.SDL_ShaderCross_CompileGraphicsShaderFromSPIRV(
					device.Handle,
					spirvInfo,
					out var shaderMetadata
				);

				NativeMemory.Free(entryPointBuffer);
				NativeMemory.Free(nameBuffer);

				if (shaderModule == nint.Zero)
				{
					Logger.LogError("Failed to compile shader!");
					Logger.LogError(SDL3.SDL.SDL_GetError());
					return null;
				}

				var shader = new Shader(device)
				{
					Handle = shaderModule,
					NumSamplers = shaderMetadata.NumSamplers,
					NumStorageTextures = shaderMetadata.NumStorageTextures,
					NumStorageBuffers = shaderMetadata.NumStorageBuffers,
					NumUniformBuffers = shaderMetadata.NumUniformBuffers,
					Name = name ?? "Shader"
				};

				return shader;
			}
		}

		/// <summary>
		/// Creates a shader for any backend from HLSL source.
		/// </summary>
		internal static unsafe Shader CreateFromHLSL(
			GraphicsDevice device,
			string name, // can be NULL
			ReadOnlySpan<byte> span,
			string entryPoint,
			string includeDir, // can be NULL
			ShaderStage shaderStage,
			bool enableDebug,
			params Span<ShaderCross.HLSLDefine> defines
		) {
			var entryPointBuffer = InteropUtilities.MarshalString(entryPoint);
			var includeDirBuffer = InteropUtilities.MarshalString(includeDir);
			var nameBuffer = InteropUtilities.MarshalString(name);

			fixed (byte* spanPtr = span)
			{
				SDL_ShaderCross.INTERNAL_HLSLDefine* definesBuffer = null;
				if (defines.Length > 0) {
					definesBuffer = (SDL_ShaderCross.INTERNAL_HLSLDefine*) NativeMemory.Alloc((nuint) (Marshal.SizeOf<SDL_ShaderCross.INTERNAL_HLSLDefine>() * (defines.Length + 1)));
					for (var i = 0; i < defines.Length; i += 1)
					{
						definesBuffer[i].Name = InteropUtilities.MarshalString(defines[i].Name);
						definesBuffer[i].Value = InteropUtilities.MarshalString(defines[i].Value);
					}
					// Null-terminate the array
					definesBuffer[defines.Length].Name = null;
					definesBuffer[defines.Length].Value = null;
				}

				SDL_ShaderCross.INTERNAL_HLSLInfo hlslInfo;
				hlslInfo.Source = spanPtr;
				hlslInfo.EntryPoint = entryPointBuffer;
				hlslInfo.IncludeDir = includeDirBuffer;
				hlslInfo.Defines = definesBuffer;
				hlslInfo.ShaderStage = (SDL_ShaderCross.ShaderStage) shaderStage;
				hlslInfo.EnableDebug = enableDebug;
				hlslInfo.Name = nameBuffer;
				hlslInfo.Props = 0;

				var shaderModule = SDL_ShaderCross.SDL_ShaderCross_CompileGraphicsShaderFromHLSL(
					device.Handle,
					hlslInfo,
					out var shaderMetadata
				);

				NativeMemory.Free(entryPointBuffer);
				NativeMemory.Free(includeDirBuffer);
				for (var i = 0; i < defines.Length; i += 1)
				{
					NativeMemory.Free(definesBuffer[i].Name);
					NativeMemory.Free(definesBuffer[i].Value);
				}
				NativeMemory.Free(definesBuffer);
				NativeMemory.Free(nameBuffer);

				if (shaderModule == nint.Zero)
				{
					Logger.LogError("Failed to compile shader!");
					Logger.LogError(SDL3.SDL.SDL_GetError());
					return null;
				}

				var shader = new Shader(device)
				{
					Handle = shaderModule,
					NumSamplers = shaderMetadata.NumSamplers,
					NumStorageTextures = shaderMetadata.NumStorageTextures,
					NumStorageBuffers = shaderMetadata.NumStorageBuffers,
					NumUniformBuffers = shaderMetadata.NumUniformBuffers,
					Name = name ?? "Shader"
				};

				return shader;
			}
		}
	}
}
