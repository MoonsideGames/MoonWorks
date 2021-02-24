namespace MoonWorks.Graphics
{
    /// <summary>
    /// Specifies how the graphics pipeline will make use of a shader.
    /// </summary>
    public struct ShaderStageState
    {
        public ShaderModule ShaderModule;
        public string EntryPointName;
        public uint UniformBufferSize;
    }
}
