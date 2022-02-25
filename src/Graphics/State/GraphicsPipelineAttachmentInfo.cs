namespace MoonWorks.Graphics
{
	/// <summary>
	/// Describes the kind of attachments that will be used with this pipeline.
	/// </summary>
	public struct GraphicsPipelineAttachmentInfo
	{
		public ColorAttachmentDescription[] colorAttachmentDescriptions;
		public uint colorAttachmentCount;
		public bool hasDepthStencilAttachment;
		public TextureFormat depthStencilFormat;
	}
}
