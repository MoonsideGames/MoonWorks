using System;
using System.Runtime.InteropServices;
using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics;

/// <summary>
/// A data container that can efficiently transfer data to and from the GPU.
/// </summary>
public class TransferBuffer : SDLGPUResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL.SDL_ReleaseGPUTransferBuffer;

	/// <summary>
	/// Size in bytes.
	/// </summary>
	public uint Size { get; private init; }

	private IntPtr MapPointer;

	/// <summary>
	/// Creates a buffer of requested size given a type and element count.
	/// </summary>
	/// <typeparam name="T">The type that the buffer will contain.</typeparam>
	/// <param name="device">The GraphicsDevice.</param>
	/// <param name="elementCount">How many elements of type T the buffer will contain.</param>
	/// <returns></returns>
	public static TransferBuffer Create<T>(
		GraphicsDevice device,
		TransferBufferUsage usage,
		uint elementCount
	) where T : unmanaged
	{
		return Create(device, new TransferBufferCreateInfo
		{
			Usage = usage,
			Size = (uint) (Marshal.SizeOf<T>() * elementCount),
			Props = 0
		});
	}

	public static TransferBuffer Create(
		GraphicsDevice device,
		in TransferBufferCreateInfo createInfo
	) {
		var handle = SDL.SDL_CreateGPUTransferBuffer(device.Handle, createInfo);
		if (handle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}
		return new TransferBuffer(device)
		{
			Handle = handle,
			Size = createInfo.Size
		};
	}

	private TransferBuffer(GraphicsDevice device) : base(device) { }

	/// <summary>
	/// Maps the transfer buffer into application address space.
	/// You must call Unmap before encoding transfer commands.
	/// </summary>
	public void Map(bool cycle)
	{
		if (MapPointer == IntPtr.Zero)
		{
			MapPointer = SDL.SDL_MapGPUTransferBuffer(
				Device.Handle,
				Handle,
				cycle
			);

			if (MapPointer == IntPtr.Zero)
			{
				Logger.LogError(SDL3.SDL.SDL_GetError());
			}
		}
	}

	/// <summary>
	/// Returns a Span of the mapped data. You can only access this after calling Map.
	/// </summary>
	public unsafe Span<T> MappedSpan<T>(uint offsetInBytes = 0) where T : unmanaged
	{
		if (MapPointer == IntPtr.Zero)
		{
			Logger.LogError("Cannot access MappedSpan if TransferBuffer is not mapped!");
			return Span<T>.Empty;
		}

		return new Span<T>((void*) (MapPointer + offsetInBytes), (int) ((Size - offsetInBytes) / Marshal.SizeOf<T>()));
	}

	/// <summary>
	/// Maps the transfer buffer into application address space and immediately returns a Span on the data.
	/// You must call Unmap before encoding transfer commands.
	/// </summary>
	public Span<T> Map<T>(bool cycle, uint offsetInBytes = 0) where T : unmanaged
	{
		Map(cycle);
		return MappedSpan<T>(offsetInBytes);
	}

	/// <summary>
	/// Unmaps the transfer buffer.
	/// </summary>
	public void Unmap()
	{
		SDL.SDL_UnmapGPUTransferBuffer(
			Device.Handle,
			Handle
		);
		MapPointer = IntPtr.Zero;
	}
}
