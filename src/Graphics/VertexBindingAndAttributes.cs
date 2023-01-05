namespace MoonWorks.Graphics
{
	// This is a convenience structure for pairing a vertex binding with its associated attributes.
	public struct VertexBindingAndAttributes
	{
		public VertexBinding VertexBinding { get; }
		public VertexAttribute[] VertexAttributes { get; }

		public VertexBindingAndAttributes(VertexBinding binding, VertexAttribute[] attributes)
		{
			VertexBinding = binding;
			VertexAttributes = attributes;
		}

		public static VertexBindingAndAttributes Create<T>(uint bindingIndex) where T : unmanaged, IVertexType
		{
			VertexBinding binding = VertexBinding.Create<T>(bindingIndex);
			VertexAttribute[] attributes = new VertexAttribute[T.Formats.Length];
			uint offset = 0;

			for (uint i = 0; i < T.Formats.Length; i += 1)
			{
				var format = T.Formats[i];

				attributes[i] = new VertexAttribute
				{
					Binding = bindingIndex,
					Location = i,
					Format = format,
					Offset = offset
				};

				offset += Conversions.VertexElementFormatSize(format);
			}

			return new VertexBindingAndAttributes(binding, attributes);
		}
	}
}
