namespace MoonWorks.Graphics
{
	public struct GraphicsPipelineCreateInfo
	{
		public DepthStencilState DepthStencilState;
		public GraphicsShaderInfo VertexShaderInfo;
		public GraphicsShaderInfo FragmentShaderInfo;
		public MultisampleState MultisampleState;
		public RasterizerState RasterizerState;
		public PrimitiveType PrimitiveType;
		public VertexInputState VertexInputState;
		public GraphicsPipelineAttachmentInfo AttachmentInfo;
		public BlendConstants BlendConstants;
	}
}
