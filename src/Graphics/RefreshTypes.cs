using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics;

// Recreate certain types in here so we can csharp-ify them and hide the Refresh namespace

public enum PrimitiveType
{
	PointList,
	LineList,
	LineStrip,
	TriangleList,
	TriangleStrip
}

public enum LoadOp
{
	Load,
	Clear,
	DontCare
}

public enum StoreOp
{
	Store,
	DontCare
}

public enum IndexElementSize
{
	Sixteen,
	ThirtyTwo
}

public enum TextureFormat
{
	/* Unsigned Normalized Float Color Formats */
	R8G8B8A8,
	B8G8R8A8,
	R5G6B5,
	A1R5G5B5,
	B4G4R4A4,
	A2R10G10B10,
	A2B10G10R10,
	R16G16,
	R16G16B16A16,
	R8,
	A8,
	/* Compressed Unsigned Normalized Float Color Formats */
	BC1,
	BC2,
	BC3,
	BC7,
	/* Signed Normalized Float Color Formats  */
	R8G8_SNORM,
	R8G8B8A8_SNORM,
	/* Signed Float Color Formats */
	R16_SFLOAT,
	R16G16_SFLOAT,
	R16G16B16A16_SFLOAT,
	R32_SFLOAT,
	R32G32_SFLOAT,
	R32G32B32A32_SFLOAT,
	/* Unsigned Integer Color Formats */
	R8_UINT,
	R8G8_UINT,
	R8G8B8A8_UINT,
	R16_UINT,
	R16G16_UINT,
	R16G16B16A16_UINT,
	/* SRGB Color Formats */
	R8G8B8A8_SRGB,
	B8G8R8A8_SRGB,
	/* Compressed SRGB Color Formats */
	BC3_SRGB,
	BC7_SRGB,
	/* Depth Formats */
	D16_UNORM,
	D24_UNORM,
	D32_SFLOAT,
	D24_UNORM_S8_UINT,
	D32_SFLOAT_S8_UINT
}

[Flags]
public enum TextureUsageFlags
{
	Sampler = 0x1,
	ColorTarget = 0x2,
	DepthStencil = 0x4,
	GraphicsStorage = 0x8,
	ComputeStorageRead = 0x20,
	ComputeStorageWrite = 0x40
}

public enum TextureType
{
	TwoD,
	ThreeD,
	Cube
}

public enum SampleCount
{
	One,
	Two,
	Four,
	Eight
}

public enum CubeMapFace
{
	PositiveX,
	NegativeX,
	PositiveY,
	NegativeY,
	PositiveZ,
	NegativeZ
}

[Flags]
public enum BufferUsageFlags
{
	Vertex = 0x1,
	Index = 0x2,
	Indirect = 0x4,
	GraphicsStorage = 0x8,
	ComputeStorageRead = 0x20,
	ComputeStorageWrite = 0x40
}

[Flags]
public enum TransferBufferMapFlags
{
	Read = 0x1,
	Write = 0x2
}

public enum ShaderStage
{
	Vertex,
	Fragment
}

public enum ShaderFormat
{
	Invalid,
	SPIRV,
	HLSL,
	DXBC,
	DXIL,
	MSL,
	METALLIB,
	SECRET
}

public enum VertexElementFormat
{
	Uint,
	Float,
	Vector2,
	Vector3,
	Vector4,
	Color,
	Byte4,
	Short2,
	Short4,
	NormalizedShort2,
	NormalizedShort4,
	HalfVector2,
	HalfVector4
}

public enum VertexInputRate
{
	Vertex,
	Instance
}

public enum FillMode
{
	Fill,
	Line
}

public enum CullMode
{
	None,
	Front,
	Back
}

public enum FrontFace
{
	CounterClockwise,
	Clockwise
}

public enum CompareOp
{
	Never,
	Less,
	Equal,
	LessOrEqual,
	Greater,
	NotEqual,
	GreaterOrEqual,
	Always
}

public enum StencilOp
{
	Keep,
	Zero,
	Replace,
	IncrementAndClamp,
	DecrementAndClamp,
	Invert,
	IncrementAndWrap,
	DecrementAndWrap
}

public enum BlendOp
{
	Add,
	Subtract,
	ReverseSubtract,
	Min,
	Max
}

public enum BlendFactor
{
	Zero,
	One,
	SourceColor,
	OneMinusSourceColor,
	DestinationColor,
	OneMinusDestinationColor,
	SourceAlpha,
	OneMinusSourceAlpha,
	DestinationAlpha,
	OneMinusDestinationAlpha,
	ConstantColor,
	OneMinusConstantColor,
	SourceAlphaSaturate
}

