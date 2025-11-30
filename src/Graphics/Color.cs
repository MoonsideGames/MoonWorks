/* MoonWorks - Game Development Framework
 * Copyright 2021 Evan Hemsley
 */

/* Derived from code by Ethan Lee (Copyright 2009-2021).
 * Released under the Microsoft Public License.
 * See fna.LICENSE for details.

 * Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */

using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics;

/// <summary>
/// Describes a 32-bit packed color.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 4, Pack = 4)]
public record struct Color(byte R, byte G, byte B, byte A = 255)
{
	public Color(int r, int g, int b, int a = 255) : this(
		(byte) int.Clamp(r, 0, 255),
		(byte) int.Clamp(g, 0, 255),
		(byte) int.Clamp(b, 0, 255),
		(byte) int.Clamp(a, 0, 255)
	) { }

	/// <summary>
	/// Constructs an RGBA color from scalars which represent red, green, blue and alpha values.
	/// </summary>
	/// <param name="r">Red component value from 0.0f to 1.0f.</param>
	/// <param name="g">Green component value from 0.0f to 1.0f.</param>
	/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
	/// <param name="alpha">Alpha component value from 0.0f to 1.0f.</param>
	public Color(float r, float g, float b, float a) : this(
		(byte) (System.Math.Clamp(r, 0, 1) * byte.MaxValue),
		(byte) (System.Math.Clamp(g, 0, 1) * byte.MaxValue),
		(byte) (System.Math.Clamp(b, 0, 1) * byte.MaxValue),
		(byte) (System.Math.Clamp(a, 0, 1) * byte.MaxValue)
	) { }

	/// <summary>
	/// Constructs an RGBA color from scalars which represent red, green, blue values. Alpha is assumed to be 1.
	/// </summary>
	/// <param name="r">Red component value from 0.0f to 1.0f.</param>
	/// <param name="g">Green component value from 0.0f to 1.0f.</param>
	/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
	public Color(float r, float g, float b) : this(
		(byte) (System.Math.Clamp(r, 0, 1) * byte.MaxValue),
		(byte) (System.Math.Clamp(g, 0, 1) * byte.MaxValue),
		(byte) (System.Math.Clamp(b, 0, 1) * byte.MaxValue),
		(byte) 255
	) { }

	public Color(Vector4 vector) : this(vector.X, vector.Y, vector.Z, vector.W) { }

	/// <summary>
	/// Gets packed RGBA value of this <see cref="Color"/>.
	/// NOTE: Because of endianness, if you format this value directly as a hex string, 
	/// the order of bytes may be wrong.
	/// </summary>
	public readonly uint PackedValue() => Unsafe.BitCast<Color, uint>(this);

	/// <summary>
	/// Gets a <see cref="Color"/> representation for a packed RGBA value.
	/// </summary>
	public static Color FromPacked(uint rgba) => Unsafe.BitCast<uint, Color>(rgba);

	/// <summary>
	/// Gets a <see cref="Vector3"/> representation for this object.
	/// </summary>
	/// <returns>A <see cref="Vector3"/> representation for this object.</returns>
	public Vector3 ToVector3()
	{
		return new Vector3(R / 255.0f, G / 255.0f, B / 255.0f);
	}

	/// <summary>
	/// Gets a <see cref="Vector4"/> representation for this object.
	/// </summary>
	/// <returns>A <see cref="Vector4"/> representation for this object.</returns>
	public Vector4 ToVector4()
	{
		return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
	}

	/// <summary>
	/// Performs linear interpolation of <see cref="Color"/>.
	/// </summary>
	/// <param name="value1">Source <see cref="Color"/>.</param>
	/// <param name="value2">Destination <see cref="Color"/>.</param>
	/// <param name="amount">Interpolation factor.</param>
	/// <returns>Interpolated <see cref="Color"/>.</returns>
	public static Color Lerp(Color value1, Color value2, float amount)
	{
		amount = float.Clamp(amount, 0.0f, 1.0f);
		return new Color(
			float.Lerp(value1.R / 255f, value2.R / 255f, amount),
			float.Lerp(value1.G / 255f, value2.G / 255f, amount),
			float.Lerp(value1.B / 255f, value2.B / 255f, amount),
			float.Lerp(value1.A / 255f, value2.A / 255f, amount)
		);
	}

	/// <summary>
	/// Translate a non-premultipled alpha <see cref="Color"/> to a <see cref="Color"/>
	/// that contains premultiplied alpha.
	/// </summary>
	/// <param name="vector">A <see cref="Vector4"/> representing color.</param>
	/// <returns>A <see cref="Color"/> which contains premultiplied alpha data.</returns>
	public static Color FromNonPremultiplied(System.Numerics.Vector4 vector)
	{
		return new Color(
			vector.X * vector.W,
			vector.Y * vector.W,
			vector.Z * vector.W,
			vector.W
		);
	}

	/// <summary>
	/// Translate a non-premultipled alpha <see cref="Color"/> to a <see cref="Color"/>
	/// that contains premultiplied alpha.
	/// </summary>
	/// <param name="r">Red component value.</param>
	/// <param name="g">Green component value.</param>
	/// <param name="b">Blue component value.</param>
	/// <param name="a">Alpha component value.</param>
	/// <returns>A <see cref="Color"/> which contains premultiplied alpha data.</returns>
	public static Color FromNonPremultiplied(byte r, byte g, byte b, byte a)
	{
		return new Color(
			r * a / 255,
			g * a / 255,
			b * a / 255,
			a
		);
	}

