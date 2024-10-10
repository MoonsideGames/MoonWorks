// Since we don't have typedef in C#...
// Manually define the graphics interop bindings to avoid namespace pollution
// The structure of these bindings are taken from SDL3.Core.
// If SDL3.Core changes, make sure to check the changes against these bindings!

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SDLBool = SDL3.SDL.SDLBool;

namespace MoonWorks.Graphics;

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
	A8Unorm = 1,
	R8Unorm = 2,
	R8G8Unorm = 3,
	R8G8B8A8Unorm = 4,
	R16Unorm = 5,
	R16G16Unorm = 6,
	R16G16B16A16Unorm = 7,
	R10G10B10A2Unorm = 8,
	B5G6R5Unorm = 9,
	B5G5R5A1Unorm = 10,
	B4G4R4A4Unorm = 11,
	B8G8R8A8Unorm = 12,
	BC1_RGBAUnorm = 13,
	BC2_RGBAUnorm = 14,
	BC3_RGBAUnorm = 15,
	BC4_RUnorm = 16,
	BC5_RGUnorm = 17,
	BC7_RGBAUnorm = 18,
	BC6H_RGBFloat = 19,
 	BC6H_RGBUfloat = 20,
	R8Snorm = 21,
	R8G8Snorm = 22,
	R8G8B8A8Snorm = 23,
	R16Snorm = 24,
	R16G16Snorm = 25,
	R16G16B16A16Snorm = 26,
	R16Float = 27,
	R16G16Float = 28,
	R16G16B16A16Float = 29,
	R32Float = 30,
	R32G32Float = 31,
	R32G32B32A32Float = 32,
	R11G11B10Ufloat = 33,
	R8Uint = 34,
	R8G8Uint = 35,
	R8G8B8A8Uint = 36,
	R16Uint = 37,
	R16G16Uint = 38,
	R16G16B16A16Uint = 39,
	R32Uint = 40,
	R32G32Uint = 41,
	R32G32B32A32Uint = 42,
	R8Int = 43,
	R8G8Int = 44,
	R8G8B8A8Int = 45,
	R16Int = 46,
	R16G16Int = 47,
	R16G16B16A16Int = 48,
	R32Int = 49,
	R32G32Int = 50,
	R32G32B32A32Int = 51,
	R8G8B8A8UnormSRGB = 52,
	B8G8R8A8UnormSRGB = 53,
	BC1_RGBAUnormSRGB = 54,
	BC2_RGBAUnormSRGB = 55,
	BC3_RGBAUnormSRGB = 56,
	BC7_RGBAUnormSRGB = 57,
	D16Unorm = 58,
	D24Unorm = 59,
	D32Float = 60,
	D24UnormS8Uint = 61,
	D32FloatS8Uint = 62,
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
}

[StructLayout(LayoutKind.Sequential)]
public struct TextureTransferInfo
{
	public IntPtr TransferBuffer;
	public uint Offset;
	public uint PixelsPerRow;
	public uint RowsPerLayer;
}

[StructLayout(LayoutKind.Sequential)]
public struct TransferBufferLocation
{
	public IntPtr TransferBuffer;
	public uint Offset;
}

[StructLayout(LayoutKind.Sequential)]
public struct TextureLocation
{
	public IntPtr Texture;
	public uint MipLevel;
	public uint Layer;
	public uint X;
	public uint Y;
	public uint Z;
}

[StructLayout(LayoutKind.Sequential)]
public struct TextureRegion
{
	public IntPtr Texture;
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
	public IntPtr Texture;
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
	public IntPtr Buffer;
	public uint Offset;
}

[StructLayout(LayoutKind.Sequential)]
public struct BufferRegion
{
	public IntPtr Buffer;
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
	public SDLBool EnableAnisotropy;
	public SDLBool EnableCompare;
	public byte Padding1;
	public byte Padding2;
	public uint Props;

