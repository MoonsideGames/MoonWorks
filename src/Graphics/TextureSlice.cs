﻿using RefreshCS;
namespace MoonWorks.Graphics;

/// <summary>
/// A texture slice specifies a subresource of a texture.
/// </summary>
public struct TextureSlice
{
	public Texture Texture;
	public uint MipLevel;
	public uint Layer;

	public uint Size => (Texture.Width * Texture.Height * Texture.Depth * Texture.BytesPerPixel(Texture.Format) / Texture.BlockSizeSquared(Texture.Format)) >> (int) MipLevel;

	public TextureSlice(Texture texture)
	{
		Texture = texture;
		MipLevel = 0;
		Layer = 0;
	}

	public Refresh.TextureSlice ToRefresh()
	{
		return new Refresh.TextureSlice
		{
			Texture = Texture.Handle,
			MipLevel = MipLevel,
			Layer = Layer
		};
	}
}
