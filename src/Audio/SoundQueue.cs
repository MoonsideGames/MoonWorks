using System;

namespace MoonWorks.Audio
{
	// NOTE: all sounds played with a playlist must have the same audio format!
	public class SoundQueue : SoundInstance
	{
		public int NeedBufferThreshold = 0;
		private uint queuedBufferCount = 0;

		public delegate void OnBufferNeededFunc();
		public OnBufferNeededFunc OnBufferNeeded;

		private object StateLock = new object();

		public SoundQueue(AudioDevice device, ushort formatTag, ushort bitsPerSample, ushort blockAlign, ushort channels, uint samplesPerSecond) : base(device, formatTag, bitsPerSample, blockAlign, channels, samplesPerSecond)
		{
			device.AddSoundQueueReference(this);
		}

		public SoundQueue(AudioDevice device, StaticSound templateSound) : base(device, templateSound.FormatTag, templateSound.BitsPerSample, templateSound.BlockAlign, templateSound.Channels, templateSound.SamplesPerSecond)
		{
			device.AddSoundQueueReference(this);
		}

		public void Update()
		{
			lock (StateLock)
			{
				if (IsDisposed) { return; }
				if (State != SoundState.Playing) { return; }

				if (NeedBufferThreshold > 0)
				{
					FAudio.FAudioSourceVoice_GetState(
						Voice,
						out var state,
						FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED
					);

					var queuedBufferCount = state.BuffersQueued;
					for (int i = 0; i < NeedBufferThreshold - queuedBufferCount; i += 1)
					{
						if (OnBufferNeeded != null)
						{
							OnBufferNeeded();
						}
					}
				}
			}
		}

		public void EnqueueSound(StaticSound sound)
		{
#if DEBUG
			if (
				sound.FormatTag != Format.wFormatTag ||
				sound.BitsPerSample != Format.wBitsPerSample ||
				sound.Channels != Format.nChannels ||
				sound.SamplesPerSecond != Format.nSamplesPerSec
			)
			{
				Logger.LogWarn("Playlist audio format mismatch!");
			}
#endif

			lock (StateLock)
			{
				FAudio.FAudioSourceVoice_SubmitSourceBuffer(
					Voice,
					ref sound.Handle,
					IntPtr.Zero
				);
			}
		}

		public override void Pause()
		{
			lock (StateLock)
			{
				if (State == SoundState.Playing)
				{
					FAudio.FAudioSourceVoice_Stop(Voice, 0, 0);
					State = SoundState.Paused;
				}
			}
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
			lock (StateLock)
			{
				if (State == SoundState.Playing)
				{
					return;
				}

				FAudio.FAudioSourceVoice_Start(Voice, 0, operationSet);
				State = SoundState.Playing;
			}
		}

		public override void Stop()
		{
			lock (StateLock)
			{
				FAudio.FAudioSourceVoice_ExitLoop(Voice, 0);
				State = SoundState.Stopped;
			}
		}

		public override void StopImmediate()
		{
			lock (StateLock)
			{
				FAudio.FAudioSourceVoice_Stop(Voice, 0, 0);
				FAudio.FAudioSourceVoice_FlushSourceBuffers(Voice);
				State = SoundState.Stopped;
			}
		}
	}
}
