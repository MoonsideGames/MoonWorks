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
		public RenderTarget renderTarget;
		public Color clearColor;
		public LoadOp loadOp;
		public StoreOp storeOp;

		public Refresh.ColorAttachmentInfo ToRefresh()
		{
			return new Refresh.ColorAttachmentInfo
			{
				renderTarget = renderTarget.Handle,
				clearColor = new Refresh.Vec4
				{
					x = clearColor.R,
					y = clearColor.G,
					z = clearColor.B,
					w = clearColor.A
				},
				loadOp = (Refresh.LoadOp) loadOp,
				storeOp = (Refresh.StoreOp) storeOp
			};
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DepthStencilAttachmentInfo
	{
		public RenderTarget depthStencilTarget;
		public DepthStencilValue depthStencilValue;
		public LoadOp loadOp;
		public StoreOp storeOp;
		public LoadOp stencilLoadOp;
		public StoreOp stencilStoreOp;

		public Refresh.DepthStencilAttachmentInfo ToRefresh()
		{
			return new Refresh.DepthStencilAttachmentInfo
			{
				depthStencilTarget = depthStencilTarget.Handle,
				depthStencilValue = depthStencilValue.ToRefresh(),
				loadOp = (Refresh.LoadOp) loadOp,
				storeOp = (Refresh.StoreOp) storeOp,
				stencilLoadOp = (Refresh.LoadOp) stencilLoadOp,
				stencilStoreOp = (Refresh.StoreOp) stencilStoreOp
			};
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ColorAttachmentDescription
	{
		public TextureFormat format;
		public SampleCount sampleCount;
	}
}
