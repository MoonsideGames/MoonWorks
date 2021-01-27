using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class GraphicsPipeline : GraphicsResource
    {
        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyGraphicsPipeline;

        public unsafe GraphicsPipeline(
            GraphicsDevice device,
            ColorBlendState colorBlendState,
            DepthStencilState depthStencilState,
            ShaderStageState vertexShaderState,
            ShaderStageState fragmentShaderState,
            MultisampleState multisampleState,
            GraphicsPipelineLayoutCreateInfo pipelineLayoutCreateInfo,
            RasterizerState rasterizerState,
            PrimitiveType primitiveType,
            VertexInputState vertexInputState,
            ViewportState viewportState,
            RenderPass renderPass
        ) : base(device)
        {
            var vertexAttributesHandle = GCHandle.Alloc(vertexInputState.VertexAttributes, GCHandleType.Pinned);
            var vertexBindingsHandle = GCHandle.Alloc(vertexInputState.VertexBindings, GCHandleType.Pinned);
            var viewportHandle = GCHandle.Alloc(viewportState.Viewports, GCHandleType.Pinned);
            var scissorHandle = GCHandle.Alloc(viewportState.Scissors, GCHandleType.Pinned);

            var colorTargetBlendStates = stackalloc Refresh.ColorTargetBlendState[colorBlendState.ColorTargetBlendStates.Length];

            for (var i = 0; i < colorBlendState.ColorTargetBlendStates.Length; i += 1)
            {
                colorTargetBlendStates[i] = colorBlendState.ColorTargetBlendStates[i].ToRefreshColorTargetBlendState();
            }

            Refresh.GraphicsPipelineCreateInfo graphicsPipelineCreateInfo;

            graphicsPipelineCreateInfo.colorBlendState.logicOpEnable = Conversions.BoolToByte(colorBlendState.LogicOpEnable);
            graphicsPipelineCreateInfo.colorBlendState.logicOp = (Refresh.LogicOp) colorBlendState.LogicOp;
            graphicsPipelineCreateInfo.colorBlendState.blendStates = (IntPtr) colorTargetBlendStates;
            graphicsPipelineCreateInfo.colorBlendState.blendStateCount = (uint) colorBlendState.ColorTargetBlendStates.Length;
            graphicsPipelineCreateInfo.colorBlendState.blendConstants[0] = colorBlendState.BlendConstants.R;
            graphicsPipelineCreateInfo.colorBlendState.blendConstants[1] = colorBlendState.BlendConstants.G;
            graphicsPipelineCreateInfo.colorBlendState.blendConstants[2] = colorBlendState.BlendConstants.B;
            graphicsPipelineCreateInfo.colorBlendState.blendConstants[3] = colorBlendState.BlendConstants.A;

            graphicsPipelineCreateInfo.depthStencilState.backStencilState = depthStencilState.BackStencilState.ToRefresh();
            graphicsPipelineCreateInfo.depthStencilState.compareOp = (Refresh.CompareOp) depthStencilState.CompareOp;
            graphicsPipelineCreateInfo.depthStencilState.depthBoundsTestEnable = Conversions.BoolToByte(depthStencilState.DepthBoundsTestEnable);
            graphicsPipelineCreateInfo.depthStencilState.depthTestEnable = Conversions.BoolToByte(depthStencilState.DepthTestEnable);
            graphicsPipelineCreateInfo.depthStencilState.depthWriteEnable = Conversions.BoolToByte(depthStencilState.DepthWriteEnable);
            graphicsPipelineCreateInfo.depthStencilState.frontStencilState = depthStencilState.FrontStencilState.ToRefresh();
            graphicsPipelineCreateInfo.depthStencilState.maxDepthBounds = depthStencilState.MaxDepthBounds;
            graphicsPipelineCreateInfo.depthStencilState.minDepthBounds = depthStencilState.MinDepthBounds;
            graphicsPipelineCreateInfo.depthStencilState.stencilTestEnable = Conversions.BoolToByte(depthStencilState.StencilTestEnable);

            graphicsPipelineCreateInfo.vertexShaderState.entryPointName = vertexShaderState.EntryPointName;
            graphicsPipelineCreateInfo.vertexShaderState.shaderModule = vertexShaderState.ShaderModule.Handle;
            graphicsPipelineCreateInfo.vertexShaderState.uniformBufferSize = vertexShaderState.UniformBufferSize;

            graphicsPipelineCreateInfo.fragmentShaderState.entryPointName = fragmentShaderState.EntryPointName;
            graphicsPipelineCreateInfo.fragmentShaderState.shaderModule = fragmentShaderState.ShaderModule.Handle;
            graphicsPipelineCreateInfo.fragmentShaderState.uniformBufferSize = fragmentShaderState.UniformBufferSize;

            graphicsPipelineCreateInfo.multisampleState.multisampleCount = (Refresh.SampleCount)multisampleState.MultisampleCount;
            graphicsPipelineCreateInfo.multisampleState.sampleMask = multisampleState.SampleMask;

            graphicsPipelineCreateInfo.pipelineLayoutCreateInfo.vertexSamplerBindingCount = pipelineLayoutCreateInfo.VertexSamplerBindingCount;
            graphicsPipelineCreateInfo.pipelineLayoutCreateInfo.fragmentSamplerBindingCount = pipelineLayoutCreateInfo.FragmentSamplerBindingCount;

            graphicsPipelineCreateInfo.rasterizerState.cullMode = (Refresh.CullMode)rasterizerState.CullMode;
            graphicsPipelineCreateInfo.rasterizerState.depthBiasClamp = rasterizerState.DepthBiasClamp;
            graphicsPipelineCreateInfo.rasterizerState.depthBiasConstantFactor = rasterizerState.DepthBiasConstantFactor;
            graphicsPipelineCreateInfo.rasterizerState.depthBiasEnable = Conversions.BoolToByte(rasterizerState.DepthBiasEnable);
            graphicsPipelineCreateInfo.rasterizerState.depthBiasSlopeFactor = rasterizerState.DepthBiasSlopeFactor;
            graphicsPipelineCreateInfo.rasterizerState.depthClampEnable = Conversions.BoolToByte(rasterizerState.DepthClampEnable);
            graphicsPipelineCreateInfo.rasterizerState.fillMode = (Refresh.FillMode)rasterizerState.FillMode;
            graphicsPipelineCreateInfo.rasterizerState.frontFace = (Refresh.FrontFace)rasterizerState.FrontFace;
            graphicsPipelineCreateInfo.rasterizerState.lineWidth = rasterizerState.LineWidth;

            graphicsPipelineCreateInfo.vertexInputState.vertexAttributes = vertexAttributesHandle.AddrOfPinnedObject();
            graphicsPipelineCreateInfo.vertexInputState.vertexAttributeCount = (uint) vertexInputState.VertexAttributes.Length;
            graphicsPipelineCreateInfo.vertexInputState.vertexBindings = vertexBindingsHandle.AddrOfPinnedObject();
            graphicsPipelineCreateInfo.vertexInputState.vertexBindingCount = (uint) vertexInputState.VertexBindings.Length;

            graphicsPipelineCreateInfo.viewportState.viewports = viewportHandle.AddrOfPinnedObject();
            graphicsPipelineCreateInfo.viewportState.viewportCount = (uint) viewportState.Viewports.Length;
            graphicsPipelineCreateInfo.viewportState.scissors = scissorHandle.AddrOfPinnedObject();
            graphicsPipelineCreateInfo.viewportState.scissorCount = (uint) viewportState.Scissors.Length;

            graphicsPipelineCreateInfo.primitiveType = (Refresh.PrimitiveType) primitiveType;
            graphicsPipelineCreateInfo.renderPass = renderPass.Handle;

            Handle = Refresh.Refresh_CreateGraphicsPipeline(device.Handle, graphicsPipelineCreateInfo);

            vertexAttributesHandle.Free();
            vertexBindingsHandle.Free();
            viewportHandle.Free();
            scissorHandle.Free();
        }
    }
}
