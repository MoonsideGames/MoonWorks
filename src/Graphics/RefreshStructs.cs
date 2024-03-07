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

		public Refresh.StencilOpState ToRefresh()
		{
			return new Refresh.StencilOpState
			{
				failOp = (Refresh.StencilOp) FailOp,
				passOp = (Refresh.StencilOp) PassOp,
				depthFailOp = (Refresh.StencilOp) DepthFailOp,
				compareOp = (Refresh.CompareOp) CompareOp
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
		/// Specifies data dependency behavior. This option is ignored if LoadOp is Load. <br/>
		///
		///   Cycle:
		///     If this texture slice has been used in commands that have not completed,
		///     the implementation may prevent a dependency on those commands
		///     at the cost of increased memory usage.
		///     You may NOT assume that any of the previous texture (not slice!) data is retained.
		///     This may prevent stalls when frequently reusing a texture slice in rendering. <br/>
		///
		///   SafeOverwrite:
		///     Overwrites the data safely using a GPU memory barrier.
		/// </summary>
		public WriteOptions WriteOption;

		public ColorAttachmentInfo(
			TextureSlice textureSlice,
			WriteOptions writeOption,
			Color clearColor,
			StoreOp storeOp = StoreOp.Store
		) {
			TextureSlice = textureSlice;
			ClearColor = clearColor;
			LoadOp = LoadOp.Clear;
			StoreOp = storeOp;
			WriteOption = writeOption;
		}

		public ColorAttachmentInfo(
			TextureSlice textureSlice,
			WriteOptions writeOption,
			LoadOp loadOp = LoadOp.DontCare,
			StoreOp storeOp = StoreOp.Store
		) {
			TextureSlice = textureSlice;
			ClearColor = Color.White;
			LoadOp = loadOp;
			StoreOp = storeOp;
			WriteOption = writeOption;
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
				writeOption = (Refresh.WriteOptions) WriteOption
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
		/// Specifies data dependency behavior. This option is ignored if LoadOp or StencilLoadOp is Load. <br/>
		///
		///   Cycle:
		///     If this texture slice has been used in commands that have not completed,
		///     the implementation may prevent a dependency on those commands
		///     at the cost of increased memory usage.
		///     You may NOT assume that any of the previous texture (not slice!) data is retained.
		///     This may prevent stalls when frequently reusing a texture slice in rendering. <br/>
		///
		///   SafeOverwrite:
		///     Overwrites the data safely using a GPU memory barrier.
		/// </summary>
		public WriteOptions WriteOption;

		public DepthStencilAttachmentInfo(
			TextureSlice textureSlice,
			WriteOptions writeOption,
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
			WriteOption = writeOption;
		}

		public DepthStencilAttachmentInfo(
			TextureSlice textureSlice,
			WriteOptions writeOption,
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
			WriteOption = writeOption;
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
				writeOption = (Refresh.WriteOptions) WriteOption
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
				bufferOffset = BufferOffset,
				bufferStride = BufferStride,
				bufferImageHeight = BufferImageHeight
			};
		}
	}
}
