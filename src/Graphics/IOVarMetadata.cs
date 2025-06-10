namespace MoonWorks.Graphics;

public enum IOVarType {
	Unknown,
	Int8,
	Uint8,
	Int16,
	Uint16,
	Int32,
	Uint32,
	Int64,
	Uint64,
	Float16,
	Float32,
	Float64
}

public struct IOVarMetadata
{
	public string Name;
	public uint Location;
	public uint Offset;
	public IOVarType VectorType;
	public uint VectorSize;
}
