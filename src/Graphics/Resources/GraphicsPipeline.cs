using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Graphics pipelines encapsulate all of the render state in a single object. <br/>
	/// These pipelines are bound before draw calls are issued.
	/// </summary>
	public class GraphicsPipeline : RefreshResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => Refresh.Refresh_ReleaseGraphicsPipeline;

		public GraphicsPipelineResourceInfo VertexShaderResourceInfo { get; }
		public GraphicsPipelineResourceInfo FragmentShaderResourceInfo { get; }
		public SampleCount SampleCount { get; }

#if DEBUG
		internal GraphicsPipelineAttachmentInfo AttachmentInfo { get; }
#endif

		public unsafe GraphicsPipeline(
			GraphicsDevice device,
			in GraphicsPipelineCreateInfo graphicsPipelineCreateInfo
		) : base(device)
		{
			Refresh.GraphicsPipelineCreateInfo refreshGraphicsPipelineCreateInfo;

			var vertexAttributes = (Refresh.VertexAttribute*) NativeMemory.Alloc(
				(nuint) (graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length * Marshal.SizeOf<Refresh.VertexAttribute>())
			);

			for (var i = 0; i < graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length; i += 1)
			{
				vertexAttributes[i] = graphicsPipelineCreateInfo.VertexInputState.VertexAttributes[i].ToRefresh();
			}

			var vertexBindings = (Refresh.VertexBinding*) NativeMemory.Alloc(
				(nuint) (graphicsPipelineCreateInfo.VertexInputState.VertexBindings.Length * Marshal.SizeOf<Refresh.VertexBinding>())
			);

			for (var i = 0; i < graphicsPipelineCreateInfo.VertexInputState.VertexBindings.Length; i += 1)
			{
				vertexBindings[i] = graphicsPipelineCreateInfo.VertexInputState.VertexBindings[i].ToRefresh();
			}

			var colorAttachmentDescriptions = stackalloc Refresh.ColorAttachmentDescription[
				graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions.Length
			];

			for (var i = 0; i < graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions.Length; i += 1)
			{
				colorAttachmentDescriptions[i].Format = (Refresh.TextureFormat) graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions[i].Format;
				colorAttachmentDescriptions[i].BlendState = graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions[i].BlendState.ToRefresh();
			}

			refreshGraphicsPipelineCreateInfo.VertexShader = graphicsPipelineCreateInfo.VertexShader.Handle;
			refreshGraphicsPipelineCreateInfo.FragmentShader = graphicsPipelineCreateInfo.FragmentShader.Handle;

			refreshGraphicsPipelineCreateInfo.VertexInputState.VertexAttributes = vertexAttributes;
			refreshGraphicsPipelineCreateInfo.VertexInputState.VertexAttributeCount = (uint) graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length;
			refreshGraphicsPipelineCreateInfo.VertexInputState.VertexBindings = vertexBindings;
			refreshGraphicsPipelineCreateInfo.VertexInputState.VertexBindingCount = (uint) graphicsPipelineCreateInfo.VertexInputState.VertexBindings.Length;

			refreshGraphicsPipelineCreateInfo.PrimitiveType = (Refresh.PrimitiveType) graphicsPipelineCreateInfo.PrimitiveType;

			refreshGraphicsPipelineCreateInfo.RasterizerState = graphicsPipelineCreateInfo.RasterizerState.ToRefresh();
			refreshGraphicsPipelineCreateInfo.MultisampleState = graphicsPipelineCreateInfo.MultisampleState.ToRefresh();
			refreshGraphicsPipelineCreateInfo.DepthStencilState = graphicsPipelineCreateInfo.DepthStencilState.ToRefresh();

			refreshGraphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentCount = (uint) graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions.Length;
			refreshGraphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions = colorAttachmentDescriptions;
			refreshGraphicsPipelineCreateInfo.AttachmentInfo.DepthStencilFormat = (Refresh.TextureFormat) graphicsPipelineCreateInfo.AttachmentInfo.DepthStencilFormat;
			refreshGraphicsPipelineCreateInfo.AttachmentInfo.HasDepthStencilAttachment = Conversions.BoolToInt(graphicsPipelineCreateInfo.AttachmentInfo.HasDepthStencilAttachment);

			refreshGraphicsPipelineCreateInfo.VertexResourceInfo = graphicsPipelineCreateInfo.VertexShaderResourceInfo.ToRefresh();
			refreshGraphicsPipelineCreateInfo.FragmentResourceInfo = graphicsPipelineCreateInfo.FragmentShaderResourceInfo.ToRefresh();

			refreshGraphicsPipelineCreateInfo.BlendConstants[0] = graphicsPipelineCreateInfo.BlendConstants.R;
			refreshGraphicsPipelineCreateInfo.BlendConstants[1] = graphicsPipelineCreateInfo.BlendConstants.G;
			refreshGraphicsPipelineCreateInfo.BlendConstants[2] = graphicsPipelineCreateInfo.BlendConstants.B;
			refreshGraphicsPipelineCreateInfo.BlendConstants[3] = graphicsPipelineCreateInfo.BlendConstants.A;

			Handle = Refresh.Refresh_CreateGraphicsPipeline(device.Handle, refreshGraphicsPipelineCreateInfo);
			if (Handle == IntPtr.Zero)
			{
				throw new Exception("Could not create graphics pipeline!");
			}

			NativeMemory.Free(vertexAttributes);
			NativeMemory.Free(vertexBindings);

			VertexShaderResourceInfo = graphicsPipelineCreateInfo.VertexShaderResourceInfo;
			FragmentShaderResourceInfo = graphicsPipelineCreateInfo.FragmentShaderResourceInfo;
			SampleCount = graphicsPipelineCreateInfo.MultisampleState.MultisampleCount;

#if DEBUG
			AttachmentInfo = graphicsPipelineCreateInfo.AttachmentInfo;
#endif
		}
	}
}
