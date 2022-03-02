using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Graphics pipelines encapsulate all of the render state in a single object.
	/// These pipelines are bound before draw calls are issued.
	/// </summary>
	public class GraphicsPipeline : GraphicsResource
	{
		protected override Action<IntPtr, IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyGraphicsPipeline;

		public GraphicsShaderInfo VertexShaderInfo { get; }
		public GraphicsShaderInfo FragmentShaderInfo { get; }

		public unsafe GraphicsPipeline(
			GraphicsDevice device,
			in GraphicsPipelineCreateInfo graphicsPipelineCreateInfo
		) : base(device)
		{
			DepthStencilState depthStencilState = graphicsPipelineCreateInfo.DepthStencilState;
			GraphicsShaderInfo vertexShaderInfo = graphicsPipelineCreateInfo.VertexShaderInfo;
			GraphicsShaderInfo fragmentShaderInfo = graphicsPipelineCreateInfo.FragmentShaderInfo;
			MultisampleState multisampleState = graphicsPipelineCreateInfo.MultisampleState;
			RasterizerState rasterizerState = graphicsPipelineCreateInfo.RasterizerState;
			PrimitiveType primitiveType = graphicsPipelineCreateInfo.PrimitiveType;
			VertexInputState vertexInputState = graphicsPipelineCreateInfo.VertexInputState;
			ViewportState viewportState = graphicsPipelineCreateInfo.ViewportState;
			GraphicsPipelineAttachmentInfo attachmentInfo = graphicsPipelineCreateInfo.AttachmentInfo;
			BlendConstants blendConstants = graphicsPipelineCreateInfo.BlendConstants;

			var vertexAttributesHandle = GCHandle.Alloc(
				vertexInputState.VertexAttributes,
				GCHandleType.Pinned
			);
			var vertexBindingsHandle = GCHandle.Alloc(
				vertexInputState.VertexBindings,
				GCHandleType.Pinned
			);
			var viewportHandle = GCHandle.Alloc(
				viewportState.Viewports,
				GCHandleType.Pinned
			);
			var scissorHandle = GCHandle.Alloc(
				viewportState.Scissors,
				GCHandleType.Pinned
			);

			var colorAttachmentDescriptions = stackalloc Refresh.ColorAttachmentDescription[
				(int) attachmentInfo.ColorAttachmentDescriptions.Length
			];

			for (var i = 0; i < attachmentInfo.ColorAttachmentDescriptions.Length; i += 1)
			{
				colorAttachmentDescriptions[i].format = (Refresh.TextureFormat) attachmentInfo.ColorAttachmentDescriptions[i].Format;
				colorAttachmentDescriptions[i].sampleCount = (Refresh.SampleCount) attachmentInfo.ColorAttachmentDescriptions[i].SampleCount;
				colorAttachmentDescriptions[i].blendState = attachmentInfo.ColorAttachmentDescriptions[i].BlendState.ToRefresh();
			}

			Refresh.GraphicsPipelineCreateInfo refreshGraphicsPipelineCreateInfo;

			refreshGraphicsPipelineCreateInfo.blendConstants[0] = blendConstants.R;
			refreshGraphicsPipelineCreateInfo.blendConstants[1] = blendConstants.G;
			refreshGraphicsPipelineCreateInfo.blendConstants[2] = blendConstants.B;
			refreshGraphicsPipelineCreateInfo.blendConstants[3] = blendConstants.A;

			refreshGraphicsPipelineCreateInfo.depthStencilState.backStencilState = depthStencilState.BackStencilState.ToRefresh();
			refreshGraphicsPipelineCreateInfo.depthStencilState.compareOp = (Refresh.CompareOp) depthStencilState.CompareOp;
			refreshGraphicsPipelineCreateInfo.depthStencilState.depthBoundsTestEnable = Conversions.BoolToByte(depthStencilState.DepthBoundsTestEnable);
			refreshGraphicsPipelineCreateInfo.depthStencilState.depthTestEnable = Conversions.BoolToByte(depthStencilState.DepthTestEnable);
			refreshGraphicsPipelineCreateInfo.depthStencilState.depthWriteEnable = Conversions.BoolToByte(depthStencilState.DepthWriteEnable);
			refreshGraphicsPipelineCreateInfo.depthStencilState.frontStencilState = depthStencilState.FrontStencilState.ToRefresh();
			refreshGraphicsPipelineCreateInfo.depthStencilState.maxDepthBounds = depthStencilState.MaxDepthBounds;
			refreshGraphicsPipelineCreateInfo.depthStencilState.minDepthBounds = depthStencilState.MinDepthBounds;
			refreshGraphicsPipelineCreateInfo.depthStencilState.stencilTestEnable = Conversions.BoolToByte(depthStencilState.StencilTestEnable);

			refreshGraphicsPipelineCreateInfo.vertexShaderInfo.entryPointName = vertexShaderInfo.EntryPointName;
			refreshGraphicsPipelineCreateInfo.vertexShaderInfo.shaderModule = vertexShaderInfo.ShaderModule.Handle;
			refreshGraphicsPipelineCreateInfo.vertexShaderInfo.uniformBufferSize = vertexShaderInfo.UniformBufferSize;
			refreshGraphicsPipelineCreateInfo.vertexShaderInfo.samplerBindingCount = vertexShaderInfo.SamplerBindingCount;

			refreshGraphicsPipelineCreateInfo.fragmentShaderInfo.entryPointName = fragmentShaderInfo.EntryPointName;
			refreshGraphicsPipelineCreateInfo.fragmentShaderInfo.shaderModule = fragmentShaderInfo.ShaderModule.Handle;
			refreshGraphicsPipelineCreateInfo.fragmentShaderInfo.uniformBufferSize = fragmentShaderInfo.UniformBufferSize;
			refreshGraphicsPipelineCreateInfo.fragmentShaderInfo.samplerBindingCount = fragmentShaderInfo.SamplerBindingCount;

			refreshGraphicsPipelineCreateInfo.multisampleState.multisampleCount = (Refresh.SampleCount) multisampleState.MultisampleCount;
			refreshGraphicsPipelineCreateInfo.multisampleState.sampleMask = multisampleState.SampleMask;

			refreshGraphicsPipelineCreateInfo.rasterizerState.cullMode = (Refresh.CullMode) rasterizerState.CullMode;
			refreshGraphicsPipelineCreateInfo.rasterizerState.depthBiasClamp = rasterizerState.DepthBiasClamp;
			refreshGraphicsPipelineCreateInfo.rasterizerState.depthBiasConstantFactor = rasterizerState.DepthBiasConstantFactor;
			refreshGraphicsPipelineCreateInfo.rasterizerState.depthBiasEnable = Conversions.BoolToByte(rasterizerState.DepthBiasEnable);
			refreshGraphicsPipelineCreateInfo.rasterizerState.depthBiasSlopeFactor = rasterizerState.DepthBiasSlopeFactor;
			refreshGraphicsPipelineCreateInfo.rasterizerState.depthClampEnable = Conversions.BoolToByte(rasterizerState.DepthClampEnable);
			refreshGraphicsPipelineCreateInfo.rasterizerState.fillMode = (Refresh.FillMode) rasterizerState.FillMode;
			refreshGraphicsPipelineCreateInfo.rasterizerState.frontFace = (Refresh.FrontFace) rasterizerState.FrontFace;
			refreshGraphicsPipelineCreateInfo.rasterizerState.lineWidth = rasterizerState.LineWidth;

			refreshGraphicsPipelineCreateInfo.vertexInputState.vertexAttributes = vertexAttributesHandle.AddrOfPinnedObject();
			refreshGraphicsPipelineCreateInfo.vertexInputState.vertexAttributeCount = (uint) vertexInputState.VertexAttributes.Length;
			refreshGraphicsPipelineCreateInfo.vertexInputState.vertexBindings = vertexBindingsHandle.AddrOfPinnedObject();
			refreshGraphicsPipelineCreateInfo.vertexInputState.vertexBindingCount = (uint) vertexInputState.VertexBindings.Length;

			refreshGraphicsPipelineCreateInfo.viewportState.viewports = viewportHandle.AddrOfPinnedObject();
			refreshGraphicsPipelineCreateInfo.viewportState.viewportCount = (uint) viewportState.Viewports.Length;
			refreshGraphicsPipelineCreateInfo.viewportState.scissors = scissorHandle.AddrOfPinnedObject();
			refreshGraphicsPipelineCreateInfo.viewportState.scissorCount = (uint) viewportState.Scissors.Length;

			refreshGraphicsPipelineCreateInfo.primitiveType = (Refresh.PrimitiveType) primitiveType;

			refreshGraphicsPipelineCreateInfo.attachmentInfo.colorAttachmentCount = (uint) attachmentInfo.ColorAttachmentDescriptions.Length;
			refreshGraphicsPipelineCreateInfo.attachmentInfo.colorAttachmentDescriptions = (IntPtr) colorAttachmentDescriptions;
			refreshGraphicsPipelineCreateInfo.attachmentInfo.depthStencilFormat = (Refresh.TextureFormat) attachmentInfo.DepthStencilFormat;
			refreshGraphicsPipelineCreateInfo.attachmentInfo.hasDepthStencilAttachment = Conversions.BoolToByte(attachmentInfo.HasDepthStencilAttachment);

			Handle = Refresh.Refresh_CreateGraphicsPipeline(device.Handle, refreshGraphicsPipelineCreateInfo);

			vertexAttributesHandle.Free();
			vertexBindingsHandle.Free();
			viewportHandle.Free();
			scissorHandle.Free();

			VertexShaderInfo = vertexShaderInfo;
			FragmentShaderInfo = fragmentShaderInfo;
		}
	}
}
