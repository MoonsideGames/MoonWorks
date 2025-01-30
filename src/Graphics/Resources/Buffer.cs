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

	/// <summary>
	/// Gets the flags indicating the intended use of the GPU Buffer.
	/// </summary>
	public BufferUsageFlags UsageFlags { get; private init;  }

	/// <summary>
	/// Gets the buffer size in bytes.
	/// </summary>
	public uint Size { get; private init; }

	/// <summary>
	/// Creates a named buffer of appropriate size given a type and element count.
	/// </summary>
	/// <typeparam name="T">The type that the buffer will contain.</typeparam>
	/// <param name="device">The GraphicsDevice.</param>
	/// <param name="usageFlags">Specifies how the buffer will be used.</param>
	/// <param name="elementCount">How many elements of type T the buffer will contain.</param>
	/// <returns>A newly created <see cref="Buffer"/> instance.</returns>
	public static Buffer Create<T>(
		GraphicsDevice device,
		string name,
		BufferUsageFlags usageFlags,
		uint elementCount
	) where T : unmanaged
	{
		var props = SDL3.SDL.SDL_CreateProperties();
		SDL3.SDL.SDL_SetStringProperty(props, SDL3.SDL.SDL_PROP_GPU_BUFFER_CREATE_NAME_STRING, name);

		var result = Create(device, new BufferCreateInfo
		{
			Usage = usageFlags,
			Size = (uint) Marshal.SizeOf<T>() * elementCount,
			Props = props
		});

		SDL3.SDL.SDL_DestroyProperties(props);

		return result;
	}

	/// <summary>
	/// Creates a buffer of appropriate size given a type and element count.
	/// </summary>
	/// <typeparam name="T">The type that the buffer will contain.</typeparam>
	/// <param name="device">The GraphicsDevice.</param>
	/// <param name="usageFlags">Specifies how the buffer will be used.</param>
	/// <param name="elementCount">How many elements of type T the buffer will contain.</param>
	/// <returns>A newly created <see cref="Buffer"/> instance.</returns>
	public static Buffer Create<T>(
		GraphicsDevice device,
		BufferUsageFlags usageFlags,
		uint elementCount
	) where T : unmanaged => Create<T>(device, null, usageFlags, elementCount);

	/// <summary>
	/// Creates a buffer given a <see cref="BufferCreateInfo"/> struct.
	/// </summary>
	/// <param name="device">The graphics device to allocate the buffer on.</param>
	/// <param name="createInfo">Parameter data used to create the buffer.</param>
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
			UsageFlags = createInfo.Usage,
			Name = SDL3.SDL.SDL_GetStringProperty(createInfo.Props, SDL3.SDL.SDL_PROP_GPU_BUFFER_CREATE_NAME_STRING, "Buffer")
		};
	}

	private Buffer(GraphicsDevice device) : base(device) { }

	/// <summary>
	/// An implicition conversion of <see cref="Buffer"/> to <see cref="BufferBinding"/>.
	/// <see cref="BufferBinding.Buffer"/> is set to the value of the buffer's handle.
	/// </summary>
	/// <param name="b">The <see cref="Buffer"/> instance to implicitly convert into </param>
	public static implicit operator BufferBinding(Buffer b)
	{
		return new BufferBinding
		{
			Buffer = b.Handle,
			Offset = 0
		};
	}
}
