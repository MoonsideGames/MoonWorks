using RefreshCS;
using System;
using System.IO;

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
			using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				Handle = CreateFromStream(device, stream);
			}
		}

		public unsafe ShaderModule(GraphicsDevice device, Stream stream) : base(device)
		{
			Handle = CreateFromStream(device, stream);
		}

		private unsafe static IntPtr CreateFromStream(GraphicsDevice device, Stream stream)
		{
			var bytecode = new byte[stream.Length];
			stream.Read(bytecode, 0, (int) stream.Length);

			fixed (byte* ptr = bytecode)
			{
				Refresh.ShaderModuleCreateInfo shaderModuleCreateInfo;
				shaderModuleCreateInfo.codeSize = (UIntPtr) bytecode.Length;
				shaderModuleCreateInfo.byteCode = (IntPtr) ptr;

				return Refresh.Refresh_CreateShaderModule(device.Handle, shaderModuleCreateInfo);
			}
		}
	}
}
