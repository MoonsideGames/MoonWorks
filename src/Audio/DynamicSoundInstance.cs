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
            bool is3D,
            bool loop
        ) : base(device, parent, is3D, loop)
        {
            queuedBuffers = new List<IntPtr>();
            queuedSizes = new List<uint>();

            buffer = new float[DynamicSound.BUFFER_SIZE];

            State = SoundState.Stopped;
        }

        public void Play()
        {
            if (State == SoundState.Playing)
            {
                return;
            }

            State = SoundState.Playing;
            Update();
            FAudio.FAudioSourceVoice_Start(Handle, 0, 0);
        }

        public void Pause()
        {
            if (State == SoundState.Playing)
            {
                FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
                State = SoundState.Paused;
            }
        }

        public void Stop(bool immediate = true)
        {
            if (immediate)
            {
                FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
                FAudio.FAudioSourceVoice_FlushSourceBuffers(Handle);
                ClearBuffers();
            }

            State = SoundState.Stopped;
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
            }

            QueueBuffers();
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
                queuedSizes.Clear();
            }
        }

        private void AddBuffer()
        {
            var parent = (DynamicSound) Parent;

            /* NOTE: this function returns samples per channel, not total samples */
            var samples = FAudio.stb_vorbis_get_samples_float_interleaved(
                parent.FileHandle,
                parent.Info.channels,
                buffer,
                buffer.Length
            );

            var sampleCount = samples * parent.Info.channels;
            var lengthInBytes = (uint) sampleCount * sizeof(float);

            IntPtr next = Marshal.AllocHGlobal((int) lengthInBytes);
            Marshal.Copy(buffer, 0, next, sampleCount);

            lock (queuedBuffers)
            {
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

            /* We have reached the end of the file, what do we do? */
            if (sampleCount < buffer.Length)
            {
                if (Loop)
                {
                    FAudio.stb_vorbis_seek_start(parent.FileHandle);
                }
                else
                {
                    Stop(false);
                }
            }
        }
    }
}
