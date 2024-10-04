using System;
using System.Runtime.InteropServices;
using SDL3;

namespace MoonWorks.Graphics;

// Recreate types in here so we can csharp-ify them and hide the SDL namespace

public enum PrimitiveType
{
	TriangleList = 0,
	TriangleStrip = 1,
	LineList = 2,
	LineStrip = 3,
	PointList = 4,
}

public enum LoadOp
{
	Load = 0,
	Clear = 1,
	DontCare = 2,
}

public enum StoreOp
{
	Store = 0,
	DontCare = 1,
	Resolve = 2,
	ResolveAndStore = 3,
}

public enum IndexElementSize
{
	Sixteen = 0,
	ThirtyTwo = 1,
}

public enum TextureFormat
{
	Invalid = 0,
	A8_UNORM = 1,
	R8_UNORM = 2,
	R8G8_UNORM = 3,
	R8G8B8A8_UNORM = 4,
	R16_UNORM = 5,
	R16G16_UNORM = 6,
	R16G16B16A16_UNORM = 7,
	R10G10B10A2_UNORM = 8,
	B5G6R5_UNORM = 9,
	B5G5R5A1_UNORM = 10,
	B4G4R4A4_UNORM = 11,
	B8G8R8A8_UNORM = 12,
	BC1_RGBA_UNORM = 13,
	BC2_RGBA_UNORM = 14,
	BC3_RGBA_UNORM = 15,
	BC4_R_UNORM = 16,
	BC5_RG_UNORM = 17,
	BC7_RGBA_UNORM = 18,
	BC6H_RGB_FLOAT = 19,
 	BC6H_RGB_UFLOAT = 20,
	R8_SNORM = 21,
	R8G8_SNORM = 22,
	R8G8B8A8_SNORM = 23,
	R16_SNORM = 24,
	R16G16_SNORM = 25,
	R16G16B16A16_SNORM = 26,
	R16_FLOAT = 27,
	R16G16_FLOAT = 28,
	R16G16B16A16_FLOAT = 29,
	R32_FLOAT = 30,
	R32G32_FLOAT = 31,
	R32G32B32A32_FLOAT = 32,
	R11G11B10_UFLOAT = 33,
	R8_UINT = 34,
	R8G8_UINT = 35,
	R8G8B8A8_UINT = 36,
	R16_UINT = 37,
	R16G16_UINT = 38,
	R16G16B16A16_UINT = 39,
	R32_UINT = 40,
	R32G32_UINT = 41,
	R32G32B32A32_UINT = 42,
	R8_INT = 43,
	R8G8_INT = 44,
	R8G8B8A8_INT = 45,
	R16_INT = 46,
	R16G16_INT = 47,
	R16G16B16A16_INT = 48,
	R32_INT = 49,
	R32G32_INT = 50,
	R32G32B32A32_INT = 51,
	R8G8B8A8_UNORM_SRGB = 52,
	B8G8R8A8_UNORM_SRGB = 53,
	BC1_RGBA_UNORM_SRGB = 54,
	BC2_RGBA_UNORM_SRGB = 55,
	BC3_RGBA_UNORM_SRGB = 56,
	BC7_RGBA_UNORM_SRGB = 57,
	D16_UNORM = 58,
	D24_UNORM = 59,
	D32_FLOAT = 60,
	D24_UNORM_S8_UINT = 61,
	D32_FLOAT_S8_UINT = 62,
}

[Flags]
public enum TextureUsageFlags : uint
{
	Sampler = 0x1,
	ColorTarget = 0x2,
	DepthStencilTarget = 0x4,
	GraphicsStorageRead = 0x08,
	ComputeStorageRead = 0x10,
	ComputeStorageWrite = 0x20,
}

public enum TextureType
{
	TwoDimensional = 0,
	TwoDimensionalArray = 1,
	ThreeDimensional = 2,
	Cube = 3,
	CubeArray = 4,
}

