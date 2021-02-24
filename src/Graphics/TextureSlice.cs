using RefreshCS;

namespace MoonWorks.Graphics
{
    /// <summary>
    /// A texture slice specifies a subregion of a texture.
    /// Many operations can use texture slices in place of textures for the sake of convenience.
    /// </summary>
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
                X = 0,
                Y = 0,
                W = (int) texture.Width,
                H = (int) texture.Height
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
