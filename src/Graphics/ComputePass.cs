using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics;

public class ComputePass
{
	public nint Handle { get; private set; }

	internal void SetHandle(nint handle)
	{
		Handle = handle;
	}

#if DEBUG
	internal bool active;

	ComputePipeline currentComputePipeline;
#endif

	/// <summary>
	/// Binds a compute pipeline so that compute work may be dispatched.
	/// </summary>
	/// <param name="computePipeline">The compute pipeline to bind.</param>
	public void BindComputePipeline(
		ComputePipeline computePipeline
	) {
#if DEBUG
		AssertComputePassActive();

		// TODO: validate formats?
#endif

		Refresh.Refresh_BindComputePipeline(
			Handle,
			computePipeline.Handle
		);

#if DEBUG
		currentComputePipeline = computePipeline;
#endif
	}

	/// <summary>
	/// Binds a texture to be used in the compute shader.
	/// This texture must have been created with the ComputeShaderRead usage flag.
	/// </summary>
	public unsafe void BindStorageTexture(
		in TextureSlice textureSlice,
		uint slot = 0
	) {
#if DEBUG
		AssertComputePassActive();
		AssertComputePipelineBound();
		AssertTextureNonNull(textureSlice.Texture);
		AssertTextureHasComputeStorageReadFlag(textureSlice.Texture);
#endif

		var refreshTextureSlice = textureSlice.ToRefresh();

		Refresh.Refresh_BindComputeStorageTextures(
			Handle,
			slot,
			&refreshTextureSlice,
			1
		);
	}

	/// <summary>
	/// Binds a buffer to be used in the compute shader.
	/// This buffer must have been created with the ComputeShaderRead usage flag.
	/// </summary>
	public unsafe void BindStorageBuffer(
		Buffer buffer,
		uint slot = 0
	) {
#if DEBUG
		AssertComputePassActive();
		AssertComputePipelineBound();
		AssertBufferNonNull(buffer);
		AssertBufferHasComputeStorageReadFlag(buffer);
#endif

		var bufferHandle = buffer.Handle;

		Refresh.Refresh_BindComputeStorageBuffers(
			Handle,
			slot,
			&bufferHandle,
			1
		);
	}



	/// <summary>
	/// Dispatches compute work.
	/// </summary>
	public void Dispatch(
		uint groupCountX,
		uint groupCountY,
		uint groupCountZ
	) {
#if DEBUG
		AssertComputePassActive();
		AssertComputePipelineBound();

		if (groupCountX < 1 || groupCountY < 1 || groupCountZ < 1)
		{
			throw new System.ArgumentException("All dimensions for the compute work groups must be >= 1!");
		}
#endif

		Refresh.Refresh_DispatchCompute(
			Handle,
			groupCountX,
			groupCountY,
			groupCountZ
		);
	}

#if DEBUG
	private void AssertComputePassActive(string message = "Render pass is not active!")
	{
		if (!active)
		{
			throw new System.InvalidOperationException(message);
		}
	}

	private void AssertComputePipelineBound(string message = "No compute pipeline is bound!")
	{
		if (currentComputePipeline == null)
		{
			throw new System.InvalidOperationException(message);
		}
	}

	private void AssertTextureNonNull(in TextureSlice textureSlice)
	{
		if (textureSlice.Texture == null || textureSlice.Texture.Handle == nint.Zero)
		{
			throw new System.NullReferenceException("Texture must not be null!");
		}
	}

	private void AssertTextureHasComputeStorageReadFlag(Texture texture)
	{
		if ((texture.UsageFlags & TextureUsageFlags.ComputeStorageRead) == 0)
		{
			throw new System.ArgumentException("The bound Texture's UsageFlags must include TextureUsageFlags.ComputeStorageRead!");
		}
	}

	private void AssertBufferNonNull(Buffer buffer)
	{
		if (buffer == null || buffer.Handle == nint.Zero)
		{
			throw new System.NullReferenceException("Buffer must not be null!");
		}
	}

	private void AssertBufferHasComputeStorageReadFlag(Buffer buffer)
	{
		if ((buffer.UsageFlags & BufferUsageFlags.ComputeStorageRead) == 0)
		{
			throw new System.ArgumentException("The bound Buffer's UsageFlags must include BufferUsageFlag.ComputeStorageRead!");
		}
	}
#endif
}