[Flags]
public enum ColorComponentFlags
{
	None = 0x0,
	R = 0x1,
	G = 0x2,
	B = 0x4,
	A = 0x8,
	RGB = R | G | B,
	RGBA = R | G| B | A
}

public enum Filter
{
	Nearest,
	Linear
}

public enum SamplerMipmapMode
{
	Nearest,
	Linear
}

public enum SamplerAddressMode
{
	Repeat,
	MirroredRepeat,
	ClampToEdge,
	ClampToBorder
}

public enum BorderColor
{
	FloatTransparentBlack,
	IntTransparentBlack,
	FloatOpaqueBlack,
	IntOpaqueBlack,
	FloatOpaqueWhite,
	IntOpaqueWhite
}

public enum TransferUsage
{
	Buffer,
	Texture
}

public enum PresentMode
{
	VSync,
	Immediate,
	Mailbox
}

public enum SwapchainComposition
{
	SDR,
	SDRLinear,
	HDRExtendedLinear,
	HDR10_ST2084
}

[Flags]
public enum BackendFlags
{
	Invalid = 0x0,
	Vulkan = 0x1,
	D3D11 = 0x2,
	Metal = 0x4,
	All = Vulkan | D3D11 | Metal
}

[StructLayout(LayoutKind.Sequential)]
public struct DepthStencilValue
{
	public float Depth;
	public uint Stencil;

	public DepthStencilValue(float depth, uint stencil)
	{
		Depth = depth;
		Stencil = stencil;
	}

