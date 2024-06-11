using System;
using System.Collections.Concurrent;

namespace MoonWorks.Graphics;

internal class CommandBufferPool
{
	private GraphicsDevice GraphicsDevice;
	private ConcurrentQueue<CommandBuffer> CommandBuffers = new ConcurrentQueue<CommandBuffer>();

	public CommandBufferPool(GraphicsDevice graphicsDevice)
	{
		GraphicsDevice = graphicsDevice;
	}

	public CommandBuffer Obtain()
	{
		if (CommandBuffers.TryDequeue(out var commandBuffer))
		{
			return commandBuffer;
		}
		else
		{
			return new CommandBuffer(GraphicsDevice);
		}
	}

	public void Return(CommandBuffer commandBuffer)
	{
		commandBuffer.Handle = IntPtr.Zero;
		CommandBuffers.Enqueue(commandBuffer);
	}
}
