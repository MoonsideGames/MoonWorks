using System;
using System.Runtime.InteropServices;
using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics;

/// <summary>
/// A data container that can be efficiently used by the GPU.
/// </summary>
public class Buffer : SDLGPUResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL.SDL_ReleaseGPUBuffer;

	public BufferUsageFlags UsageFlags { get; private init;  }

	/// <summary>
	/// Size in bytes.
	/// </summary>
	public uint Size { get; private init; }

	private string name = "Buffer";
	public override string Name
	{
		get => name;

		set
		{
			if (Device.DebugMode)
			{
				SDL.SDL_SetGPUBufferName(
					Device.Handle,
					Handle,
					value
				);
			}

			name = value;
		}
	}

	/// <summary>
	/// Creates a buffer of appropriate size given a type and element count.
	/// </summary>
	/// <typeparam name="T">The type that the buffer will contain.</typeparam>
	/// <param name="device">The GraphicsDevice.</param>
	/// <param name="usageFlags">Specifies how the buffer will be used.</param>
	/// <param name="elementCount">How many elements of type T the buffer will contain.</param>
	/// <returns></returns>
	public static Buffer Create<T>(
		GraphicsDevice device,
		BufferUsageFlags usageFlags,
		uint elementCount
	) where T : unmanaged
	{
		return Create(device, new BufferCreateInfo
		{
			Usage = usageFlags,
			Size = (uint) Marshal.SizeOf<T>() * elementCount
		});
	}

	public static Buffer Create(
		GraphicsDevice device,
		in BufferCreateInfo createInfo
	) {
		var handle = SDL.SDL_CreateGPUBuffer(device.Handle, createInfo);
		if (handle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}
		return new Buffer(device)
		{
			Handle = handle,
			Size = createInfo.Size,
			UsageFlags = createInfo.Usage
		};
	}

	private Buffer(GraphicsDevice device) : base(device) { }

	public static implicit operator BufferBinding(Buffer b)
	{
		return new BufferBinding
		{
			Buffer = b.Handle,
			Offset = 0
		};
	}
}
