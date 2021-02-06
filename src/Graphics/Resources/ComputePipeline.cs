using RefreshCS;
using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
    public class ComputePipeline : GraphicsResource
    {
        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyComputePipeline;

        public ShaderStageState ComputeShaderState { get; }

        public unsafe ComputePipeline(
            GraphicsDevice device,
            ShaderStageState computeShaderState,
            uint bufferBindingCount,
            uint imageBindingCount
        ) : base(device) {
            var computePipelineLayoutCreateInfo = new Refresh.ComputePipelineLayoutCreateInfo
            {
                bufferBindingCount = bufferBindingCount,
                imageBindingCount = imageBindingCount
            };

            var computePipelineCreateInfo = new Refresh.ComputePipelineCreateInfo
            {
                pipelineLayoutCreateInfo = computePipelineLayoutCreateInfo,
                computeShaderState = new Refresh.ShaderStageState
                {
                    entryPointName = computeShaderState.EntryPointName,
                    shaderModule = computeShaderState.ShaderModule.Handle,
                    uniformBufferSize = computeShaderState.UniformBufferSize
                }
            };

            Handle = Refresh.Refresh_CreateComputePipeline(
                device.Handle,
                computePipelineCreateInfo
            );

            ComputeShaderState = computeShaderState;
        }

        public unsafe uint PushComputeShaderUniforms<T>(
            params T[] uniforms
        ) where T : unmanaged
        {
            fixed (T* ptr = &uniforms[0])
            {
                return Refresh.Refresh_PushComputeShaderUniforms(
                    Device.Handle,
                    Handle,
                    (IntPtr) ptr,
                    (uint) (uniforms.Length * Marshal.SizeOf<T>())
                );
            }
        }
    }
}
