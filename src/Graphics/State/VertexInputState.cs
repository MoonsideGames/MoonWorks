namespace MoonWorks.Graphics
{
	/// <summary>
	/// Specifies how to interpet vertex data in a buffer to be passed to the vertex shader.
	/// </summary>
	public struct VertexInputState
	{
		public VertexBinding[] VertexBindings;
		public VertexAttribute[] VertexAttributes;

		public VertexInputState()
		{
			VertexBindings = new VertexBinding[0];
			VertexAttributes = new VertexAttribute[0];
		}

		public VertexInputState(
			VertexBinding vertexBinding,
			params VertexAttribute[] vertexAttributes
		) {
			VertexBindings = new VertexBinding[] { vertexBinding };
			VertexAttributes = vertexAttributes;
		}
	}
}
