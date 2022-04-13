using System;
using System.IO;
using WellspringCS;

namespace MoonWorks.Graphics.Font
{
    public class Font : IDisposable
    {
		public IntPtr Handle { get; }

		private bool IsDisposed;

        public unsafe Font(string path)
        {
            var bytes = File.ReadAllBytes(path);
			fixed (byte* pByte = &bytes[0])
			{
				Handle = Wellspring.Wellspring_CreateFont((IntPtr) pByte, (uint) bytes.Length);
			}
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