	// Modified from one of the responses here:
	// https://stackoverflow.com/questions/3018313/algorithm-to-convert-rgb-to-hsv-and-hsv-to-rgb-in-range-0-255-for-both/6930407#6930407
	public static Color FromHSV(float r, float g, float b)
	{
		r = (100 + r) % 1f;

		float hueSlice = 6 * r; // [0, 6)
		float hueSliceInteger = MathF.Floor(hueSlice);

		// In [0,1) for each hue slice
		float hueSliceInterpolant = hueSlice - hueSliceInteger;

		Vector3 tempRGB = new Vector3(
			b * (1f - g),
			b * (1f - g * hueSliceInterpolant),
			b * (1f - g * (1f - hueSliceInterpolant))
		);

		// The idea here to avoid conditions is to notice that the conversion code can be rewritten:
		//    if      ( var_i == 0 ) { R = V         ; G = TempRGB.z ; B = TempRGB.x }
		//    else if ( var_i == 2 ) { R = TempRGB.x ; G = V         ; B = TempRGB.z }
		//    else if ( var_i == 4 ) { R = TempRGB.z ; G = TempRGB.x ; B = V     }
		//
		//    else if ( var_i == 1 ) { R = TempRGB.y ; G = V         ; B = TempRGB.x }
		//    else if ( var_i == 3 ) { R = TempRGB.x ; G = TempRGB.y ; B = V     }
		//    else if ( var_i == 5 ) { R = V         ; G = TempRGB.x ; B = TempRGB.y }
		//
		// This shows several things:
		//  . A separation between even and odd slices
		//  . If slices (0,2,4) and (1,3,5) can be rewritten as basically being slices (0,1,2) then
		//      the operation simply amounts to performing a "rotate right" on the RGB components
		//  . The base value to rotate is either (V, B, R) for even slices or (G, V, R) for odd slices
		//
		float isOddSlice =  hueSliceInteger % 2f;                          // 0 if even (slices 0, 2, 4), 1 if odd (slices 1, 3, 5)
		float threeSliceSelector = 0.5f * (hueSliceInteger - isOddSlice);  // (0, 1, 2) corresponding to slices (0, 2, 4) and (1, 3, 5)

		Vector3 scrollingRGBForEvenSlices = new Vector3(b, tempRGB.Z, tempRGB.X); // (V, Temp Blue, Temp Red) for even slices (0, 2, 4)
		Vector3 scrollingRGBForOddSlices = new Vector3(tempRGB.Y, b, tempRGB.X);  // (Temp Green, V, Temp Red) for odd slices (1, 3, 5)
		Vector3 scrollingRGB = Vector3.Lerp(scrollingRGBForEvenSlices, scrollingRGBForOddSlices, isOddSlice);

		float IsNotFirstSlice = float.Clamp(threeSliceSelector, 0f, 1f);        // 1 if NOT the first slice (true for slices 1 and 2)
		float IsNotSecondSlice = float.Clamp(threeSliceSelector - 1f, 0f, 1f);  // 1 if NOT the first or second slice (true only for slice 2)

		Vector3 color = Vector3.Lerp(
			scrollingRGB,
			Vector3.Lerp(
				new Vector3(scrollingRGB.Z, scrollingRGB.X, scrollingRGB.Y),
				new Vector3(scrollingRGB.Y, scrollingRGB.Z, scrollingRGB.X),
				IsNotSecondSlice
			),
			IsNotFirstSlice
		);

		return new Color(color.X, color.Y, color.Z, 1f);
	}

	public static Color FromHSV(int r, int g, int b)
	{
		return FromHSV(r / 255f, g / 255f, b / 255f);
	}

	/// <summary>
	/// Multiply <see cref="Color"/> by value.
	/// </summary>
	/// <param name="value">Source <see cref="Color"/>.</param>
	/// <param name="scale">Multiplicator.</param>
	/// <returns>Multiplication result.</returns>
	public static Color operator *(Color value, float scale)
	{
		return new Color(
			value.R * scale / 255f,
			value.G * scale / 255f,
			value.B * scale / 255f,
			value.A * scale / 255f
		);
	}

	/// <summary>
	/// Returns a <see cref="String"/> representation of this <see cref="Color"/> in the format:
	/// {R:[red] G:[green] B:[blue] A:[alpha]}
	/// </summary>
	/// <returns><see cref="String"/> representation of this <see cref="Color"/>.</returns>
	public override string ToString()
	{
		StringBuilder sb = new StringBuilder(25);
		sb.Append("{R:");
		sb.Append(R);
		sb.Append(" G:");
		sb.Append(G);
		sb.Append(" B:");
		sb.Append(B);
		sb.Append(" A:");
		sb.Append(A);
		sb.Append("}");
		return sb.ToString();
	}

	/// <summary>
	/// Returns a hexadecimal <see cref="String"/> representation of this <see cref="Color"/> in the format:
	/// 0x[R][G][B][A]
	/// </summary>
	/// <returns><see cref="String"/> representation of this <see cref="Color"/>.</returns>
	public string ToHexString()
	{
		StringBuilder sb = new StringBuilder(25);
		sb.Append("#");
		sb.Append(R.ToString("X2"));
		sb.Append(G.ToString("X2"));
		sb.Append(B.ToString("X2"));
		sb.Append(A.ToString("X2"));
		return sb.ToString();
	}

	public static implicit operator VertexStructs.Ubyte4Norm(Color color) => new VertexStructs.Ubyte4Norm
	{
		X = color.R,
		Y = color.G,
		Z = color.B,
		W = color.A
	};

