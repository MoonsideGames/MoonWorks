using System;
using WellspringCS;

namespace MoonWorks.Graphics.Font
{
	public class TextBatch
	{
		private GraphicsDevice GraphicsDevice { get; }
		public IntPtr Handle { get; }

		public Buffer VertexBuffer { get; protected set; } = null;
		public Buffer IndexBuffer { get; protected set; } = null;
		public Texture Texture { get; protected set; }
		public uint PrimitiveCount { get; protected set; }

		public TextBatch(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice;
			Handle = Wellspring.Wellspring_CreateTextBatch();
		}

		public void Start(Packer packer)
		{
			Wellspring.Wellspring_StartTextBatch(Handle, packer.Handle);
			Texture = packer.Texture;
			PrimitiveCount = 0;
		}

		public unsafe void Draw(
			string text,
			float x,
			float y,
			float depth,
			Color color,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment verticalAlignment = VerticalAlignment.Baseline
		) {
			var byteCount = System.Text.Encoding.UTF8.GetByteCount(text);

			if (StringBytes.Length < byteCount)
			{
				System.Array.Resize(ref StringBytes, byteCount);
			}

			fixed (char* chars = text)
			fixed (byte* bytes = StringBytes)
			{
				System.Text.Encoding.UTF8.GetBytes(chars, text.Length, bytes, byteCount);

				var result = Wellspring.Wellspring_Draw(
					Handle,
					x,
					y,
					depth,
					new Wellspring.Color { R = color.R, G = color.G, B = color.B, A = color.A },
					(Wellspring.HorizontalAlignment) horizontalAlignment,
					(Wellspring.VerticalAlignment) verticalAlignment,
					(IntPtr) bytes,
					(uint) byteCount
				);

				if (result == 0)
				{
					throw new System.ArgumentException("Could not decode string!");
				}
			}
		}

		// Call this after you have made all the Draw calls you want.
		public unsafe void UploadBufferData(CommandBuffer commandBuffer)
		{
			Wellspring.Wellspring_GetBufferData(
				Handle,
				out IntPtr vertexDataPointer,
				out uint vertexDataLengthInBytes,
				out IntPtr indexDataPointer,
				out uint indexDataLengthInBytes
			);

			if (VertexBuffer == null)
			{
				VertexBuffer = new Buffer(GraphicsDevice, BufferUsageFlags.Vertex, vertexDataLengthInBytes);
			}
			else if (VertexBuffer.Size < vertexDataLengthInBytes)
			{
				VertexBuffer.Dispose();
				VertexBuffer = new Buffer(GraphicsDevice, BufferUsageFlags.Vertex, vertexDataLengthInBytes);
			}

			if (IndexBuffer == null)
			{
				IndexBuffer = new Buffer(GraphicsDevice, BufferUsageFlags.Index, indexDataLengthInBytes);
			}
			else if (IndexBuffer.Size < indexDataLengthInBytes)
			{
				IndexBuffer.Dispose();
				IndexBuffer = new Buffer(GraphicsDevice, BufferUsageFlags.Index, indexDataLengthInBytes);
			}

			commandBuffer.SetBufferData(VertexBuffer, vertexDataPointer, 0, vertexDataLengthInBytes);
			commandBuffer.SetBufferData(IndexBuffer, indexDataPointer, 0, indexDataLengthInBytes);
		}
	}
}
