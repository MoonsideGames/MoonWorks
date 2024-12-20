using System;
using System.Numerics;
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
		public Buffer InstanceBuffer { get; protected set; } = null;
		public uint PrimitiveCount { get; protected set; }
		private int InstanceIndex = 0;

		private TransferBuffer VertexTransferBuffer;
		private TransferBuffer InstanceTransferBuffer;

		public Font CurrentFont { get; private set; }

		private byte* StringBytes;
		private int StringBytesLength;

		public TextBatch(GraphicsDevice device) : base(device)
		{
			GraphicsDevice = device;
			Handle = Wellspring.Wellspring_CreateTextBatch();
			Name = "TextBatch";

			StringBytesLength = 128;
			StringBytes = (byte*) NativeMemory.Alloc((nuint) StringBytesLength);

			VertexBuffer = Buffer.Create<Vertex>(GraphicsDevice, BufferUsageFlags.Vertex | BufferUsageFlags.ComputeStorageWrite, INITIAL_VERTEX_COUNT);
			IndexBuffer = Buffer.Create<uint>(GraphicsDevice, BufferUsageFlags.Index, INITIAL_INDEX_COUNT);
			InstanceBuffer = Buffer.Create<ComputeInstanceData>(GraphicsDevice, BufferUsageFlags.ComputeStorageRead, INITIAL_CHAR_COUNT);

			VertexTransferBuffer = TransferBuffer.Create<byte>(GraphicsDevice, TransferBufferUsage.Upload, VertexBuffer.Size + IndexBuffer.Size);
			VertexTransferBuffer.Name = "TextBatch VertexTransferBuffer";

			InstanceTransferBuffer = TransferBuffer.Create<ComputeInstanceData>(GraphicsDevice, TransferBufferUsage.Upload, INITIAL_CHAR_COUNT);
			InstanceTransferBuffer.Name = "TextBatch InstanceTransferBuffer";
		}

		// Call this to initialize or reset the batch.
		public void Start(Font font)
		{
			Wellspring.Wellspring_StartTextBatch(Handle, font.Handle);
			CurrentFont = font;
			PrimitiveCount = 0;
			InstanceIndex = 0;
			InstanceTransferBuffer.Map(true);
		}

		// Add text with size and color to the batch
		public unsafe bool Add(
			string text,
			int pixelSize,
			Vector3 position,
			Vector3 rotation,
			Vector2 scale,
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

			var instanceDatas = InstanceTransferBuffer.MappedSpan<ComputeInstanceData>();
			instanceDatas[InstanceIndex].Translation = position;
			instanceDatas[InstanceIndex].Rotation = rotation;
			instanceDatas[InstanceIndex].Scale = scale;
			InstanceIndex += 1;

			return true;
		}

		// Call this after you have made all the Add calls you want, but before beginning a render pass.
		public unsafe void UploadBufferData(CommandBuffer commandBuffer)
		{
			InstanceTransferBuffer.Unmap();

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
				VertexBuffer = Buffer.Create<byte>(GraphicsDevice, BufferUsageFlags.Vertex, vertexDataLengthInBytes);
				newTransferBufferNeeded = true;
			}

			if (IndexBuffer.Size < indexDataLengthInBytes)
			{
				IndexBuffer.Dispose();
				IndexBuffer = Buffer.Create<byte>(GraphicsDevice, BufferUsageFlags.Index, vertexDataLengthInBytes);
				newTransferBufferNeeded = true;
			}

			if (newTransferBufferNeeded)
			{
				VertexTransferBuffer.Dispose();
				VertexTransferBuffer = TransferBuffer.Create<byte>(GraphicsDevice, TransferBufferUsage.Upload, VertexBuffer.Size + IndexBuffer.Size);
				VertexTransferBuffer.Name = "TextBatch TransferBuffer";
			}

			if (vertexDataLengthInBytes > 0 && indexDataLengthInBytes > 0)
			{
				var transferVertexSpan = VertexTransferBuffer.Map<byte>(true);
				var transferIndexSpan = transferVertexSpan[vertexSpan.Length..];

				vertexSpan.CopyTo(transferVertexSpan);
				indexSpan.CopyTo(transferIndexSpan);

				VertexTransferBuffer.Unmap();

				var copyPass = commandBuffer.BeginCopyPass();
				copyPass.UploadToBuffer(
					new TransferBufferLocation
					{
						TransferBuffer = VertexTransferBuffer.Handle,
						Offset = 0
					},
					new BufferRegion
					{
						Buffer = VertexBuffer.Handle,
						Offset = 0,
						Size = (uint) vertexSpan.Length
					},
					true
				);
				copyPass.UploadToBuffer(
					new TransferBufferLocation
					{
						TransferBuffer = VertexTransferBuffer.Handle,
						Offset = (uint) vertexSpan.Length
					},
					new BufferRegion
					{
						Buffer = IndexBuffer.Handle,
						Offset = 0,
						Size = (uint) indexSpan.Length
					},
					true
				);
				copyPass.UploadToBuffer(
					new TransferBufferLocation(InstanceTransferBuffer),
					new BufferRegion(InstanceBuffer),
					true
				);
				commandBuffer.EndCopyPass(copyPass);

				var computePass = commandBuffer.BeginComputePass(
					TransformedVertexBuffer,
					InstanceBuffer
				);


				commandBuffer.EndComputePass(computePass);
			}

			PrimitiveCount = vertexCount / 2;
		}

		// Call this AFTER binding your text pipeline!
		public void Render(
			RenderPass renderPass
		) {
			renderPass.CommandBuffer.PushVertexUniformData(transformMatrix);
			renderPass.CommandBuffer.PushFragmentUniformData(CurrentFont.DistanceRange);

			renderPass.BindFragmentSamplers(new TextureSamplerBinding(
				CurrentFont.Texture,
				GraphicsDevice.LinearSampler
			));
			renderPass.BindVertexBuffers(VertexBuffer);
			renderPass.BindIndexBuffer(IndexBuffer, IndexElementSize.ThirtyTwo);

			renderPass.DrawIndexedPrimitives(
				PrimitiveCount * 3,
				1,
				0,
				0,
				0
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
					InstanceBuffer.Dispose();
					VertexTransferBuffer.Dispose();
					InstanceTransferBuffer.Dispose();
				}

				NativeMemory.Free(StringBytes);
				Wellspring.Wellspring_DestroyTextBatch(Handle);
			}
			base.Dispose(disposing);
		}

		[StructLayout(LayoutKind.Explicit, Size = 40)]
		record struct ComputeInstanceData
		{
			[FieldOffset(0)]
			public Vector3 Translation;
			[FieldOffset(16)]
			public Vector3 Rotation;
			[FieldOffset(32)]
			public Vector2 Scale;
		}
	}
}
