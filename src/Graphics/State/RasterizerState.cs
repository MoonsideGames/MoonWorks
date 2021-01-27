namespace MoonWorks.Graphics
{
    public struct RasterizerState
    {
        public CullMode CullMode;
        public float DepthBiasClamp;
        public float DepthBiasConstantFactor;
        public bool DepthBiasEnable;
        public float DepthBiasSlopeFactor;
        public bool DepthClampEnable;
        public FillMode FillMode;
        public FrontFace FrontFace;
        public float LineWidth;

        public static readonly RasterizerState CW_CullFront = new RasterizerState
        {
            CullMode = CullMode.Front,
            FrontFace = FrontFace.Clockwise,
            FillMode = FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CW_CullBack = new RasterizerState
        {
            CullMode = CullMode.Back,
            FrontFace = FrontFace.Clockwise,
            FillMode = FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CW_CullNone = new RasterizerState
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.Clockwise,
            FillMode = FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CW_Wireframe = new RasterizerState
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.Clockwise,
            FillMode = FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CCW_CullFront = new RasterizerState
        {
            CullMode = CullMode.Front,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CCW_CullBack = new RasterizerState
        {
            CullMode = CullMode.Back,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CCW_CullNone = new RasterizerState
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };

        public static readonly RasterizerState CCW_Wireframe = new RasterizerState
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Fill,
            DepthBiasEnable = false,
            LineWidth = 1f
        };
    }
}
