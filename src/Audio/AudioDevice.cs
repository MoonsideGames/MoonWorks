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
		public byte[] Handle3D { get; }
		public FAudio.FAudioDeviceDetails DeviceDetails { get; }

		private IntPtr trueMasteringVoice;

		// this is a fun little trick where we use a submix voice as a "faux" mastering voice
		// this lets us maintain API consistency for effects like panning and reverb
		private SubmixVoice fauxMasteringVoice;
		public SubmixVoice MasteringVoice => fauxMasteringVoice;

		public float CurveDistanceScalar = 1f;
		public float DopplerScale = 1f;
		public float SpeedOfSound = 343.5f;

		private readonly HashSet<GCHandle> resourceHandles = new HashSet<GCHandle>();
		private readonly HashSet<UpdatingSourceVoice> updatingSourceVoices = new HashSet<UpdatingSourceVoice>();

		private AudioTweenManager AudioTweenManager;

		private SourceVoicePool VoicePool;
		private List<SourceVoice> VoicesToReturn = new List<SourceVoice>();

		private const int Step = 200;
		private TimeSpan UpdateInterval;
		private System.Diagnostics.Stopwatch TickStopwatch = new System.Diagnostics.Stopwatch();
		private long previousTickTime;
		private Thread Thread;
		private AutoResetEvent WakeSignal;
		internal readonly object StateLock = new object();

		private bool Running;
		public bool IsDisposed { get; private set; }

		internal unsafe AudioDevice()
		{
			UpdateInterval = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / Step);

			FAudio.FAudioCreate(out var handle, 0, FAudio.FAUDIO_DEFAULT_PROCESSOR);
			Handle = handle;

			/* Find a suitable device */

			FAudio.FAudio_GetDeviceCount(Handle, out var devices);

			if (devices == 0)
			{
				Logger.LogError("No audio devices found!");
				FAudio.FAudio_Release(Handle);
				Handle = IntPtr.Zero;
				return;
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
				out trueMasteringVoice,
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

			fauxMasteringVoice = SubmixVoice.CreateFauxMasteringVoice(this);

			/* Init 3D Audio */

			Handle3D = new byte[FAudio.F3DAUDIO_HANDLE_BYTESIZE];
			FAudio.F3DAudioInitialize(
				DeviceDetails.OutputFormat.dwChannelMask,
				SpeedOfSound,
				Handle3D
			);

			AudioTweenManager = new AudioTweenManager();
			VoicePool = new SourceVoicePool(this);

			WakeSignal = new AutoResetEvent(true);

			Thread = new Thread(ThreadMain);
			Thread.IsBackground = true;
			Thread.Start();

			Running = true;

			TickStopwatch.Start();
			previousTickTime = 0;
		}

		private void ThreadMain()
		{
			while (Running)
			{
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

				WakeSignal.WaitOne(UpdateInterval);
			}
		}

		private void ThreadMainTick()
		{
			long tickDelta = TickStopwatch.Elapsed.Ticks - previousTickTime;
			previousTickTime = TickStopwatch.Elapsed.Ticks;
			float elapsedSeconds = (float) tickDelta / System.TimeSpan.TicksPerSecond;

			AudioTweenManager.Update(elapsedSeconds);

			foreach (var voice in updatingSourceVoices)
			{
				voice.Update();
			}

			foreach (var voice in VoicesToReturn)
			{
				if (voice is UpdatingSourceVoice updatingSourceVoice)
				{
					updatingSourceVoices.Remove(updatingSourceVoice);
				}

				voice.Reset();
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
		/// Obtains an appropriate source voice from the voice pool.
		/// </summary>
		/// <param name="format">The format that the voice must match.</param>
		/// <returns>A source voice with the given format.</returns>
		public T Obtain<T>(Format format) where T : SourceVoice, IPoolable<T>
		{
			lock (StateLock)
			{
				var voice = VoicePool.Obtain<T>(format);

				if (voice is UpdatingSourceVoice updatingSourceVoice)
				{
					updatingSourceVoices.Add(updatingSourceVoice);
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
				resourceHandles.Add(resourceReference);

				if (resourceReference.Target is UpdatingSourceVoice updatableVoice)
				{
					updatingSourceVoices.Add(updatableVoice);
				}
			}
		}

		internal void RemoveResourceReference(GCHandle resourceReference)
		{
			lock (StateLock)
			{
				resourceHandles.Remove(resourceReference);

				if (resourceReference.Target is UpdatingSourceVoice updatableVoice)
				{
					updatingSourceVoices.Remove(updatableVoice);
				}
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

					// dispose all source voices first
					foreach (var handle in resourceHandles)
					{
						if (handle.Target is SourceVoice voice)
						{
							voice.Dispose();
						}
					}

					// dispose all submix voices except the faux mastering voice
					foreach (var handle in resourceHandles)
					{
						if (handle.Target is SubmixVoice voice && voice != fauxMasteringVoice)
						{
							voice.Dispose();
						}
					}

					// dispose the faux mastering voice
					fauxMasteringVoice.Dispose();

					// dispose the true mastering voice
					FAudio.FAudioVoice_DestroyVoice(trueMasteringVoice);

					// destroy all other audio resources
					foreach (var handle in resourceHandles)
					{
						if (handle.Target is AudioResource resource)
						{
							resource.Dispose();
						}
					}

					resourceHandles.Clear();
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
