using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MoonWorks.Audio
{
	/// <summary>
	/// AudioDevice manages all audio-related concerns.
	/// </summary>
	public class AudioDevice : IDisposable
	{
		public IntPtr Handle { get; }
		public Audio3D.F3DAudio Handle3D { get; }
		public FAudio.FAudioDeviceDetails DeviceDetails { get; }

		private readonly IntPtr TrueMasteringVoice;

		// this is a fun little trick where we use a submix voice as a "faux" mastering voice
		// this lets us maintain API consistency for effects like panning and reverb
		private readonly SubmixVoice FauxMasteringVoice;
		public SubmixVoice MasteringVoice => FauxMasteringVoice;

		public Audio3D.Listener Listener { get; private set; }
		public float DopplerScale = 1f;
		public float SpeedOfSound = 343.5f;

		private readonly HashSet<GCHandle> ResourceHandles = [];
		private readonly HashSet<StreamingAudioSource> StreamingAudioSources = [];
		private readonly HashSet<TransientVoice> TransientVoices = [];
		private readonly HashSet<SourceVoice> SpatialVoices = [];

		private readonly AudioTweenManager AudioTweenManager;

		private readonly SourceVoicePool VoicePool;
		private readonly List<SourceVoice> VoicesToReturn = [];

		private const int Step = 200;
		private readonly TimeSpan UpdateInterval;
		private readonly System.Diagnostics.Stopwatch ThreadTimer = new();
		private readonly System.Diagnostics.Stopwatch TimeElapsedStopwatch = new();
		private long previousTickTime;
		private readonly Thread Thread;
		private readonly AutoResetEvent WakeSignal;
		internal readonly object StateLock = new();

		private bool Running;
		public bool IsDisposed { get; private set; }

		internal AudioDevice()
		{
			UpdateInterval = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / Step);

			FAudio.FAudioCreate(out var handle, 0, FAudio.FAUDIO_DEFAULT_PROCESSOR);
			Handle = handle;

			/* Find a suitable device */

			FAudio.FAudio_GetDeviceCount(Handle, out var devices);

			if (devices == 0)
			{
				Logger.LogError("No audio devices found! Setting dummy driver!");

				SDL3.SDL.SDL_SetHint("SDL_AUDIO_DRIVER", "dummy");
				FAudio.FAudio_GetDeviceCount(Handle, out devices);
			}

			FAudio.FAudioDeviceDetails deviceDetails;

			uint i = 0;
			for (i = 0; i < devices; i++)
			{
				FAudio.FAudio_GetDeviceDetails(
					Handle,
					i,
					out deviceDetails
				);
				if ((deviceDetails.Role & FAudio.FAudioDeviceRole.FAudioDefaultGameDevice) == FAudio.FAudioDeviceRole.FAudioDefaultGameDevice)
				{
					DeviceDetails = deviceDetails;
					break;
				}
			}

			if (i == devices)
			{
				i = 0; /* whatever we'll just use the first one I guess */
				FAudio.FAudio_GetDeviceDetails(
					Handle,
					i,
					out deviceDetails
				);
				DeviceDetails = deviceDetails;
			}

			/* Init Mastering Voice */
			var result = FAudio.FAudio_CreateMasteringVoice(
				Handle,
				out TrueMasteringVoice,
				FAudio.FAUDIO_DEFAULT_CHANNELS,
				FAudio.FAUDIO_DEFAULT_SAMPLERATE,
				0,
				i,
				IntPtr.Zero
			);

			if (result != 0)
			{
				Logger.LogError("Failed to create a mastering voice!");
				Logger.LogError("Audio device creation failed!");
				return;
			}

			FauxMasteringVoice = SubmixVoice.CreateFauxMasteringVoice(this);

			/* Init 3D Audio */
			Handle3D = Audio3D.F3DAudioInitialize(DeviceDetails.OutputFormat.dwChannelMask, SpeedOfSound);

			AudioTweenManager = new AudioTweenManager();
			VoicePool = new SourceVoicePool(this);

			WakeSignal = new AutoResetEvent(true);

			Thread = new Thread(ThreadMain);
			Thread.IsBackground = true;
			Thread.Start();

			TimeElapsedStopwatch.Start();
			previousTickTime = 0;
		}

		private void ThreadMain()
		{
			Running = true;

			while (Running)
			{
				ThreadTimer.Restart();

				lock (StateLock)
				{
					try
					{
						ThreadMainTick();
					}
					catch (Exception e)
					{
						Logger.LogError(e.ToString());
					}
				}

				ThreadTimer.Stop();

				if (ThreadTimer.Elapsed < UpdateInterval)
				{
					WakeSignal.WaitOne(UpdateInterval - ThreadTimer.Elapsed);
				}
			}
		}

		private void ThreadMainTick()
		{
			long tickDelta = TimeElapsedStopwatch.Elapsed.Ticks - previousTickTime;
			previousTickTime = TimeElapsedStopwatch.Elapsed.Ticks;
			float elapsedSeconds = (float) tickDelta / System.TimeSpan.TicksPerSecond;

			AudioTweenManager.Update(elapsedSeconds);

			lock (StreamingAudioSources)
			{
				foreach (var voice in StreamingAudioSources)
				{
					voice.Update();
				}
			}

			if (Listener != null)
			{
				foreach (var voice in SpatialVoices)
				{
					if (voice.State == SoundState.Playing)
					{
						voice.Calculate3D();
					}
				}
			}

			foreach (var voice in TransientVoices)
			{
				if (voice.State == SoundState.Playing && voice.BuffersQueued == 0)
				{
					VoicesToReturn.Add(voice);
					TransientVoices.Remove(voice);
				}
			}

			foreach (var voice in VoicesToReturn)
			{
				voice.Reset();
				UnregisterSpatialVoice(voice);
				VoicePool.Return(voice);
			}

			VoicesToReturn.Clear();
		}

		/// <summary>
		/// Triggers all pending operations with the given syncGroup value.
		/// </summary>
		public void TriggerSyncGroup(uint syncGroup)
		{
			FAudio.FAudio_CommitChanges(Handle, syncGroup);
		}

		/// <summary>
		/// Sets a listener. There can only be one listener.
		/// A listener must be attached for spatial audio to function.
		/// </summary>
		public void AttachListener(Audio3D.Listener listener)
		{
			// TODO: should we allow multiple listeners?
			Listener = listener;
		}

		/// <summary>
		/// Obtains an appropriate source voice from the voice pool.
		/// </summary>
		/// <param name="format">The format that the voice must match.</param>
		/// <returns>A source voice with the given format.</returns>
		public T Obtain<T>(Format format) where T : SourceVoice, IPoolable<T>
		{
			lock (StateLock)
			{
				var voice = VoicePool.Obtain<T>(format);

				if (voice is TransientVoice transientVoice)
				{
					TransientVoices.Add(transientVoice);
				}

				return voice;
			}
		}

		/// <summary>
		/// Returns the source voice to the voice pool.
		/// </summary>
		/// <param name="voice"></param>
		internal void Return(SourceVoice voice)
		{
			lock (StateLock)
			{
				VoicesToReturn.Add(voice);
			}
		}

		internal void RegisterStreamingAudioSource(StreamingAudioSource source)
		{
			lock (StreamingAudioSources)
			{
				StreamingAudioSources.Add(source);
			}
		}

		internal void UnregisterStreamingAudioSource(StreamingAudioSource source)
		{
			lock (StreamingAudioSources)
			{
				StreamingAudioSources.Remove(source);
			}
		}

		internal void RegisterSpatialVoice(SourceVoice voice)
		{
			lock (SpatialVoices)
			{
				SpatialVoices.Add(voice);
			}
		}

		internal void UnregisterSpatialVoice(SourceVoice voice)
		{
			lock (SpatialVoices)
			{
				SpatialVoices.Remove(voice);
			}
		}

		internal void CreateTween(
			Voice voice,
			AudioTweenProperty property,
			System.Func<float, float> easingFunction,
			float start,
			float end,
			float duration,
			float delayTime
		) {
			lock (StateLock)
			{
				AudioTweenManager.CreateTween(
					voice,
					property,
					easingFunction,
					start,
					end,
					duration,
					delayTime
				);
			}
		}

		internal void ClearTweens(
			Voice voice,
			AudioTweenProperty property
		) {
			lock (StateLock)
			{
				AudioTweenManager.ClearTweens(voice, property);
			}
		}

		internal void WakeThread()
		{
			WakeSignal.Set();
		}

		internal void AddResourceReference(GCHandle resourceReference)
		{
			lock (StateLock)
			{
				ResourceHandles.Add(resourceReference);
			}
		}

		internal void RemoveResourceReference(GCHandle resourceReference)
		{
			lock (StateLock)
			{
				ResourceHandles.Remove(resourceReference);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Running = false;

				if (disposing)
				{
					Thread.Join();

					// dispose audio sources first
					foreach (var handle in ResourceHandles)
					{
						if (handle.Target is StreamingAudioSource streaming)
						{
							streaming.Dispose();
						}
					}

					// then dispose source voices
					foreach (var handle in ResourceHandles)
					{
						if (handle.Target is SourceVoice voice)
						{
							voice.Dispose();
						}
					}

					// dispose all submix voices except the faux mastering voice
					foreach (var handle in ResourceHandles)
					{
						if (handle.Target is SubmixVoice voice && voice != FauxMasteringVoice)
						{
							voice.Dispose();
						}
					}

					// dispose the faux mastering voice
					FauxMasteringVoice.Dispose();

					// dispose the true mastering voice
					FAudio.FAudioVoice_DestroyVoice(TrueMasteringVoice);

					// destroy all other audio resources
					foreach (var handle in ResourceHandles)
					{
						if (handle.Target is AudioResource resource)
						{
							resource.Dispose();
						}
					}

					ResourceHandles.Clear();
				}

				FAudio.FAudio_Release(Handle);

				IsDisposed = true;
			}
		}

		~AudioDevice()
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