	public static implicit operator FColor(Color color) => new FColor
	{
		R = color.R / 255f,
		G = color.G / 255f,
		B = color.B / 255f,
		A = color.A / 255f
	};


#region Built-In Colors
	/// <summary>
	/// Transparent color (R:255, G:255, B:255, A:0).
	/// RGBA Hex: #FFFFFF00.
	/// </summary>
	public static Color Transparent => FromPacked(0xFFFFFF00u);

	/// <summary>
	/// AliceBlue color (R:240, G:248, B:255, A:255).
	/// RGBA Hex: #F0F8FFFF.
	/// </summary>
	public static Color AliceBlue => FromPacked(0xF0F8FFFFu);

	/// <summary>
	/// AntiqueWhite color (R:250, G:235, B:215, A:255).
	/// RGBA Hex: #FAEBD7FF.
	/// </summary>
	public static Color AntiqueWhite => FromPacked(0xFAEBD7FFu);

	/// <summary>
	/// Aqua color (R:0, G:255, B:255, A:255).
	/// RGBA Hex: #00FFFFFF.
	/// </summary>
	public static Color Aqua => FromPacked(0x00FFFFFFu);

	/// <summary>
	/// Aquamarine color (R:127, G:255, B:212, A:255).
	/// RGBA Hex: #7FFFD4FF.
	/// </summary>
	public static Color Aquamarine => FromPacked(0x7FFFD4FFu);

	/// <summary>
	/// Azure color (R:240, G:255, B:255, A:255).
	/// RGBA Hex: #F0FFFFFF.
	/// </summary>
	public static Color Azure => FromPacked(0xF0FFFFFFu);

	/// <summary>
	/// Beige color (R:245, G:245, B:220, A:255).
	/// RGBA Hex: #F5F5DCFF.
	/// </summary>
	public static Color Beige => FromPacked(0xF5F5DCFFu);

	/// <summary>
	/// Bisque color (R:255, G:228, B:196, A:255).
	/// RGBA Hex: #FFE4C4FF.
	/// </summary>
	public static Color Bisque => FromPacked(0xFFE4C4FFu);

	/// <summary>
	/// Black color (R:0, G:0, B:0, A:255).
	/// RGBA Hex: #000000FF.
	/// </summary>
	public static Color Black => FromPacked(0x000000FFu);

	/// <summary>
	/// BlanchedAlmond color (R:255, G:235, B:205, A:255).
	/// RGBA Hex: #FFEBCDFF.
	/// </summary>
	public static Color BlanchedAlmond => FromPacked(0xFFEBCDFFu);

	/// <summary>
	/// Blue color (R:0, G:0, B:255, A:255).
	/// RGBA Hex: #0000FFFF.
	/// </summary>
	public static Color Blue => FromPacked(0x0000FFFFu);

	/// <summary>
	/// BlueViolet color (R:138, G:43, B:226, A:255).
	/// RGBA Hex: #8A2BE2FF.
	/// </summary>
	public static Color BlueViolet => FromPacked(0x8A2BE2FFu);

	/// <summary>
	/// Brown color (R:165, G:42, B:42, A:255).
	/// RGBA Hex: #A52A2AFF.
	/// </summary>
	public static Color Brown => FromPacked(0xA52A2AFFu);

	/// <summary>
	/// BurlyWood color (R:222, G:184, B:135, A:255).
	/// RGBA Hex: #DEB887FF.
	/// </summary>
	public static Color BurlyWood => FromPacked(0xDEB887FFu);

	/// <summary>
	/// CadetBlue color (R:95, G:158, B:160, A:255).
	/// RGBA Hex: #5F9EA0FF.
	/// </summary>
	public static Color CadetBlue => FromPacked(0x5F9EA0FFu);

	/// <summary>
	/// Chartreuse color (R:127, G:255, B:0, A:255).
	/// RGBA Hex: #7FFF00FF.
	/// </summary>
	public static Color Chartreuse => FromPacked(0x7FFF00FFu);

	/// <summary>
	/// Chocolate color (R:210, G:105, B:30, A:255).
	/// RGBA Hex: #D2691EFF.
	/// </summary>
	public static Color Chocolate => FromPacked(0xD2691EFFu);

	/// <summary>
	/// Coral color (R:255, G:127, B:80, A:255).
	/// RGBA Hex: #FF7F50FF.
	/// </summary>
	public static Color Coral => FromPacked(0xFF7F50FFu);

	/// <summary>
	/// CornflowerBlue color (R:100, G:149, B:237, A:255).
	/// RGBA Hex: #6495EDFF.
	/// </summary>
	public static Color CornflowerBlue => FromPacked(0x6495EDFFu);

	/// <summary>
	/// Cornsilk color (R:255, G:248, B:220, A:255).
	/// RGBA Hex: #FFF8DCFF.
	/// </summary>
	public static Color Cornsilk => FromPacked(0xFFF8DCFFu);

	/// <summary>
	/// Crimson color (R:220, G:20, B:60, A:255).
	/// RGBA Hex: #DC143CFF.
	/// </summary>
	public static Color Crimson => FromPacked(0xDC143CFFu);

	/// <summary>
	/// Cyan color (R:0, G:255, B:255, A:255).
	/// RGBA Hex: #00FFFFFF.
	/// </summary>
	public static Color Cyan => FromPacked(0x00FFFFFFu);

	/// <summary>
	/// DarkBlue color (R:0, G:0, B:139, A:255).
	/// RGBA Hex: #00008BFF.
	/// </summary>
	public static Color DarkBlue => FromPacked(0x00008BFFu);

