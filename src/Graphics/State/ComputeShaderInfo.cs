namespace MoonWorks.Graphics
{
	/// <summary>
	/// Information that the pipeline needs about a shader.
	/// </summary>
	public struct ComputeShaderInfo
	{
		public ShaderModule ShaderModule;
		public string EntryPointName;
		public uint UniformBufferSize;
		public uint bufferBindingCount;
		public uint imageBindingCount;
	}
}
