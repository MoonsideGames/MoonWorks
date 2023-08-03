using System;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Emits audio from submitted audio buffers.
	/// </summary>
	public abstract class SourceVoice : Voice
	{
		private Format format;
		public Format Format => format;

		protected bool PlaybackInitiated;

		/// <summary>
		/// The number of buffers queued in the voice.
		/// This includes the currently playing voice!
		/// </summary>
		public uint BuffersQueued
		{
			get
			{
				FAudio.FAudioSourceVoice_GetState(
					Handle,
					out var state,
					FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED
				);

				return state.BuffersQueued;
			}
		}

		private SoundState state;
		public SoundState State
		{
			get
			{
				if (BuffersQueued == 0)
				{
					Stop();
				}

				return state;
			}

			internal set
			{
				state = value;
			}
		}

		protected object StateLock = new object();

		public SourceVoice(
			AudioDevice device,
			Format format
		) : base(device, format.Channels, device.DeviceDetails.OutputFormat.Format.nChannels)
		{
			this.format = format;
			var fAudioFormat = format.ToFAudioFormat();

			 FAudio.FAudio_CreateSourceVoice(
				device.Handle,
				out handle,
				ref fAudioFormat,
				FAudio.FAUDIO_VOICE_USEFILTER,
				FAudio.FAUDIO_DEFAULT_FREQ_RATIO,
				IntPtr.Zero,
				IntPtr.Zero, // default sends to mastering voice!
				IntPtr.Zero
			);
		}

		/// <summary>
		/// Starts consumption and processing of audio by the voice.
		/// Delivers the result to any connected submix or mastering voice.
		/// </summary>
		/// <param name="syncGroup">Optional. Denotes that the operation will be pending until AudioDevice.TriggerSyncGroup is called.</param>
		public void Play(uint syncGroup = FAudio.FAUDIO_COMMIT_NOW)
		{
			lock (StateLock)
			{
				FAudio.FAudioSourceVoice_Start(Handle, 0, syncGroup);

				State = SoundState.Playing;
			}
		}

		/// <summary>
		/// Pauses playback.
		/// All source buffers that are queued on the voice and the current cursor position are preserved.
		/// </summary>
		/// <param name="syncGroup">Optional. Denotes that the operation will be pending until AudioDevice.TriggerSyncGroup is called.</param>
		public void Pause(uint syncGroup = FAudio.FAUDIO_COMMIT_NOW)
		{
			lock (StateLock)
			{
				FAudio.FAudioSourceVoice_Stop(Handle, 0, syncGroup);

				State = SoundState.Paused;
			}
		}

		/// <summary>
		/// Stops looping the voice when it reaches the end of the current loop region.
		/// If the cursor for the voice is not in a loop region, ExitLoop does nothing.
		/// </summary>
		/// <param name="syncGroup">Optional. Denotes that the operation will be pending until AudioDevice.TriggerSyncGroup is called.</param>
		public void ExitLoop(uint syncGroup = FAudio.FAUDIO_COMMIT_NOW)
		{
			lock (StateLock)
			{
				FAudio.FAudioSourceVoice_ExitLoop(Handle, syncGroup);
			}
		}

		/// <summary>
		/// Stops playback and removes all pending audio buffers from the voice queue.
		/// </summary>
		/// <param name="syncGroup">Optional. Denotes that the operation will be pending until AudioDevice.TriggerSyncGroup is called.</param>
		public void Stop(uint syncGroup = FAudio.FAUDIO_COMMIT_NOW)
		{
			lock (StateLock)
			{
				FAudio.FAudioSourceVoice_Stop(Handle, 0, syncGroup);
				FAudio.FAudioSourceVoice_FlushSourceBuffers(Handle);

				State = SoundState.Stopped;
			}
		}

		/// <summary>
		/// Adds an AudioBuffer to the voice queue.
		/// The voice processes and plays back the buffers in its queue in the order that they were submitted.
		/// </summary>
		/// <param name="buffer">The buffer to submit to the voice.</param>
		public void Submit(AudioBuffer buffer)
		{
			Submit(buffer.ToFAudioBuffer());
		}

		/// <summary>
		/// Calculates positional sound. This must be called continuously to update positional sound.
		/// </summary>
		/// <param name="listener"></param>
		/// <param name="emitter"></param>
		public unsafe void Apply3D(AudioListener listener, AudioEmitter emitter)
		{
			Is3D = true;

			emitter.emitterData.CurveDistanceScaler = Device.CurveDistanceScalar;
			emitter.emitterData.ChannelCount = SourceChannelCount;

			var dspSettings = new FAudio.F3DAUDIO_DSP_SETTINGS
			{
				DopplerFactor = DopplerFactor,
				SrcChannelCount = SourceChannelCount,
				DstChannelCount = DestinationChannelCount,
				pMatrixCoefficients = (nint) pMatrixCoefficients
			};

			FAudio.F3DAudioCalculate(
				Device.Handle3D,
				ref listener.listenerData,
				ref emitter.emitterData,
				FAudio.F3DAUDIO_CALCULATE_MATRIX | FAudio.F3DAUDIO_CALCULATE_DOPPLER,
				ref dspSettings
			);

			UpdatePitch();

			FAudio.FAudioVoice_SetOutputMatrix(
				Handle,
				OutputVoice.Handle,
				SourceChannelCount,
				DestinationChannelCount,
				(nint) pMatrixCoefficients,
				0
			);
		}

		/// <summary>
		/// Specifies that this source voice can be returned to the voice pool.
		/// Holding on to the reference after calling this will cause problems!
		/// </summary>
		public void Return()
		{
			Stop();
			Device.Return(this);
		}

		/// <summary>
		/// Called automatically by AudioDevice in the audio thread.
		/// Don't call this yourself! You might regret it!
		/// </summary>
		public virtual void Update() { }

		/// <summary>
		/// Adds an FAudio buffer to the voice queue.
		/// The voice processes and plays back the buffers in its queue in the order that they were submitted.
		/// </summary>
		/// <param name="buffer">The buffer to submit to the voice.</param>
		protected void Submit(FAudio.FAudioBuffer buffer)
		{
			lock (StateLock)
			{
				FAudio.FAudioSourceVoice_SubmitSourceBuffer(
					Handle,
					ref buffer,
					IntPtr.Zero
				);
			}
		}

		public override void Reset()
		{
			Stop();
			PlaybackInitiated = false;
			base.Reset();
		}

		protected override unsafe void Destroy()
		{
			Stop();
			base.Destroy();
		}
	}
}
