using RefreshCS;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Shader modules expect input in Refresh bytecode format.
	/// </summary>
	public class ShaderModule : GraphicsResource
	{
		protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyShaderModule;

		public unsafe ShaderModule(GraphicsDevice device, string filePath) : base(device)
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			Handle = CreateFromStream(device, stream);
		}

		public unsafe ShaderModule(GraphicsDevice device, Stream stream) : base(device)
		{
			Handle = CreateFromStream(device, stream);
		}

		private static unsafe IntPtr CreateFromStream(GraphicsDevice device, Stream stream)
		{
			var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
			var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
			stream.ReadExactly(bytecodeSpan);

			Refresh.ShaderModuleCreateInfo shaderModuleCreateInfo;
			shaderModuleCreateInfo.codeSize = (nuint) stream.Length;
			shaderModuleCreateInfo.byteCode = (nint) bytecodeBuffer;

			var shaderModule = Refresh.Refresh_CreateShaderModule(device.Handle, shaderModuleCreateInfo);

			NativeMemory.Free(bytecodeBuffer);
			return shaderModule;
		}
	}
}
