using System;

namespace MoonWorks.Audio
{
    public class StaticSoundInstance : SoundInstance
    {
        public bool Loop { get; protected set; }

        public StaticSoundInstance(
            AudioDevice device,
            Sound parent,
            bool is3D
        ) : base(device, parent, is3D) { }

        public void Play(bool loop = false)
        {
            if (State == SoundState.Playing)
            {
                return;
            }

            if (loop)
            {
                Loop = true;
                Parent.Handle.LoopCount = 255;
                Parent.Handle.LoopBegin = 0;
                Parent.Handle.LoopLength = Parent.LoopLength;
            }
            else
            {
                Loop = false;
                Parent.Handle.LoopCount = 0;
                Parent.Handle.LoopBegin = 0;
                Parent.Handle.LoopLength = 0;
            }

            FAudio.FAudioSourceVoice_SubmitSourceBuffer(
                Handle,
                ref Parent.Handle,
                IntPtr.Zero
            );

            FAudio.FAudioSourceVoice_Start(Handle, 0, 0);
            State = SoundState.Playing;
        }

        public void Pause()
        {
            if (State == SoundState.Paused)
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
                State = SoundState.Stopped;
            }
            else
            {
                FAudio.FAudioSourceVoice_ExitLoop(Handle, 0);
            }
        }
    }
}
