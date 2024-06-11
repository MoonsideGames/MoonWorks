namespace MoonWorks.Graphics;

/// <summary>
/// Can be defined on your struct type to enable simplified vertex input state definition.
/// </summary>
public interface IVertexType
{
	/// <summary>
	/// An ordered list of the types in your vertex struct.
	/// </summary>
	static abstract VertexElementFormat[] Formats { get; }
}
