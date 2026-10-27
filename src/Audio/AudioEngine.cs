using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Miniaudio;

namespace MoonWorks.Audio
{
	/// <summary>
	/// AudioDevice manages all audio-related concerns.
	/// </summary>
	public unsafe class AudioEngine : IDisposable
	{
		public readonly Interop.ma_engine Engine;

		private readonly HashSet<GCHandle> ResourceHandles = [];

		private readonly AudioTweenManager AudioTweenManager;
		internal readonly object StateLock = new();

		private const int Step = 200;
		private readonly TimeSpan UpdateInterval = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / Step);
		private readonly Thread Thread;
		private readonly AutoResetEvent WakeSignal;
		private readonly System.Diagnostics.Stopwatch ThreadTimer = new();
		private readonly System.Diagnostics.Stopwatch TimeElapsedStopwatch = new();
		private long previousTickTime;

		private bool Running;
		public bool IsDisposed { get; private set; }

		internal AudioEngine()
		{
			var result = Interop.ma_engine_init(Unsafe.NullRef<Interop.ma_engine_config>(), out Engine);
			if (result != Interop.ma_result.MA_SUCCESS)
			{
				Logger.LogError("Failed to initialize audio engine!");

				// TODO: integrate with SDL for audio
				// Logger.LogError("No audio devices found! Setting dummy driver!");
				// SDL3.SDL.SDL_SetHint("SDL_AUDIO_DRIVER", "dummy");
			}

			AudioTweenManager = new AudioTweenManager();

			WakeSignal = new AutoResetEvent(true);

			Thread = new Thread(ThreadMain)
			{
				IsBackground = true
			};
			Thread.Start();
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
		}

		public void PlaySound(string filename)
		{

		}

		public static bool DecodeMemory(Span<byte> data, out ulong frameCount, out IntPtr pcmFramesOut)
		{
			// FIXME: would be nice for this to just stay as a span
			fixed (byte* b = data)
			{
				var result = Interop.ma_decode_memory((nint) b, (nuint) data.Length, Unsafe.NullRef<Interop.ma_decoder_config>(), out frameCount, out pcmFramesOut);
				if (result != Interop.ma_result.MA_SUCCESS)
				{
					return false;
				}

				return true;
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
						if (handle.Target is SubmixVoice voice)
						{
							voice.Dispose();
						}
					}

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

				Interop.ma_engine_uninit(Engine);
				IsDisposed = true;
			}
		}

		~AudioEngine()
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
