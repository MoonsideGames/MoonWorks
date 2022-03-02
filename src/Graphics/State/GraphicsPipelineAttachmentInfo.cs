namespace MoonWorks.Graphics
{
	/// <summary>
	/// Describes the kind of attachments that will be used with this pipeline.
	/// </summary>
	public struct GraphicsPipelineAttachmentInfo
	{
		public ColorAttachmentDescription[] ColorAttachmentDescriptions;
		public bool HasDepthStencilAttachment;
		public TextureFormat DepthStencilFormat;

		public GraphicsPipelineAttachmentInfo(
			params ColorAttachmentDescription[] colorAttachmentDescriptions
		) {
			ColorAttachmentDescriptions = colorAttachmentDescriptions;
			HasDepthStencilAttachment = false;
			DepthStencilFormat = TextureFormat.D16;
		}

		public GraphicsPipelineAttachmentInfo(
			TextureFormat depthStencilFormat,
			params ColorAttachmentDescription[] colorAttachmentDescriptions
		) {
			ColorAttachmentDescriptions = colorAttachmentDescriptions;
			HasDepthStencilAttachment = true;
			DepthStencilFormat = depthStencilFormat;
		}
	}
}
