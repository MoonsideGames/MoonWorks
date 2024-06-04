using System;
using System.Runtime.InteropServices;
using SDL2_gpuCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Graphics pipelines encapsulate all of the render state in a single object. <br/>
	/// These pipelines are bound before draw calls are issued.
	/// </summary>
	public class GraphicsPipeline : SDL_GpuResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL_Gpu.SDL_GpuReleaseGraphicsPipeline;

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
			SDL_Gpu.GraphicsPipelineCreateInfo sdlGraphicsPipelineCreateInfo;

			var vertexAttributes = (SDL_Gpu.VertexAttribute*) NativeMemory.Alloc(
				(nuint) (graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length * Marshal.SizeOf<SDL_Gpu.VertexAttribute>())
			);

			for (var i = 0; i < graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length; i += 1)
			{
				vertexAttributes[i] = graphicsPipelineCreateInfo.VertexInputState.VertexAttributes[i].ToSDL();
			}

			var vertexBindings = (SDL_Gpu.VertexBinding*) NativeMemory.Alloc(
				(nuint) (graphicsPipelineCreateInfo.VertexInputState.VertexBindings.Length * Marshal.SizeOf<SDL_Gpu.VertexBinding>())
			);

			for (var i = 0; i < graphicsPipelineCreateInfo.VertexInputState.VertexBindings.Length; i += 1)
			{
				vertexBindings[i] = graphicsPipelineCreateInfo.VertexInputState.VertexBindings[i].ToSDL();
			}

			var colorAttachmentDescriptions = stackalloc SDL_Gpu.ColorAttachmentDescription[
				graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions.Length
			];

			for (var i = 0; i < graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions.Length; i += 1)
			{
				colorAttachmentDescriptions[i].Format = (SDL_Gpu.TextureFormat) graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions[i].Format;
				colorAttachmentDescriptions[i].BlendState = graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions[i].BlendState.ToSDL();
			}

			sdlGraphicsPipelineCreateInfo.VertexShader = graphicsPipelineCreateInfo.VertexShader.Handle;
			sdlGraphicsPipelineCreateInfo.FragmentShader = graphicsPipelineCreateInfo.FragmentShader.Handle;

			sdlGraphicsPipelineCreateInfo.VertexInputState.VertexAttributes = vertexAttributes;
			sdlGraphicsPipelineCreateInfo.VertexInputState.VertexAttributeCount = (uint) graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length;
			sdlGraphicsPipelineCreateInfo.VertexInputState.VertexBindings = vertexBindings;
			sdlGraphicsPipelineCreateInfo.VertexInputState.VertexBindingCount = (uint) graphicsPipelineCreateInfo.VertexInputState.VertexBindings.Length;

			sdlGraphicsPipelineCreateInfo.PrimitiveType = (SDL_Gpu.PrimitiveType) graphicsPipelineCreateInfo.PrimitiveType;

			sdlGraphicsPipelineCreateInfo.RasterizerState = graphicsPipelineCreateInfo.RasterizerState.ToSDL();
			sdlGraphicsPipelineCreateInfo.MultisampleState = graphicsPipelineCreateInfo.MultisampleState.ToSDL();
			sdlGraphicsPipelineCreateInfo.DepthStencilState = graphicsPipelineCreateInfo.DepthStencilState.ToSDL();

			sdlGraphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentCount = (uint) graphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions.Length;
			sdlGraphicsPipelineCreateInfo.AttachmentInfo.ColorAttachmentDescriptions = colorAttachmentDescriptions;
			sdlGraphicsPipelineCreateInfo.AttachmentInfo.DepthStencilFormat = (SDL_Gpu.TextureFormat) graphicsPipelineCreateInfo.AttachmentInfo.DepthStencilFormat;
			sdlGraphicsPipelineCreateInfo.AttachmentInfo.HasDepthStencilAttachment = Conversions.BoolToInt(graphicsPipelineCreateInfo.AttachmentInfo.HasDepthStencilAttachment);

			sdlGraphicsPipelineCreateInfo.VertexResourceInfo = graphicsPipelineCreateInfo.VertexShaderResourceInfo.ToSDL();
			sdlGraphicsPipelineCreateInfo.FragmentResourceInfo = graphicsPipelineCreateInfo.FragmentShaderResourceInfo.ToSDL();

			sdlGraphicsPipelineCreateInfo.BlendConstants[0] = graphicsPipelineCreateInfo.BlendConstants.R;
			sdlGraphicsPipelineCreateInfo.BlendConstants[1] = graphicsPipelineCreateInfo.BlendConstants.G;
			sdlGraphicsPipelineCreateInfo.BlendConstants[2] = graphicsPipelineCreateInfo.BlendConstants.B;
			sdlGraphicsPipelineCreateInfo.BlendConstants[3] = graphicsPipelineCreateInfo.BlendConstants.A;

			Handle = SDL_Gpu.SDL_GpuCreateGraphicsPipeline(device.Handle, sdlGraphicsPipelineCreateInfo);
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
