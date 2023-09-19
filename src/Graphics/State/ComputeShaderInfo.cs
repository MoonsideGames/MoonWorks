using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Information that the compute pipeline needs about a compute shader.
	/// </summary>
	public struct ComputeShaderInfo
	{
		public ShaderModule ShaderModule;
		public string EntryPointName;
		public uint UniformBufferSize;
		public uint BufferBindingCount;
		public uint ImageBindingCount;

		public unsafe static ComputeShaderInfo Create<T>(
			ShaderModule shaderModule,
			string entryPointName,
			uint bufferBindingCount,
			uint imageBindingCount
		) where T : unmanaged
		{
			return new ComputeShaderInfo
			{
				ShaderModule = shaderModule,
				EntryPointName = entryPointName,
				UniformBufferSize = (uint) Marshal.SizeOf<T>(),
				BufferBindingCount = bufferBindingCount,
				ImageBindingCount = imageBindingCount
			};
		}

        public static ComputeShaderInfo Create(
            ShaderModule shaderModule,
            string entryPointName,
            uint bufferBindingCount,
            uint imageBindingCount
        )
        {
            return new ComputeShaderInfo
			{
				ShaderModule = shaderModule,
				EntryPointName = entryPointName,
				UniformBufferSize = 0,
				BufferBindingCount = bufferBindingCount,
				ImageBindingCount = imageBindingCount
			};
        }
	}
}
