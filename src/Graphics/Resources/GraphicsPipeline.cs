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

		public ShaderStageState VertexShaderState { get; }
		public ShaderStageState FragmentShaderState { get; }

		public unsafe GraphicsPipeline(
			GraphicsDevice device,
			in GraphicsPipelineCreateInfo graphicsPipelineCreateInfo
		) : base(device)
		{
			ColorBlendState colorBlendState = graphicsPipelineCreateInfo.ColorBlendState;
			DepthStencilState depthStencilState = graphicsPipelineCreateInfo.DepthStencilState;
			ShaderStageState vertexShaderState = graphicsPipelineCreateInfo.VertexShaderState;
			ShaderStageState fragmentShaderState = graphicsPipelineCreateInfo.FragmentShaderState;
			MultisampleState multisampleState = graphicsPipelineCreateInfo.MultisampleState;
			GraphicsPipelineLayoutInfo pipelineLayoutInfo = graphicsPipelineCreateInfo.PipelineLayoutInfo;
			RasterizerState rasterizerState = graphicsPipelineCreateInfo.RasterizerState;
			PrimitiveType primitiveType = graphicsPipelineCreateInfo.PrimitiveType;
			VertexInputState vertexInputState = graphicsPipelineCreateInfo.VertexInputState;
			ViewportState viewportState = graphicsPipelineCreateInfo.ViewportState;
			GraphicsPipelineAttachmentInfo attachmentInfo = graphicsPipelineCreateInfo.AttachmentInfo;

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

			var colorTargetBlendStates = stackalloc Refresh.ColorTargetBlendState[
				colorBlendState.ColorTargetBlendStates.Length
			];

			for (var i = 0; i < colorBlendState.ColorTargetBlendStates.Length; i += 1)
			{
				colorTargetBlendStates[i] = colorBlendState.ColorTargetBlendStates[i].ToRefreshColorTargetBlendState();
			}

			var colorAttachmentDescriptions = stackalloc Refresh.ColorAttachmentDescription[
				(int) attachmentInfo.ColorAttachmentCount
			];

			for (var i = 0; i < attachmentInfo.ColorAttachmentCount; i += 1)
			{
				colorAttachmentDescriptions[i].format = (Refresh.TextureFormat) attachmentInfo.ColorAttachmentDescriptions[i].Format;
				colorAttachmentDescriptions[i].sampleCount = (Refresh.SampleCount) attachmentInfo.ColorAttachmentDescriptions[i].SampleCount;
			}

			Refresh.GraphicsPipelineCreateInfo refreshGraphicsPipelineCreateInfo;

			refreshGraphicsPipelineCreateInfo.colorBlendState.logicOpEnable = Conversions.BoolToByte(colorBlendState.LogicOpEnable);
			refreshGraphicsPipelineCreateInfo.colorBlendState.logicOp = (Refresh.LogicOp) colorBlendState.LogicOp;
			refreshGraphicsPipelineCreateInfo.colorBlendState.blendStates = (IntPtr) colorTargetBlendStates;
			refreshGraphicsPipelineCreateInfo.colorBlendState.blendStateCount = (uint) colorBlendState.ColorTargetBlendStates.Length;
			refreshGraphicsPipelineCreateInfo.colorBlendState.blendConstants[0] = colorBlendState.BlendConstants.R;
			refreshGraphicsPipelineCreateInfo.colorBlendState.blendConstants[1] = colorBlendState.BlendConstants.G;
			refreshGraphicsPipelineCreateInfo.colorBlendState.blendConstants[2] = colorBlendState.BlendConstants.B;
			refreshGraphicsPipelineCreateInfo.colorBlendState.blendConstants[3] = colorBlendState.BlendConstants.A;

			refreshGraphicsPipelineCreateInfo.depthStencilState.backStencilState = depthStencilState.BackStencilState.ToRefresh();
			refreshGraphicsPipelineCreateInfo.depthStencilState.compareOp = (Refresh.CompareOp) depthStencilState.CompareOp;
			refreshGraphicsPipelineCreateInfo.depthStencilState.depthBoundsTestEnable = Conversions.BoolToByte(depthStencilState.DepthBoundsTestEnable);
			refreshGraphicsPipelineCreateInfo.depthStencilState.depthTestEnable = Conversions.BoolToByte(depthStencilState.DepthTestEnable);
			refreshGraphicsPipelineCreateInfo.depthStencilState.depthWriteEnable = Conversions.BoolToByte(depthStencilState.DepthWriteEnable);
			refreshGraphicsPipelineCreateInfo.depthStencilState.frontStencilState = depthStencilState.FrontStencilState.ToRefresh();
			refreshGraphicsPipelineCreateInfo.depthStencilState.maxDepthBounds = depthStencilState.MaxDepthBounds;
			refreshGraphicsPipelineCreateInfo.depthStencilState.minDepthBounds = depthStencilState.MinDepthBounds;
			refreshGraphicsPipelineCreateInfo.depthStencilState.stencilTestEnable = Conversions.BoolToByte(depthStencilState.StencilTestEnable);

			refreshGraphicsPipelineCreateInfo.vertexShaderState.entryPointName = vertexShaderState.EntryPointName;
			refreshGraphicsPipelineCreateInfo.vertexShaderState.shaderModule = vertexShaderState.ShaderModule.Handle;
			refreshGraphicsPipelineCreateInfo.vertexShaderState.uniformBufferSize = vertexShaderState.UniformBufferSize;

			refreshGraphicsPipelineCreateInfo.fragmentShaderState.entryPointName = fragmentShaderState.EntryPointName;
			refreshGraphicsPipelineCreateInfo.fragmentShaderState.shaderModule = fragmentShaderState.ShaderModule.Handle;
			refreshGraphicsPipelineCreateInfo.fragmentShaderState.uniformBufferSize = fragmentShaderState.UniformBufferSize;

			refreshGraphicsPipelineCreateInfo.multisampleState.multisampleCount = (Refresh.SampleCount) multisampleState.MultisampleCount;
			refreshGraphicsPipelineCreateInfo.multisampleState.sampleMask = multisampleState.SampleMask;

			refreshGraphicsPipelineCreateInfo.pipelineLayoutCreateInfo.vertexSamplerBindingCount = pipelineLayoutInfo.VertexSamplerBindingCount;
			refreshGraphicsPipelineCreateInfo.pipelineLayoutCreateInfo.fragmentSamplerBindingCount = pipelineLayoutInfo.FragmentSamplerBindingCount;

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

			refreshGraphicsPipelineCreateInfo.attachmentInfo.colorAttachmentCount = attachmentInfo.ColorAttachmentCount;
			refreshGraphicsPipelineCreateInfo.attachmentInfo.colorAttachmentDescriptions = (IntPtr) colorAttachmentDescriptions;
			refreshGraphicsPipelineCreateInfo.attachmentInfo.depthStencilFormat = (Refresh.TextureFormat) attachmentInfo.DepthStencilFormat;
			refreshGraphicsPipelineCreateInfo.attachmentInfo.hasDepthStencilAttachment = Conversions.BoolToByte(attachmentInfo.HasDepthStencilAttachment);

			Handle = Refresh.Refresh_CreateGraphicsPipeline(device.Handle, refreshGraphicsPipelineCreateInfo);

			vertexAttributesHandle.Free();
			vertexBindingsHandle.Free();
			viewportHandle.Free();
			scissorHandle.Free();

			VertexShaderState = vertexShaderState;
			FragmentShaderState = fragmentShaderState;
		}
	}
}
