namespace MoonWorks.Graphics
{
	public struct GraphicsPipelineCreateInfo
	{
		public DepthStencilState DepthStencilState;
		public GraphicsShaderInfo VertexShaderState;
		public GraphicsShaderInfo FragmentShaderState;
		public MultisampleState MultisampleState;
		public RasterizerState RasterizerState;
		public PrimitiveType PrimitiveType;
		public VertexInputState VertexInputState;
		public ViewportState ViewportState;
		public GraphicsPipelineAttachmentInfo AttachmentInfo;
		public BlendConstants BlendConstants;
	}
}