public enum SampleCount
{
	One = 0,
	Two = 1,
	Four = 2,
	Eight = 3,
}

public enum CubeMapFace
{
	PositiveX = 0,
	NegativeX = 1,
	PositiveY = 2,
	NegativeY = 3,
	PositiveZ = 4,
	NegativeZ = 5,
}

[Flags]
public enum BufferUsageFlags : uint
{
	Vertex = 0x1,
	Index = 0x2,
	Indirect = 0x4,
	GraphicsStorageRead = 0x08,
	ComputeStorageRead = 0x10,
	ComputeStorageWrite = 0x20,
}

public enum TransferBufferUsage
{
	Upload = 0,
	Download = 1,
}

public enum ShaderStage
{
	Vertex = 0,
	Fragment = 1,
}

[Flags]
public enum ShaderFormat : uint
{
	Private = 0x1,
	SPIRV = 0x2,
	DXBC = 0x4,
	DXIL = 0x08,
	MSL = 0x10,
	MetalLib = 0x20,
}

public enum VertexElementFormat
{
	Invalid = 0,
	Int = 1,
	Int2 = 2,
	Int3 = 3,
	Int4 = 4,
	Uint = 5,
	Uint2 = 6,
	Uint3 = 7,
	Uint4 = 8,
	Float = 9,
	Float2 = 10,
	Float3 = 11,
	Float4 = 12,
	Byte2 = 13,
	Byte4 = 14,
	Ubyte2 = 15,
	Ubyte4 = 16,
	Byte2Norm = 17,
	Byte4Norm = 18,
	Ubyte2Norm = 19,
	Ubyte4Norm = 20,
	Short2 = 21,
	Short4 = 22,
	Ushort2 = 23,
	Ushort4 = 24,
	Short2Norm = 25,
	Short4Norm = 26,
	Ushort2Norm = 27,
	Ushort4Norm = 28,
	Half2 = 29,
	Half4 = 30,
}

public enum VertexInputRate
{
	Vertex = 0,
	Instance = 1,
}

public enum FillMode
{
	Fill = 0,
	Line = 1,
}

public enum CullMode
{
	None = 0,
	Front = 1,
	Back = 2,
}

public enum FrontFace
{
	CounterClockwise = 0,
	Clockwise = 1,
}

public enum CompareOp
{
	Invalid = 0,
	Never = 1,
	Less = 2,
	Equal = 3,
	LessOrEqual = 4,
	Greater = 5,
	NotEqual = 6,
	GreaterOrEqual = 7,
	Always = 8,
}

public enum StencilOp
{
	Invalid = 0,
	Keep = 1,
	Zero = 2,
	Replace = 3,
	IncrementAndClamp = 4,
	DecrementAndClamp = 5,
	Invert = 6,
	IncrementAndWrap = 7,
	DecrementAndWrap = 8,
}

public enum BlendOp
{
	Invalid = 0,
	Add = 1,
	Subtract = 2,
	ReverseSubtract = 3,
	Min = 4,
	Max = 5,
}

public enum BlendFactor
{
	Invalid = 0,
	Zero = 1,
	One = 2,
	SrcColor = 3,
	OneMinusSrcColor = 4,
	DstColor = 5,
	OneMinusDstColor = 6,
	SrcAlpha = 7,
	OneMinusSrcAlpha = 8,
	DstAlpha = 9,
	OneMinusDstAlpha = 10,
	ConstantColor = 11,
	OneMinusConstantColor = 12,
	SrcAlphaSaturate = 13,
}

[Flags]
public enum ColorComponentFlags : byte
{
	None = 0x0,
	R = 0x1,
	G = 0x2,
	B = 0x4,
	A = 0x08,
	RGB = R | G | B,
	RGBA = R | G | B | A
}

public enum Filter
{
	Nearest = 0,
	Linear = 1,
}

