namespace MoonWorks.Graphics
{
	/// <summary>
	/// Determines how data is written to and read from the depth/stencil buffer.
	/// </summary>
	public struct DepthStencilState
	{
		/// <summary>
		/// If disabled, no depth culling will occur.
		/// </summary>
		public bool DepthTestEnable;

		/// <summary>
		/// Describes the stencil operation for back-facing primitives.
		/// </summary>
		public StencilOpState BackStencilState;

		/// <summary>
		/// Describes the stencil operation for front-facing primitives.
		/// </summary>
		public StencilOpState FrontStencilState;

		/// <summary>
		/// The comparison operator used in the depth test.
		/// </summary>
		public CompareOp CompareOp;

		/// <summary>
		/// If depth lies outside of these bounds the pixel will be culled.
		/// </summary>
		public bool DepthBoundsTestEnable;

		/// <summary>
		/// Specifies whether depth values will be written to the buffer during rendering.
		/// </summary>
		public bool DepthWriteEnable;

		/// <summary>
		/// The minimum depth value in the depth bounds test.
		/// </summary>
		public float MinDepthBounds;

		/// <summary>
		/// The maximum depth value in the depth bounds test.
		/// </summary>
		public float MaxDepthBounds;

		/// <summary>
		/// If disabled, no stencil culling will occur.
		/// </summary>
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
