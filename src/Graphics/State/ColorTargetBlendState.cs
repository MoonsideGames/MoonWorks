using RefreshCS;

namespace MoonWorks.Graphics
{
    public struct ColorTargetBlendState
    {
        public bool BlendEnable;
        public BlendOp AlphaBlendOp;
        public BlendOp ColorBlendOp;
        public ColorComponentFlags ColorWriteMask;
        public BlendFactor DestinationAlphaBlendFactor;
        public BlendFactor DestinationColorBlendFactor;
        public BlendFactor SourceAlphaBlendFactor;
        public BlendFactor SourceColorBlendFactor;

        public static readonly ColorTargetBlendState Additive = new ColorTargetBlendState
        {
            BlendEnable = true,
            AlphaBlendOp = BlendOp.Add,
            ColorBlendOp = BlendOp.Add,
            ColorWriteMask = ColorComponentFlags.RGBA,
            SourceColorBlendFactor = BlendFactor.SourceAlpha,
            SourceAlphaBlendFactor = BlendFactor.SourceAlpha,
            DestinationColorBlendFactor = BlendFactor.One,
            DestinationAlphaBlendFactor = BlendFactor.One
        };

        public static readonly ColorTargetBlendState AlphaBlend = new ColorTargetBlendState
        {
            BlendEnable = true,
            AlphaBlendOp = BlendOp.Add,
            ColorBlendOp = BlendOp.Add,
            ColorWriteMask = ColorComponentFlags.RGBA,
            SourceColorBlendFactor = BlendFactor.One,
            SourceAlphaBlendFactor = BlendFactor.One,
            DestinationColorBlendFactor = BlendFactor.OneMinusSourceAlpha,
            DestinationAlphaBlendFactor = BlendFactor.OneMinusSourceAlpha
        };

        public static readonly ColorTargetBlendState NonPremultiplied = new ColorTargetBlendState
        {
            BlendEnable = true,
            AlphaBlendOp = BlendOp.Add,
            ColorBlendOp = BlendOp.Add,
            ColorWriteMask = ColorComponentFlags.RGBA,
            SourceColorBlendFactor = BlendFactor.SourceAlpha,
            SourceAlphaBlendFactor = BlendFactor.SourceAlpha,
            DestinationColorBlendFactor = BlendFactor.OneMinusSourceAlpha,
            DestinationAlphaBlendFactor = BlendFactor.OneMinusSourceAlpha
        };

        public static readonly ColorTargetBlendState Opaque = new ColorTargetBlendState
        {
            BlendEnable = true,
            AlphaBlendOp = BlendOp.Add,
            ColorBlendOp = BlendOp.Add,
            ColorWriteMask = ColorComponentFlags.RGBA,
            SourceColorBlendFactor = BlendFactor.One,
            SourceAlphaBlendFactor = BlendFactor.One,
            DestinationColorBlendFactor = BlendFactor.Zero,
            DestinationAlphaBlendFactor = BlendFactor.Zero
        };

        public static readonly ColorTargetBlendState None = new ColorTargetBlendState
        {
            BlendEnable = false,
            ColorWriteMask = ColorComponentFlags.RGBA
        };

        public static readonly ColorTargetBlendState Disable = new ColorTargetBlendState
        {
            BlendEnable = false,
            ColorWriteMask = ColorComponentFlags.None
        };

        public Refresh.ColorTargetBlendState ToRefreshColorTargetBlendState()
        {
            return new Refresh.ColorTargetBlendState
            {
                blendEnable = Conversions.BoolToByte(BlendEnable),
                alphaBlendOp = (Refresh.BlendOp)AlphaBlendOp,
                colorBlendOp = (Refresh.BlendOp)ColorBlendOp,
                colorWriteMask = (Refresh.ColorComponentFlags)ColorWriteMask,
                destinationAlphaBlendFactor = (Refresh.BlendFactor)DestinationAlphaBlendFactor,
                destinationColorBlendFactor = (Refresh.BlendFactor)DestinationColorBlendFactor,
                sourceAlphaBlendFactor = (Refresh.BlendFactor)SourceAlphaBlendFactor,
                sourceColorBlendFactor = (Refresh.BlendFactor)SourceColorBlendFactor
            };
        }
    }
}