	public static SamplerCreateInfo AnisotropicClamp = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.ClampToEdge,
		AddressModeV = SamplerAddressMode.ClampToEdge,
		AddressModeW = SamplerAddressMode.ClampToEdge,
		EnableAnisotropy = true,
		MaxAnisotropy = 4,
		MipLodBias = 0f,
		MinLod = 0,
		MaxLod = 1000 /* VK_LOD_CLAMP_NONE */
	};

	public static SamplerCreateInfo AnisotropicWrap = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.Repeat,
		AddressModeV = SamplerAddressMode.Repeat,
		AddressModeW = SamplerAddressMode.Repeat,
		EnableAnisotropy = true,
		MaxAnisotropy = 4,
		MaxLod = 1000 /* VK_LOD_CLAMP_NONE */
	};

	public static readonly SamplerCreateInfo LinearClamp = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.ClampToEdge,
		AddressModeV = SamplerAddressMode.ClampToEdge,
		AddressModeW = SamplerAddressMode.ClampToEdge,
		MaxLod = 1000
	};

	public static readonly SamplerCreateInfo LinearWrap = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.Repeat,
		AddressModeV = SamplerAddressMode.Repeat,
		AddressModeW = SamplerAddressMode.Repeat,
		MaxLod = 1000
	};

	public static readonly SamplerCreateInfo PointClamp = new SamplerCreateInfo
	{
		MinFilter = Filter.Nearest,
		MagFilter = Filter.Nearest,
		MipmapMode = SamplerMipmapMode.Nearest,
		AddressModeU = SamplerAddressMode.ClampToEdge,
		AddressModeV = SamplerAddressMode.ClampToEdge,
		AddressModeW = SamplerAddressMode.ClampToEdge,
		MaxLod = 1000
	};

	public static readonly SamplerCreateInfo PointWrap = new SamplerCreateInfo
	{
		MinFilter = Filter.Nearest,
		MagFilter = Filter.Nearest,
		MipmapMode = SamplerMipmapMode.Nearest,
		AddressModeU = SamplerAddressMode.Repeat,
		AddressModeV = SamplerAddressMode.Repeat,
		AddressModeW = SamplerAddressMode.Repeat,
		MaxLod = 1000
	};
}

[StructLayout(LayoutKind.Sequential)]
public struct VertexBufferDescription
{
	public uint Slot;
	public uint Pitch;
	public VertexInputRate InputRate;
	public uint InstanceStepRate;

	public static VertexBufferDescription Create<T>(
		uint slot = 0,
		VertexInputRate inputRate = VertexInputRate.Vertex,
		uint stepRate = 0
	) where T : unmanaged
	{
		return new VertexBufferDescription
		{
			Slot = slot,
			Pitch = (uint) Marshal.SizeOf<T>(),
			InputRate = inputRate,
			InstanceStepRate = 0
		};
	}
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
public unsafe struct INTERNAL_VertexInputState
{
	public VertexBufferDescription* VertexBufferDescriptions;
	public uint NumVertexBuffers;
	public VertexAttribute* VertexAttributes;
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
	public SDLBool EnableBlend;
	public SDLBool EnableColorWriteMask;
	public byte Padding2;
	public byte Padding3;

	public static ColorTargetBlendState None = new ColorTargetBlendState
	{
		EnableBlend = false,
		ColorWriteMask = ColorComponentFlags.None
	};
}

public struct ShaderCreateInfo
{
	public ShaderFormat Format;
	public ShaderStage Stage;
	public uint NumSamplers;
	public uint NumStorageTextures;
	public uint NumStorageBuffers;
	public uint NumUniformBuffers;
	public uint Props;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct INTERNAL_ShaderCreateInfo
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
	public SDLBool EnableDepthBias;
	public SDLBool EnableDepthClip;
	public byte Padding1;
	public byte Padding2;

	public static RasterizerState CW_CullFront = new RasterizerState
	{
		CullMode = CullMode.Front,
		FrontFace = FrontFace.Clockwise,
		FillMode = FillMode.Fill
	};

	public static RasterizerState CW_CullBack = new RasterizerState
	{
		CullMode = CullMode.Back,
		FrontFace = FrontFace.Clockwise,
		FillMode = FillMode.Fill
	};

