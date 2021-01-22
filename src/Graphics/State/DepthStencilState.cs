using RefreshCS;

namespace MoonWorks.Graphics
{
    public struct DepthStencilState
    {
        public bool DepthTestEnable;
        public StencilOpState BackStencilState;
        public StencilOpState FrontStencilState;
        public CompareOp CompareOp;
        public bool DepthBoundsTestEnable;
        public bool DepthWriteEnable;
        public float MinDepthBounds;
        public float MaxDepthBounds;
        public bool StencilTestEnable;

        public static readonly DepthStencilState DepthReadWrite = new DepthStencilState
        {
            DepthTestEnable = true,
            DepthWriteEnable = true,
            DepthBoundsTestEnable = false,
            StencilTestEnable = false,
            CompareOp = CompareOp.LessOrEqual
        };

        public static readonly DepthStencilState DepthRead = new DepthStencilState
        {
            DepthTestEnable = true,
            DepthWriteEnable = false,
            DepthBoundsTestEnable = false,
            StencilTestEnable = false,
            CompareOp = CompareOp.LessOrEqual
        };

        public static readonly DepthStencilState Disable = new DepthStencilState
        {
            DepthTestEnable = false,
            DepthWriteEnable = false,
            DepthBoundsTestEnable = false,
            StencilTestEnable = false
        };
    }
}
