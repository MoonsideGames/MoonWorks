using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics;

/// <summary>
/// A data container that can be efficiently used by the GPU.
/// </summary>
public class Buffer : RefreshResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => Refresh.Refresh_ReleaseBuffer;

	public BufferUsageFlags UsageFlags { get; }

	/// <summary>
	/// Size in bytes.
	/// </summary>
	public uint Size { get; }

	private string name;
	public string Name
	{
		get => name;

		set
		{
			if (Device.DebugMode)
			{
				Refresh.Refresh_SetBufferName(
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
	public unsafe static Buffer Create<T>(
		GraphicsDevice device,
		BufferUsageFlags usageFlags,
		uint elementCount
	) where T : unmanaged
	{
		return new Buffer(
			device,
			usageFlags,
			(uint) Marshal.SizeOf<T>() * elementCount
		);
	}

	/// <summary>
	/// Creates a buffer.
	/// </summary>
	/// <param name="device">An initialized GraphicsDevice.</param>
	/// <param name="usageFlags">Specifies how the buffer will be used.</param>
	/// <param name="sizeInBytes">The length of the array. Cannot be resized.</param>
	public Buffer(
		GraphicsDevice device,
		BufferUsageFlags usageFlags,
		uint sizeInBytes
	) : base(device)
	{
		Handle = Refresh.Refresh_CreateBuffer(
			device.Handle,
			(Refresh.BufferUsageFlags) usageFlags,
			sizeInBytes
		);
		UsageFlags = usageFlags;
		Size = sizeInBytes;
		name = "";
	}

	public static implicit operator BufferBinding(Buffer b)
	{
		return new BufferBinding(b, 0);
	}
}
