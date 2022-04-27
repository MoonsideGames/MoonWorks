using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Information that the pipeline needs about a shader.
	/// </summary>
	public struct GraphicsShaderInfo
	{
		public ShaderModule ShaderModule;
		public string EntryPointName;
		public uint UniformBufferSize;
		public uint SamplerBindingCount;

		public unsafe static GraphicsShaderInfo Create<T>(
			ShaderModule shaderModule,
			string entryPointName,
			uint samplerBindingCount
		) where T : unmanaged
		{
			return new GraphicsShaderInfo
			{
				ShaderModule = shaderModule,
				EntryPointName = entryPointName,
				UniformBufferSize = (uint) sizeof(T),
				SamplerBindingCount = samplerBindingCount
			};
		}

		public static GraphicsShaderInfo Create(
			ShaderModule shaderModule,
			string entryPointName,
			uint samplerBindingCount
		) {
			return new GraphicsShaderInfo
			{
				ShaderModule = shaderModule,
				EntryPointName = entryPointName,
				UniformBufferSize = 0,
				SamplerBindingCount = samplerBindingCount
			};
		}
	}
}
