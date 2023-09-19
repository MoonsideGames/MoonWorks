namespace MoonWorks.Graphics
{
	/// <summary>
	/// Specifies how the vertex shader will interpet vertex data in a buffer.
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

		public VertexInputState(
			VertexBindingAndAttributes bindingAndAttributes
		) {
			VertexBindings = new VertexBinding[] { bindingAndAttributes.VertexBinding };
			VertexAttributes = bindingAndAttributes.VertexAttributes;
		}

		public VertexInputState(
			VertexBindingAndAttributes[] bindingAndAttributesArray
		) {
			VertexBindings = new VertexBinding[bindingAndAttributesArray.Length];
			var attributesLength = 0;

			for (var i = 0; i < bindingAndAttributesArray.Length; i += 1)
			{
				VertexBindings[i] = bindingAndAttributesArray[i].VertexBinding;
				attributesLength += bindingAndAttributesArray[i].VertexAttributes.Length;
			}

			VertexAttributes = new VertexAttribute[attributesLength];

			var attributeIndex = 0;
			for (var i = 0; i < bindingAndAttributesArray.Length; i += 1)
			{
				for (var j = 0; j < bindingAndAttributesArray[i].VertexAttributes.Length; j += 1)
				{
					VertexAttributes[attributeIndex] = bindingAndAttributesArray[i].VertexAttributes[j];
					attributeIndex += 1;
				}
			}
		}

		public static VertexInputState CreateSingleBinding<T>() where T : unmanaged, IVertexType
		{
			return new VertexInputState(VertexBindingAndAttributes.Create<T>(0));
		}
	}
}
