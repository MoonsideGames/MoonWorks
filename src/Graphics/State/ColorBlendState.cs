namespace MoonWorks.Graphics
{
	/// <summary>
	/// Describes how the graphics pipeline will blend colors.
	/// You must provide one ColorTargetBlendState per color target in the pipeline.
	/// </summary>
	public unsafe struct ColorBlendState
	{
		public bool LogicOpEnable;
		public LogicOp LogicOp;
		public BlendConstants BlendConstants;
		public ColorTargetBlendState[] ColorTargetBlendStates;
	}
}
