namespace MoonWorks.Input
{
	public class Axis
	{
		/// <summary>
		/// An axis value between -1 and 1.
		/// </summary>
		public float Value { get; private set; }

		internal void Update(float value)
		{
			Value = value;
		}
	}
}
