namespace MoonWorks.Input
{
	public class Trigger
	{
		/// <summary>
		/// A trigger value between 0 and 1.
		/// </summary>
		public float Value { get; private set; }

		internal void Update(float value)
		{
			Value = value;
		}
	}
}
