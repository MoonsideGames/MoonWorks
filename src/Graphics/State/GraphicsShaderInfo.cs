namespace MoonWorks.Graphics
{
	/// <summary>
	/// Information that the pipeline needs about a shader.
	/// </summary>
	public struct GraphicsShaderInfo
	{
		public ShaderModule ShaderModule;
		public string EntryPointName;
		public uint UniformBufferSize;
		public uint SamplerBindingCount;
	}
}
