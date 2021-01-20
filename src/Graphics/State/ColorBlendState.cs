using RefreshCS;

namespace MoonWorks.Graphics
{
    public unsafe struct ColorBlendState
    {
        public bool LogicOpEnable;
        public Refresh.LogicOp LogicOp;
        public BlendConstants BlendConstants;
        public ColorTargetBlendState[] ColorTargetBlendStates;
    }
}