	/// <summary>
	/// DarkCyan color (R:0, G:139, B:139, A:255).
	/// RGBA Hex: #008B8BFF.
	/// </summary>
	public static Color DarkCyan => FromPacked(0x008B8BFFu);

	/// <summary>
	/// DarkGoldenrod color (R:184, G:134, B:11, A:255).
	/// RGBA Hex: #B8860BFF.
	/// </summary>
	public static Color DarkGoldenrod => FromPacked(0xB8860BFFu);

	/// <summary>
	/// DarkGray color (R:169, G:169, B:169, A:255).
	/// RGBA Hex: #A9A9A9FF.
	/// </summary>
	public static Color DarkGray => FromPacked(0xA9A9A9FFu);

	/// <summary>
	/// DarkGreen color (R:0, G:100, B:0, A:255).
	/// RGBA Hex: #006400FF.
	/// </summary>
	public static Color DarkGreen => FromPacked(0x006400FFu);

	/// <summary>
	/// DarkKhaki color (R:189, G:183, B:107, A:255).
	/// RGBA Hex: #BDB76BFF.
	/// </summary>
	public static Color DarkKhaki => FromPacked(0xBDB76BFFu);

	/// <summary>
	/// DarkMagenta color (R:139, G:0, B:139, A:255).
	/// RGBA Hex: #8B008BFF.
	/// </summary>
	public static Color DarkMagenta => FromPacked(0x8B008BFFu);

	/// <summary>
	/// DarkOliveGreen color (R:85, G:107, B:47, A:255).
	/// RGBA Hex: #556B2FFF.
	/// </summary>
	public static Color DarkOliveGreen => FromPacked(0x556B2FFFu);

	/// <summary>
	/// DarkOrange color (R:255, G:140, B:0, A:255).
	/// RGBA Hex: #FF8C00FF.
	/// </summary>
	public static Color DarkOrange => FromPacked(0xFF8C00FFu);

	/// <summary>
	/// DarkOrchid color (R:153, G:50, B:204, A:255).
	/// RGBA Hex: #9932CCFF.
	/// </summary>
	public static Color DarkOrchid => FromPacked(0x9932CCFFu);

	/// <summary>
	/// DarkRed color (R:139, G:0, B:0, A:255).
	/// RGBA Hex: #8B0000FF.
	/// </summary>
	public static Color DarkRed => FromPacked(0x8B0000FFu);

	/// <summary>
	/// DarkSalmon color (R:233, G:150, B:122, A:255).
	/// RGBA Hex: #E9967AFF.
	/// </summary>
	public static Color DarkSalmon => FromPacked(0xE9967AFFu);

	/// <summary>
	/// DarkSeaGreen color (R:143, G:188, B:143, A:255).
	/// RGBA Hex: #8FBC8FFF.
	/// </summary>
	public static Color DarkSeaGreen => FromPacked(0x8FBC8FFFu);

	/// <summary>
	/// DarkSlateBlue color (R:72, G:61, B:139, A:255).
	/// RGBA Hex: #483D8BFF.
	/// </summary>
	public static Color DarkSlateBlue => FromPacked(0x483D8BFFu);

	/// <summary>
	/// DarkSlateGray color (R:47, G:79, B:79, A:255).
	/// RGBA Hex: #2F4F4FFF.
	/// </summary>
	public static Color DarkSlateGray => FromPacked(0x2F4F4FFFu);

	/// <summary>
	/// DarkTurquoise color (R:0, G:206, B:209, A:255).
	/// RGBA Hex: #00CED1FF.
	/// </summary>
	public static Color DarkTurquoise => FromPacked(0x00CED1FFu);

	/// <summary>
	/// DarkViolet color (R:148, G:0, B:211, A:255).
	/// RGBA Hex: #9400D3FF.
	/// </summary>
	public static Color DarkViolet => FromPacked(0x9400D3FFu);

	/// <summary>
	/// DeepPink color (R:255, G:20, B:147, A:255).
	/// RGBA Hex: #FF1493FF.
	/// </summary>
	public static Color DeepPink => FromPacked(0xFF1493FFu);

	/// <summary>
	/// DeepSkyBlue color (R:0, G:191, B:255, A:255).
	/// RGBA Hex: #00BFFFFF.
	/// </summary>
	public static Color DeepSkyBlue => FromPacked(0x00BFFFFFu);

	/// <summary>
	/// DimGray color (R:105, G:105, B:105, A:255).
	/// RGBA Hex: #696969FF.
	/// </summary>
	public static Color DimGray => FromPacked(0x696969FFu);

	/// <summary>
	/// DodgerBlue color (R:30, G:144, B:255, A:255).
	/// RGBA Hex: #1E90FFFF.
	/// </summary>
	public static Color DodgerBlue => FromPacked(0x1E90FFFFu);

	/// <summary>
	/// Firebrick color (R:178, G:34, B:34, A:255).
	/// RGBA Hex: #B22222FF.
	/// </summary>
	public static Color Firebrick => FromPacked(0xB22222FFu);

	/// <summary>
	/// FloralWhite color (R:255, G:250, B:240, A:255).
	/// RGBA Hex: #FFFAF0FF.
	/// </summary>
	public static Color FloralWhite => FromPacked(0xFFFAF0FFu);

	/// <summary>
	/// ForestGreen color (R:34, G:139, B:34, A:255).
	/// RGBA Hex: #228B22FF.
	/// </summary>
	public static Color ForestGreen => FromPacked(0x228B22FFu);

	/// <summary>
	/// Fuchsia color (R:255, G:0, B:255, A:255).
	/// RGBA Hex: #FF00FFFF.
	/// </summary>
	public static Color Fuchsia => FromPacked(0xFF00FFFFu);

