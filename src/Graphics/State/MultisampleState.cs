using RefreshCS;

namespace MoonWorks.Graphics
{
    public struct MultisampleState
    {
        public Refresh.SampleCount MultisampleCount;
        public uint SampleMask;

        public static readonly MultisampleState None = new MultisampleState
        {
            MultisampleCount = Refresh.SampleCount.One,
            SampleMask = uint.MaxValue
        };
    }
}
