using System.IO;

namespace MoonWorks.Audio
{
	public static class AudioUtils
	{
		public struct WaveHeaderData
		{
			public int FileLength;
			public short FormatTag;
			public short Channels;
			public int SampleRate;
			public short BitsPerSample;
			public short BlockAlign;
			public int DataLength;
		}

		public static WaveHeaderData ReadWaveHeaderData(string filePath)
		{
			WaveHeaderData headerData;
			var fileInfo = new FileInfo(filePath);
			using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			using BinaryReader br = new BinaryReader(fs);

			headerData.FileLength = (int)fileInfo.Length - 8;
			fs.Position = 20;
			headerData.FormatTag = br.ReadInt16();
			fs.Position = 22;
			headerData.Channels = br.ReadInt16();
			fs.Position = 24;
			headerData.SampleRate = br.ReadInt32();
			fs.Position = 32;
			headerData.BlockAlign = br.ReadInt16();
			fs.Position = 34;
			headerData.BitsPerSample = br.ReadInt16();
			fs.Position = 40;
			headerData.DataLength = br.ReadInt32();

			return headerData;
		}
	}
}