	/// <summary>
	/// Gainsboro color (R:220, G:220, B:220, A:255).
	/// RGBA Hex: #DCDCDCFF.
	/// </summary>
	public static Color Gainsboro => FromPacked(0xDCDCDCFFu);

	/// <summary>
	/// GhostWhite color (R:248, G:248, B:255, A:255).
	/// RGBA Hex: #F8F8FFFF.
	/// </summary>
	public static Color GhostWhite => FromPacked(0xF8F8FFFFu);

	/// <summary>
	/// Gold color (R:255, G:215, B:0, A:255).
	/// RGBA Hex: #FFD700FF.
	/// </summary>
	public static Color Gold => FromPacked(0xFFD700FFu);

	/// <summary>
	/// Goldenrod color (R:218, G:165, B:32, A:255).
	/// RGBA Hex: #DAA520FF.
	/// </summary>
	public static Color Goldenrod => FromPacked(0xDAA520FFu);

	/// <summary>
	/// Gray color (R:128, G:128, B:128, A:255).
	/// RGBA Hex: #808080FF.
	/// </summary>
	public static Color Gray => FromPacked(0x808080FFu);

	/// <summary>
	/// Green color (R:0, G:128, B:0, A:255).
	/// RGBA Hex: #008000FF.
	/// </summary>
	public static Color Green => FromPacked(0x008000FFu);

	/// <summary>
	/// GreenYellow color (R:173, G:255, B:47, A:255).
	/// RGBA Hex: #ADFF2FFF.
	/// </summary>
	public static Color GreenYellow => FromPacked(0xADFF2FFFu);

	/// <summary>
	/// Honeydew color (R:240, G:255, B:240, A:255).
	/// RGBA Hex: #F0FFF0FF.
	/// </summary>
	public static Color Honeydew => FromPacked(0xF0FFF0FFu);

	/// <summary>
	/// HotPink color (R:255, G:105, B:180, A:255).
	/// RGBA Hex: #FF69B4FF.
	/// </summary>
	public static Color HotPink => FromPacked(0xFF69B4FFu);

	/// <summary>
	/// IndianRed color (R:205, G:92, B:92, A:255).
	/// RGBA Hex: #CD5C5CFF.
	/// </summary>
	public static Color IndianRed => FromPacked(0xCD5C5CFFu);

	/// <summary>
	/// Indigo color (R:75, G:0, B:130, A:255).
	/// RGBA Hex: #4B0082FF.
	/// </summary>
	public static Color Indigo => FromPacked(0x4B0082FFu);

	/// <summary>
	/// Ivory color (R:255, G:255, B:240, A:255).
	/// RGBA Hex: #FFFFF0FF.
	/// </summary>
	public static Color Ivory => FromPacked(0xFFFFF0FFu);

	/// <summary>
	/// Khaki color (R:240, G:230, B:140, A:255).
	/// RGBA Hex: #F0E68CFF.
	/// </summary>
	public static Color Khaki => FromPacked(0xF0E68CFFu);

	/// <summary>
	/// Lavender color (R:230, G:230, B:250, A:255).
	/// RGBA Hex: #E6E6FAFF.
	/// </summary>
	public static Color Lavender => FromPacked(0xE6E6FAFFu);

	/// <summary>
	/// LavenderBlush color (R:255, G:240, B:245, A:255).
	/// RGBA Hex: #FFF0F5FF.
	/// </summary>
	public static Color LavenderBlush => FromPacked(0xFFF0F5FFu);

	/// <summary>
	/// LawnGreen color (R:124, G:252, B:0, A:255).
	/// RGBA Hex: #7CFC00FF.
	/// </summary>
	public static Color LawnGreen => FromPacked(0x7CFC00FFu);

	/// <summary>
	/// LemonChiffon color (R:255, G:250, B:205, A:255).
	/// RGBA Hex: #FFFACDFF.
	/// </summary>
	public static Color LemonChiffon => FromPacked(0xFFFACDFFu);

	/// <summary>
	/// LightBlue color (R:173, G:216, B:230, A:255).
	/// RGBA Hex: #ADD8E6FF.
	/// </summary>
	public static Color LightBlue => FromPacked(0xADD8E6FFu);

	/// <summary>
	/// LightCoral color (R:240, G:128, B:128, A:255).
	/// RGBA Hex: #F08080FF.
	/// </summary>
	public static Color LightCoral => FromPacked(0xF08080FFu);

	/// <summary>
	/// LightCyan color (R:224, G:255, B:255, A:255).
	/// RGBA Hex: #E0FFFFFF.
	/// </summary>
	public static Color LightCyan => FromPacked(0xE0FFFFFFu);

	/// <summary>
	/// LightGoldenrodYellow color (R:250, G:250, B:210, A:255).
	/// RGBA Hex: #FAFAD2FF.
	/// </summary>
	public static Color LightGoldenrodYellow => FromPacked(0xFAFAD2FFu);

	/// <summary>
	/// LightGray color (R:211, G:211, B:211, A:255).
	/// RGBA Hex: #D3D3D3FF.
	/// </summary>
	public static Color LightGray => FromPacked(0xD3D3D3FFu);

	/// <summary>
	/// LightGreen color (R:144, G:238, B:144, A:255).
	/// RGBA Hex: #90EE90FF.
	/// </summary>
	public static Color LightGreen => FromPacked(0x90EE90FFu);

	/// <summary>
	/// LightPink color (R:255, G:182, B:193, A:255).
	/// RGBA Hex: #FFB6C1FF.
	/// </summary>
	public static Color LightPink => FromPacked(0xFFB6C1FFu);

