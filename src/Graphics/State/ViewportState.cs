namespace MoonWorks.Graphics
{
	/// <summary>
	/// Describes the dimensions of viewports and scissor areas.
	/// </summary>
	public struct ViewportState
	{
		public Viewport[] Viewports;
		public Rect[] Scissors;

		/// <summary>
		/// A default single viewport with no scissor area.
		/// </summary>
		public ViewportState(int width, int height)
		{
			Viewports = new Viewport[] { new Viewport(width, height) };
			Scissors = new Rect[] { new Rect(width, height) };
		}
	}
}
