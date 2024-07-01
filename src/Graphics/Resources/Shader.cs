using RefreshCS;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Shaders are used to create graphics pipelines.
	/// Graphics pipelines take a vertex shader and a fragment shader.
	/// </summary>
	public class Shader : RefreshResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => Refresh.Refresh_ReleaseShader;

		public uint SamplerCount { get; }
		public uint StorageTextureCount { get; }
		public uint StorageBufferCount { get; }
		public uint UniformBufferCount { get; }

		public unsafe Shader(
			GraphicsDevice device,
			string filePath,
			string entryPointName,
			in ShaderCreateInfo shaderCreateInfo
		) : base(device)
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			Handle = CreateFromStream(
				device,
				stream,
				entryPointName,
				shaderCreateInfo
			);

			SamplerCount = shaderCreateInfo.SamplerCount;
			StorageTextureCount = shaderCreateInfo.StorageTextureCount;
			StorageBufferCount = shaderCreateInfo.StorageBufferCount;
			UniformBufferCount = shaderCreateInfo.UniformBufferCount;
		}

		public unsafe Shader(
			GraphicsDevice device,
			Stream stream,
			string entryPointName,
			in ShaderCreateInfo shaderCreateInfo
		) : base(device)
		{
			Handle = CreateFromStream(
				device,
				stream,
				entryPointName,
				shaderCreateInfo
			);

			SamplerCount = shaderCreateInfo.SamplerCount;
			StorageTextureCount = shaderCreateInfo.StorageTextureCount;
			StorageBufferCount = shaderCreateInfo.StorageBufferCount;
			UniformBufferCount = shaderCreateInfo.UniformBufferCount;
		}

		private static unsafe IntPtr CreateFromStream(
			GraphicsDevice device,
			Stream stream,
			string entryPointName,
			in ShaderCreateInfo shaderCreateInfo
		) {
			var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
			var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
			stream.ReadExactly(bytecodeSpan);

			Refresh.ShaderCreateInfo refreshShaderCreateInfo;
			refreshShaderCreateInfo.CodeSize = (nuint) stream.Length;
			refreshShaderCreateInfo.Code = (byte*) bytecodeBuffer;
			refreshShaderCreateInfo.EntryPointName = entryPointName;
			refreshShaderCreateInfo.Stage = (Refresh.ShaderStage) shaderCreateInfo.ShaderStage;
			refreshShaderCreateInfo.Format = (Refresh.ShaderFormat) shaderCreateInfo.ShaderFormat;
			refreshShaderCreateInfo.SamplerCount = shaderCreateInfo.SamplerCount;
			refreshShaderCreateInfo.StorageTextureCount = shaderCreateInfo.StorageTextureCount;
			refreshShaderCreateInfo.StorageBufferCount = shaderCreateInfo.StorageBufferCount;
			refreshShaderCreateInfo.UniformBufferCount = shaderCreateInfo.UniformBufferCount;

			var shaderModule = Refresh.Refresh_CreateShader(
				device.Handle,
				refreshShaderCreateInfo
			);

			if (shaderModule == nint.Zero)
			{
				throw new InvalidOperationException("Shader compilation failed!");
			}

			NativeMemory.Free(bytecodeBuffer);
			return shaderModule;
		}
	}
}
