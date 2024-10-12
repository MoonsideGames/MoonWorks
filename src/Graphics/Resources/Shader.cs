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
	public class Shader : RefreshResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL.SDL_ReleaseGPUShader;

		public uint NumSamplers { get; private init; }
		public uint NumStorageTextures { get; private init; }
		public uint NumStorageBuffers { get; private init; }
		public uint NumUniformBuffers { get; private init; }

		private Shader(GraphicsDevice device) : base(device) { }

		public static Shader CreateFromFile(
			GraphicsDevice device,
			string filePath,
			string entryPoint,
			in ShaderCreateInfo shaderCreateInfo
		) {
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			return CreateFromStream(
				device,
				stream,
				entryPoint,
				shaderCreateInfo
			);
		}

		public static unsafe Shader CreateFromStream(
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
				throw new InvalidOperationException("Shader compilation failed!");
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
	}
}
