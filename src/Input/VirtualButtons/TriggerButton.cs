namespace MoonWorks.Input
{
	public class TriggerButton : VirtualButton
	{
		public Trigger Parent { get; }
		public TriggerCode Code => Parent.Code;

		private float threshold = 0.7f;
		public float Threshold
		{
			get => threshold;
			set => threshold = System.Math.Clamp(value, 0, 1);
		}

		internal TriggerButton(Trigger parent)
		{
			Parent = parent;
		}

		internal override bool CheckPressed()
		{
			return Parent.Value >= Threshold;
		}
	}
}
