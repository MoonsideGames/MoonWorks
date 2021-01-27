using RefreshCS;

namespace MoonWorks.Graphics
{
    public struct TextureSlice
    {
        public Texture Texture { get; }
        public Rect Rectangle { get;  }
        public uint Depth { get; }
        public uint Layer { get; }
        public uint Level { get; }

        public TextureSlice(Texture texture)
        {
            Texture = texture;
            Rectangle = new Rect
            {
                x = 0,
                y = 0,
                w = (int) texture.Width,
                h = (int) texture.Height
            };
            Depth = 0;
            Layer = 0;
            Level = 0;
        }

        public TextureSlice(Texture texture, Rect rectangle, uint depth = 0, uint layer = 0, uint level = 0)
        {
            Texture = texture;
            Rectangle = rectangle;
            Depth = depth;
            Layer = layer;
            Level = level;
        }

        public Refresh.TextureSlice ToRefreshTextureSlice()
        {
            Refresh.TextureSlice textureSlice = new Refresh.TextureSlice
            {
                texture = Texture.Handle,
                rectangle = Rectangle.ToRefresh(),
                depth = Depth,
                layer = Layer,
                level = Level
            };

            return textureSlice;
        }
    }
}
