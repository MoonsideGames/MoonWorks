namespace MoonWorks.Graphics
{
	/// <summary>
	/// Describes how many samplers will be used in each shader stage.
	/// </summary>
	public struct GraphicsPipelineLayoutInfo
	{
		public uint VertexSamplerBindingCount;
		public uint FragmentSamplerBindingCount;
	}
}