	public static RasterizerState CW_CullNone = new RasterizerState
	{
		CullMode = CullMode.None,
		FrontFace = FrontFace.Clockwise,
		FillMode = FillMode.Fill
	};

	public static RasterizerState CW_Wireframe = new RasterizerState
	{
		CullMode = CullMode.None,
		FrontFace = FrontFace.Clockwise,
		FillMode = FillMode.Line
	};

	public static RasterizerState CCW_CullFront = new RasterizerState
	{
		CullMode = CullMode.Front,
		FrontFace = FrontFace.CounterClockwise,
		FillMode = FillMode.Fill
	};

	public static readonly RasterizerState CCW_CullBack = new RasterizerState
	{
		CullMode = CullMode.Back,
		FrontFace = FrontFace.CounterClockwise,
		FillMode = FillMode.Fill
	};

	public static readonly RasterizerState CCW_CullNone = new RasterizerState
	{
		CullMode = CullMode.None,
		FrontFace = FrontFace.CounterClockwise,
		FillMode = FillMode.Fill
	};

	public static readonly RasterizerState CCW_Wireframe = new RasterizerState
	{
		CullMode = CullMode.None,
		FrontFace = FrontFace.CounterClockwise,
		FillMode = FillMode.Line
	};
}

[StructLayout(LayoutKind.Sequential)]
public struct MultisampleState
{
	public SampleCount SampleCount;
	public uint SampleMask;
	public SDLBool EnableMask;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;

	public static MultisampleState None = new MultisampleState
	{
		SampleCount = SampleCount.One
	};
}

[StructLayout(LayoutKind.Sequential)]
public struct DepthStencilState
{
	public CompareOp CompareOp;
	public StencilOpState BackStencilState;
	public StencilOpState FrontStencilState;
	public byte CompareMask;
	public byte WriteMask;
	public SDLBool EnableDepthTest;
	public SDLBool EnableDepthWrite;
	public SDLBool EnableStencilTest;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;

