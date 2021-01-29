using System;
using System.Collections.Generic;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class GraphicsDevice : IDisposable
    {
        public IntPtr Handle { get; }

        public bool IsDisposed { get; private set; }

        private readonly Queue<CommandBuffer> commandBufferPool;

        private readonly List<WeakReference<GraphicsResource>> resources = new List<WeakReference<GraphicsResource>>();

        public GraphicsDevice(
            IntPtr deviceWindowHandle,
            Refresh.PresentMode presentMode,
            bool debugMode,
            int initialCommandBufferPoolSize = 4
        ) {
            var presentationParameters = new Refresh.PresentationParameters
            {
                deviceWindowHandle = deviceWindowHandle,
                presentMode = presentMode
            };

            Handle = Refresh.Refresh_CreateDevice(
                presentationParameters,
                Conversions.BoolToByte(debugMode)
            );

            commandBufferPool = new Queue<CommandBuffer>(initialCommandBufferPoolSize);
            for (var i = 0; i < initialCommandBufferPoolSize; i += 1)
            {
                commandBufferPool.Enqueue(new CommandBuffer(this));
            }
        }

        public CommandBuffer AcquireCommandBuffer()
        {
            var commandBufferHandle = Refresh.Refresh_AcquireCommandBuffer(Handle, 0);
            if (commandBufferPool.Count == 0)
            {
                commandBufferPool.Enqueue(new CommandBuffer(this));
            }

            var commandBuffer = commandBufferPool.Dequeue();
            commandBuffer.Handle = commandBufferHandle;

            return commandBuffer;
        }

        public unsafe void Submit(params CommandBuffer[] commandBuffers)
        {
            var commandBufferPtrs = stackalloc IntPtr[commandBuffers.Length];

            for (var i = 0; i < commandBuffers.Length; i += 1)
            {
                commandBufferPtrs[i] = commandBuffers[i].Handle;
            }

            Refresh.Refresh_Submit(
                Handle,
                (uint) commandBuffers.Length,
                (IntPtr) commandBufferPtrs
            );

            // return to pool
            for (var i = 0; i < commandBuffers.Length; i += 1)
            {
                commandBuffers[i].Handle = IntPtr.Zero;
                commandBufferPool.Enqueue(commandBuffers[i]);
            }
        }

        public void Wait()
        {
            Refresh.Refresh_Wait(Handle);
        }

        internal void AddResourceReference(WeakReference<GraphicsResource> resourceReference)
        {
            lock (resources)
            {
                resources.Add(resourceReference);
            }
        }

        internal void RemoveResourceReference(WeakReference<GraphicsResource> resourceReference)
        {
            lock (resources)
            {
                resources.Remove(resourceReference);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    lock (resources)
                    {
                        foreach (var resource in resources)
                        {
                            if (resource.TryGetTarget(out var target))
                            {
                                target.Dispose();
                            }
                        }
                        resources.Clear();
                    }

                    Refresh.Refresh_DestroyDevice(Handle);
                }

                IsDisposed = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~GraphicsDevice()
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
