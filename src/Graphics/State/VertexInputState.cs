namespace MoonWorks.Graphics
{
	/// <summary>
	/// Specifies how to interpet vertex data in a buffer to be passed to the vertex shader.
	/// </summary>
	public struct VertexInputState
	{
		public VertexBinding[] VertexBindings;
		public VertexAttribute[] VertexAttributes;

		public static readonly VertexInputState Empty = new VertexInputState
		{
			VertexBindings = System.Array.Empty<VertexBinding>(),
			VertexAttributes = System.Array.Empty<VertexAttribute>()
		};

		public VertexInputState(
			VertexBinding vertexBinding,
			VertexAttribute[] vertexAttributes
		) {
			VertexBindings = new VertexBinding[] { vertexBinding };
			VertexAttributes = vertexAttributes;
		}

		public VertexInputState(
			VertexBinding[] vertexBindings,
			VertexAttribute[] vertexAttributes
		) {
			VertexBindings = vertexBindings;
			VertexAttributes = vertexAttributes;
		}

		public static VertexInputState CreateSingleBinding<T>() where T : unmanaged, IVertexType
		{
			return new VertexInputState(
				VertexBinding.Create<T>(),
				default(T).Attributes()
			);
		}
	}
}
