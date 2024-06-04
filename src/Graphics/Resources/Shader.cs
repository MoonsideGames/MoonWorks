using SDL2_gpuCS;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Shader modules expect input in Refresh bytecode format.
	/// </summary>
	public class Shader : SDL_GpuResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL_Gpu.SDL_GpuReleaseShader;

		public unsafe Shader(
			GraphicsDevice device,
			string filePath,
			string entryPointName,
			ShaderStage shaderStage,
			ShaderFormat shaderFormat
		) : base(device)
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			Handle = CreateFromStream(device, stream, entryPointName, shaderStage, shaderFormat);
		}

		public unsafe Shader(
			GraphicsDevice device,
			Stream stream,
			string entryPointName,
			ShaderStage shaderStage,
			ShaderFormat shaderFormat
		) : base(device)
		{
			Handle = CreateFromStream(device, stream, entryPointName, shaderStage, shaderFormat);
		}

		private static unsafe IntPtr CreateFromStream(
			GraphicsDevice device,
			Stream stream,
			string entryPointName,
			ShaderStage shaderStage,
			ShaderFormat shaderFormat
		) {
			var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
			var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
			stream.ReadExactly(bytecodeSpan);

			SDL_Gpu.ShaderCreateInfo shaderCreateInfo;
			shaderCreateInfo.CodeSize = (nuint) stream.Length;
			shaderCreateInfo.Code = (byte*) bytecodeBuffer;
			shaderCreateInfo.EntryPointName = entryPointName;
			shaderCreateInfo.Stage = (SDL_Gpu.ShaderStage) shaderStage;
			shaderCreateInfo.Format = (SDL_Gpu.ShaderFormat) shaderFormat;

			var shaderModule = SDL_Gpu.SDL_GpuCreateShader(
				device.Handle,
				shaderCreateInfo
			);

			NativeMemory.Free(bytecodeBuffer);
			return shaderModule;
		}
	}
}
