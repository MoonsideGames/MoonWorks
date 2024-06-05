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
			Refresh.GraphicsPipelineCreateInfo sdlGraphicsPipelineCreateInfo;

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

			sdlGraphicsPipelineCreateInfo.VertexShader = graphicsPipelineCreateInfo.VertexShader.Handle;
			sdlGraphicsPipelineCreateInfo.FragmentShader = graphicsPipelineCreateInfo.FragmentShader.Handle;

			sdlGraphicsPipelineCreateInfo.VertexInputState.VertexAttributes = vertexAttributes;
			sdlGraphicsPipelineCreateInfo.VertexInputState.VertexAttributeCount = (uint) graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length;
			sdlGraphicsPipelineCreateInfo.VertexInputState.VertexBindings = vertexBindings;
			sdlGraphicsPipelineCreateInfo.VertexInputState.VertexBindingCount = (uint) graphicsPipelineCreateInfo.VertexInputState.VertexBindings.Length;

			sdlGraphicsPipelineCreateInfo.PrimitiveType = (Refresh.PrimitiveType) graphicsPipelineCreateInfo.PrimitiveType;

			sdlGraphicsPipelineCreateInfo.RasterizerState = graphicsPipelineCreateInfo.RasterizerState.ToRefresh();
			sdlGraphicsPipelineCreateInfo.MultisampleState = graphicsPipelineCreateInfo.MultisampleState.ToRefresh();
			sdlGraphicsPipelineCreateInfo.DepthStencilState = graphicsPipelineCreateInfo.DepthStencilState.ToRefresh();

			sdlGraphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentCount = (uint) graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions.Length;
			sdlGraphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions = colorAttachmentDescriptions;
			sdlGraphicsPipelineCreateInfo.AttachmentInfo.DepthStencilFormat = (Refresh.TextureFormat) graphicsPipelineCreateInfo.AttachmentInfo.DepthStencilFormat;
			sdlGraphicsPipelineCreateInfo.AttachmentInfo.HasDepthStencilAttachment = Conversions.BoolToInt(graphicsPipelineCreateInfo.AttachmentInfo.HasDepthStencilAttachment);

			sdlGraphicsPipelineCreateInfo.VertexResourceInfo = graphicsPipelineCreateInfo.VertexShaderResourceInfo.ToRefresh();
			sdlGraphicsPipelineCreateInfo.FragmentResourceInfo = graphicsPipelineCreateInfo.FragmentShaderResourceInfo.ToRefresh();

			sdlGraphicsPipelineCreateInfo.BlendConstants[0] = graphicsPipelineCreateInfo.BlendConstants.R;
			sdlGraphicsPipelineCreateInfo.BlendConstants[1] = graphicsPipelineCreateInfo.BlendConstants.G;
			sdlGraphicsPipelineCreateInfo.BlendConstants[2] = graphicsPipelineCreateInfo.BlendConstants.B;
			sdlGraphicsPipelineCreateInfo.BlendConstants[3] = graphicsPipelineCreateInfo.BlendConstants.A;

			Handle = Refresh.Refresh_CreateGraphicsPipeline(device.Handle, sdlGraphicsPipelineCreateInfo);
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
