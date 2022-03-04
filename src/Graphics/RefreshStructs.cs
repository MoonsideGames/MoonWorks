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

		// Shortcut for the common case of having a single vertex binding.
		public static VertexBinding Create<T>()
		{
			return new VertexBinding
			{
				Binding = 0,
				InputRate = VertexInputRate.Vertex,
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

		public static VertexAttribute Create<T>(
			string fieldName,
			uint location,
			uint binding = 0
		)
		{
			var fieldInfo = typeof(T).GetField(fieldName);

			if (fieldInfo == null)
			{
				throw new System.ArgumentException("Field not recognized!");
			}

			return new VertexAttribute
			{
				Binding = binding,
				Location = location,
				Format = Conversions.TypeToVertexElementFormat(fieldInfo.FieldType),
				Offset = (uint) Marshal.OffsetOf<T>(fieldName)
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

	[StructLayout(LayoutKind.Sequential)]
	public struct ColorAttachmentInfo
	{
		public Texture Texture;
		public uint Depth;
		public uint Layer;
		public uint Level;
		public SampleCount SampleCount;
		public Color ClearColor;
		public LoadOp LoadOp;
		public StoreOp StoreOp;

		public ColorAttachmentInfo(Texture texture, Color clearColor, StoreOp storeOp = StoreOp.Store)
		{
			Texture = texture;
			Depth = 0;
			Layer = 0;
			Level = 0;
			SampleCount = SampleCount.One;
			ClearColor = clearColor;
			LoadOp = LoadOp.Clear;
			StoreOp = storeOp;
		}

		public ColorAttachmentInfo(Texture texture, LoadOp loadOp = LoadOp.DontCare, StoreOp storeOp = StoreOp.Store)
		{
			Texture = texture;
			Depth = 0;
			Layer = 0;
			Level = 0;
			SampleCount = SampleCount.One;
			ClearColor = Color.White;
			LoadOp = loadOp;
			StoreOp = storeOp;
		}

		public Refresh.ColorAttachmentInfo ToRefresh()
		{
			return new Refresh.ColorAttachmentInfo
			{
				texture = Texture.Handle,
				depth = Depth,
				layer = Layer,
				level = Level,
				sampleCount = (Refresh.SampleCount) SampleCount,
				clearColor = new Refresh.Vec4
				{
					x = ClearColor.R / 255f,
					y = ClearColor.G / 255f,
					z = ClearColor.B / 255f,
					w = ClearColor.A / 255f
				},
				loadOp = (Refresh.LoadOp) LoadOp,
				storeOp = (Refresh.StoreOp) StoreOp
			};
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DepthStencilAttachmentInfo
	{
		public Texture Texture;
		public uint Depth;
		public uint Layer;
		public uint Level;
		public DepthStencilValue DepthStencilClearValue;
		public LoadOp LoadOp;
		public StoreOp StoreOp;
		public LoadOp StencilLoadOp;
		public StoreOp StencilStoreOp;

		public DepthStencilAttachmentInfo(Texture texture, DepthStencilValue clearValue)
		{
			Texture = texture;
			Depth = 0;
			Layer = 0;
			Level = 0;
			DepthStencilClearValue = clearValue;
			LoadOp = LoadOp.Clear;
			StoreOp = StoreOp.DontCare;
			StencilLoadOp = LoadOp.DontCare;
			StencilStoreOp = StoreOp.DontCare;
		}

		public Refresh.DepthStencilAttachmentInfo ToRefresh()
		{
			return new Refresh.DepthStencilAttachmentInfo
			{
				texture = Texture.Handle,
				depth = Depth,
				layer = Layer,
				level = Level,
				depthStencilClearValue = DepthStencilClearValue.ToRefresh(),
				loadOp = (Refresh.LoadOp) LoadOp,
				storeOp = (Refresh.StoreOp) StoreOp,
				stencilLoadOp = (Refresh.LoadOp) StencilLoadOp,
				stencilStoreOp = (Refresh.StoreOp) StencilStoreOp
			};
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ColorAttachmentDescription
	{
		public TextureFormat Format;
		public SampleCount SampleCount;
		public ColorAttachmentBlendState BlendState;

		public ColorAttachmentDescription(
			TextureFormat format,
			ColorAttachmentBlendState blendState,
			SampleCount sampleCount = SampleCount.One
		) {
			Format = format;
			SampleCount = sampleCount;
			BlendState = blendState;
		}
	}
}
