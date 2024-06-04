using SDL2_gpuCS;
namespace MoonWorks.Graphics;

/// <summary>
/// A buffer-offset pair to be used when binding buffers.
/// </summary>
public readonly record struct BufferBinding(
	GpuBuffer Buffer,
	uint Offset
) {
	public SDL_Gpu.BufferBinding ToSDL()
	{
		return new SDL_Gpu.BufferBinding
		{
			Buffer = Buffer.Handle,
			Offset = Offset
		};
	}
}
