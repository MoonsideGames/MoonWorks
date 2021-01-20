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

        public static readonly RasterizerState CullClockwise = new RasterizerState
        {
            CullMode = Refresh.CullMode.Front,
            FrontFace = Refresh.FrontFace.Clockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CullCounterClockwise = new RasterizerState
        {
            CullMode = Refresh.CullMode.Back,
            FrontFace = Refresh.FrontFace.Clockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CullNone = new RasterizerState
        {
            CullMode = Refresh.CullMode.None,
            FrontFace = Refresh.FrontFace.Clockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState Wireframe = new RasterizerState
        {
            CullMode = Refresh.CullMode.None,
            FrontFace = Refresh.FrontFace.Clockwise,
            FillMode = Refresh.FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };
    }
}
