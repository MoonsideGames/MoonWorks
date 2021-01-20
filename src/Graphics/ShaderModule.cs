using RefreshCS;
using System;
using System.IO;

namespace MoonWorks.Graphics
{
    public class ShaderModule : GraphicsResource
    {
        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyShaderModule;

        public unsafe ShaderModule(GraphicsDevice device, FileInfo fileInfo) : base(device)
        {
            fixed (uint* ptr = Bytecode.ReadBytecode(fileInfo))
            {
                Refresh.ShaderModuleCreateInfo shaderModuleCreateInfo;
                shaderModuleCreateInfo.codeSize = (UIntPtr) fileInfo.Length;
                shaderModuleCreateInfo.byteCode = (IntPtr) ptr;

                Handle = Refresh.Refresh_CreateShaderModule(device.Handle, ref shaderModuleCreateInfo);
            }
        }
    }
}
