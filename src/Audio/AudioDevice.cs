using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

		private readonly List<WeakReference<AudioResource>> resources = new List<WeakReference<AudioResource>>();
		private readonly List<WeakReference<StreamingSound>> streamingSounds = new List<WeakReference<StreamingSound>>();

		private bool IsDisposed;

		public unsafe AudioDevice()
		{
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
				Handle = IntPtr.Zero;
				FAudio.FAudio_Release(Handle);
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
		}

		internal void Update()
		{
			for (var i = streamingSounds.Count - 1; i >= 0; i--)
			{
				var weakReference = streamingSounds[i];
				if (weakReference.TryGetTarget(out var streamingSound))
				{
					streamingSound.Update();
				}
				else
				{
					streamingSounds.RemoveAt(i);
				}
			}
		}

		public void SyncPlay()
		{
			FAudio.FAudio_CommitChanges(Handle, 1);
		}

		internal void AddDynamicSoundInstance(StreamingSound instance)
		{
			streamingSounds.Add(new WeakReference<StreamingSound>(instance));
		}

		internal void AddResourceReference(WeakReference<AudioResource> resourceReference)
		{
			lock (resources)
			{
				resources.Add(resourceReference);
			}
		}

		internal void RemoveResourceReference(WeakReference<AudioResource> resourceReference)
		{
			lock (resources)
			{
				resources.Remove(resourceReference);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					for (var i = resources.Count - 1; i >= 0; i--)
					{
						var weakReference = resources[i];

						if (weakReference.TryGetTarget(out var resource))
						{
							resource.Dispose();
						}
					}
					resources.Clear();
				}

				FAudio.FAudioVoice_DestroyVoice(MasteringVoice);
				FAudio.FAudio_Release(Handle);

				IsDisposed = true;
			}
		}

		// TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
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
