using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Specifies how many samples should be used in rasterization.
	/// </summary>
	public struct MultisampleState
	{
		public SampleCount MultisampleCount;
		public uint SampleMask;

		public static readonly MultisampleState None = new MultisampleState
		{
			MultisampleCount = SampleCount.One,
			SampleMask = uint.MaxValue
		};

		public MultisampleState(
			SampleCount sampleCount,
			uint sampleMask = uint.MaxValue
		) {
			MultisampleCount = sampleCount;
			SampleMask = sampleMask;
		}

		public Refresh.MultisampleState ToRefresh()
		{
			return new Refresh.MultisampleState
			{
				MultisampleCount = (Refresh.SampleCount) MultisampleCount,
				SampleMask = SampleMask
			};
		}
	}
}
