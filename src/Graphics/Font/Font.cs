using System;
using System.IO;
using System.Runtime.InteropServices;
using WellspringCS;

namespace MoonWorks.Graphics.Font
{
    public class Font : IDisposable
    {
		public IntPtr Handle { get; }

		private bool IsDisposed;

        public unsafe Font(string path)
        {
	        var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
	        var fileByteBuffer = NativeMemory.Alloc((nuint) fileStream.Length);
	        var fileByteSpan = new Span<byte>(fileByteBuffer, (int) fileStream.Length);
	        fileStream.ReadExactly(fileByteSpan);
	        fileStream.Close();

	        Handle = Wellspring.Wellspring_CreateFont((IntPtr) fileByteBuffer, (uint) fileByteSpan.Length);

	        NativeMemory.Free(fileByteBuffer);
        }

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Wellspring.Wellspring_DestroyFont(Handle);
				IsDisposed = true;
			}
		}

		~Font()
		{
		    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		    Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
