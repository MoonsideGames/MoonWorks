namespace MoonWorks.Graphics
{
	/// <summary>
	/// Specifies how the rasterizer should be configured for a graphics pipeline.
	/// </summary>
	public struct RasterizerState
	{
		/// <summary>
		/// Specifies whether front faces, back faces, none, or both should be culled.
		/// </summary>
		public CullMode CullMode;

		/// <summary>
		/// Specifies maximum depth bias of a fragment. Only applies if depth biasing is enabled.
		/// </summary>
		public float DepthBiasClamp;

		/// <summary>
		/// The constant depth value added to each fragment. Only applies if depth biasing is enabled.
		/// </summary>
		public float DepthBiasConstantFactor;

		/// <summary>
		/// Specifies whether depth biasing is enabled. Only applies if depth biasing is enabled.
		/// </summary>
		public bool DepthBiasEnable;

		/// <summary>
		/// Factor applied to a fragment's slope in depth bias calculations. Only applies if depth biasing is enabled.
		/// </summary>
		public float DepthBiasSlopeFactor;
		public bool DepthClampEnable;

		/// <summary>
		/// Specifies how triangles should be drawn.
		/// </summary>
		public FillMode FillMode;

		/// <summary>
		/// Specifies which triangle winding order is designated as front-facing.
		/// </summary>
		public FrontFace FrontFace;

		public static readonly RasterizerState CW_CullFront = new RasterizerState
		{
			CullMode = CullMode.Front,
			FrontFace = FrontFace.Clockwise,
			FillMode = FillMode.Fill,
			DepthBiasEnable = false
		};

		public static readonly RasterizerState CW_CullBack = new RasterizerState
		{
			CullMode = CullMode.Back,
			FrontFace = FrontFace.Clockwise,
			FillMode = FillMode.Fill,
			DepthBiasEnable = false
		};

		public static readonly RasterizerState CW_CullNone = new RasterizerState
		{
			CullMode = CullMode.None,
			FrontFace = FrontFace.Clockwise,
			FillMode = FillMode.Fill,
			DepthBiasEnable = false
		};

		public static readonly RasterizerState CW_Wireframe = new RasterizerState
		{
			CullMode = CullMode.None,
			FrontFace = FrontFace.Clockwise,
			FillMode = FillMode.Fill,
			DepthBiasEnable = false
		};

		public static readonly RasterizerState CCW_CullFront = new RasterizerState
		{
			CullMode = CullMode.Front,
			FrontFace = FrontFace.CounterClockwise,
			FillMode = FillMode.Fill,
			DepthBiasEnable = false
		};

		public static readonly RasterizerState CCW_CullBack = new RasterizerState
		{
			CullMode = CullMode.Back,
			FrontFace = FrontFace.CounterClockwise,
			FillMode = FillMode.Fill,
			DepthBiasEnable = false
		};

		public static readonly RasterizerState CCW_CullNone = new RasterizerState
		{
			CullMode = CullMode.None,
			FrontFace = FrontFace.CounterClockwise,
			FillMode = FillMode.Fill,
			DepthBiasEnable = false
		};

		public static readonly RasterizerState CCW_Wireframe = new RasterizerState
		{
			CullMode = CullMode.None,
			FrontFace = FrontFace.CounterClockwise,
			FillMode = FillMode.Fill,
			DepthBiasEnable = false
		};
	}
}