	// FIXME: can we do an unsafe cast somehow?
	public Refresh.DepthStencilValue ToRefresh()
	{
		return new Refresh.DepthStencilValue
		{
			Depth = Depth,
			Stencil = Stencil
		};
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
	public int X;
	public int Y;
	public int W;
	public int H;

	public Rect(int x, int y, int w, int h)
	{
		X = x;
		Y = y;
		W = w;
		H = h;
	}

	public Rect(int w, int h)
	{
		X = 0;
		Y = 0;
		W = w;
		H = h;
	}

	// FIXME: can we do an unsafe cast somehow?
	public Refresh.Rect ToRefresh()
	{
		return new Refresh.Rect
		{
			X = X,
			Y = Y,
			W = W,
			H = H
		};
	}
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

	public Viewport(float w, float h)
	{
		X = 0;
		Y = 0;
		W = w;
		H = h;
		MinDepth = 0;
		MaxDepth = 1;
	}

	public Viewport(float x, float y, float w, float h)
	{
		X = x;
		Y = y;
		W = w;
		H = h;
		MinDepth = 0;
		MaxDepth = 1;
	}

	public Viewport(float x, float y, float w, float h, float minDepth, float maxDepth)
	{
		X = x;
		Y = y;
		W = w;
		H = h;
		MinDepth = minDepth;
		MaxDepth = maxDepth;
	}

	public Refresh.Viewport ToRefresh()
	{
		return new Refresh.Viewport
		{
			X = X,
			Y = Y,
			W = W,
			H = H,
			MinDepth = MinDepth,
			MaxDepth = MaxDepth
		};
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct VertexBinding
{
	public uint Binding;
	public uint Stride;
	public VertexInputRate InputRate;
	public uint StepRate;

	public static VertexBinding Create<T>(
		uint binding = 0,
		VertexInputRate inputRate = VertexInputRate.Vertex,
		uint stepRate = 1
	) where T : unmanaged
	{
		return new VertexBinding
		{
			Binding = binding,
			InputRate = inputRate,
			Stride = (uint) Marshal.SizeOf<T>(),
			StepRate = stepRate
		};
	}

	public Refresh.VertexBinding ToRefresh()
	{
		return new Refresh.VertexBinding
		{
			Binding = Binding,
			Stride = Stride,
			InputRate = (Refresh.VertexInputRate) InputRate,
			StepRate = StepRate
		};
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct VertexAttribute
{
	public uint Location;
	public uint Binding;
	public VertexElementFormat Format;
	public uint Offset;

	public Refresh.VertexAttribute ToRefresh()
	{
		return new Refresh.VertexAttribute
		{
			Location = Location,
			Binding = Binding,
			Format = (Refresh.VertexElementFormat) Format,
			Offset = Offset
		};
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct StencilOpState
{
	public StencilOp FailOp;
	public StencilOp PassOp;
	public StencilOp DepthFailOp;
	public CompareOp CompareOp;

	public Refresh.StencilOpState ToRefresh()
	{
		return new Refresh.StencilOpState
		{
			FailOp = (Refresh.StencilOp) FailOp,
			PassOp = (Refresh.StencilOp) PassOp,
			DepthFailOp = (Refresh.StencilOp) DepthFailOp,
			CompareOp = (Refresh.CompareOp) CompareOp
		};
	}
}

/// <summary>
/// Determines how a color texture will be read/written in a render pass.
/// </summary>
public struct ColorAttachmentInfo
{
	public TextureSlice TextureSlice;

	/// <summary>
	/// If LoadOp is set to Clear, the texture slice will be cleared to this color.
	/// </summary>
	public Color ClearColor;

	/// <summary>
	/// Determines what is done with the texture slice memory
	/// at the beginning of the render pass. <br/>
	///
	///   Load:
	///     Loads the data currently in the texture slice. <br/>
	///
	///   Clear:
	///     Clears the texture slice to a single color. <br/>
	///
	///   DontCare:
	///     The driver will do whatever it wants with the texture slice data.
	///     This is a good option if you know that every single pixel will be written in the render pass.
	/// </summary>
	public LoadOp LoadOp;

	/// <summary>
	/// Determines what is done with the texture slice memory
	/// at the end of the render pass. <br/>
	///
	///   Store:
	///     Stores the results of the render pass in the texture slice memory. <br/>
	///
	///   DontCare:
	///     The driver will do whatever it wants with the texture slice memory.
	/// </summary>
	public StoreOp StoreOp;

	/// <summary>
	/// If true, cycles the texture if it is bound.
	/// </summary>
	public bool Cycle;

	public ColorAttachmentInfo(
		TextureSlice textureSlice,
		bool cycle,
		Color clearColor,
		StoreOp storeOp = StoreOp.Store
	) {
		TextureSlice = textureSlice;
		ClearColor = clearColor;
		LoadOp = LoadOp.Clear;
		StoreOp = storeOp;
		Cycle = cycle;
	}

	public ColorAttachmentInfo(
		TextureSlice textureSlice,
		bool cycle,
		LoadOp loadOp = LoadOp.DontCare,
		StoreOp storeOp = StoreOp.Store
	) {
		TextureSlice = textureSlice;
		ClearColor = Color.White;
		LoadOp = loadOp;
		StoreOp = storeOp;
		Cycle = cycle;
	}

	public Refresh.ColorAttachmentInfo ToRefresh()
	{
		return new Refresh.ColorAttachmentInfo
		{
			TextureSlice = TextureSlice.ToRefresh(),
			ClearColor = new Refresh.Color
			{
				R = ClearColor.R / 255f,
				G = ClearColor.G / 255f,
				B = ClearColor.B / 255f,
				A = ClearColor.A / 255f
			},
			LoadOp = (Refresh.LoadOp) LoadOp,
			StoreOp = (Refresh.StoreOp) StoreOp,
			Cycle = Conversions.BoolToInt(Cycle)
		};
	}
}

/// <summary>
/// Determines how a depth/stencil texture will be read/written in a render pass.
/// </summary>
public struct DepthStencilAttachmentInfo
{
	public TextureSlice TextureSlice;

	/// <summary>
	/// If LoadOp is set to Clear, the texture slice depth will be cleared to this depth value. <br/>
	/// If StencilLoadOp is set to Clear, the texture slice stencil value will be cleared to this stencil value.
	/// </summary>
	public DepthStencilValue DepthStencilClearValue;

	/// <summary>
	/// Determines what is done with the texture slice depth values
	/// at the beginning of the render pass. <br/>
	///
	///   Load:
	///     Loads the data currently in the texture slice. <br/>
	///
	///   Clear:
	///     Clears the texture slice to a single depth value. <br/>
	///
	///   DontCare:
	///     The driver will do whatever it wants with the texture slice data.
	///     This is a good option if you know that every single pixel will be written in the render pass.
	/// </summary>
	public LoadOp LoadOp;

	/// <summary>
	/// Determines what is done with the texture slice depth values
	/// at the end of the render pass. <br/>
	///
	///   Store:
	///     Stores the results of the render pass in the texture slice memory. <br/>
	///
	///   DontCare:
	///     The driver will do whatever it wants with the texture slice memory.
	///     This is usually a good option for depth textures that don't need to be reused.
	/// </summary>
	public StoreOp StoreOp;

	/// <summary>
	/// Determines what is done with the texture slice stencil values
	/// at the beginning of the render pass. <br/>
	///
	///   Load:
	///     Loads the data currently in the texture slice. <br/>
	///
	///   Clear:
	///     Clears the texture slice to a single stencil value. <br/>
	///
	///   DontCare:
	///     The driver will do whatever it wants with the texture slice data.
	///     This is a good option if you know that every single pixel will be written in the render pass.
	/// </summary>
	public LoadOp StencilLoadOp;

	/// <summary>
	/// Determines what is done with the texture slice stencil values
	/// at the end of the render pass. <br/>
	///
	///   Store:
	///     Stores the results of the render pass in the texture slice memory. <br/>
	///
	///   DontCare:
	///     The driver will do whatever it wants with the texture slice memory.
	///     This is usually a good option for stencil textures that don't need to be reused.
	/// </summary>
	public StoreOp StencilStoreOp;

	/// <summary>
	/// If true, cycles the texture if it is bound.
	/// </summary>
	public bool Cycle;

	public DepthStencilAttachmentInfo(
		TextureSlice textureSlice,
		bool cycle,
		DepthStencilValue clearValue,
		StoreOp depthStoreOp = StoreOp.DontCare,
		StoreOp stencilStoreOp = StoreOp.DontCare
	){
		TextureSlice = textureSlice;
		DepthStencilClearValue = clearValue;
		LoadOp = LoadOp.Clear;
		StoreOp = depthStoreOp;
		StencilLoadOp = LoadOp.Clear;
		StencilStoreOp = stencilStoreOp;
		Cycle = cycle;
	}

	public DepthStencilAttachmentInfo(
		TextureSlice textureSlice,
		bool cycle,
		LoadOp loadOp = LoadOp.DontCare,
		StoreOp storeOp = StoreOp.DontCare,
		LoadOp stencilLoadOp = LoadOp.DontCare,
		StoreOp stencilStoreOp = StoreOp.DontCare
	) {
		TextureSlice = textureSlice;
		DepthStencilClearValue = new DepthStencilValue();
		LoadOp = loadOp;
		StoreOp = storeOp;
		StencilLoadOp = stencilLoadOp;
		StencilStoreOp = stencilStoreOp;
		Cycle = cycle;
	}

	public DepthStencilAttachmentInfo(
		TextureSlice textureSlice,
		bool cycle,
		DepthStencilValue clearValue,
		LoadOp loadOp,
		StoreOp storeOp,
		LoadOp stencilLoadOp,
		StoreOp stencilStoreOp
	) {
		TextureSlice = textureSlice;
		DepthStencilClearValue = clearValue;
		LoadOp = loadOp;
		StoreOp = storeOp;
		StencilLoadOp = stencilLoadOp;
		StencilStoreOp = stencilStoreOp;
		Cycle = cycle;
	}

	public Refresh.DepthStencilAttachmentInfo ToRefresh()
	{
		return new Refresh.DepthStencilAttachmentInfo
		{
			TextureSlice = TextureSlice.ToRefresh(),
			DepthStencilClearValue = DepthStencilClearValue.ToRefresh(),
			LoadOp = (Refresh.LoadOp) LoadOp,
			StoreOp = (Refresh.StoreOp) StoreOp,
			StencilLoadOp = (Refresh.LoadOp) StencilLoadOp,
			StencilStoreOp = (Refresh.StoreOp) StencilStoreOp,
			Cycle = Conversions.BoolToInt(Cycle)
		};
	}
}

/// <summary>
/// Defines how color blending will be performed in a GraphicsPipeline.
/// </summary>
public struct ColorAttachmentBlendState
{
	/// <summary>
	/// If disabled, no blending will occur.
	/// </summary>
	public bool BlendEnable;

	/// <summary>
	/// Selects which blend operation to use with alpha values.
	/// </summary>
	public BlendOp AlphaBlendOp;
	/// <summary>
	/// Selects which blend operation to use with color values.
	/// </summary>
	public BlendOp ColorBlendOp;

	/// <summary>
	/// Specifies which of the RGBA components are enabled for writing.
	/// </summary>
	public ColorComponentFlags ColorWriteMask;

	/// <summary>
	/// Selects which blend factor is used to determine the alpha destination factor.
	/// </summary>
	public BlendFactor DestinationAlphaBlendFactor;

	/// <summary>
	/// Selects which blend factor is used to determine the color destination factor.
	/// </summary>
	public BlendFactor DestinationColorBlendFactor;

	/// <summary>
	/// Selects which blend factor is used to determine the alpha source factor.
	/// </summary>
	public BlendFactor SourceAlphaBlendFactor;

	/// <summary>
	/// Selects which blend factor is used to determine the color source factor.
	/// </summary>
	public BlendFactor SourceColorBlendFactor;

	public static readonly ColorAttachmentBlendState Additive = new ColorAttachmentBlendState
	{
		BlendEnable = true,
		AlphaBlendOp = BlendOp.Add,
		ColorBlendOp = BlendOp.Add,
		ColorWriteMask = ColorComponentFlags.RGBA,
		SourceColorBlendFactor = BlendFactor.SourceAlpha,
		SourceAlphaBlendFactor = BlendFactor.SourceAlpha,
		DestinationColorBlendFactor = BlendFactor.One,
		DestinationAlphaBlendFactor = BlendFactor.One
	};

	public static readonly ColorAttachmentBlendState AlphaBlend = new ColorAttachmentBlendState
	{
		BlendEnable = true,
		AlphaBlendOp = BlendOp.Add,
		ColorBlendOp = BlendOp.Add,
		ColorWriteMask = ColorComponentFlags.RGBA,
		SourceColorBlendFactor = BlendFactor.One,
		SourceAlphaBlendFactor = BlendFactor.One,
		DestinationColorBlendFactor = BlendFactor.OneMinusSourceAlpha,
		DestinationAlphaBlendFactor = BlendFactor.OneMinusSourceAlpha
	};

	public static readonly ColorAttachmentBlendState NonPremultiplied = new ColorAttachmentBlendState
	{
		BlendEnable = true,
		AlphaBlendOp = BlendOp.Add,
		ColorBlendOp = BlendOp.Add,
		ColorWriteMask = ColorComponentFlags.RGBA,
		SourceColorBlendFactor = BlendFactor.SourceAlpha,
		SourceAlphaBlendFactor = BlendFactor.SourceAlpha,
		DestinationColorBlendFactor = BlendFactor.OneMinusSourceAlpha,
		DestinationAlphaBlendFactor = BlendFactor.OneMinusSourceAlpha
	};

	public static readonly ColorAttachmentBlendState Opaque = new ColorAttachmentBlendState
	{
		BlendEnable = true,
		AlphaBlendOp = BlendOp.Add,
		ColorBlendOp = BlendOp.Add,
		ColorWriteMask = ColorComponentFlags.RGBA,
		SourceColorBlendFactor = BlendFactor.One,
		SourceAlphaBlendFactor = BlendFactor.One,
		DestinationColorBlendFactor = BlendFactor.Zero,
		DestinationAlphaBlendFactor = BlendFactor.Zero
	};

	public static readonly ColorAttachmentBlendState None = new ColorAttachmentBlendState
	{
		BlendEnable = false,
		ColorWriteMask = ColorComponentFlags.RGBA
	};

	public static readonly ColorAttachmentBlendState Disable = new ColorAttachmentBlendState
	{
		BlendEnable = false,
		ColorWriteMask = ColorComponentFlags.None
	};

	public Refresh.ColorAttachmentBlendState ToRefresh()
	{
		return new Refresh.ColorAttachmentBlendState
		{
			BlendEnable = Conversions.BoolToInt(BlendEnable),
			AlphaBlendOp = (Refresh.BlendOp) AlphaBlendOp,
			ColorBlendOp = (Refresh.BlendOp) ColorBlendOp,
			ColorWriteMask = (Refresh.ColorComponentFlags) ColorWriteMask,
			DestinationAlphaBlendFactor = (Refresh.BlendFactor) DestinationAlphaBlendFactor,
			DestinationColorBlendFactor = (Refresh.BlendFactor) DestinationColorBlendFactor,
			SourceAlphaBlendFactor = (Refresh.BlendFactor) SourceAlphaBlendFactor,
			SourceColorBlendFactor = (Refresh.BlendFactor) SourceColorBlendFactor
		};
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct ColorAttachmentDescription
{
	public TextureFormat Format;
	public ColorAttachmentBlendState BlendState;

	public ColorAttachmentDescription(
		TextureFormat format,
		ColorAttachmentBlendState blendState
	) {
		Format = format;
		BlendState = blendState;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct IndirectDrawCommand
{
	public uint VertexCount;
	public uint InstanceCount;
	public uint FirstVertex;
	public uint FirstInstance;

	public IndirectDrawCommand(
		uint vertexCount,
		uint instanceCount,
		uint firstVertex,
		uint firstInstance
	) {
		VertexCount = vertexCount;
		InstanceCount = instanceCount;
		FirstVertex = firstVertex;
		FirstInstance = firstInstance;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct BufferCopy
{
	public uint SrcOffset;
	public uint DstOffset;
	public uint Size;

	public BufferCopy(
		uint srcOffset,
		uint dstOffset,
		uint size
	) {
		SrcOffset = srcOffset;
		DstOffset = dstOffset;
		Size = size;
	}

	public Refresh.BufferCopy ToRefresh()
	{
		return new Refresh.BufferCopy
		{
			SourceOffset = SrcOffset,
			DestinationOffset = DstOffset,
			Size = Size
		};
	}
}

/// <summary>
/// Parameters for a copy between buffer and image.
/// </summary>
/// <param name="BufferOffset">The offset into the buffer.</param>
/// <param name="BufferStride">If 0, image data is assumed tightly packed.</param>
/// <param name="BufferImageHeight">If 0, image data is assumed tightly packed.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct BufferImageCopy(
	uint BufferOffset,
	uint BufferStride,
	uint BufferImageHeight
) {
	public Refresh.BufferImageCopy ToRefresh()
	{
		return new Refresh.BufferImageCopy
		{
			BufferOffset = BufferOffset,
			BufferStride = BufferStride,
			BufferImageHeight = BufferImageHeight
		};
	}
}

/// <summary>
/// A buffer-offset pair to be used when binding buffers.
/// </summary>
public readonly record struct BufferBinding(
	GpuBuffer Buffer,
	uint Offset
) {
	public Refresh.BufferBinding ToRefresh()
	{
		return new Refresh.BufferBinding
		{
			Buffer = Buffer.Handle,
			Offset = Offset
		};
	}
}

/// <summary>
/// A texture-sampler pair to be used when binding samplers.
/// </summary>
public readonly record struct TextureSamplerBinding(
	Texture Texture,
	Sampler Sampler
) {
	public Refresh.TextureSamplerBinding ToRefresh()
	{
		return new Refresh.TextureSamplerBinding
		{
			Texture = Texture.Handle,
			Sampler = Sampler.Handle
		};
	}
}

public readonly record struct StorageBufferReadWriteBinding(
	GpuBuffer Buffer,
	bool Cycle
) {
	public Refresh.StorageBufferReadWriteBinding ToRefresh()
	{
		return new Refresh.StorageBufferReadWriteBinding
		{
			Buffer = Buffer.Handle,
			Cycle = Conversions.BoolToInt(Cycle)
		};
	}
}

public readonly record struct StorageTextureReadWriteBinding(
	in TextureSlice TextureSlice,
	bool Cycle
) {
	public Refresh.StorageTextureReadWriteBinding ToRefresh()
	{
		return new Refresh.StorageTextureReadWriteBinding
		{
			TextureSlice = TextureSlice.ToRefresh(),
			Cycle = Conversions.BoolToInt(Cycle)
		};
	}
}

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
		FillMode = FillMode.Line,
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
		FillMode = FillMode.Line,
		DepthBiasEnable = false
	};

	public Refresh.RasterizerState ToRefresh()
	{
		return new Refresh.RasterizerState
		{
			CullMode = (Refresh.CullMode) CullMode,
			DepthBiasClamp = DepthBiasClamp,
			DepthBiasConstantFactor = DepthBiasConstantFactor,
			DepthBiasEnable = Conversions.BoolToInt(DepthBiasEnable),
			DepthBiasSlopeFactor = DepthBiasSlopeFactor,
			FillMode = (Refresh.FillMode) FillMode,
			FrontFace = (Refresh.FrontFace) FrontFace
		};
	}
}

/// <summary>
/// Specifies how many samples should be used in rasterization.
/// </summary>
public struct MultisampleState
{
	public SampleCount MultisampleCount;
	public uint SampleMask;

	public static readonly MultisampleState None = new MultisampleState
	{
		MultisampleCount = SampleCount.One,
		SampleMask = uint.MaxValue
	};

	public MultisampleState(
		SampleCount sampleCount,
		uint sampleMask = uint.MaxValue
	) {
		MultisampleCount = sampleCount;
		SampleMask = sampleMask;
	}

	public Refresh.MultisampleState ToRefresh()
	{
		return new Refresh.MultisampleState
		{
			MultisampleCount = (Refresh.SampleCount) MultisampleCount,
			SampleMask = SampleMask
		};
	}
}

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
	/// Describes the back-face stencil operation.
	/// </summary>
	public StencilOpState BackStencilState;

	/// <summary>
	/// Describes the front-face stencil operation.
	/// </summary>
	public StencilOpState FrontStencilState;

	/// <summary>
	/// The compare mask for stencil ops.
	/// </summary>
	public uint CompareMask;

	/// <summary>
	/// The write mask for stencil ops.
	/// </summary>
	public uint WriteMask;

	/// <summary>
	/// The stencil reference value.
	/// </summary>
	public uint Reference;

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

	public Refresh.DepthStencilState ToRefresh()
	{
		return new Refresh.DepthStencilState
		{
			DepthTestEnable = Conversions.BoolToInt(DepthTestEnable),
			BackStencilState = BackStencilState.ToRefresh(),
			FrontStencilState = FrontStencilState.ToRefresh(),
			CompareMask = CompareMask,
			WriteMask = WriteMask,
			Reference = Reference,
			CompareOp = (Refresh.CompareOp) CompareOp,
			DepthBoundsTestEnable = Conversions.BoolToInt(DepthBoundsTestEnable),
			DepthWriteEnable = Conversions.BoolToInt(DepthWriteEnable),
			MinDepthBounds = MinDepthBounds,
			MaxDepthBounds = MaxDepthBounds,
			StencilTestEnable = Conversions.BoolToInt(StencilTestEnable)
		};
	}
}

/// <summary>
/// Describes the kind of attachments that will be used with this pipeline.
/// </summary>
public struct GraphicsPipelineAttachmentInfo
{
	public ColorAttachmentDescription[] ColorAttachmentDescriptions;
	public bool HasDepthStencilAttachment;
	public TextureFormat DepthStencilFormat;

	public GraphicsPipelineAttachmentInfo(
		params ColorAttachmentDescription[] colorAttachmentDescriptions
	) {
		ColorAttachmentDescriptions = colorAttachmentDescriptions;
		HasDepthStencilAttachment = false;
		DepthStencilFormat = TextureFormat.D16_UNORM;
	}

	public GraphicsPipelineAttachmentInfo(
		TextureFormat depthStencilFormat,
		params ColorAttachmentDescription[] colorAttachmentDescriptions
	) {
		ColorAttachmentDescriptions = colorAttachmentDescriptions;
		HasDepthStencilAttachment = true;
		DepthStencilFormat = depthStencilFormat;
	}
}

public struct BlendConstants
{
	public float R;
	public float G;
	public float B;
	public float A;
}

/// <summary>
/// All of the information that is used to create a GraphicsPipeline.
/// </summary>
public struct GraphicsPipelineCreateInfo
{
	public Shader VertexShader;
	public Shader FragmentShader;
	public VertexInputState VertexInputState;
	public PrimitiveType PrimitiveType;
	public RasterizerState RasterizerState;
	public MultisampleState MultisampleState;
	public DepthStencilState DepthStencilState;
	public GraphicsPipelineAttachmentInfo AttachmentInfo;
	public BlendConstants BlendConstants;
}

public struct ComputePipelineCreateInfo
{
	public ShaderFormat ShaderFormat;
	public uint ReadOnlyStorageTextureCount;
	public uint ReadOnlyStorageBufferCount;
	public uint ReadWriteStorageTextureCount;
	public uint ReadWriteStorageBufferCount;
	public uint UniformBufferCount;
	public uint ThreadCountX;
	public uint ThreadCountY;
	public uint ThreadCountZ;
}

public struct ShaderCreateInfo
{
	public ShaderStage ShaderStage;
	public ShaderFormat ShaderFormat;
	public uint SamplerCount;
	public uint StorageTextureCount;
	public uint StorageBufferCount;
	public uint UniformBufferCount;
}

/// <summary>
/// All of the information that is used to create a texture.
/// </summary>
public struct TextureCreateInfo
{
	public uint Width;
	public uint Height;
	public uint Depth;
	public bool IsCube;
	public uint LayerCount;
	public uint LevelCount;
	public SampleCount SampleCount;
	public TextureFormat Format;
	public TextureUsageFlags UsageFlags;

	public Refresh.TextureCreateInfo ToRefresh()
	{
		return new Refresh.TextureCreateInfo
		{
			Width = Width,
			Height = Height,
			Depth = Depth,
			IsCube = Conversions.BoolToInt(IsCube),
			LayerCount = LayerCount,
			LevelCount = LevelCount,
			SampleCount = (Refresh.SampleCount) SampleCount,
			Format = (Refresh.TextureFormat) Format,
			UsageFlags = (Refresh.TextureUsageFlags) UsageFlags
		};
	}
}


/// <summary>
/// All of the information that is used to create a sampler.
/// </summary>
public struct SamplerCreateInfo
{
	/// <summary>
	/// Minification filter mode. Used when the image is downscaled.
	/// </summary>
	public Filter MinFilter;
	/// <summary>
	/// Magnification filter mode. Used when the image is upscaled.
	/// </summary>
	public Filter MagFilter;
	/// <summary>
	/// Filter mode applied to mipmap lookups.
	/// </summary>
	public SamplerMipmapMode MipmapMode;
	/// <summary>
	/// Horizontal addressing mode.
	/// </summary>
	public SamplerAddressMode AddressModeU;
	/// <summary>
	/// Vertical addressing mode.
	/// </summary>
	public SamplerAddressMode AddressModeV;
	/// <summary>
	/// Depth addressing mode.
	/// </summary>
	public SamplerAddressMode AddressModeW;
	/// <summary>
	/// Bias value added to mipmap level of detail calculation.
	/// </summary>
	public float MipLodBias;
	/// <summary>
	/// Enables anisotropic filtering.
	/// </summary>
	public bool AnisotropyEnable;
	/// <summary>
	/// Maximum anisotropy value.
	/// </summary>
	public float MaxAnisotropy;
	public bool CompareEnable;
	public CompareOp CompareOp;
	/// <summary>
	/// Clamps the LOD value to a specified minimum.
	/// </summary>
	public float MinLod;
	/// <summary>
	/// Clamps the LOD value to a specified maximum.
	/// </summary>
	public float MaxLod;
	/// <summary>
	/// If an address mode is set to ClampToBorder, will replace color with this color when samples are outside the [0, 1) range.
	/// </summary>
	public BorderColor BorderColor;

	public static readonly SamplerCreateInfo AnisotropicClamp = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.ClampToEdge,
		AddressModeV = SamplerAddressMode.ClampToEdge,
		AddressModeW = SamplerAddressMode.ClampToEdge,
		CompareEnable = false,
		AnisotropyEnable = true,
		MaxAnisotropy = 4,
		MipLodBias = 0f,
		MinLod = 0,
		MaxLod = 1000 /* VK_LOD_CLAMP_NONE */
	};

	public static readonly SamplerCreateInfo AnisotropicWrap = new SamplerCreateInfo
	{
		MinFilter = Filter.Linear,
		MagFilter = Filter.Linear,
		MipmapMode = SamplerMipmapMode.Linear,
		AddressModeU = SamplerAddressMode.Repeat,
		AddressModeV = SamplerAddressMode.Repeat,
		AddressModeW = SamplerAddressMode.Repeat,
		CompareEnable = false,
		AnisotropyEnable = true,
		MaxAnisotropy = 4,
		MipLodBias = 0f,
		MinLod = 0,
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
		CompareEnable = false,
		AnisotropyEnable = false,
		MipLodBias = 0f,
		MinLod = 0,
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
		CompareEnable = false,
		AnisotropyEnable = false,
		MipLodBias = 0f,
		MinLod = 0,
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
		CompareEnable = false,
		AnisotropyEnable = false,
		MipLodBias = 0f,
		MinLod = 0,
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
		CompareEnable = false,
		AnisotropyEnable = false,
		MipLodBias = 0f,
		MinLod = 0,
		MaxLod = 1000
	};

	public Refresh.SamplerCreateInfo ToRefresh()
	{
		return new Refresh.SamplerCreateInfo
		{
			MinFilter = (Refresh.Filter) MinFilter,
			MagFilter = (Refresh.Filter) MagFilter,
			MipmapMode = (Refresh.SamplerMipmapMode) MipmapMode,
			AddressModeU = (Refresh.SamplerAddressMode) AddressModeU,
			AddressModeV = (Refresh.SamplerAddressMode) AddressModeV,
			AddressModeW = (Refresh.SamplerAddressMode) AddressModeW,
			MipLodBias = MipLodBias,
			AnisotropyEnable = Conversions.BoolToInt(AnisotropyEnable),
			MaxAnisotropy = MaxAnisotropy,
			CompareEnable = Conversions.BoolToInt(CompareEnable),
			CompareOp = (Refresh.CompareOp) CompareOp,
			MinLod = MinLod,
			MaxLod = MaxLod,
			BorderColor = (Refresh.BorderColor) BorderColor
		};
	}
}
