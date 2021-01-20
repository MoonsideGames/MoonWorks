using System;

namespace MoonWorks.Audio
{
    public class StaticSoundInstance : SoundInstance
    {
        public bool Loop { get; }

        private SoundState _state = SoundState.Stopped;
        public override SoundState State
        {
            get
            {
                FAudio.FAudioSourceVoice_GetState(
                    Handle,
                    out var state,
                    FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED
                );
                if (state.BuffersQueued == 0)
                {
                    Stop(true);
                }

                return _state;
            }

            protected set
            {
                _state = value;
            }
        }

        public StaticSoundInstance(
            AudioDevice device,
            StaticSound parent,
            bool is3D,
            bool loop = false
        ) : base(device, parent, is3D)
        {
            Loop = loop;
        }

        public void Play()
        {
            var parent = (StaticSound) Parent;

            if (State == SoundState.Playing)
            {
                return;
            }

            if (Loop)
            {
                parent.Handle.LoopCount = 255;
                parent.Handle.LoopBegin = parent.LoopStart;
                parent.Handle.LoopLength = parent.LoopLength;
            }
            else
            {
                parent.Handle.LoopCount = 0;
                parent.Handle.LoopBegin = 0;
                parent.Handle.LoopLength = 0;
            }

            FAudio.FAudioSourceVoice_SubmitSourceBuffer(
                Handle,
                ref parent.Handle,
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
