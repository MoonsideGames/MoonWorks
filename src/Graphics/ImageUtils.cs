using System;
using System.IO;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
	public static class ImageUtils
	{
		/// <summary>
		/// Gets pointer to pixel data from compressed image byte data.
		///
		/// The returned pointer must be freed by calling FreePixelData.
		/// </summary>
		public static unsafe IntPtr GetPixelDataFromBytes(
			Span<byte> data,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			fixed (byte* ptr = data)
			{
				var pixelData =
					Refresh.Refresh_Image_Load(
					(nint) ptr,
					data.Length,
					out var w,
					out var h,
					out var len
				);

				width = (uint) w;
				height = (uint) h;
				sizeInBytes = (uint) len;

				return pixelData;
			}
		}

		/// <summary>
		/// Gets pointer to pixel data from a compressed image stream.
		///
		/// The returned pointer must be freed by calling FreePixelData.
		/// </summary>
		public static unsafe IntPtr GetPixelDataFromStream(
			Stream stream,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			var length = stream.Length;
			var buffer = NativeMemory.Alloc((nuint) length);
			var span = new Span<byte>(buffer, (int) length);
			stream.ReadExactly(span);

			var pixelData = GetPixelDataFromBytes(span, out width, out height, out sizeInBytes);

			NativeMemory.Free(buffer);

			return pixelData;
		}

		/// <summary>
		/// Gets pointer to pixel data from a compressed image file.
		///
		/// The returned pointer must be freed by calling FreePixelData.
		/// </summary>
		public static IntPtr GetPixelDataFromFile(
			string path,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			return GetPixelDataFromStream(fileStream, out width, out height, out sizeInBytes);
		}

		/// <summary>
		/// Get metadata from compressed image bytes.
		/// </summary>
		public static unsafe bool ImageInfoFromBytes(
			Span<byte> data,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			fixed (byte* ptr = data)
			{
				var result =
					Refresh.Refresh_Image_Info(
					(nint) ptr,
					data.Length,
					out var w,
					out var h,
					out var len
				);

				width = (uint) w;
				height = (uint) h;
				sizeInBytes = (uint) len;

				return Conversions.ByteToBool(result);
			}
		}

		/// <summary>
		/// Get metadata from a compressed image stream.
		/// </summary>
		public static unsafe bool ImageInfoFromStream(
			Stream stream,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			var length = stream.Length;
			var buffer = NativeMemory.Alloc((nuint) length);
			var span = new Span<byte>(buffer, (int) length);
			stream.ReadExactly(span);

			var result = ImageInfoFromBytes(span, out width, out height, out sizeInBytes);

			NativeMemory.Free(buffer);

			return result;
		}

		/// <summary>
		/// Get metadata from a compressed image file.
		/// </summary>
		public static bool ImageInfoFromFile(
			string path,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			return ImageInfoFromStream(fileStream, out width, out height, out sizeInBytes);
		}

		/// <summary>
		/// Frees pixel data obtained from GetPixelData methods.
		/// </summary>
		public static void FreePixelData(IntPtr pixels)
		{
			Refresh.Refresh_Image_Free(pixels);
		}

		/// <summary>
		/// Decodes image data into a TransferBuffer to prepare for image upload.
		/// </summary>
		public static unsafe uint DecodeIntoTransferBuffer(
			Span<byte> data,
			TransferBuffer transferBuffer,
			uint bufferOffsetInBytes,
			SetDataOptions option
		) {
			var pixelData = GetPixelDataFromBytes(data, out var w, out var h, out var sizeInBytes);
			var length = transferBuffer.SetData(new Span<byte>((void*) pixelData, (int) sizeInBytes), bufferOffsetInBytes, option);
			FreePixelData(pixelData);
			return length;
		}

		/// <summary>
		/// Decodes an image stream into a TransferBuffer to prepare for image upload.
		/// </summary>
		public static unsafe uint DecodeIntoTransferBuffer(
			Stream stream,
			TransferBuffer transferBuffer,
			uint bufferOffsetInBytes,
			SetDataOptions option
		) {
			var pixelData = GetPixelDataFromStream(stream, out var w, out var h, out var sizeInBytes);
			var length = transferBuffer.SetData(new Span<byte>((void*) pixelData, (int) sizeInBytes), bufferOffsetInBytes, option);
			FreePixelData(pixelData);
			return length;
		}

		/// <summary>
		/// Decodes an image file into a TransferBuffer to prepare for image upload.
		/// </summary>
		public static unsafe uint DecodeIntoTransferBuffer(
			string path,
			TransferBuffer transferBuffer,
			uint bufferOffsetInBytes,
			SetDataOptions option
		) {
			var pixelData = GetPixelDataFromFile(path, out var w, out var h, out var sizeInBytes);
			var length = transferBuffer.SetData(new Span<byte>((void*) pixelData, (int) sizeInBytes), bufferOffsetInBytes, option);
			FreePixelData(pixelData);
			return length;
		}

		/// <summary>
		/// Saves pixel data contained in a TransferBuffer to a PNG file.
		/// </summary>
		public static unsafe void SavePNG(
			string path,
			TransferBuffer transferBuffer,
			uint bufferOffsetInBytes,
			int width,
			int height,
			bool bgra
		) {
			var sizeInBytes = width * height * 4;

			var pixelsPtr = NativeMemory.Alloc((nuint) sizeInBytes);
			var pixelsSpan = new Span<byte>(pixelsPtr, sizeInBytes);

			transferBuffer.GetData(pixelsSpan, bufferOffsetInBytes);

			if (bgra)
			{
				// if data is bgra, we have to swap the R and B channels
				var rgbaPtr = NativeMemory.Alloc((nuint) sizeInBytes);
				var rgbaSpan = new Span<byte>(rgbaPtr, sizeInBytes);

				for (var i = 0; i < sizeInBytes; i += 4)
				{
					rgbaSpan[i] = pixelsSpan[i + 2];
					rgbaSpan[i + 1] = pixelsSpan[i + 1];
					rgbaSpan[i + 2] = pixelsSpan[i];
					rgbaSpan[i + 3] = pixelsSpan[i + 3];
				}

				NativeMemory.Free(pixelsPtr);
				pixelsPtr = rgbaPtr;
			}

			Refresh.Refresh_Image_SavePNG(path, (nint) pixelsPtr, width, height);
			NativeMemory.Free(pixelsPtr);
		}
	}
}
