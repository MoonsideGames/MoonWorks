using System;
using System.Collections.Generic;
using System.Threading;

namespace MoonWorks.Audio
{
	public class AudioDevice : IDisposable
	{
		public IntPtr Handle { get; }
		public byte[] Handle3D { get; }
		public IntPtr MasteringVoice { get; }
		public FAudio.FAudioDeviceDetails DeviceDetails { get; }

		public float CurveDistanceScalar = 1f;
		public float DopplerScale = 1f;
		public float SpeedOfSound = 343.5f;

		private float masteringVolume = 1f;
		public float MasteringVolume
		{
			get => masteringVolume;
			set
			{
				masteringVolume = value;
				FAudio.FAudioVoice_SetVolume(MasteringVoice, masteringVolume, 0);
			}
		}

		private readonly HashSet<WeakReference> resources = new HashSet<WeakReference>();
		private readonly HashSet<WeakReference> autoUpdateStreamingSoundReferences = new HashSet<WeakReference>();

		private AudioTweenManager AudioTweenManager;

		private const int Step = 200;
		private TimeSpan UpdateInterval;
		private System.Diagnostics.Stopwatch TickStopwatch = new System.Diagnostics.Stopwatch();
		private long previousTickTime;
		private Thread Thread;
		private AutoResetEvent WakeSignal;
		internal readonly object StateLock = new object();

		private bool IsDisposed;

		public unsafe AudioDevice()
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
			IntPtr masteringVoice;

			if (FAudio.FAudio_CreateMasteringVoice(
				Handle,
				out masteringVoice,
				FAudio.FAUDIO_DEFAULT_CHANNELS,
				FAudio.FAUDIO_DEFAULT_SAMPLERATE,
				0,
				i,
				IntPtr.Zero
			) != 0)
			{
				Logger.LogError("No mastering voice found!");
				FAudio.FAudio_Release(Handle);
				Handle = IntPtr.Zero;
				return;
			}

			MasteringVoice = masteringVoice;

			/* Init 3D Audio */

			Handle3D = new byte[FAudio.F3DAUDIO_HANDLE_BYTESIZE];
			FAudio.F3DAudioInitialize(
				DeviceDetails.OutputFormat.dwChannelMask,
				SpeedOfSound,
				Handle3D
			);

			AudioTweenManager = new AudioTweenManager();

			Logger.LogInfo("Setting up audio thread...");
			WakeSignal = new AutoResetEvent(true);

			Thread = new Thread(ThreadMain);
			Thread.IsBackground = true;
			Thread.Start();

			TickStopwatch.Start();
			previousTickTime = 0;
		}

		private void ThreadMain()
		{
			while (!IsDisposed)
			{
				lock (StateLock)
				{
					ThreadMainTick();
				}

				WakeSignal.WaitOne(UpdateInterval);
			}
		}

		private void ThreadMainTick()
		{
			long tickDelta = TickStopwatch.Elapsed.Ticks - previousTickTime;
			previousTickTime = TickStopwatch.Elapsed.Ticks;
			float elapsedSeconds = (float) tickDelta / System.TimeSpan.TicksPerSecond;

			foreach (var weakReference in autoUpdateStreamingSoundReferences)
			{
				if (weakReference.Target is StreamingSound streamingSound)
				{
					streamingSound.Update();
				}
				else
				{
					autoUpdateStreamingSoundReferences.Remove(weakReference);
				}
			}

			AudioTweenManager.Update(elapsedSeconds);
		}

		public void SyncPlay()
		{
			FAudio.FAudio_CommitChanges(Handle, 1);
		}

		internal void CreateTween(
			SoundInstance soundInstance,
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
					soundInstance,
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
			WeakReference soundReference,
			AudioTweenProperty property
		) {
			lock (StateLock)
			{
				AudioTweenManager.ClearTweens(soundReference, property);
			}
		}

		internal void WakeThread()
		{
			WakeSignal.Set();
		}

		internal void AddResourceReference(AudioResource resource)
		{
			lock (StateLock)
			{
				resources.Add(resource.weakReference);

				if (resource is StreamingSound streamingSound && streamingSound.AutoUpdate)
				{
					AddAutoUpdateStreamingSoundInstance(streamingSound);
				}
			}
		}

		internal void RemoveResourceReference(AudioResource resource)
		{
			lock (StateLock)
			{
				resources.Remove(resource.weakReference);

				if (resource is StreamingSound streamingSound && streamingSound.AutoUpdate)
				{
					RemoveAutoUpdateStreamingSoundInstance(streamingSound);
				}
			}
		}

		private void AddAutoUpdateStreamingSoundInstance(StreamingSound instance)
		{
			autoUpdateStreamingSoundReferences.Add(instance.weakReference);
		}

		private void RemoveAutoUpdateStreamingSoundInstance(StreamingSound instance)
		{
			autoUpdateStreamingSoundReferences.Remove(instance.weakReference);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				lock (StateLock)
				{
					if (disposing)
					{
						foreach (var weakReference in resources)
						{
							var target = weakReference.Target;

							if (target != null)
							{
								(target as IDisposable).Dispose();
							}
						}
						resources.Clear();
					}

					FAudio.FAudioVoice_DestroyVoice(MasteringVoice);
					FAudio.FAudio_Release(Handle);

					IsDisposed = true;
				}
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
