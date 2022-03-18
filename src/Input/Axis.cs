namespace MoonWorks.Input
{
	public class Axis
	{
		public float Value { get; private set; }

		internal void Update(float value)
		{
			Value = value;
		}
	}
}
