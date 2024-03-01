using RefreshCS;
using System.Runtime.InteropServices;

/* Recreate some structs in here so we don't need to explicitly
 * reference the RefreshCS namespace when using MoonWorks.Graphics
 */
namespace MoonWorks.Graphics
{
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
				depth = Depth,
				stencil = Stencil
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
				x = X,
				y = Y,
				w = W,
				h = H
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
				x = X,
				y = Y,
				w = W,
				h = H,
				minDepth = MinDepth,
				maxDepth = MaxDepth
			};
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct VertexBinding
	{
		public uint Binding;
		public uint Stride;
		public VertexInputRate InputRate;

		public static VertexBinding Create<T>(uint binding = 0, VertexInputRate inputRate = VertexInputRate.Vertex) where T : unmanaged
		{
			return new VertexBinding
			{
				Binding = binding,
				InputRate = inputRate,
				Stride = (uint) Marshal.SizeOf<T>()
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
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct StencilOpState
	{
		public StencilOp FailOp;
		public StencilOp PassOp;
		public StencilOp DepthFailOp;
		public CompareOp CompareOp;
		public uint CompareMask;
		public uint WriteMask;
		public uint Reference;

		// FIXME: can we do an explicit cast here?
		public Refresh.StencilOpState ToRefresh()
		{
			return new Refresh.StencilOpState
			{
				failOp = (Refresh.StencilOp) FailOp,
				passOp = (Refresh.StencilOp) PassOp,
				depthFailOp = (Refresh.StencilOp) DepthFailOp,
				compareOp = (Refresh.CompareOp) CompareOp,
				compareMask = CompareMask,
				writeMask = WriteMask,
				reference = Reference
			};
		}
	}

	public struct ColorAttachmentInfo
	{
		public TextureSlice TextureSlice;
		public Color ClearColor;
		public LoadOp LoadOp;
		public StoreOp StoreOp;
		public bool SafeDiscard;

		public ColorAttachmentInfo(
			TextureSlice textureSlice,
			Color clearColor,
			bool safeDiscard = true,
			StoreOp storeOp = StoreOp.Store
		) {
			TextureSlice = textureSlice;
			ClearColor = clearColor;
			LoadOp = LoadOp.Clear;
			StoreOp = storeOp;
			SafeDiscard = safeDiscard;
		}

		public ColorAttachmentInfo(
			TextureSlice textureSlice,
			LoadOp loadOp = LoadOp.DontCare,
			StoreOp storeOp = StoreOp.Store
		) {
			TextureSlice = textureSlice;
			ClearColor = Color.White;
			LoadOp = loadOp;
			StoreOp = storeOp;
		}

		public Refresh.ColorAttachmentInfo ToRefresh()
		{
			return new Refresh.ColorAttachmentInfo
			{
				textureSlice = TextureSlice.ToRefreshTextureSlice(),
				clearColor = new Refresh.Vec4
				{
					x = ClearColor.R / 255f,
					y = ClearColor.G / 255f,
					z = ClearColor.B / 255f,
					w = ClearColor.A / 255f
				},
				loadOp = (Refresh.LoadOp) LoadOp,
				storeOp = (Refresh.StoreOp) StoreOp,
				safeDiscard = Conversions.BoolToByte(SafeDiscard)
			};
		}
	}

	public struct DepthStencilAttachmentInfo
	{
		public TextureSlice TextureSlice;
		public DepthStencilValue DepthStencilClearValue;
		public LoadOp LoadOp;
		public StoreOp StoreOp;
		public LoadOp StencilLoadOp;
		public StoreOp StencilStoreOp;
		public bool SafeDiscard;

		public DepthStencilAttachmentInfo(
			TextureSlice textureSlice,
			DepthStencilValue clearValue,
			bool safeDiscard = true,
			StoreOp depthStoreOp = StoreOp.Store,
			StoreOp stencilStoreOp = StoreOp.Store
		){
			TextureSlice = textureSlice;
			DepthStencilClearValue = clearValue;
			LoadOp = LoadOp.Clear;
			StoreOp = depthStoreOp;
			StencilLoadOp = LoadOp.Clear;
			StencilStoreOp = stencilStoreOp;
			SafeDiscard = safeDiscard;
		}

		public DepthStencilAttachmentInfo(
			TextureSlice textureSlice,
			LoadOp loadOp = LoadOp.DontCare,
			StoreOp storeOp = StoreOp.Store,
			LoadOp stencilLoadOp = LoadOp.DontCare,
			StoreOp stencilStoreOp = StoreOp.Store
		) {
			TextureSlice = textureSlice;
			DepthStencilClearValue = new DepthStencilValue();
			LoadOp = loadOp;
			StoreOp = storeOp;
			StencilLoadOp = stencilLoadOp;
			StencilStoreOp = stencilStoreOp;
		}

		public Refresh.DepthStencilAttachmentInfo ToRefresh()
		{
			return new Refresh.DepthStencilAttachmentInfo
			{
				textureSlice = TextureSlice.ToRefreshTextureSlice(),
				depthStencilClearValue = DepthStencilClearValue.ToRefresh(),
				loadOp = (Refresh.LoadOp) LoadOp,
				storeOp = (Refresh.StoreOp) StoreOp,
				stencilLoadOp = (Refresh.LoadOp) StencilLoadOp,
				stencilStoreOp = (Refresh.StoreOp) StencilStoreOp,
				safeDiscard = Conversions.BoolToByte(SafeDiscard)
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
				srcOffset = SrcOffset,
				dstOffset = DstOffset,
				size = Size
			};
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BufferImageCopy
	{
		public uint BufferOffset;
		public uint BufferStride; // if 0, image assumed to be tightly packed
		public uint BufferImageHeight; // if 0, image assumed to be tightly packed

		public BufferImageCopy(
			uint bufferOffset,
			uint bufferStride,
			uint bufferImageHeight
		) {
			BufferOffset = bufferOffset;
			BufferStride = bufferStride;
			BufferImageHeight = bufferImageHeight;
		}

		public Refresh.BufferImageCopy ToRefresh()
		{
			return new Refresh.BufferImageCopy
			{
				bufferOffset = BufferOffset,
				bufferStride = BufferStride,
				bufferImageHeight = BufferImageHeight
			};
		}
	}
}
