using RefreshCS;

namespace MoonWorks.Graphics
{
    public struct ColorTargetBlendState
    {
        public bool BlendEnable;
        public Refresh.BlendOp AlphaBlendOp;
        public Refresh.BlendOp ColorBlendOp;
        public Refresh.ColorComponentFlags ColorWriteMask;
        public Refresh.BlendFactor DestinationAlphaBlendFactor;
        public Refresh.BlendFactor DestinationColorBlendFactor;
        public Refresh.BlendFactor SourceAlphaBlendFactor;
        public Refresh.BlendFactor SourceColorBlendFactor;

        public static readonly ColorTargetBlendState Additive = new ColorTargetBlendState
        {
            BlendEnable = true,
            AlphaBlendOp = Refresh.BlendOp.Add,
            ColorBlendOp = Refresh.BlendOp.Add,
            ColorWriteMask = Refresh.ColorComponentFlags.RGBA,
            SourceColorBlendFactor = Refresh.BlendFactor.SourceAlpha,
            SourceAlphaBlendFactor = Refresh.BlendFactor.SourceAlpha,
            DestinationColorBlendFactor = Refresh.BlendFactor.One,
            DestinationAlphaBlendFactor = Refresh.BlendFactor.One
        };

        public static readonly ColorTargetBlendState AlphaBlend = new ColorTargetBlendState
        {
            BlendEnable = true,
            AlphaBlendOp = Refresh.BlendOp.Add,
            ColorBlendOp = Refresh.BlendOp.Add,
            ColorWriteMask = Refresh.ColorComponentFlags.RGBA,
            SourceColorBlendFactor = Refresh.BlendFactor.One,
            SourceAlphaBlendFactor = Refresh.BlendFactor.One,
            DestinationColorBlendFactor = Refresh.BlendFactor.OneMinusSourceAlpha,
            DestinationAlphaBlendFactor = Refresh.BlendFactor.OneMinusSourceAlpha
        };

        public static readonly ColorTargetBlendState NonPremultiplied = new ColorTargetBlendState
        {
            BlendEnable = true,
            AlphaBlendOp = Refresh.BlendOp.Add,
            ColorBlendOp = Refresh.BlendOp.Add,
            ColorWriteMask = Refresh.ColorComponentFlags.RGBA,
            SourceColorBlendFactor = Refresh.BlendFactor.SourceAlpha,
            SourceAlphaBlendFactor = Refresh.BlendFactor.SourceAlpha,
            DestinationColorBlendFactor = Refresh.BlendFactor.OneMinusSourceAlpha,
            DestinationAlphaBlendFactor = Refresh.BlendFactor.OneMinusSourceAlpha
        };

        public static readonly ColorTargetBlendState Opaque = new ColorTargetBlendState
        {
            BlendEnable = true,
            AlphaBlendOp = Refresh.BlendOp.Add,
            ColorBlendOp = Refresh.BlendOp.Add,
            ColorWriteMask = Refresh.ColorComponentFlags.RGBA,
            SourceColorBlendFactor = Refresh.BlendFactor.One,
            SourceAlphaBlendFactor = Refresh.BlendFactor.One,
            DestinationColorBlendFactor = Refresh.BlendFactor.Zero,
            DestinationAlphaBlendFactor = Refresh.BlendFactor.Zero
        };

        public static readonly ColorTargetBlendState None = new ColorTargetBlendState
        {
            BlendEnable = false,
            ColorWriteMask = Refresh.ColorComponentFlags.RGBA
        };

        public Refresh.ColorTargetBlendState ToRefreshColorTargetBlendState()
        {
            return new Refresh.ColorTargetBlendState
            {
                blendEnable = Conversions.BoolToByte(BlendEnable),
                alphaBlendOp = AlphaBlendOp,
                colorBlendOp = ColorBlendOp,
                colorWriteMask = ColorWriteMask,
                destinationAlphaBlendFactor = DestinationAlphaBlendFactor,
                destinationColorBlendFactor = DestinationColorBlendFactor,
                sourceAlphaBlendFactor = SourceAlphaBlendFactor,
                sourceColorBlendFactor = SourceColorBlendFactor
            };
        }
    }
}
