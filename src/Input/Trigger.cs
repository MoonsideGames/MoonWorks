namespace MoonWorks.Input
{
	public class Trigger
	{
		public float Value { get; private set; }

		internal void Update(float value)
		{
			Value = value;
		}
	}
}
