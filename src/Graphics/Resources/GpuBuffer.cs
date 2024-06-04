using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// GpuBuffers are generic data containers that can be used by the GPU.
	/// </summary>
	public class GpuBuffer : SDL_GpuResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => Refresh.Refresh_QueueDestroyGpuBuffer;

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
					Refresh.Refresh_SetGpuBufferName(
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
		public unsafe static GpuBuffer Create<T>(
			GraphicsDevice device,
			BufferUsageFlags usageFlags,
			uint elementCount
		) where T : unmanaged
		{
			return new GpuBuffer(
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
		public GpuBuffer(
			GraphicsDevice device,
			BufferUsageFlags usageFlags,
			uint sizeInBytes
		) : base(device)
		{
			Handle = Refresh.Refresh_CreateGpuBuffer(
				device.Handle,
				(Refresh.BufferUsageFlags) usageFlags,
				sizeInBytes
			);
			Size = sizeInBytes;
			name = "";
		}

		public static implicit operator BufferBinding(GpuBuffer b)
		{
			return new BufferBinding(b, 0);
		}
	}
}
