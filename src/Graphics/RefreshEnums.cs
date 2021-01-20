using System;

/* Recreate all the enums in here so we don't need to explicitly
 * reference the RefreshCS namespace when using MoonWorks.Graphics
 */
namespace MoonWorks.Graphics
{
    public enum PresentMode
    {
        Immediate,
        Mailbox,
        FIFO,
        FIFORelaxed
    }

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

    public enum ColorFormat
    {
        R8G8B8A8,
        R5G6B5,
        A1R5G5B5,
        B4G4R4A4,
        BC1,
        BC2,
        BC3,
        R8G8_SNORM,
        R8G8B8A8_SNORM,
        A2R10G10B10,
        R16G16,
        R16G16B16A16,
        R8,
        R32_SFLOAT,
        R32G32_SFLOAT,
        R32G32B32A32_SFLOAT,
        R16_SFLOAT,
        R16G16_SFLOAT,
        R16G16B16A16_SFLOAT
    }

    public enum DepthFormat
    {
        Depth16,
        Depth32,
        Depth16Stencil8,
        Depth32Stencil8
    }

    [Flags]
    public enum TextureUsageFlags : uint
    {
        SamplerBit = 1,
        ColorTargetBit = 2
    }

    public enum SampleCount
    {
        One,
        Two,
        Four,
        Eight,
        Sixteen,
        ThirtyTwo,
        SixtyFour
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
    public enum BufferUsageFlags : uint
    {
        Vertex = 1,
        Index = 2,
        Compute = 4
    }

    public enum VertexElementFormat
    {
        Single,
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
        Line,
        Point
    }

    public enum CullMode
    {
        None,
        Front,
        Back,
        FrontAndBack
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

    public enum LogicOp
    {
        Clear,
        And,
        AndReverse,
        Copy,
        AndInverted,
        NoOp,
        Xor,
        Or,
        Nor,
        Equivalent,
        Invert,
        OrReverse,
        CopyInverted,
        OrInverted,
        Nand,
        Set
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
        ConstantAlpha,
        OneMinusConstantAlpha,
        SourceAlphaSaturate,
        SourceOneColor,
        OneMinusSourceOneColor,
        SourceOneAlpha,
        OneMinusSourceOneAlpha
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

        RGBA = R | G | B | A
    }

    public enum ShaderStageType
    {
        Vertex,
        Fragment
    }

    public enum Filter
    {
        Nearest,
        Linear,
        Cubic
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
}
