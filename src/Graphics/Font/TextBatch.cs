using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using WellspringCS;

namespace MoonWorks.Graphics.Font
{
	public unsafe class TextBatch : GraphicsResource
	{
		public const int MAX_CHAR_COUNT = 8192;
		public const int MAX_VERTEX_COUNT = MAX_CHAR_COUNT * 4;
		public const int MAX_INDEX_COUNT = MAX_CHAR_COUNT * 6;
		public const int MAX_CHUNK_COUNT = 128;

		private GraphicsDevice GraphicsDevice { get; }
		public IntPtr Handle { get; }

		public Buffer VertexBuffer { get; protected init; } = null;
		public Buffer IndexBuffer { get; protected init; } = null;
		public Buffer ChunkDataBuffer { get; protected init; }
		public uint VertexCount { get; protected set; }

		private TransferBuffer VertexTransferBuffer;
		private TransferBuffer ChunkDataTransferBuffer;
		private int ChunkCount = 0;

		private int VertexSize;
		private int ChunkDataSize;

		private byte* StringBytes;
		private int StringBytesLength;

		private TextureSamplerBinding[] FontTextureBindings = new TextureSamplerBinding[4];
		private readonly Dictionary<Font, uint> FontIndices = new Dictionary<Font, uint>();
		private uint CurrentFontIndex = 0;

		private record struct ChunkData(
			Matrix4x4 Transform,
			Vector4 Color,
			float DistanceRange,
			uint FontIndex,
			Vector2 Padding
		);

		public TextBatch(GraphicsDevice device) : base(device)
		{
			GraphicsDevice = device;
			Handle = Wellspring.Wellspring_CreateTextBatch();
			Name = "TextBatch";

			StringBytesLength = 128;
			StringBytes = (byte*) NativeMemory.Alloc((nuint) StringBytesLength);

			VertexBuffer = Buffer.Create<Vertex>(GraphicsDevice, "TextBatch Vertex Buffer", BufferUsageFlags.Vertex, MAX_VERTEX_COUNT);
			IndexBuffer = Buffer.Create<uint>(GraphicsDevice, "TextBatch Index Buffer", BufferUsageFlags.Index, MAX_INDEX_COUNT);
			ChunkDataBuffer = Buffer.Create<ChunkData>(GraphicsDevice, "TextBatch Chunk Data Buffer", BufferUsageFlags.GraphicsStorageRead, MAX_CHUNK_COUNT);

			VertexTransferBuffer = TransferBuffer.Create<byte>(GraphicsDevice, "TextBatch TransferBuffer", TransferBufferUsage.Upload, VertexBuffer.Size);
			ChunkDataTransferBuffer = TransferBuffer.Create<ChunkData>(GraphicsDevice, "TextBatch Chunk TransferBuffer", TransferBufferUsage.Upload, MAX_CHUNK_COUNT);

			TransferBuffer spriteIndexTransferBuffer = TransferBuffer.Create<uint>(
				GraphicsDevice,
				"SpriteIndex TransferBuffer",
				TransferBufferUsage.Upload,
				MAX_CHAR_COUNT * 6
			);

			var indexSpan = spriteIndexTransferBuffer.Map<uint>(false);

			for (int i = 0, j = 0; i < MAX_INDEX_COUNT; i += 6, j += 4)
			{
				indexSpan[i]     =  (uint) j;
				indexSpan[i + 1] =  (uint) j + 1;
				indexSpan[i + 2] =  (uint) j + 2;
				indexSpan[i + 3] =  (uint) j + 3;
				indexSpan[i + 4] =  (uint) j + 2;
				indexSpan[i + 5] =  (uint) j + 1;
			}
			spriteIndexTransferBuffer.Unmap();

			var cmdbuf = GraphicsDevice.AcquireCommandBuffer();
			var copyPass = cmdbuf.BeginCopyPass();
			copyPass.UploadToBuffer(spriteIndexTransferBuffer, IndexBuffer, false);
			cmdbuf.EndCopyPass(copyPass);
			GraphicsDevice.Submit(cmdbuf);

			VertexSize = Marshal.SizeOf<Wellspring.Vertex>();
			ChunkDataSize = Marshal.SizeOf<ChunkData>();
		}

