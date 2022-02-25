namespace MoonWorks.Graphics
{
	/// <summary>
	/// Describes the kind of attachments that will be used with this pipeline.
	/// </summary>
	public struct GraphicsPipelineAttachmentInfo
	{
		public ColorAttachmentDescription[] ColorAttachmentDescriptions;
		public uint ColorAttachmentCount;
		public bool HasDepthStencilAttachment;
		public TextureFormat DepthStencilFormat;
	}
}