	public static DepthStencilState Disable = new DepthStencilState
	{
		EnableDepthTest = false,
		EnableDepthWrite = false,
		EnableStencilTest = false
	};
}

[StructLayout(LayoutKind.Sequential)]
public struct ColorTargetDescription
{
	public TextureFormat Format;
	public ColorTargetBlendState BlendState;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct INTERNAL_GraphicsPipelineTargetInfo
{
	public ColorTargetDescription* ColorTargetDescriptions;
	public uint NumColorTargets;
	public TextureFormat DepthStencilFormat;
	public SDLBool HasDepthStencilTarget;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

public struct VertexInputState
{
	public VertexBufferDescription[] VertexBufferDescriptions;
	public VertexAttribute[] VertexAttributes;

	public static VertexInputState Empty = new VertexInputState
	{
		VertexBufferDescriptions = [],
		VertexAttributes = []
	};

	public static VertexInputState CreateSingleBinding<T>(uint slot = 0, VertexInputRate inputRate = VertexInputRate.Vertex, uint stepRate = 0, uint locationOffset = 0) where T : unmanaged, IVertexType
	{
		var description = VertexBufferDescription.Create<T>(slot, inputRate, stepRate);
		var attributes = new VertexAttribute[T.Formats.Length];
		uint offset = 0;

		for (uint i = 0; i < T.Formats.Length; i += 1)
		{
			var format = T.Formats[i];

			attributes[i] = new VertexAttribute
			{
				BufferSlot = slot,
				Location = locationOffset + i,
				Format = format,
				Offset = offset
			};

			offset += Conversions.VertexElementFormatSize(format);
		}

		return new VertexInputState
		{
			VertexBufferDescriptions = [description],
			VertexAttributes = attributes
		};
	}
}

public struct GraphicsPipelineTargetInfo
{
	public ColorTargetDescription[] ColorTargetDescriptions;
	public TextureFormat DepthStencilFormat;
	public SDLBool HasDepthStencilTarget;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

public struct GraphicsPipelineCreateInfo
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
public struct INTERNAL_GraphicsPipelineCreateInfo
{
	public IntPtr VertexShader;
	public IntPtr FragmentShader;
	public INTERNAL_VertexInputState VertexInputState;
	public PrimitiveType PrimitiveType;
	public RasterizerState RasterizerState;
	public MultisampleState MultisampleState;
	public DepthStencilState DepthStencilState;
	public INTERNAL_GraphicsPipelineTargetInfo TargetInfo;
	public uint Props;
}

public struct ComputePipelineCreateInfo
{
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
internal unsafe struct INTERNAL_ComputePipelineCreateInfo
{
	public UIntPtr CodeSize;
	public byte* Code;
	public byte* EntryPoint;
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
	public IntPtr Texture;
	public uint MipLevel;
	public uint LayerOrDepthPlane;
	public Color ClearColor;
	public LoadOp LoadOp;
	public StoreOp StoreOp;
	public IntPtr ResolveTexture;
	public uint ResolveMipLevel;
	public uint ResolveLayer;
	public SDLBool Cycle;
	public SDLBool CycleResolveTexture;
	public byte Padding1;
	public byte Padding2;
}

[StructLayout(LayoutKind.Sequential)]
public struct DepthStencilTargetInfo
{
	public IntPtr Texture;
	public float ClearDepth;
	public LoadOp LoadOp;
	public StoreOp StoreOp;
	public LoadOp StencilLoadOp;
	public StoreOp StencilStoreOp;
	public SDLBool Cycle;
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
	public SDLBool Cycle;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

[StructLayout(LayoutKind.Sequential)]
public struct BufferBinding
{
	public IntPtr Buffer;
	public uint Offset;
}

[StructLayout(LayoutKind.Sequential)]
public struct TextureSamplerBinding
{
	public IntPtr Texture;
	public IntPtr Sampler;

	public TextureSamplerBinding(Texture texture, Sampler sampler)
	{
		Texture = texture.Handle;
		Sampler = sampler.Handle;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct StorageBufferReadWriteBinding
{
	public IntPtr Buffer;
	public SDLBool Cycle;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

[StructLayout(LayoutKind.Sequential)]
public struct StorageTextureReadWriteBinding
{
	public IntPtr Texture;
	public uint MipLevel;
	public uint Layer;
	public SDLBool Cycle;
	public byte Padding1;
	public byte Padding2;
	public byte Padding3;
}

public struct FColor
{
	public float R;
	public float G;
	public float B;
	public float A;
}

public readonly record struct TextureHandle
{
	public readonly IntPtr Handle;
}

internal static partial class SDL_GPU
{
	const string nativeLibName = "SDL3";

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_GPUSupportsShaderFormats(ShaderFormat format_flags, string name);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_GPUSupportsProperties(uint props);

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateGPUDevice(ShaderFormat format_flags, SDLBool debug_mode, string name);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateGPUDeviceWithProperties(uint props);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DestroyGPUDevice(IntPtr device);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial int SDL_GetNumGPUDrivers();

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	[return: MarshalUsing(typeof(SDL3.SDL.SDLOwnedStringMarshaller))]
	public static partial string SDL_GetGPUDriver(int index);

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	[return: MarshalUsing(typeof(SDL3.SDL.SDLOwnedStringMarshaller))]
	public static partial string SDL_GetGPUDeviceDriver(IntPtr device);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial ShaderFormat SDL_GetGPUShaderFormats(IntPtr device);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateGPUComputePipeline(IntPtr device, in INTERNAL_ComputePipelineCreateInfo createinfo);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateGPUGraphicsPipeline(IntPtr device, in INTERNAL_GraphicsPipelineCreateInfo createinfo);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateGPUSampler(IntPtr device, in SamplerCreateInfo createinfo);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateGPUShader(IntPtr device, in INTERNAL_ShaderCreateInfo createinfo);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateGPUTexture(IntPtr device, in TextureCreateInfo createinfo);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateGPUBuffer(IntPtr device, in BufferCreateInfo createinfo);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateGPUTransferBuffer(IntPtr device, in TransferBufferCreateInfo createinfo);

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_SetGPUBufferName(IntPtr device, IntPtr buffer, string text);

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_SetGPUTextureName(IntPtr device, IntPtr texture, string text);

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_InsertGPUDebugLabel(IntPtr command_buffer, string text);

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_PushGPUDebugGroup(IntPtr command_buffer, string name);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_PopGPUDebugGroup(IntPtr command_buffer);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_ReleaseGPUTexture(IntPtr device, IntPtr texture);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_ReleaseGPUSampler(IntPtr device, IntPtr sampler);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_ReleaseGPUBuffer(IntPtr device, IntPtr buffer);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_ReleaseGPUTransferBuffer(IntPtr device, IntPtr transfer_buffer);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_ReleaseGPUComputePipeline(IntPtr device, IntPtr compute_pipeline);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_ReleaseGPUShader(IntPtr device, IntPtr shader);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_ReleaseGPUGraphicsPipeline(IntPtr device, IntPtr graphics_pipeline);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_AcquireGPUCommandBuffer(IntPtr device);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_PushGPUVertexUniformData(IntPtr command_buffer, uint slot_index, IntPtr data, uint length);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_PushGPUFragmentUniformData(IntPtr command_buffer, uint slot_index, IntPtr data, uint length);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_PushGPUComputeUniformData(IntPtr command_buffer, uint slot_index, IntPtr data, uint length);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_BeginGPURenderPass(IntPtr command_buffer, Span<ColorTargetInfo> color_target_infos, uint num_color_targets, in DepthStencilTargetInfo depth_stencil_target_info);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUGraphicsPipeline(IntPtr render_pass, IntPtr graphics_pipeline);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_SetGPUViewport(IntPtr render_pass, in Viewport viewport);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_SetGPUScissor(IntPtr render_pass, in Rect scissor);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_SetGPUBlendConstants(IntPtr render_pass, Color blend_constants);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_SetGPUStencilReference(IntPtr render_pass, byte reference);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUVertexBuffers(IntPtr render_pass, uint first_slot, Span<BufferBinding> bindings, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUIndexBuffer(IntPtr render_pass, in BufferBinding binding, IndexElementSize index_element_size);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUVertexSamplers(IntPtr render_pass, uint first_slot, Span<TextureSamplerBinding> texture_sampler_bindings, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUVertexStorageTextures(IntPtr render_pass, uint first_slot, Span<IntPtr> storage_textures, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUVertexStorageBuffers(IntPtr render_pass, uint first_slot, Span<IntPtr> storage_buffers, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUFragmentSamplers(IntPtr render_pass, uint first_slot, Span<TextureSamplerBinding> texture_sampler_bindings, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUFragmentStorageTextures(IntPtr render_pass, uint first_slot, Span<IntPtr> storage_textures, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUFragmentStorageBuffers(IntPtr render_pass, uint first_slot, Span<IntPtr> storage_buffers, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DrawGPUIndexedPrimitives(IntPtr render_pass, uint num_indices, uint num_instances, uint first_index, int vertex_offset, uint first_instance);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DrawGPUPrimitives(IntPtr render_pass, uint num_vertices, uint num_instances, uint first_vertex, uint first_instance);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DrawGPUPrimitivesIndirect(IntPtr render_pass, IntPtr buffer, uint offset, uint draw_count);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DrawGPUIndexedPrimitivesIndirect(IntPtr render_pass, IntPtr buffer, uint offset, uint draw_count);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_EndGPURenderPass(IntPtr render_pass);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_BeginGPUComputePass(IntPtr command_buffer, Span<StorageTextureReadWriteBinding> storage_texture_bindings, uint num_storage_texture_bindings, Span<StorageBufferReadWriteBinding> storage_buffer_bindings, uint num_storage_buffer_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUComputePipeline(IntPtr compute_pass, IntPtr compute_pipeline);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUComputeSamplers(IntPtr compute_pass, uint first_slot, Span<TextureSamplerBinding> texture_sampler_bindings, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUComputeStorageTextures(IntPtr compute_pass, uint first_slot, Span<IntPtr> storage_textures, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BindGPUComputeStorageBuffers(IntPtr compute_pass, uint first_slot, Span<IntPtr> storage_buffers, uint num_bindings);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DispatchGPUCompute(IntPtr compute_pass, uint groupcount_x, uint groupcount_y, uint groupcount_z);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DispatchGPUComputeIndirect(IntPtr compute_pass, IntPtr buffer, uint offset);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_EndGPUComputePass(IntPtr compute_pass);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_MapGPUTransferBuffer(IntPtr device, IntPtr transfer_buffer, SDLBool cycle);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_UnmapGPUTransferBuffer(IntPtr device, IntPtr transfer_buffer);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_BeginGPUCopyPass(IntPtr command_buffer);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_UploadToGPUTexture(IntPtr copy_pass, in TextureTransferInfo source, in TextureRegion destination, SDLBool cycle);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_UploadToGPUBuffer(IntPtr copy_pass, in TransferBufferLocation source, in BufferRegion destination, SDLBool cycle);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_CopyGPUTextureToTexture(IntPtr copy_pass, in TextureLocation source, in TextureLocation destination, uint w, uint h, uint d, SDLBool cycle);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_CopyGPUBufferToBuffer(IntPtr copy_pass, in BufferLocation source, in BufferLocation destination, uint size, SDLBool cycle);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DownloadFromGPUTexture(IntPtr copy_pass, in TextureRegion source, in TextureTransferInfo destination);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DownloadFromGPUBuffer(IntPtr copy_pass, in BufferRegion source, in TransferBufferLocation destination);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_EndGPUCopyPass(IntPtr copy_pass);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_GenerateMipmapsForGPUTexture(IntPtr command_buffer, IntPtr texture);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_BlitGPUTexture(IntPtr command_buffer, in BlitInfo info);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_WindowSupportsGPUSwapchainComposition(IntPtr device, IntPtr window, SwapchainComposition swapchain_composition);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_WindowSupportsGPUPresentMode(IntPtr device, IntPtr window, PresentMode present_mode);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_ClaimWindowForGPUDevice(IntPtr device, IntPtr window);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_ReleaseWindowFromGPUDevice(IntPtr device, IntPtr window);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_SetGPUSwapchainParameters(IntPtr device, IntPtr window, SwapchainComposition swapchain_composition, PresentMode present_mode);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial TextureFormat SDL_GetGPUSwapchainTextureFormat(IntPtr device, IntPtr window);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_AcquireGPUSwapchainTexture(IntPtr command_buffer, IntPtr window, out IntPtr swapchain_texture, out uint swapchain_texture_width, out uint swapchain_texture_height);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_SubmitGPUCommandBuffer(IntPtr command_buffer);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_SubmitGPUCommandBufferAndAcquireFence(IntPtr command_buffer);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_WaitForGPUIdle(IntPtr device);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_WaitForGPUFences(IntPtr device, SDLBool wait_all, Span<IntPtr> fences, uint num_fences);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_QueryGPUFence(IntPtr device, IntPtr fence);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_ReleaseGPUFence(IntPtr device, IntPtr fence);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial uint SDL_GPUTextureFormatTexelBlockSize(TextureFormat format);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_GPUTextureSupportsFormat(IntPtr device, TextureFormat format, TextureType type, TextureUsageFlags usage);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_GPUTextureSupportsSampleCount(IntPtr device, TextureFormat format, SampleCount sample_count);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial uint SDL_CalculateGPUTextureFormatSize(TextureFormat format, uint width, uint height, uint depth);
}

internal static partial class IRO
{
	const string nativeLibName = "IRO";

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr IRO_LoadImage(
		IntPtr bufferPtr,
		uint bufferLength,
		out uint w,
		out uint h,
		out uint len
	);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool IRO_GetImageInfo(
		IntPtr bufferPtr,
		uint bufferLength,
		out uint w,
		out uint h,
		out uint len
	);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void IRO_FreeImage(IntPtr mem);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void IRO_WriteFunc(
		IntPtr context,
		IntPtr data,
		int size
	);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool IRO_EncodePNG(
		IRO_WriteFunc writeFunc,
		IntPtr context,
		IntPtr data,
		uint w,
		uint h
	);
}
