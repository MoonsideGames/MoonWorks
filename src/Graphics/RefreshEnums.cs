using System;

namespace MoonWorks
{
	/// <summary>
	/// Presentation mode for a window.
	/// </summary>
	public enum PresentMode
	{
		/// <summary>
		/// Does not wait for v-blank to update the window. Can cause visible tearing.
		/// </summary>
		Immediate,
		/// <summary>
		/// Waits for v-blank and uses a queue to hold present requests.
		/// Allows for low latency while preventing tearing.
		/// May not be supported on non-Vulkan non-Linux systems or older hardware.
		/// </summary>
		Mailbox,
		/// <summary>
		/// Waits for v-blank and adds present requests to a queue.
		/// Will probably cause latency.
		/// Required to be supported by all compliant hardware.
		/// </summary>
		FIFO,
		/// <summary>
		/// Usually waits for v-blank, but if v-blank has passed since last update will update immediately.
		/// May cause visible tearing.
		/// </summary>
		FIFORelaxed
	}
}

/* Recreate all the enums in here so we don't need to explicitly
 * reference the RefreshCS namespace when using MoonWorks.Graphics
 */
namespace MoonWorks.Graphics
{
	public enum PrimitiveType
	{
		PointList,
		LineList,
		LineStrip,
		TriangleList,
		TriangleStrip
	}

	/// <summary>
	/// Describes the operation that a render pass will use when loading a render target.
	/// </summary>
	public enum LoadOp
	{
		Load,
		Clear,
		DontCare
	}

	/// <summary>
	/// Describes the operation that a render pass will use when storing a render target.
	/// </summary>
	public enum StoreOp
	{
		Store,
		DontCare
	}

	[Flags]
	public enum ClearOptionsFlags : uint
	{
		Color = 1,
		Depth = 2,
		Stencil = 4,
		DepthStencil = Depth | Stencil,
		All = Color | Depth | Stencil
	}

	public enum IndexElementSize
	{
		Sixteen,
		ThirtyTwo
	}

	public enum TextureFormat
	{
		R8G8B8A8,
		B8G8R8A8,
		R5G6B5,
		A1R5G5B5,
		B4G4R4A4,
		A2R10G10B10,
		R16G16,
		R16G16B16A16,
		R8,
		BC1,
		BC2,
		BC3,
		BC7,
		R8G8_SNORM,
		R8G8B8A8_SNORM,
		R16_SFLOAT,
		R16G16_SFLOAT,
		R16G16B16A16_SFLOAT,
		R32_SFLOAT,
		R32G32_SFLOAT,
		R32G32B32A32_SFLOAT,

		R8_UINT,
		R8G8_UINT,
		R8G8B8A8_UINT,
		R16_UINT,
		R16G16_UINT,
		R16G16B16A16_UINT,
		D16,
		D32,
		D16S8,
		D32S8
	}

	[Flags]
	public enum TextureUsageFlags : uint
	{
		Sampler = 1,
		ColorTarget = 2,
		DepthStencilTarget = 4,
		Compute = 8
	}

	public enum SampleCount
	{
		One,
		Two,
		Four,
		Eight
	}

	public enum CubeMapFace : uint
	{
		PositiveX,
		NegativeX,
		PositiveY,
		NegativeY,
		PositiveZ,
		NegativeZ
	}

	[Flags]
	public enum BufferUsageFlags : uint
	{
		Vertex = 1,
		Index = 2,
		Compute = 4,
		Indirect = 8
	}

	public enum VertexElementFormat
	{
		UInt,
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
	public enum ColorComponentFlags : uint
	{
		R = 1,
		G = 2,
		B = 4,
		A = 8,

		RG = R | G,
		RB = R | B,
		RA = R | A,
		GB = G | B,
		GA = G | A,
		BA = B | A,

		RGB = R | G | B,
		RGA = R | G | A,
		GBA = G | B | A,

		RGBA = R | G | B | A,
		None = 0
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

	public enum TransferOptions
	{
		Cycle,
		Unsafe
	}

	public enum WriteOptions
	{
		Cycle,
		Unsafe,
		Safe
	}

	public enum Backend
	{
		Vulkan,
		D3D11,
		PS5,
		Invalid
	}
}
