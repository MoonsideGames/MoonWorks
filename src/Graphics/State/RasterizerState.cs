using RefreshCS;

namespace MoonWorks.Graphics
{
    public struct RasterizerState
    {
        public Refresh.CullMode CullMode;
        public float DepthBiasClamp;
        public float DepthBiasConstantFactor;
        public bool DepthBiasEnable;
        public float DepthBiasSlopeFactor;
        public bool DepthClampEnable;
        public Refresh.FillMode FillMode;
        public Refresh.FrontFace FrontFace;
        public float LineWidth;

        public static readonly RasterizerState CW_CullFront = new RasterizerState
        {
            CullMode = Refresh.CullMode.Front,
            FrontFace = Refresh.FrontFace.Clockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CW_CullBack = new RasterizerState
        {
            CullMode = Refresh.CullMode.Back,
            FrontFace = Refresh.FrontFace.Clockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CW_CullNone = new RasterizerState
        {
            CullMode = Refresh.CullMode.None,
            FrontFace = Refresh.FrontFace.Clockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CW_Wireframe = new RasterizerState
        {
            CullMode = Refresh.CullMode.None,
            FrontFace = Refresh.FrontFace.Clockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CCW_CullFront = new RasterizerState
        {
            CullMode = Refresh.CullMode.Front,
            FrontFace = Refresh.FrontFace.CounterClockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CCW_CullBack = new RasterizerState
        {
            CullMode = Refresh.CullMode.Back,
            FrontFace = Refresh.FrontFace.CounterClockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CCW_CullNone = new RasterizerState
        {
            CullMode = Refresh.CullMode.None,
            FrontFace = Refresh.FrontFace.CounterClockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CCW_Wireframe = new RasterizerState
        {
            CullMode = Refresh.CullMode.None,
            FrontFace = Refresh.FrontFace.CounterClockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };
    }
}
