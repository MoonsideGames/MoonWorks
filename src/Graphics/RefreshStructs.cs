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
        public float depth;
        public uint stencil;

        // FIXME: can we do an unsafe cast somehow?
        public Refresh.DepthStencilValue ToRefresh()
        {
            return new Refresh.DepthStencilValue
            {
                depth = depth,
                stencil = stencil
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int x;
        public int y;
        public int w;
        public int h;

        // FIXME: can we do an unsafe cast somehow?
        public Refresh.Rect ToRefresh()
        {
            return new Refresh.Rect
            {
                x = x,
                y = y,
                w = w,
                h = h
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec4
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Viewport
    {
        public float x;
        public float y;
        public float w;
        public float h;
        public float minDepth;
        public float maxDepth;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexBinding
    {
        public uint binding;
        public uint stride;
        public VertexInputRate inputRate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexAttribute
    {
        public uint location;
        public uint binding;
        public VertexElementFormat format;
        public uint offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorTargetDescription
    {
        public ColorFormat format;
        public SampleCount multisampleCount;
        public LoadOp loadOp;
        public StoreOp storeOp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DepthStencilTargetDescription
    {
        public DepthFormat depthFormat;
        public LoadOp loadOp;
        public StoreOp storeOp;
        public LoadOp stencilLoadOp;
        public StoreOp stencilStoreOp;
    }
}
