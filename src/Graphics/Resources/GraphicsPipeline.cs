using System;
using System.Runtime.InteropServices;
using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics;

/// <summary>
/// Graphics pipelines encapsulate all of the render state in a single object. <br/>
/// These pipelines are bound before draw calls are issued.
/// </summary>
public class GraphicsPipeline : SDLGPUResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL.SDL_ReleaseGPUGraphicsPipeline;

	public Shader VertexShader;
	public Shader FragmentShader;

	public static unsafe GraphicsPipeline Create(
		GraphicsDevice device,
		in GraphicsPipelineCreateInfo graphicsPipelineCreateInfo
	) {
		INTERNAL_GraphicsPipelineCreateInfo createInfo;

		var vertexAttributes = (VertexAttribute*) NativeMemory.Alloc(
			(nuint) (graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length * Marshal.SizeOf<VertexAttribute>())
		);

		for (var i = 0; i < graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length; i += 1)
		{
			vertexAttributes[i] = graphicsPipelineCreateInfo.VertexInputState.VertexAttributes[i];
		}

		var vertexBindings = (VertexBufferDescription*) NativeMemory.Alloc(
			(nuint) (graphicsPipelineCreateInfo.VertexInputState.VertexBufferDescriptions.Length * Marshal.SizeOf<VertexBufferDescription>())
		);

		for (var i = 0; i < graphicsPipelineCreateInfo.VertexInputState.VertexBufferDescriptions.Length; i += 1)
		{
			vertexBindings[i] = graphicsPipelineCreateInfo.VertexInputState.VertexBufferDescriptions[i];
		}

		var numColorTargets = graphicsPipelineCreateInfo.TargetInfo.ColorTargetDescriptions != null ? graphicsPipelineCreateInfo.TargetInfo.ColorTargetDescriptions.Length : 0;

		var colorAttachmentDescriptions = stackalloc ColorTargetDescription[
			numColorTargets
		];

		for (var i = 0; i < numColorTargets; i += 1)
		{
			colorAttachmentDescriptions[i].Format = graphicsPipelineCreateInfo.TargetInfo.ColorTargetDescriptions[i].Format;
			colorAttachmentDescriptions[i].BlendState = graphicsPipelineCreateInfo.TargetInfo.ColorTargetDescriptions[i].BlendState;
		}

		createInfo.VertexShader = graphicsPipelineCreateInfo.VertexShader.Handle;
		createInfo.FragmentShader = graphicsPipelineCreateInfo.FragmentShader.Handle;

		createInfo.VertexInputState.VertexAttributes = vertexAttributes;
		createInfo.VertexInputState.NumVertexAttributes = (uint) graphicsPipelineCreateInfo.VertexInputState.VertexAttributes.Length;
		createInfo.VertexInputState.VertexBufferDescriptions = vertexBindings;
		createInfo.VertexInputState.NumVertexBuffers = (uint) graphicsPipelineCreateInfo.VertexInputState.VertexBufferDescriptions.Length;

		createInfo.PrimitiveType = graphicsPipelineCreateInfo.PrimitiveType;
		createInfo.RasterizerState = graphicsPipelineCreateInfo.RasterizerState;
		createInfo.MultisampleState = graphicsPipelineCreateInfo.MultisampleState;
		createInfo.DepthStencilState = graphicsPipelineCreateInfo.DepthStencilState;

		createInfo.TargetInfo = new INTERNAL_GraphicsPipelineTargetInfo
		{
			NumColorTargets = (uint) numColorTargets,
			ColorTargetDescriptions = colorAttachmentDescriptions,
			DepthStencilFormat = graphicsPipelineCreateInfo.TargetInfo.DepthStencilFormat,
			HasDepthStencilTarget = graphicsPipelineCreateInfo.TargetInfo.HasDepthStencilTarget
		};

		createInfo.Props = graphicsPipelineCreateInfo.Props;

		var handle = SDL.SDL_CreateGPUGraphicsPipeline(device.Handle, createInfo);

		NativeMemory.Free(vertexAttributes);
		NativeMemory.Free(vertexBindings);

		if (handle == IntPtr.Zero)
		{
			throw new Exception("Could not create graphics pipeline!");
		}

		return new GraphicsPipeline(device)
		{
			Handle = handle,
			VertexShader = graphicsPipelineCreateInfo.VertexShader,
			FragmentShader = graphicsPipelineCreateInfo.FragmentShader,
		};
	}

	private GraphicsPipeline(GraphicsDevice device) : base(device)
	{
		Name = "GraphicsPipeline";
	}
}
