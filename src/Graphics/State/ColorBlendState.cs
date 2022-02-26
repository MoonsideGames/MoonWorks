namespace MoonWorks.Graphics
{
	/// <summary>
	/// Describes how the graphics pipeline will blend colors.
	/// </summary>
	public unsafe struct ColorBlendState
	{
		public bool LogicOpEnable;
		public LogicOp LogicOp;
		public BlendConstants BlendConstants;
	}
}