	/// <summary>
	/// LightSalmon color (R:255, G:160, B:122, A:255).
	/// RGBA Hex: #FFA07AFF.
	/// </summary>
	public static Color LightSalmon => FromPacked(0xFFA07AFFu);

	/// <summary>
	/// LightSeaGreen color (R:32, G:178, B:170, A:255).
	/// RGBA Hex: #20B2AAFF.
	/// </summary>
	public static Color LightSeaGreen => FromPacked(0x20B2AAFFu);

	/// <summary>
	/// LightSkyBlue color (R:135, G:206, B:250, A:255).
	/// RGBA Hex: #87CEFAFF.
	/// </summary>
	public static Color LightSkyBlue => FromPacked(0x87CEFAFFu);

	/// <summary>
	/// LightSlateGray color (R:119, G:136, B:153, A:255).
	/// RGBA Hex: #778899FF.
	/// </summary>
	public static Color LightSlateGray => FromPacked(0x778899FFu);

	/// <summary>
	/// LightSteelBlue color (R:176, G:196, B:222, A:255).
	/// RGBA Hex: #B0C4DEFF.
	/// </summary>
	public static Color LightSteelBlue => FromPacked(0xB0C4DEFFu);

	/// <summary>
	/// LightYellow color (R:255, G:255, B:224, A:255).
	/// RGBA Hex: #FFFFE0FF.
	/// </summary>
	public static Color LightYellow => FromPacked(0xFFFFE0FFu);

	/// <summary>
	/// Lime color (R:0, G:255, B:0, A:255).
	/// RGBA Hex: #00FF00FF.
	/// </summary>
	public static Color Lime => FromPacked(0x00FF00FFu);

	/// <summary>
	/// LimeGreen color (R:50, G:205, B:50, A:255).
	/// RGBA Hex: #32CD32FF.
	/// </summary>
	public static Color LimeGreen => FromPacked(0x32CD32FFu);

	/// <summary>
	/// Linen color (R:250, G:240, B:230, A:255).
	/// RGBA Hex: #FAF0E6FF.
	/// </summary>
	public static Color Linen => FromPacked(0xFAF0E6FFu);

	/// <summary>
	/// Magenta color (R:255, G:0, B:255, A:255).
	/// RGBA Hex: #FF00FFFF.
	/// </summary>
	public static Color Magenta => FromPacked(0xFF00FFFFu);

	/// <summary>
	/// Maroon color (R:128, G:0, B:0, A:255).
	/// RGBA Hex: #800000FF.
	/// </summary>
	public static Color Maroon => FromPacked(0x800000FFu);

	/// <summary>
	/// MediumAquamarine color (R:102, G:205, B:170, A:255).
	/// RGBA Hex: #66CDAAFF.
	/// </summary>
	public static Color MediumAquamarine => FromPacked(0x66CDAAFFu);

	/// <summary>
	/// MediumBlue color (R:0, G:0, B:205, A:255).
	/// RGBA Hex: #0000CDFF.
	/// </summary>
	public static Color MediumBlue => FromPacked(0x0000CDFFu);

	/// <summary>
	/// MediumOrchid color (R:186, G:85, B:211, A:255).
	/// RGBA Hex: #BA55D3FF.
	/// </summary>
	public static Color MediumOrchid => FromPacked(0xBA55D3FFu);

	/// <summary>
	/// MediumPurple color (R:147, G:112, B:219, A:255).
	/// RGBA Hex: #9370DBFF.
	/// </summary>
	public static Color MediumPurple => FromPacked(0x9370DBFFu);

	/// <summary>
	/// MediumSeaGreen color (R:60, G:179, B:113, A:255).
	/// RGBA Hex: #3CB371FF.
	/// </summary>
	public static Color MediumSeaGreen => FromPacked(0x3CB371FFu);

	/// <summary>
	/// MediumSlateBlue color (R:123, G:104, B:238, A:255).
	/// RGBA Hex: #7B68EEFF.
	/// </summary>
	public static Color MediumSlateBlue => FromPacked(0x7B68EEFFu);

	/// <summary>
	/// MediumSpringGreen color (R:0, G:250, B:154, A:255).
	/// RGBA Hex: #00FA9AFF.
	/// </summary>
	public static Color MediumSpringGreen => FromPacked(0x00FA9AFFu);

	/// <summary>
	/// MediumTurquoise color (R:72, G:209, B:204, A:255).
	/// RGBA Hex: #48D1CCFF.
	/// </summary>
	public static Color MediumTurquoise => FromPacked(0x48D1CCFFu);

	/// <summary>
	/// MediumVioletRed color (R:199, G:21, B:133, A:255).
	/// RGBA Hex: #C71585FF.
	/// </summary>
	public static Color MediumVioletRed => FromPacked(0xC71585FFu);

	/// <summary>
	/// MidnightBlue color (R:25, G:25, B:112, A:255).
	/// RGBA Hex: #191970FF.
	/// </summary>
	public static Color MidnightBlue => FromPacked(0x191970FFu);

	/// <summary>
	/// MintCream color (R:245, G:255, B:250, A:255).
	/// RGBA Hex: #F5FFFAFF.
	/// </summary>
	public static Color MintCream => FromPacked(0xF5FFFAFFu);

	/// <summary>
	/// MistyRose color (R:255, G:228, B:225, A:255).
	/// RGBA Hex: #FFE4E1FF.
	/// </summary>
	public static Color MistyRose => FromPacked(0xFFE4E1FFu);

