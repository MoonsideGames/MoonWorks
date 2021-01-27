namespace MoonWorks.Graphics
{
    public unsafe struct ColorBlendState
    {
        public bool LogicOpEnable;
        public LogicOp LogicOp;
        public BlendConstants BlendConstants;
        public ColorTargetBlendState[] ColorTargetBlendStates;
    }
}