public enum SamplerMipmapMode
{
	Nearest = 0,
	Linear = 1,
}

public enum SamplerAddressMode
{
	Repeat = 0,
	MirroredRepeat = 1,
	ClampToEdge = 2,
}

public enum PresentMode
{
	VSync = 0,
	Immediate = 1,
	Mailbox = 2,
}

public enum SwapchainComposition
{
	SDR = 0,
	SDRLinear = 1,
	HDRExtendedLinear = 2,
	HDR10ST2048 = 3,
}

public enum FlipMode
{
	None = 0,
	Horizontal = 1,
	Vertical = 2,
}

public struct Rect
{
	public int X;
	public int Y;
	public int W;
	public int H;

	public static implicit operator SDL.SDL_Rect(Rect rect) => new SDL.SDL_Rect
	{
		x = rect.X,
		y = rect.Y,
		w = rect.W,
		h = rect.H
	};
}

[StructLayout(LayoutKind.Sequential)]
public struct Viewport
{
	public float X;
	public float Y;
	public float W;
	public float H;
	public float MinDepth;
	public float MaxDepth;

	public static implicit operator SDL.SDL_GPUViewport(Viewport viewport) => new SDL.SDL_GPUViewport
	{
		x = viewport.X,
		y = viewport.Y,
		w = viewport.W,
		h = viewport.H,
		min_depth = viewport.MinDepth,
		max_depth = viewport.MaxDepth
	};
}

[StructLayout(LayoutKind.Sequential)]
public struct TextureTransferInfo
{
	public TransferBuffer TransferBuffer;
	public uint Offset;
	public uint PixelsPerRow;
	public uint RowsPerLayer;
}

[StructLayout(LayoutKind.Sequential)]
public struct TransferBufferLocation
{
	public TransferBuffer TransferBuffer;
	public uint Offset;
}

[StructLayout(LayoutKind.Sequential)]
public struct TextureLocation
{
	public Texture Texture;
	public uint MipLevel;
	public uint Layer;
	public uint X;
	public uint Y;
	public uint Z;
}

[StructLayout(LayoutKind.Sequential)]
public struct TextureRegion
{
	public Texture Texture;
	public uint MipLevel;
	public uint Layer;
	public uint X;
	public uint Y;
	public uint Z;
	public uint W;
	public uint H;
	public uint D;
}

[StructLayout(LayoutKind.Sequential)]
public struct BlitRegion
{
	public Texture Texture;
	public uint MipLevel;
	public uint LayerOrDepthPlane;
	public uint X;
	public uint Y;
	public uint W;
	public uint H;
}

[StructLayout(LayoutKind.Sequential)]
public struct BufferLocation
{
	public Buffer Buffer;
	public uint Offset;
}

[StructLayout(LayoutKind.Sequential)]
public struct BufferRegion
{
	public Buffer Buffer;
	public uint Offset;
	public uint Size;
}

[StructLayout(LayoutKind.Sequential)]
public struct IndirectDrawCommand
{
	public uint NumVertices;
	public uint NumInstances;
	public uint FirstVertex;
	public uint FirstIndex;
}

[StructLayout(LayoutKind.Sequential)]
public struct IndexedIndirectDrawCommand
{
	public uint NumIndices;
	public uint NumInstances;
	public uint FirstIndex;
	public int VertexOffset;
	public uint FirstInstance;
}

[StructLayout(LayoutKind.Sequential)]
public struct IndirectDispatchCommand
{
	public uint GroupCountX;
	public uint GroupCountY;
	public uint GroupCountZ;
}

[StructLayout(LayoutKind.Sequential)]
public struct SamplerCreateInfo
{
	public Filter MinFilter;
	public Filter MagFilter;
	public SamplerMipmapMode MipmapMode;
	public SamplerAddressMode AddressModeU;
	public SamplerAddressMode AddressModeV;
	public SamplerAddressMode AddressModeW;
	public float MipLodBias;
	public float MaxAnisotropy;
	public CompareOp CompareOp;
	public float MinLod;
	public float MaxLod;
	public bool EnableAnisotropy;
	public bool EnableCompare;
	public byte Padding1;
	public byte Padding2;
	public uint Props;
}