	/// <summary>
	/// Moccasin color (R:255, G:228, B:181, A:255).
	/// RGBA Hex: #FFE4B5FF.
	/// </summary>
	public static Color Moccasin => FromPacked(0xFFE4B5FFu);

	/// <summary>
	/// NavajoWhite color (R:255, G:222, B:173, A:255).
	/// RGBA Hex: #FFDEADFF.
	/// </summary>
	public static Color NavajoWhite => FromPacked(0xFFDEADFFu);

	/// <summary>
	/// Navy color (R:0, G:0, B:128, A:255).
	/// RGBA Hex: #000080FF.
	/// </summary>
	public static Color Navy => FromPacked(0x000080FFu);

	/// <summary>
	/// OldLace color (R:253, G:245, B:230, A:255).
	/// RGBA Hex: #FDF5E6FF.
	/// </summary>
	public static Color OldLace => FromPacked(0xFDF5E6FFu);

	/// <summary>
	/// Olive color (R:128, G:128, B:0, A:255).
	/// RGBA Hex: #808000FF.
	/// </summary>
	public static Color Olive => FromPacked(0x808000FFu);

	/// <summary>
	/// OliveDrab color (R:107, G:142, B:35, A:255).
	/// RGBA Hex: #6B8E23FF.
	/// </summary>
	public static Color OliveDrab => FromPacked(0x6B8E23FFu);

	/// <summary>
	/// Orange color (R:255, G:165, B:0, A:255).
	/// RGBA Hex: #FFA500FF.
	/// </summary>
	public static Color Orange => FromPacked(0xFFA500FFu);

	/// <summary>
	/// OrangeRed color (R:255, G:69, B:0, A:255).
	/// RGBA Hex: #FF4500FF.
	/// </summary>
	public static Color OrangeRed => FromPacked(0xFF4500FFu);

	/// <summary>
	/// Orchid color (R:218, G:112, B:214, A:255).
	/// RGBA Hex: #DA70D6FF.
	/// </summary>
	public static Color Orchid => FromPacked(0xDA70D6FFu);

	/// <summary>
	/// PaleGoldenrod color (R:238, G:232, B:170, A:255).
	/// RGBA Hex: #EEE8AAFF.
	/// </summary>
	public static Color PaleGoldenrod => FromPacked(0xEEE8AAFFu);

	/// <summary>
	/// PaleGreen color (R:152, G:251, B:152, A:255).
	/// RGBA Hex: #98FB98FF.
	/// </summary>
	public static Color PaleGreen => FromPacked(0x98FB98FFu);

	/// <summary>
	/// PaleTurquoise color (R:175, G:238, B:238, A:255).
	/// RGBA Hex: #AFEEEEFF.
	/// </summary>
	public static Color PaleTurquoise => FromPacked(0xAFEEEEFFu);

	/// <summary>
	/// PaleVioletRed color (R:219, G:112, B:147, A:255).
	/// RGBA Hex: #DB7093FF.
	/// </summary>
	public static Color PaleVioletRed => FromPacked(0xDB7093FFu);

	/// <summary>
	/// PapayaWhip color (R:255, G:239, B:213, A:255).
	/// RGBA Hex: #FFEFD5FF.
	/// </summary>
	public static Color PapayaWhip => FromPacked(0xFFEFD5FFu);

	/// <summary>
	/// PeachPuff color (R:255, G:218, B:185, A:255).
	/// RGBA Hex: #FFDAB9FF.
	/// </summary>
	public static Color PeachPuff => FromPacked(0xFFDAB9FFu);

	/// <summary>
	/// Peru color (R:205, G:133, B:63, A:255).
	/// RGBA Hex: #CD853FFF.
	/// </summary>
	public static Color Peru => FromPacked(0xCD853FFFu);

	/// <summary>
	/// Pink color (R:255, G:192, B:203, A:255).
	/// RGBA Hex: #FFC0CBFF.
	/// </summary>
	public static Color Pink => FromPacked(0xFFC0CBFFu);

	/// <summary>
	/// Plum color (R:221, G:160, B:221, A:255).
	/// RGBA Hex: #DDA0DDFF.
	/// </summary>
	public static Color Plum => FromPacked(0xDDA0DDFFu);

	/// <summary>
	/// PowderBlue color (R:176, G:224, B:230, A:255).
	/// RGBA Hex: #B0E0E6FF.
	/// </summary>
	public static Color PowderBlue => FromPacked(0xB0E0E6FFu);

	/// <summary>
	/// Purple color (R:128, G:0, B:128, A:255).
	/// RGBA Hex: #800080FF.
	/// </summary>
	public static Color Purple => FromPacked(0x800080FFu);

	/// <summary>
	/// Red color (R:255, G:0, B:0, A:255).
	/// RGBA Hex: #FF0000FF.
	/// </summary>
	public static Color Red => FromPacked(0xFF0000FFu);

	/// <summary>
	/// RosyBrown color (R:188, G:143, B:143, A:255).
	/// RGBA Hex: #BC8F8FFF.
	/// </summary>
	public static Color RosyBrown => FromPacked(0xBC8F8FFFu);

	/// <summary>
	/// RoyalBlue color (R:65, G:105, B:225, A:255).
	/// RGBA Hex: #4169E1FF.
	/// </summary>
	public static Color RoyalBlue => FromPacked(0x4169E1FFu);

	/// <summary>
	/// SaddleBrown color (R:139, G:69, B:19, A:255).
	/// RGBA Hex: #8B4513FF.
	/// </summary>
	public static Color SaddleBrown => FromPacked(0x8B4513FFu);

