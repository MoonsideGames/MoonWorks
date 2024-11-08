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

		public uint NumSamplers { get; private init; }
		public uint NumStorageTextures { get; private init; }
		public uint NumStorageBuffers { get; private init; }
		public uint NumUniformBuffers { get; private init; }

		private Shader(GraphicsDevice device) : base(device) { }

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

			var entryPointLength = Encoding.UTF8.GetByteCount(entryPoint) + 1;
			var entryPointBuffer = NativeMemory.Alloc((nuint) entryPointLength);
			var buffer = new Span<byte>(entryPointBuffer, entryPointLength);
			var byteCount = Encoding.UTF8.GetBytes(entryPoint, buffer);
			buffer[byteCount] = 0;

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
				return null;
			}

			var shader = new Shader(device)
			{
				Handle = shaderModule,
				NumSamplers = createInfo.NumSamplers,
				NumStorageTextures = createInfo.NumStorageTextures,
				NumStorageBuffers = createInfo.NumStorageBuffers,
				NumUniformBuffers = createInfo.NumUniformBuffers
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
			in ShaderCross.ShaderResourceInfo resourceInfo
		) {
			var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
			var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
			stream.ReadExactly(bytecodeSpan);

			var shaderModule = SDL_ShaderCross.SDL_ShaderCross_CompileGraphicsShaderFromSPIRV(
				device.Handle,
				bytecodeSpan,
				(nuint) stream.Length,
				entryPoint,
				shaderStage,
				resourceInfo.ToNative()
			);

			NativeMemory.Free(bytecodeBuffer);

			if (shaderModule == nint.Zero)
			{
				Logger.LogError("Failed to compile shader!");
				return null;
			}

			var shader = new Shader(device)
			{
				Handle = shaderModule,
				NumSamplers = resourceInfo.NumSamplers,
				NumStorageTextures = resourceInfo.NumStorageTextures,
				NumStorageBuffers = resourceInfo.NumStorageBuffers,
				NumUniformBuffers = resourceInfo.NumUniformBuffers
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
			ShaderStage shaderStage,
			in ShaderCross.ShaderResourceInfo resourceInfo
		) {
			byte* hlslBuffer = (byte*) NativeMemory.Alloc((nuint) stream.Length + 1);
			var hlslSpan = new Span<byte>(hlslBuffer, (int) stream.Length);
			stream.ReadExactly(hlslSpan);
			hlslBuffer[(int)stream.Length] = 0; // ensure null-terminated

			var shaderModule = SDL_ShaderCross.SDL_ShaderCross_CompileGraphicsShaderFromHLSL(
				device.Handle,
				hlslSpan,
				entryPoint,
				shaderStage,
				resourceInfo.ToNative()
			);

			NativeMemory.Free(hlslBuffer);

			if (shaderModule == nint.Zero)
			{
				Logger.LogError("Failed to compile shader!");
				return null;
			}

			var shader = new Shader(device)
			{
				Handle = shaderModule,
				NumSamplers = resourceInfo.NumSamplers,
				NumStorageTextures = resourceInfo.NumStorageTextures,
				NumStorageBuffers = resourceInfo.NumStorageBuffers,
				NumUniformBuffers = resourceInfo.NumUniformBuffers
			};

			return shader;
		}
	}
}