[StructLayout(LayoutKind.Sequential)]
public struct VertexBufferDescription
{
	public uint Slot;
	public uint Pitch;
	public VertexInputRate InputRate;
	public uint InstanceStepRate;
}

[StructLayout(LayoutKind.Sequential)]
public struct VertexAttribute
{
	public uint Location;
	public uint BufferSlot;
	public VertexElementFormat Format;
	public uint Offset;
}

[StructLayout(LayoutKind.Sequential)]
public ref struct VertexInputState
{
	public Span<VertexBufferDescription> VertexBufferDescriptions;
	public uint NumVertexBuffer;
	public Span<VertexAttribute> VertexAttributes;
	public uint NumVertexAttributes;
}

[StructLayout(LayoutKind.Sequential)]
public struct StencilOpState
{
	public StencilOp FailOp;
	public StencilOp PassOp;
	public StencilOp DepthFailOp;
	public CompareOp CompareOp;
}

[StructLayout(LayoutKind.Sequential)]
public struct ColorTargetBlendState
{
	public BlendFactor SrcColorBlendFactor;
	public BlendFactor DstColorBlendFactor;
	public BlendOp ColorBlendOp;
	public BlendFactor SrcAlphaBlendFactor;
	public BlendFactor DstAlphaBlendFactor;
	public BlendOp AlphaBlendOp;
	public ColorComponentFlags ColorWriteMask;
	public bool EnableBlend;
	public bool EnableColorWriteMask;
	public byte Padding2;
	public byte Padding3;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ShaderCreateInfo
{
	public UIntPtr CodeSize;
	public byte* Code;
	public byte* EntryPoint;
	public ShaderFormat Format;
	public ShaderStage Stage;
	public uint NumSamplers;
	public uint NumStorageTextures;
	public uint NumStorageBuffers;
	public uint NumUniformBuffers;
	public uint Props;
}

[StructLayout(LayoutKind.Sequential)]
public struct TextureCreateInfo
{
	public TextureType Type;
	public TextureFormat Format;
	public TextureUsageFlags Usage;
	public uint Width;
	public uint Height;
	public uint LayerCountOrDepth;
	public uint NumLevels;
	public SampleCount SampleCount;
	public uint Props;
}

[StructLayout(LayoutKind.Sequential)]
public struct BufferCreateInfo
{
	public BufferUsageFlags Usage;
	public uint Size;
	public uint Props;
}

[StructLayout(LayoutKind.Sequential)]
public struct TransferBufferCreateInfo
{
	public TransferBufferUsage Usage;
	public uint Size;
	public uint Props;
}

[StructLayout(LayoutKind.Sequential)]
public struct RasterizerState
{
	public FillMode FillMode;
	public CullMode CullMode;
	public FrontFace FrontFace;
	public float DepthBiasConstantFactor;
	public float DepthBiasClamp;
	public float DepthBiasSlopFactor;
	public bool EnableDepthBias;
	public bool EnableDepthClip;
	public byte Padding1;
	public byte Padding2;
}

[StructLayout(LayoutKind.Sequential)]
public struct MultisampleState
{
	public SampleCount SampleCount;
	public uint SampleMask;
	public bool EnableMask;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

[StructLayout(LayoutKind.Sequential)]
public struct DepthStencilState
{
	public CompareOp CompareOp;
	public StencilOpState BackStencilState;
	public StencilOpState FrontStencilState;
	public byte CompareMask;
	public byte WriteMask;
	public bool EnableDepthTest;
	public bool EnableDepthWrite;
	public bool EnableStencilTest;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

[StructLayout(LayoutKind.Sequential)]
public struct ColorTargetDescription
{
	public TextureFormat Format;
	public ColorTargetBlendState BlendState;
}

[StructLayout(LayoutKind.Sequential)]
public ref struct GraphicsPipelineTargetInfo
{
	public Span<ColorTargetDescription> ColorTargetDescriptions;
	public uint NumColorTargets;
	public TextureFormat DepthStencilFormat;
	public bool HasDepthStencilTarget;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

[StructLayout(LayoutKind.Sequential)]
public ref struct GraphicsPipelineCreateInfo
{
	public Shader VertexShader;
	public Shader FragmentShader;
	public VertexInputState VertexInputState;
	public PrimitiveType PrimitiveType;
	public RasterizerState RasterizerState;
	public MultisampleState MultisampleState;
	public DepthStencilState DepthStencilState;
	public GraphicsPipelineTargetInfo TargetInfo;
	public uint Props;
}

[StructLayout(LayoutKind.Sequential)]
public ref struct ComputePipelineCreateInfo
{
	public UIntPtr CodeSize;
	public Span<byte> Code;
	public Span<byte> EntryPoint;
	public ShaderFormat Format;
	public uint NumSamplers;
	public uint NumReadonlyStorageTextures;
	public uint NumReadonlyStorageBuffers;
	public uint NumReadWriteStorageTextures;
	public uint NumReadWriteStorageBuffers;
	public uint NumUniformBuffers;
	public uint ThreadCountX;
	public uint ThreadCountY;
	public uint ThreadCountZ;
	public uint Props;
}

[StructLayout(LayoutKind.Sequential)]
public struct ColorTargetInfo
{
	public Texture Texture;
	public uint MipLevel;
	public uint LayerOrDepthPlane;
	public Color ClearColor;
	public LoadOp LoadOp;
	public StoreOp StoreOp;
	public Texture ResolveTexture;
	public uint ResolveMipLevel;
	public uint ResolveLayer;
	public bool Cycle;
	public bool CycleResolveTexture;
	public byte Padding1;
	public byte Padding2;
}

[StructLayout(LayoutKind.Sequential)]
public struct DepthStencilTargetInfo
{
	public Texture Texture;
	public float ClearDepth;
	public LoadOp LoadOp;
	public StoreOp StoreOp;
	public LoadOp StencilLoadOp;
	public StoreOp StencilStoreOp;
	public bool Cycle;
	public byte ClearStencil;
	public byte Padding1;
	public byte Padding2;
}

[StructLayout(LayoutKind.Sequential)]
public struct BlitInfo
{
	public BlitRegion Source;
	public BlitRegion Destination;
	public LoadOp LoadOp;
	public Color ClearColor;
	public FlipMode FlipMode;
	public Filter Filter;
	public bool Cycle;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

[StructLayout(LayoutKind.Sequential)]
public struct BufferBinding
{
	public Buffer Buffer;
	public uint Offset;

	public static implicit operator SDL.SDL_GPUBufferBinding(BufferBinding binding) => new SDL.SDL_GPUBufferBinding
	{
		buffer = binding.Buffer.Handle,
		offset = binding.Offset
	};
}

[StructLayout(LayoutKind.Sequential)]
public struct TextureSamplerBinding
{
	public Texture Texture;
	public Sampler Sampler;

	public static implicit operator SDL.SDL_GPUTextureSamplerBinding(TextureSamplerBinding binding) => new SDL.SDL_GPUTextureSamplerBinding
	{
		texture = binding.Texture.Handle,
		sampler = binding.Sampler.Handle
	};
}

[StructLayout(LayoutKind.Sequential)]
public struct StorageBufferReadWriteBinding
{
	public Buffer Buffer;
	public bool Cycle;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

[StructLayout(LayoutKind.Sequential)]
public struct StorageTextureReadWriteBinding
{
	public Texture Texture;
	public uint MipLevel;
	public uint Layer;
	public bool Cycle;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}
