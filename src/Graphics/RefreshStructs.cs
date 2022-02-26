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
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct VertexBinding
	{
		public uint Binding;
		public uint Stride;
		public VertexInputRate InputRate;
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

	[StructLayout(LayoutKind.Sequential)]
	public struct ColorAttachmentInfo
	{
		public RenderTarget RenderTarget;
		public Color ClearColor;
		public LoadOp LoadOp;
		public StoreOp StoreOp;

		public Refresh.ColorAttachmentInfo ToRefresh()
		{
			return new Refresh.ColorAttachmentInfo
			{
				renderTarget = RenderTarget.Handle,
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
		public RenderTarget DepthStencilTarget;
		public DepthStencilValue DepthStencilValue;
		public LoadOp LoadOp;
		public StoreOp StoreOp;
		public LoadOp StencilLoadOp;
		public StoreOp StencilStoreOp;

		public Refresh.DepthStencilAttachmentInfo ToRefresh()
		{
			return new Refresh.DepthStencilAttachmentInfo
			{
				depthStencilTarget = DepthStencilTarget.Handle,
				depthStencilValue = DepthStencilValue.ToRefresh(),
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
	}
}
