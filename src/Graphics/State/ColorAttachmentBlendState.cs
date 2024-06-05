using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Defines how color blending will be performed in a GraphicsPipeline.
	/// </summary>
	public struct ColorAttachmentBlendState
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

		public static readonly ColorAttachmentBlendState Additive = new ColorAttachmentBlendState
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

		public static readonly ColorAttachmentBlendState AlphaBlend = new ColorAttachmentBlendState
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

		public static readonly ColorAttachmentBlendState NonPremultiplied = new ColorAttachmentBlendState
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

		public static readonly ColorAttachmentBlendState Opaque = new ColorAttachmentBlendState
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

		public static readonly ColorAttachmentBlendState None = new ColorAttachmentBlendState
		{
			BlendEnable = false,
			ColorWriteMask = ColorComponentFlags.RGBA
		};

		public static readonly ColorAttachmentBlendState Disable = new ColorAttachmentBlendState
		{
			BlendEnable = false,
			ColorWriteMask = ColorComponentFlags.None
		};

		public Refresh.ColorAttachmentBlendState ToRefresh()
		{
			return new Refresh.ColorAttachmentBlendState
			{
				BlendEnable = Conversions.BoolToInt(BlendEnable),
				AlphaBlendOp = (Refresh.BlendOp) AlphaBlendOp,
				ColorBlendOp = (Refresh.BlendOp) ColorBlendOp,
				ColorWriteMask = (Refresh.ColorComponentFlags) ColorWriteMask,
				DestinationAlphaBlendFactor = (Refresh.BlendFactor) DestinationAlphaBlendFactor,
				DestinationColorBlendFactor = (Refresh.BlendFactor) DestinationColorBlendFactor,
				SourceAlphaBlendFactor = (Refresh.BlendFactor) SourceAlphaBlendFactor,
				SourceColorBlendFactor = (Refresh.BlendFactor) SourceColorBlendFactor
			};
		}
	}
}