		// Call this to initialize or reset the batch.
		public void Start()
		{
			Wellspring.Wellspring_StartTextBatch(Handle);
			VertexCount = 0;

			for (var i = 0; i < 4; i += 1)
			{
				FontTextureBindings[i].Texture = GraphicsDevice.DummyTexture;
				FontTextureBindings[i].Sampler = GraphicsDevice.LinearSampler;
			}
			FontIndices.Clear();
			CurrentFontIndex = 0;

			ChunkDataTransferBuffer.Map(true);
			ChunkCount = 0;
		}

		// Add a chunk of text to the batch
		public unsafe bool Add(
			Font font,
			string text,
			int pixelSize,
			Matrix4x4 transform,
			Color color,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment verticalAlignment = VerticalAlignment.Baseline
		) {
			if (!FontIndices.TryGetValue(font, out uint fontIndex))
			{
				fontIndex = CurrentFontIndex;
				FontTextureBindings[fontIndex].Texture = font.Texture;
				FontIndices.Add(font, fontIndex);
				CurrentFontIndex += 1;
			}

			var byteCount = System.Text.Encoding.UTF8.GetByteCount(text);

			if (StringBytesLength < byteCount)
			{
				StringBytes = (byte*) NativeMemory.Realloc(StringBytes, (nuint) byteCount);
			}

			fixed (char* chars = text)
			{
				System.Text.Encoding.UTF8.GetBytes(chars, text.Length, StringBytes, byteCount);

				var result = Wellspring.Wellspring_AddChunkToTextBatch(
					Handle,
					font.Handle,
					pixelSize,
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

			var chunkDatas = ChunkDataTransferBuffer.MappedSpan<ChunkData>();
			chunkDatas[ChunkCount].Transform = transform;
			chunkDatas[ChunkCount].Color = color.ToVector4();
			chunkDatas[ChunkCount].DistanceRange = font.DistanceRange;
			chunkDatas[ChunkCount].FontIndex = fontIndex;
			ChunkCount += 1;

			return true;
		}

		// Call this after you have made all the Add calls you want, but before beginning a render pass.
		public unsafe void UploadBufferData(CommandBuffer commandBuffer)
		{
			ChunkDataTransferBuffer.Unmap();

			Wellspring.Wellspring_GetBufferData(
				Handle,
				out var vertexSpan
			);

			if (vertexSpan.Length > 0)
			{
				var transferVertexSpan = VertexTransferBuffer.Map<Wellspring.Vertex>(true);
				vertexSpan.CopyTo(transferVertexSpan);
				VertexTransferBuffer.Unmap();

				var copyPass = commandBuffer.BeginCopyPass();
				copyPass.UploadToBuffer(
					new TransferBufferLocation
					{
						TransferBuffer = ChunkDataTransferBuffer.Handle,
						Offset = 0
					},
					new BufferRegion
					{
						Buffer = ChunkDataBuffer.Handle,
						Offset = 0,
						Size = (uint)(ChunkCount * ChunkDataSize)
					},
					true
				);
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
						Size = (uint) (vertexSpan.Length * VertexSize)
					},
					true
				);
				commandBuffer.EndCopyPass(copyPass);
			}

			VertexCount = (uint) vertexSpan.Length;
		}

		// Call this AFTER binding your text pipeline!
		public void Render(
			RenderPass renderPass,
			Matrix4x4 viewProjectionMatrix
		) {
			renderPass.CommandBuffer.PushVertexUniformData(viewProjectionMatrix);

			renderPass.BindFragmentSamplers(FontTextureBindings);
			renderPass.BindVertexBuffers(VertexBuffer);
			renderPass.BindIndexBuffer(IndexBuffer, IndexElementSize.ThirtyTwo);
			renderPass.BindVertexStorageBuffers(ChunkDataBuffer);

			renderPass.DrawIndexedPrimitives(
				(VertexCount / 2) * 3,
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
					ChunkDataBuffer.Dispose();
					VertexTransferBuffer.Dispose();
					ChunkDataTransferBuffer.Dispose();
				}

				NativeMemory.Free(StringBytes);
				Wellspring.Wellspring_DestroyTextBatch(Handle);
			}
			base.Dispose(disposing);
		}
	}
}
