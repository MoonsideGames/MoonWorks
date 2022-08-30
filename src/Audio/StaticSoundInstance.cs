using System;

namespace MoonWorks.Audio
{
	public class StaticSoundInstance : SoundInstance
	{
		public StaticSound Parent { get; }

		public bool Loop { get; set; }

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
					StopImmediate();
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
			StaticSound parent
		) : base(device, parent.FormatTag, parent.BitsPerSample, parent.BlockAlign, parent.Channels, parent.SamplesPerSecond)
		{
			Parent = parent;
		}

		public override void Play()
		{
			if (State == SoundState.Playing)
			{
				return;
			}

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

		public override void Stop()
		{
			FAudio.FAudioSourceVoice_ExitLoop(Handle, 0);
			State = SoundState.Stopped;
		}

		public override void StopImmediate()
		{
			FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
			FAudio.FAudioSourceVoice_FlushSourceBuffers(Handle);
			State = SoundState.Stopped;
		}

		public void Seek(uint sampleFrame)
		{
			if (State == SoundState.Playing)
			{
				FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
				FAudio.FAudioSourceVoice_FlushSourceBuffers(Handle);
			}

			Parent.Handle.PlayBegin = sampleFrame;
		}

		public void Free()
		{
			Parent.FreeInstance(this);
		}

		internal void Reset()
		{
			Pan = 0;
			Pitch = 0;
			Volume = 1;
			Reverb = 0;
			Loop = false;
			Is3D = false;
			FilterType = FilterType.None;
			Reverb = 0;
		}
	}
}
