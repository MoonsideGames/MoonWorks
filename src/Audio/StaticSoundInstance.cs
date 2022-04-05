using System;

namespace MoonWorks.Audio
{
	public class StaticSoundInstance : SoundInstance
	{
		public StaticSound Parent { get; }

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

		internal StaticSoundInstance(
			AudioDevice device,
			StaticSound parent,
			bool is3D
		) : base(device, parent.FormatTag, parent.BitsPerSample, parent.BlockAlign, parent.Channels, parent.SamplesPerSecond, is3D)
		{
			Parent = parent;
		}

		public override void Play(bool loop = false)
		{
			if (State == SoundState.Playing)
			{
				return;
			}

			Loop = loop;

			if (Loop)
			{
				Parent.Handle.LoopCount = 255;
				Parent.Handle.LoopBegin = Parent.LoopStart;
				Parent.Handle.LoopLength = Parent.LoopLength;
			}
			else
			{
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

		public override void Pause()
		{
			if (State == SoundState.Paused)
			{
				FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
				State = SoundState.Paused;
			}
		}

		public override void Stop(bool immediate = true)
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

		public void Free()
		{
			Parent.FreeInstance(this);
		}
	}
}
