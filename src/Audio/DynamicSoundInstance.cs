using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
    public class DynamicSoundInstance : SoundInstance
    {
        private List<IntPtr> queuedBuffers;
        private List<uint> queuedSizes;
        private const int MINIMUM_BUFFER_CHECK = 3;

        public int PendingBufferCount => queuedBuffers.Count;

        private readonly float[] buffer;

        public override SoundState State { get; protected set; }

        internal DynamicSoundInstance(
            AudioDevice device,
            DynamicSound parent,
            bool is3D
        ) : base(device, parent, is3D)
        {
            queuedBuffers = new List<IntPtr>();
            queuedSizes = new List<uint>();

            buffer = new float[DynamicSound.BUFFER_SIZE];

            State = SoundState.Stopped;
        }

        public void Play()
        {
            Update();

            if (State == SoundState.Playing)
            {
                return;
            }

            QueueBuffers();

            FAudio.FAudioSourceVoice_Start(Handle, 0, 0);
            State = SoundState.Playing;
        }

        public void Pause()
        {
            if (State == SoundState.Playing)
            {
                FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
                State = SoundState.Paused;
            }
        }

        public void Stop()
        {
            FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
            FAudio.FAudioSourceVoice_FlushSourceBuffers(Handle);
            State = SoundState.Stopped;
            ClearBuffers();
        }

        internal void Update()
        {
            if (State != SoundState.Playing)
            {
                return;
            }

            FAudio.FAudioSourceVoice_GetState(
                Handle,
                out var state,
                FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED
            );

            while (PendingBufferCount > state.BuffersQueued)
            lock (queuedBuffers)
            {
                Marshal.FreeHGlobal(queuedBuffers[0]);
                queuedBuffers.RemoveAt(0);
                queuedSizes.RemoveAt(0);
            }
        }

        private void QueueBuffers()
        {
            for (
                int i = MINIMUM_BUFFER_CHECK - PendingBufferCount;
                i > 0;
                i -= 1
            ) {
                AddBuffer();
            }
        }

        private void ClearBuffers()
        {
            lock (queuedBuffers)
            {
                foreach (IntPtr buf in queuedBuffers)
                {
                    Marshal.FreeHGlobal(buf);
                }
                queuedBuffers.Clear();
                queuedBuffers.Clear();
            }
        }

        private void AddBuffer()
        {
            var parent = (DynamicSound) Parent;

            var samples = FAudio.stb_vorbis_get_samples_float_interleaved(
                parent.FileHandle,
                parent.Info.channels,
                buffer,
                buffer.Length
            );

            IntPtr next = Marshal.AllocHGlobal(buffer.Length * sizeof(float));
            Marshal.Copy(buffer, 0, next, buffer.Length);

            lock (queuedBuffers)
            {
                var lengthInBytes = (uint) buffer.Length * sizeof(float);

                queuedBuffers.Add(next);
                if (State != SoundState.Stopped)
                {
                    FAudio.FAudioBuffer buf = new FAudio.FAudioBuffer
                    {
                        AudioBytes = lengthInBytes,
                        pAudioData = next,
                        PlayLength = (
                            lengthInBytes /
                            (uint) parent.Info.channels /
                            (uint) (parent.Format.wBitsPerSample / 8)
                        )
                    };

                    FAudio.FAudioSourceVoice_SubmitSourceBuffer(
                        Handle,
                        ref buf,
                        IntPtr.Zero
                    );
                }
                else
                {
                    queuedSizes.Add(lengthInBytes);
                }
            }
        }
    }
}
