using System;
using System.Numerics;

namespace MoonWorks.Graphics;

public static class PixelStructs
{
	public record struct A8Unorm(byte A);
	public record struct R8Unorm(byte R);
	public record struct R8G8Unorm(byte R, byte G);
	public record struct R8G8B8A8Unorm(byte R, byte G, byte B, byte A);
	public record struct R16Unorm(ushort R);
	public record struct R16G16Unorm(ushort R, ushort G);
	public record struct R16G16B16A16Unorm(ushort R, ushort G, ushort B, ushort A);
	public record struct R10G10B10A2Unorm(uint PackedValue)
	{
		public R10G10B10A2Unorm(float r, float g, float b, float a) : this(Pack(r, g, b, a)) { }

		static uint Pack(float r, float g, float b, float a)
		{
			return
				 BitwisePack<uint>(r, 10)        |
				(BitwisePack<uint>(g, 10) << 10) |
				(BitwisePack<uint>(b, 10) << 20) |
				(BitwisePack<uint>(a,  2) << 30);
		}
	}

	public record struct B5G6R5Unorm(ushort PackedValue)
	{
		public B5G6R5Unorm(float b, float g, float r) : this(Pack(b, g, r)) { }

		static ushort Pack(float b, float g, float r)
		{
			return (ushort) (
				(BitwisePack<ushort>(b, 5) << 11) |
				(BitwisePack<ushort>(g, 6) << 5)  |
				(BitwisePack<ushort>(r, 5))
			);
		}
	}

	public record struct B5G5R5A1Unorm(ushort PackedValue)
	{
		public B5G5R5A1Unorm(float b, float g, float r, float a) : this(Pack(b, g, r, a)) { }

		static ushort Pack(float b, float g, float r, float a)
		{
			return (ushort) (
				(BitwisePack<ushort>(b, 5) << 10) |
				(BitwisePack<ushort>(g, 5) << 5)  |
				(BitwisePack<ushort>(r, 5))       |
				(BitwisePack<ushort>(a, 1) << 15)
			);
		}
	}

	public record struct B4G4R4A4Unorm(ushort PackedValue)
	{
		public B4G4R4A4Unorm(float b, float g, float r, float a) : this(Pack(b, g, r, a)) { }

		static ushort Pack(float b, float g, float r, float a)
		{
			return (ushort) (
				(BitwisePack<ushort>(b, 4) << 8) |
				(BitwisePack<ushort>(g, 4) << 4) |
				(BitwisePack<ushort>(r, 4))      |
				(BitwisePack<ushort>(a, 4) << 12)
			);
		}
	}

	public record struct B8G8R8A8Unorm(byte B, byte G, byte R, byte A);
	public record struct R8Snorm(sbyte R);
	public record struct R8G8Snorm(sbyte R, sbyte G);
	public record struct R8G8B8A8Snorm(sbyte R, sbyte G, sbyte B, sbyte A);
	public record struct R16Snorm(short R);
	public record struct R16G16Snorm(short G);
	public record struct R16G16B16A16(short R, short G, short B, short A);
	public record struct R16Float(Half R);
	public record struct R16G16Float(Half R, Half G);
	public record struct R16G16B16A16Float(Half R, Half G, Half B, Half A);
	public record struct R32Float(float R);
	public record struct R32G32Float(float R, float G);
	public record struct R32G32B32A32Float(float R, float G, float B, float A);
	public record struct R8Uint(byte R);
	public record struct R8G8Uint(byte R, byte G);
	public record struct R8G8B8A8Uint(byte R, byte G, byte B, byte A);
	public record struct R16Uint(ushort R);
	public record struct R16G16Uint(ushort G);
	public record struct R16G16B16A16Uint(ushort R, ushort G, ushort B, ushort A);
	public record struct R32Uint(uint R);
	public record struct R32G32Uint(uint R, uint G);
	public record struct R32G32B32A32(uint R, uint G, uint B, uint A);
	public record struct R8Int(sbyte R);
	public record struct R8G8Int(sbyte R, sbyte G);
	public record struct R8G8B8A8Int(sbyte R, sbyte G, sbyte B, sbyte A);
	public record struct R16Int(short R);
	public record struct R16G16Int(short R, short G);
	public record struct R16G16B16A16Int(short R, short G, short B, short A);
	public record struct R32Int(int R);
	public record struct R32G32Int(int R, int G);
	public record struct R32G32B32A32Int(int R, int G, int B, int A);
	public record struct R8G8B8A8UnormSrgb(byte R, byte G, byte B, byte A);
	public record struct B8G8R8A8UnormSrgb(byte B, byte G, byte R, byte A);
	public record struct D16Unorm(ushort D);
	public record struct D32Float(float D);

	static T BitwisePack<T>(float value, int bits) where T : INumber<T>
	{
		return T.CreateChecked(System.MathF.Truncate(((1 << bits) - 1) * System.Math.Clamp(value, 0, 1)));
	}
}
