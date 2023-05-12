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
					Voice,
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

		public bool AutoFree { get; internal set; }

		internal StaticSoundInstance(
			AudioDevice device,
			StaticSound parent
		) : base(device, parent.FormatTag, parent.BitsPerSample, parent.BlockAlign, parent.Channels, parent.SamplesPerSecond)
		{
			Parent = parent;
		}

		public override void Play()
		{
			PlayUsingOperationSet(0);
		}

		public override void QueueSyncPlay()
		{
			PlayUsingOperationSet(1);
		}

		private void PlayUsingOperationSet(uint operationSet)
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
				Voice,
				ref Parent.Handle,
				IntPtr.Zero
			);

			FAudio.FAudioSourceVoice_Start(Voice, 0, operationSet);
			State = SoundState.Playing;

			if (AutoFree)
			{
				Device.AddAutoFreeStaticSoundInstance(this);
			}
		}

		public override void Pause()
		{
			if (State == SoundState.Paused)
			{
				FAudio.FAudioSourceVoice_Stop(Voice, 0, 0);
				State = SoundState.Paused;
			}
		}

		public override void Stop()
		{
			FAudio.FAudioSourceVoice_ExitLoop(Voice, 0);
			State = SoundState.Stopped;
		}

		public override void StopImmediate()
		{
			FAudio.FAudioSourceVoice_Stop(Voice, 0, 0);
			FAudio.FAudioSourceVoice_FlushSourceBuffers(Voice);
			State = SoundState.Stopped;
		}

		public void Seek(uint sampleFrame)
		{
			if (State == SoundState.Playing)
			{
				FAudio.FAudioSourceVoice_Stop(Voice, 0, 0);
				FAudio.FAudioSourceVoice_FlushSourceBuffers(Voice);
			}

			Parent.Handle.PlayBegin = sampleFrame;
		}

		// Call this when you no longer need the sound instance.
		// If AutoFree is set, this will automatically be called when the sound instance stops playing.
		// If the sound isn't stopped when you call this, things might get weird!
		public void Free()
		{
			Parent.FreeInstance(this);
		}

		internal void Reset()
		{
			Pan = 0;
			Pitch = 0;
			Volume = 1;
			Loop = false;
			Is3D = false;
			FilterType = FilterType.None;
		}
	}
}