	/// <summary>
	/// Salmon color (R:250, G:128, B:114, A:255).
	/// RGBA Hex: #FA8072FF.
	/// </summary>
	public static Color Salmon => FromPacked(0xFA8072FFu);

	/// <summary>
	/// SandyBrown color (R:244, G:164, B:96, A:255).
	/// RGBA Hex: #F4A460FF.
	/// </summary>
	public static Color SandyBrown => FromPacked(0xF4A460FFu);

	/// <summary>
	/// SeaGreen color (R:46, G:139, B:87, A:255).
	/// RGBA Hex: #2E8B57FF.
	/// </summary>
	public static Color SeaGreen => FromPacked(0x2E8B57FFu);

	/// <summary>
	/// SeaShell color (R:255, G:245, B:238, A:255).
	/// RGBA Hex: #FFF5EEFF.
	/// </summary>
	public static Color SeaShell => FromPacked(0xFFF5EEFFu);

	/// <summary>
	/// Sienna color (R:160, G:82, B:45, A:255).
	/// RGBA Hex: #A0522DFF.
	/// </summary>
	public static Color Sienna => FromPacked(0xA0522DFFu);

	/// <summary>
	/// Silver color (R:192, G:192, B:192, A:255).
	/// RGBA Hex: #C0C0C0FF.
	/// </summary>
	public static Color Silver => FromPacked(0xC0C0C0FFu);

	/// <summary>
	/// SkyBlue color (R:135, G:206, B:235, A:255).
	/// RGBA Hex: #87CEEBFF.
	/// </summary>
	public static Color SkyBlue => FromPacked(0x87CEEBFFu);

	/// <summary>
	/// SlateBlue color (R:106, G:90, B:205, A:255).
	/// RGBA Hex: #6A5ACDFF.
	/// </summary>
	public static Color SlateBlue => FromPacked(0x6A5ACDFFu);

	/// <summary>
	/// SlateGray color (R:112, G:128, B:144, A:255).
	/// RGBA Hex: #708090FF.
	/// </summary>
	public static Color SlateGray => FromPacked(0x708090FFu);

	/// <summary>
	/// Snow color (R:255, G:250, B:250, A:255).
	/// RGBA Hex: #FFFAFAFF.
	/// </summary>
	public static Color Snow => FromPacked(0xFFFAFAFFu);

	/// <summary>
	/// SpringGreen color (R:0, G:255, B:127, A:255).
	/// RGBA Hex: #00FF7FFF.
	/// </summary>
	public static Color SpringGreen => FromPacked(0x00FF7FFFu);

	/// <summary>
	/// SteelBlue color (R:70, G:130, B:180, A:255).
	/// RGBA Hex: #4682B4FF.
	/// </summary>
	public static Color SteelBlue => FromPacked(0x4682B4FFu);

	/// <summary>
	/// Tan color (R:210, G:180, B:140, A:255).
	/// RGBA Hex: #D2B48CFF.
	/// </summary>
	public static Color Tan => FromPacked(0xD2B48CFFu);

	/// <summary>
	/// Teal color (R:0, G:128, B:128, A:255).
	/// RGBA Hex: #008080FF.
	/// </summary>
	public static Color Teal => FromPacked(0x008080FFu);

	/// <summary>
	/// Thistle color (R:216, G:191, B:216, A:255).
	/// RGBA Hex: #D8BFD8FF.
	/// </summary>
	public static Color Thistle => FromPacked(0xD8BFD8FFu);

	/// <summary>
	/// Tomato color (R:255, G:99, B:71, A:255).
	/// RGBA Hex: #FF6347FF.
	/// </summary>
	public static Color Tomato => FromPacked(0xFF6347FFu);

	/// <summary>
	/// Turquoise color (R:64, G:224, B:208, A:255).
	/// RGBA Hex: #40E0D0FF.
	/// </summary>
	public static Color Turquoise => FromPacked(0x40E0D0FFu);

	/// <summary>
	/// Violet color (R:238, G:130, B:238, A:255).
	/// RGBA Hex: #EE82EEFF.
	/// </summary>
	public static Color Violet => FromPacked(0xEE82EEFFu);

	/// <summary>
	/// Wheat color (R:245, G:222, B:179, A:255).
	/// RGBA Hex: #F5DEB3FF.
	/// </summary>
	public static Color Wheat => FromPacked(0xF5DEB3FFu);

	/// <summary>
	/// White color (R:255, G:255, B:255, A:255).
	/// RGBA Hex: #FFFFFFFF.
	/// </summary>
	public static Color White => FromPacked(0xFFFFFFFFu);

	/// <summary>
	/// WhiteSmoke color (R:245, G:245, B:245, A:255).
	/// RGBA Hex: #F5F5F5FF.
	/// </summary>
	public static Color WhiteSmoke => FromPacked(0xF5F5F5FFu);

	/// <summary>
	/// Yellow color (R:255, G:255, B:0, A:255).
	/// RGBA Hex: #FFFF00FF.
	/// </summary>
	public static Color Yellow => FromPacked(0xFFFF00FFu);

	/// <summary>
	/// YellowGreen color (R:154, G:205, B:50, A:255).
	/// RGBA Hex: #9ACD32FF.
	/// </summary>
	public static Color YellowGreen => FromPacked(0x9ACD32FFu);

	/// <summary>
	/// RebeccaPurple color (R:102, G:51, B:153, A:255).
	/// RGBA Hex: #663399FF.
	/// </summary>
	public static Color RebeccaPurple => FromPacked(0x663399FFu);

#endregion Built-In Colors

}