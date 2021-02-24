using RefreshCS;

namespace MoonWorks.Graphics
{
    public struct ColorTargetBlendState
    {
        /// <summary>
        /// If disabled, no blending will occur.
        /// </summary>
        public bool BlendEnable;

        /// <summary>
        /// Selects which blend operation to use with alpha values.
        /// </summary>
        public BlendOp AlphaBlendOp;
        /// <summary>
        /// Selects which blend operation to use with color values.
        /// </summary>
        public BlendOp ColorBlendOp;

        /// <summary>
        /// Specifies which of the RGBA components are enabled for writing.
        /// </summary>
        public ColorComponentFlags ColorWriteMask;

        /// <summary>
        /// Selects which blend factor is used to determine the alpha destination factor.
        /// </summary>
        public BlendFactor DestinationAlphaBlendFactor;

        /// <summary>
        /// Selects which blend factor is used to determine the color destination factor.
        /// </summary>
        public BlendFactor DestinationColorBlendFactor;

        /// <summary>
        /// Selects which blend factor is used to determine the alpha source factor.
        /// </summary>
        public BlendFactor SourceAlphaBlendFactor;

        /// <summary>
        /// Selects which blend factor is used to determine the color source factor.
        /// </summary>
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
