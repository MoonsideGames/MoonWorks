namespace MoonWorks.Graphics
{
	public struct GraphicsPipelineCreateInfo
	{
		public ColorBlendState ColorBlendState;
		public DepthStencilState DepthStencilState;
		public ShaderStageState VertexShaderState;
		public ShaderStageState FragmentShaderState;
		public MultisampleState MultisampleState;
		public GraphicsPipelineLayoutInfo PipelineLayoutInfo;
		public RasterizerState RasterizerState;
		public PrimitiveType PrimitiveType;
		public VertexInputState VertexInputState;
		public ViewportState ViewportState;
		public RenderPass RenderPass;
	}
}
