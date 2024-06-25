using System;
using System.Runtime.InteropServices;
using WellspringCS;

namespace MoonWorks.Graphics.Font
{
	public unsafe class TextBatch : GraphicsResource
	{
		public const int INITIAL_CHAR_COUNT = 64;
		public const int INITIAL_VERTEX_COUNT = INITIAL_CHAR_COUNT * 4;
		public const int INITIAL_INDEX_COUNT = INITIAL_CHAR_COUNT * 6;

		private GraphicsDevice GraphicsDevice { get; }
		public IntPtr Handle { get; }

		public Buffer VertexBuffer { get; protected set; } = null;
		public Buffer IndexBuffer { get; protected set; } = null;
		public uint PrimitiveCount { get; protected set; }

		private TransferBuffer TransferBuffer;

		public Font CurrentFont { get; private set; }

		private byte* StringBytes;
		private int StringBytesLength;

		public TextBatch(GraphicsDevice device) : base(device)
		{
			GraphicsDevice = device;
			Handle = Wellspring.Wellspring_CreateTextBatch();

			StringBytesLength = 128;
			StringBytes = (byte*) NativeMemory.Alloc((nuint) StringBytesLength);

			VertexBuffer = Buffer.Create<Vertex>(GraphicsDevice, BufferUsageFlags.Vertex, INITIAL_VERTEX_COUNT);
			IndexBuffer = Buffer.Create<uint>(GraphicsDevice, BufferUsageFlags.Index, INITIAL_INDEX_COUNT);

			TransferBuffer = TransferBuffer.Create<byte>(GraphicsDevice, TransferBufferUsage.Upload, VertexBuffer.Size + IndexBuffer.Size);
		}

		// Call this to initialize or reset the batch.
		public void Start(Font font)
		{
			Wellspring.Wellspring_StartTextBatch(Handle, font.Handle);
			CurrentFont = font;
			PrimitiveCount = 0;
		}

		// Add text with size and color to the batch
		public unsafe bool Add(
			string text,
			int pixelSize,
			Color color,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment verticalAlignment = VerticalAlignment.Baseline
		) {
			var byteCount = System.Text.Encoding.UTF8.GetByteCount(text);

			if (StringBytesLength < byteCount)
			{
				StringBytes = (byte*) NativeMemory.Realloc(StringBytes, (nuint) byteCount);
			}

			fixed (char* chars = text)
			{
				System.Text.Encoding.UTF8.GetBytes(chars, text.Length, StringBytes, byteCount);

				var result = Wellspring.Wellspring_AddToTextBatch(
					Handle,
					pixelSize,
					new Wellspring.Color { R = color.R, G = color.G, B = color.B, A = color.A },
					(Wellspring.HorizontalAlignment) horizontalAlignment,
					(Wellspring.VerticalAlignment) verticalAlignment,
					(IntPtr) StringBytes,
					(uint) byteCount
				);

				if (result == 0)
				{
					Logger.LogWarn("Could not decode string: " + text);
					return false;
				}
			}

			return true;
		}

		// Call this after you have made all the Add calls you want, but before beginning a render pass.
		public unsafe void UploadBufferData(CommandBuffer commandBuffer)
		{
			Wellspring.Wellspring_GetBufferData(
				Handle,
				out uint vertexCount,
				out IntPtr vertexDataPointer,
				out uint vertexDataLengthInBytes,
				out IntPtr indexDataPointer,
				out uint indexDataLengthInBytes
			);

			var vertexSpan = new Span<byte>((void*) vertexDataPointer, (int) vertexDataLengthInBytes);
			var indexSpan = new Span<byte>((void*) indexDataPointer, (int) indexDataLengthInBytes);

			var newTransferBufferNeeded = false;

			if (VertexBuffer.Size < vertexDataLengthInBytes)
			{
				VertexBuffer.Dispose();
				VertexBuffer = new Buffer(GraphicsDevice, BufferUsageFlags.Vertex, vertexDataLengthInBytes);
				newTransferBufferNeeded = true;
			}

			if (IndexBuffer.Size < indexDataLengthInBytes)
			{
				IndexBuffer.Dispose();
				IndexBuffer = new Buffer(GraphicsDevice, BufferUsageFlags.Index, vertexDataLengthInBytes);
				newTransferBufferNeeded = true;
			}

			if (newTransferBufferNeeded)
			{
				TransferBuffer.Dispose();
				TransferBuffer = new TransferBuffer(GraphicsDevice, TransferBufferUsage.Upload, VertexBuffer.Size + IndexBuffer.Size);
			}

			if (vertexDataLengthInBytes > 0 && indexDataLengthInBytes > 0)
			{
				TransferBuffer.SetData(vertexSpan, true);
				TransferBuffer.SetData(indexSpan, (uint) vertexSpan.Length, false);

				var copyPass = commandBuffer.BeginCopyPass();
				copyPass.UploadToBuffer(
					new TransferBufferLocation(TransferBuffer),
					new BufferRegion(VertexBuffer, 0, (uint) vertexSpan.Length),
					true
				);
				copyPass.UploadToBuffer(
					new TransferBufferLocation(TransferBuffer, (uint) vertexSpan.Length),
					new BufferRegion(IndexBuffer, 0, (uint) indexSpan.Length),
					true
				);
				commandBuffer.EndCopyPass(copyPass);
			}

			PrimitiveCount = vertexCount / 2;
		}

		// Call this AFTER binding your text pipeline!
		public void Render(
			CommandBuffer commandBuffer,
			RenderPass renderPass,
			Math.Float.Matrix4x4 transformMatrix
		) {
			commandBuffer.PushVertexUniformData(transformMatrix);
			commandBuffer.PushFragmentUniformData(CurrentFont.DistanceRange);

			renderPass.BindFragmentSampler(new TextureSamplerBinding(
				CurrentFont.Texture,
				GraphicsDevice.LinearSampler
			));
			renderPass.BindVertexBuffer(VertexBuffer);
			renderPass.BindIndexBuffer(IndexBuffer, IndexElementSize.ThirtyTwo);

			renderPass.DrawIndexedPrimitives(
				0,
				0,
				PrimitiveCount
			);
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					VertexBuffer.Dispose();
					IndexBuffer.Dispose();
				}

				NativeMemory.Free(StringBytes);
				Wellspring.Wellspring_DestroyTextBatch(Handle);
			}
			base.Dispose(disposing);
		}
	}
}
