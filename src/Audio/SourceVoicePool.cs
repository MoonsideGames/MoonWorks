using System.Collections.Generic;

namespace MoonWorks.Audio
{
	internal class SourceVoicePool
	{
		private AudioDevice Device;

		Dictionary<(System.Type, Format), Queue<SourceVoice>> VoiceLists = new Dictionary<(System.Type, Format), Queue<SourceVoice>>();

		public SourceVoicePool(AudioDevice device)
		{
			Device = device;
		}

		public T Obtain<T>(Format format) where T : SourceVoice, IPoolable<T>
		{
			if (!VoiceLists.ContainsKey((typeof(T), format)))
			{
				VoiceLists.Add((typeof(T), format), new Queue<SourceVoice>());
			}

			var list = VoiceLists[(typeof(T), format)];

			if (list.Count == 0)
			{
				list.Enqueue(T.Create(Device, format));
			}

			return (T) list.Dequeue();
		}

		public void Return(SourceVoice voice)
		{
			var list = VoiceLists[(voice.GetType(), voice.Format)];
			list.Enqueue(voice);
		}
	}
}
