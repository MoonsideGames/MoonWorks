using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using MoonWorks.Graphics;
using MoonWorks.Storage;

namespace MoonWorks.Video
{
	// Note that all public methods are async.
	internal class VideoAV1Stream : GraphicsResource
	{
		public IntPtr Handle => handle;
		IntPtr handle;

		private IntPtr ByteBuffer;

		public bool Loaded => handle != IntPtr.Zero;
		public bool Ended => Handle == IntPtr.Zero || Dav1dfile.df_eos(Handle) == 1;

		public IntPtr yDataHandle;
		public IntPtr uDataHandle;
		public IntPtr vDataHandle;
		public uint yDataLength;
		public uint uvDataLength;
		public uint yStride;
		public uint uvStride;

		public bool FrameDataUpdated { get; set; }

		private BlockingCollection<Action> Actions = new BlockingCollection<Action>();

		private bool Running = false;

		Thread Thread;

		public VideoAV1Stream(GraphicsDevice device) : base(device)
		{
			handle = IntPtr.Zero;
			Name = "VideoAV1Stream";

			Running = true;

			Thread = new Thread(ThreadMain);
			Thread.Start();
		}

		private void ThreadMain()
		{
			while (Running)
			{
				// block until we can take an action, then run it
				var action = Actions.Take();
				action.Invoke();
			}

			// shutting down...
			while (Actions.TryTake(out var action))
			{
				action.Invoke();
			}
		}

		public void Load(TitleStorage storage, string filename)
		{
			Actions.Add(() => LoadHelper(storage, filename));
		}

		public void Reset()
		{
			Actions.Add(ResetHelper);
		}

		public void ReadNextFrame()
		{
			Actions.Add(ReadNextFrameHelper);
		}

		public void Unload()
		{
			Actions.Add(UnloadHelper);
		}

		private unsafe void LoadHelper(TitleStorage storage, string path)
		{
			if (!Loaded)
			{
				if (!storage.GetFileSize(path, out var size))
				{
					return;
				}

				ByteBuffer = (nint) NativeMemory.Alloc((nuint) size);
				var span = new Span<byte>((void*) ByteBuffer, (int) size);
				if (!storage.ReadFile(path, span))
				{
					return;
				}

				if (Dav1dfile.df_open_from_memory(ByteBuffer, (uint) size, out handle) == 0)
				{
					Logger.LogError("Failed to load video file: " + path);
					throw new Exception("Failed to load video file!");
				}

				Reset();
			}
		}

		private void ResetHelper()
		{
			if (Loaded)
			{
				Dav1dfile.df_reset(handle);
				ReadNextFrame();
			}
		}

		private void ReadNextFrameHelper()
		{
			if (Loaded && !Ended)
			{
				lock (this)
				{
					if (Dav1dfile.df_readvideo(
						handle,
						1,
						out var yDataHandle,
						out var uDataHandle,
						out var vDataHandle,
						out var yDataLength,
						out var uvDataLength,
						out var yStride,
						out var uvStride) == 1
					) {
						this.yDataHandle = yDataHandle;
						this.uDataHandle = uDataHandle;
						this.vDataHandle = vDataHandle;
						this.yDataLength = yDataLength;
						this.uvDataLength = uvDataLength;
						this.yStride = yStride;
						this.uvStride = uvStride;

						FrameDataUpdated = true;
					}
				}
			}
		}

		private unsafe void UnloadHelper()
		{
			if (Loaded)
			{
				Dav1dfile.df_close(handle);
				handle = IntPtr.Zero;
				NativeMemory.Free((void*) ByteBuffer);
				ByteBuffer = IntPtr.Zero;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Unload();
				Running = false;

				if (disposing)
				{
					Thread.Join();
				}
			}
			base.Dispose(disposing);
		}
	}
}
