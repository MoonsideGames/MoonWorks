using RefreshCS;
using System;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Shader modules expect input in SPIR-V bytecode format.
	/// </summary>
	public class ShaderModule : GraphicsResource
	{
		protected override Action<IntPtr, IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyShaderModule;

		public unsafe ShaderModule(GraphicsDevice device, string filePath) : base(device)
		{
			var bytecode = Bytecode.ReadBytecodeAsUInt32(filePath);

			fixed (uint* ptr = bytecode)
			{
				Refresh.ShaderModuleCreateInfo shaderModuleCreateInfo;
				shaderModuleCreateInfo.codeSize = (UIntPtr) (bytecode.Length * sizeof(uint));
				shaderModuleCreateInfo.byteCode = (IntPtr) ptr;

				Handle = Refresh.Refresh_CreateShaderModule(device.Handle, shaderModuleCreateInfo);
			}
		}
	}
}
