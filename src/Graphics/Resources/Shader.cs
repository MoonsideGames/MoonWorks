using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
		public static Shader Create(
			GraphicsDevice device,
			string filePath,
			string entryPoint,
			in ShaderCreateInfo shaderCreateInfo
		) {
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			return Create(
				device,
				stream,
				entryPoint,
				shaderCreateInfo
			);
		}

		/// <summary>
		/// Creates a shader using a specified shader format.
		/// </summary>
		public static unsafe Shader Create(
			GraphicsDevice device,
			Stream stream,
			string entryPoint,
			in ShaderCreateInfo shaderCreateInfo
		) {
			var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
			var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
			stream.ReadExactly(bytecodeSpan);

			var entryPointBuffer = MarshalString(entryPoint);

			INTERNAL_ShaderCreateInfo createInfo;
			createInfo.CodeSize = (nuint) stream.Length;
			createInfo.Code = (byte*) bytecodeBuffer;
			createInfo.EntryPoint = (byte*) entryPointBuffer;
			createInfo.Stage = shaderCreateInfo.Stage;
			createInfo.Format = shaderCreateInfo.Format;
			createInfo.NumSamplers = shaderCreateInfo.NumSamplers;
			createInfo.NumStorageTextures = shaderCreateInfo.NumStorageTextures;
			createInfo.NumStorageBuffers = shaderCreateInfo.NumStorageBuffers;
			createInfo.NumUniformBuffers = shaderCreateInfo.NumUniformBuffers;
			createInfo.Props = shaderCreateInfo.Props;

			var shaderModule = SDL.SDL_CreateGPUShader(
				device.Handle,
				createInfo
			);

			NativeMemory.Free(bytecodeBuffer);
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

			return shader;
		}

		/// <summary>
		/// Creates a shader for any backend from SPIRV bytecode.
		/// </summary>
		internal static unsafe Shader CreateFromSPIRV(
			GraphicsDevice device,
			Stream stream,
			string entryPoint,
			ShaderStage shaderStage,
			bool enableDebug,
			string name // can be null
		) {
			var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
			var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
			stream.ReadExactly(bytecodeSpan);

			var entryPointBuffer = MarshalString(entryPoint);
			var nameBuffer = MarshalString(name);

			SDL_ShaderCross.INTERNAL_SPIRVInfo spirvInfo;
			spirvInfo.Bytecode = (byte*) bytecodeBuffer;
			spirvInfo.BytecodeSize = (nuint) stream.Length;
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

			NativeMemory.Free(bytecodeBuffer);
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

		/// <summary>
		/// Creates a shader for any backend from HLSL source.
		/// </summary>
		internal static unsafe Shader CreateFromHLSL(
			GraphicsDevice device,
			Stream stream,
			string entryPoint,
			string includeDir, // can be NULL
			ShaderStage shaderStage,
			bool enableDebug,
			string name, // can be NULL
			params Span<ShaderCross.HLSLDefine> defines
		) {
			byte* hlslBuffer = (byte*) NativeMemory.Alloc((nuint) stream.Length + 1);
			var hlslSpan = new Span<byte>(hlslBuffer, (int) stream.Length);
			stream.ReadExactly(hlslSpan);
			hlslBuffer[(int)stream.Length] = 0; // ensure null-terminated

			var entryPointBuffer = MarshalString(entryPoint);
			var includeDirBuffer = MarshalString(includeDir);
			var nameBuffer = MarshalString(name);

			SDL_ShaderCross.INTERNAL_HLSLDefine* definesBuffer = null;
			if (defines.Length > 0) {
				definesBuffer = (SDL_ShaderCross.INTERNAL_HLSLDefine*) NativeMemory.Alloc((nuint) (Marshal.SizeOf<SDL_ShaderCross.INTERNAL_HLSLDefine>() * (defines.Length + 1)));
				for (var i = 0; i < defines.Length; i += 1)
				{
					definesBuffer[i].Name = MarshalString(defines[i].Name);
					definesBuffer[i].Value = MarshalString(defines[i].Value);
				}
				// Null-terminate the array
				definesBuffer[defines.Length].Name = null;
				definesBuffer[defines.Length].Value = null;
			}

			SDL_ShaderCross.INTERNAL_HLSLInfo hlslInfo;
			hlslInfo.Source = hlslBuffer;
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

			NativeMemory.Free(hlslBuffer);
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

		// MUST call NativeMemory.Free on the result eventually!
		private static unsafe byte* MarshalString(string s)
		{
			if (s == null) { return null; }

			var length = Encoding.UTF8.GetByteCount(s) + 1;
			var buffer = (byte*) NativeMemory.Alloc((nuint) length);
			var span = new Span<byte>(buffer, length);
			var byteCount = Encoding.UTF8.GetBytes(s, span);
			span[byteCount] = 0;

			return buffer;
		}
	}
}
