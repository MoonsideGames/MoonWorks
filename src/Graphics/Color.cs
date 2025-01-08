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

namespace MoonWorks.Graphics;

/// <summary>
/// Describes a 32-bit packed color.
/// </summary>
public record struct Color(byte R, byte G, byte B, byte A)
{
	public Color(byte r, byte g, byte b) : this(r, g, b, 255) { }

	/// <summary>
	/// Constructs an RGBA color from scalars which represent red, green, blue and alpha values.
	/// </summary>
	/// <param name="r">Red component value from 0.0f to 1.0f.</param>
	/// <param name="g">Green component value from 0.0f to 1.0f.</param>
	/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
	/// <param name="a">Alpha component value from 0.0f to 1.0f.</param>
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
		255
	) { }

	/// <summary>
	/// Constructs an RGBA color from a <see cref="Vector4"/>. The XYZW components of the vector
	/// translate to RGBA respectively.
	/// </summary>
	/// <param name="vector">The <see cref="Vector4"/> struct.</param>
	public Color(Vector4 vector) : this(vector.X, vector.Y, vector.Z, vector.W) { }

	/// <summary>
	/// Gets packed value of this <see cref="Color"/>.
	/// </summary>
	public readonly uint PackedValue() => Unsafe.BitCast<Color, uint>(this);

	/// <summary>
	/// Transparent color (R:0,G:0,B:0,A:0).
	/// </summary>
	public static Color Transparent => new(0, 0, 0, 0);

	/// <summary>
	/// AliceBlue color (R:240,G:248,B:255,A:255).
	/// </summary>
	public static Color AliceBlue => new(240, 248, 255);

	/// <summary>
	/// AntiqueWhite color (R:250,G:235,B:215,A:255).
	/// </summary>
	public static Color AntiqueWhite => new(250, 235, 215);

	/// <summary>
	/// Aqua color (R:0,G:255,B:255,A:255).
	/// </summary>
	public static Color Aqua => new(0, 255, 255);

	/// <summary>
	/// Aquamarine color (R:127,G:255,B:212,A:255).
	/// </summary>
	public static Color Aquamarine => new(127, 255, 212);

	/// <summary>
	/// Azure color (R:240,G:255,B:255,A:255).
	/// </summary>
	public static Color Azure => new(240, 255, 255);

	/// <summary>
	/// Beige color (R:245,G:245,B:220,A:255).
	/// </summary>
	public static Color Beige => new(245, 245, 220);

	/// <summary>
	/// Bisque color (R:255,G:228,B:196,A:255).
	/// </summary>
	public static Color Bisque => new(255, 228, 196);

	/// <summary>
	/// Black color (R:0,G:0,B:0,A:255).
	/// </summary>
	public static Color Black => new(0, 0, 0);

	/// <summary>
	/// BlanchedAlmond color (R:255,G:235,B:205,A:255).
	/// </summary>
	public static Color BlanchedAlmond => new(255, 235, 205);

	/// <summary>
	/// Blue color (R:0,G:0,B:255,A:255).
	/// </summary>
	public static Color Blue => new(0, 0, 255);

	/// <summary>
	/// BlueViolet color (R:138,G:43,B:226,A:255).
	/// </summary>
	public static Color BlueViolet => new(138, 43, 226);

	/// <summary>
	/// Brown color (R:165,G:42,B:42,A:255).
	/// </summary>
	public static Color Brown => new(165, 42, 42);

	/// <summary>
	/// BurlyWood color (R:222,G:184,B:135,A:255).
	/// </summary>
	public static Color BurlyWood => new(222, 184, 135);

	/// <summary>
	/// CadetBlue color (R:95,G:158,B:160,A:255).
	/// </summary>
	public static Color CadetBlue => new(95, 158, 160);

	/// <summary>
	/// Chartreuse color (R:127,G:255,B:0,A:255).
	/// </summary>
	public static Color Chartreuse => new(127, 255, 0);

	/// <summary>
	/// Chocolate color (R:210,G:105,B:30,A:255).
	/// </summary>
	public static Color Chocolate => new(210, 105, 30);

	/// <summary>
	/// Coral color (R:255,G:127,B:80,A:255).
	/// </summary>
	public static Color Coral => new (255, 127, 80);

	/// <summary>
	/// CornflowerBlue color (R:100,G:149,B:237,A:255).
	/// </summary>
	public static Color CornflowerBlue => new(100, 149, 237);

	/// <summary>
	/// Cornsilk color (R:255,G:248,B:220,A:255).
	/// </summary>
	public static Color Cornsilk => new(255, 248, 220);

	/// <summary>
	/// Crimson color (R:220,G:20,B:60,A:255).
	/// </summary>
	public static Color Crimson => new(220, 20, 60);

	/// <summary>
	/// Cyan color (R:0,G:255,B:255,A:255).
	/// </summary>
	public static Color Cyan => new(0, 255, 255);

	/// <summary>
	/// DarkBlue color (R:0,G:0,B:139,A:255).
	/// </summary>
	public static Color DarkBlue => new(0, 0, 139);

	/// <summary>
	/// DarkCyan color (R:0,G:139,B:139,A:255).
	/// </summary>
	public static Color DarkCyan => new(0, 139, 139);

	/// <summary>
	/// DarkGoldenrod color (R:184,G:134,B:11,A:255).
	/// </summary>
	public static Color DarkGoldenrod => new(184, 134, 11);

	/// <summary>
	/// DarkGray color (R:169,G:169,B:169,A:255).
	/// </summary>
	public static Color DarkGray => new(169, 169, 169);

	/// <summary>
	/// DarkGreen color (R:0,G:100,B:0,A:255).
	/// </summary>
	public static Color DarkGreen => new(0, 100, 0);

	/// <summary>
	/// DarkKhaki color (R:189,G:183,B:107,A:255).
	/// </summary>
	public static Color DarkKhaki => new(189, 183, 107);

	/// <summary>
	/// DarkMagenta color (R:139,G:0,B:139,A:255).
	/// </summary>
	public static Color DarkMagenta => new(139, 0, 139);

	/// <summary>
	/// DarkOliveGreen color (R:85,G:107,B:47,A:255).
	/// </summary>
	public static Color DarkOliveGreen => new(85, 107, 47);

	/// <summary>
	/// DarkOrange color (R:255,G:140,B:0,A:255).
	/// </summary>
	public static Color DarkOrange => new(255, 140, 0);

	/// <summary>
	/// DarkOrchid color (R:153,G:50,B:204,A:255).
	/// </summary>
	public static Color DarkOrchid => new(153, 50, 204);

	/// <summary>
	/// DarkRed color (R:139,G:0,B:0,A:255).
	/// </summary>
	public static Color DarkRed => new(139, 0, 0);

	/// <summary>
	/// DarkSalmon color (R:233,G:150,B:122,A:255).
	/// </summary>
	public static Color DarkSalmon => new(233, 150, 122);

	/// <summary>
	/// DarkSeaGreen color (R:143,G:188,B:139,A:255).
	/// </summary>
	public static Color DarkSeaGreen => new(143, 188, 139);

	/// <summary>
	/// DarkSlateBlue color (R:72,G:61,B:139,A:255).
	/// </summary>
	public static Color DarkSlateBlue => new(72, 61, 139);

	/// <summary>
	/// DarkSlateGray color (R:47,G:79,B:79,A:255).
	/// </summary>
	public static Color DarkSlateGray => new(47, 79, 79);

	/// <summary>
	/// DarkTurquoise color (R:0,G:206,B:209,A:255).
	/// </summary>
	public static Color DarkTurquoise => new(0, 206, 209);

	/// <summary>
	/// DarkViolet color (R:148,G:0,B:211,A:255).
	/// </summary>
	public static Color DarkViolet => new(148, 0, 211);

	/// <summary>
	/// DeepPink color (R:255,G:20,B:147,A:255).
	/// </summary>
	public static Color DeepPink => new(255, 20, 147);

	/// <summary>
	/// DeepSkyBlue color (R:0,G:191,B:255,A:255).
	/// </summary>
	public static Color DeepSkyBlue => new(0, 191, 255);

	/// <summary>
	/// DimGray color (R:105,G:105,B:105,A:255).
	/// </summary>
	public static Color DimGray => new(105, 105, 105);

	/// <summary>
	/// DodgerBlue color (R:30,G:144,B:255,A:255).
	/// </summary>
	public static Color DodgerBlue => new(30, 144, 255);

	/// <summary>
	/// Firebrick color (R:178,G:34,B:34,A:255).
	/// </summary>
	public static Color Firebrick => new(178, 34, 34);

	/// <summary>
	/// FloralWhite color (R:255,G:250,B:240,A:255).
	/// </summary>
	public static Color FloralWhite => new(255, 250, 240);

	/// <summary>
	/// ForestGreen color (R:34,G:139,B:34,A:255).
	/// </summary>
	public static Color ForestGreen => new(34, 139, 34);

	/// <summary>
	/// Fuchsia color (R:255,G:0,B:255,A:255).
	/// </summary>
	public static Color Fuchsia => new(255, 0, 255);

	/// <summary>
	/// Gainsboro color (R:220,G:220,B:220,A:255).
	/// </summary>
	public static Color Gainsboro => new(220, 220, 220);

	/// <summary>
	/// GhostWhite color (R:248,G:248,B:255,A:255).
	/// </summary>
	public static Color GhostWhite => new(248, 248, 255);

	/// <summary>
	/// Gold color (R:255,G:215,B:0,A:255).
	/// </summary>
	public static Color Gold => new(255, 215, 0);

	/// <summary>
	/// Goldenrod color (R:218,G:165,B:32,A:255).
	/// </summary>
	public static Color Goldenrod => new(218, 165, 32);

	/// <summary>
	/// Gray color (R:128,G:128,B:128,A:255).
	/// </summary>
	public static Color Gray => new(128, 128, 128);

	/// <summary>
	/// Green color (R:0,G:128,B:0,A:255).
	/// </summary>
	public static Color Green => new(0, 128, 0);

	/// <summary>
	/// GreenYellow color (R:173,G:255,B:47,A:255).
	/// </summary>
	public static Color GreenYellow => new(173, 255, 47);

	/// <summary>
	/// Honeydew color (R:240,G:255,B:240,A:255).
	/// </summary>
	public static Color Honeydew => new(240, 255, 240);

	/// <summary>
	/// HotPink color (R:255,G:105,B:180,A:255).
	/// </summary>
	public static Color HotPink => new(255, 105, 180);

	/// <summary>
	/// IndianRed color (R:205,G:92,B:92,A:255).
	/// </summary>
	public static Color IndianRed => new(205, 92, 92);

	/// <summary>
	/// Indigo color (R:75,G:0,B:130,A:255).
	/// </summary>
	public static Color Indigo => new(75, 0, 130);

	/// <summary>
	/// Ivory color (R:255,G:255,B:240,A:255).
	/// </summary>
	public static Color Ivory => new(255, 255, 240);

	/// <summary>
	/// Khaki color (R:240,G:230,B:140,A:255).
	/// </summary>
	public static Color Khaki => new(240, 230, 140);

	/// <summary>
	/// Lavender color (R:230,G:230,B:250,A:255).
	/// </summary>
	public static Color Lavender => new(230, 230, 250);

	/// <summary>
	/// LavenderBlush color (R:255,G:240,B:245,A:255).
	/// </summary>
	public static Color LavenderBlush => new(255, 240, 245);

	/// <summary>
	/// LawnGreen color (R:124,G:252,B:0,A:255).
	/// </summary>
	public static Color LawnGreen => new(124, 252, 0);

	/// <summary>
	/// LemonChiffon color (R:255,G:250,B:205,A:255).
	/// </summary>
	public static Color LemonChiffon => new(255, 250, 205);

	/// <summary>
	/// LightBlue color (R:173,G:216,B:230,A:255).
	/// </summary>
	public static Color LightBlue => new(173, 216, 230);

	/// <summary>
	/// LightCoral color (R:240,G:128,B:128,A:255).
	/// </summary>
	public static Color LightCoral => new(240, 128, 128);

	/// <summary>
	/// LightCyan color (R:224,G:255,B:255,A:255).
	/// </summary>
	public static Color LightCyan => new(224, 255, 255);

	/// <summary>
	/// LightGoldenrodYellow color (R:250,G:250,B:210,A:255).
	/// </summary>
	public static Color LightGoldenrodYellow => new(250, 250, 210);

	/// <summary>
	/// LightGray color (R:211,G:211,B:211,A:255).
	/// </summary>
	public static Color LightGray => new(211, 211, 211);

	/// <summary>
	/// LightGreen color (R:144,G:238,B:144,A:255).
	/// </summary>
	public static Color LightGreen => new(144, 238, 144);

	/// <summary>
	/// LightPink color (R:255,G:182,B:193,A:255).
	/// </summary>
	public static Color LightPink => new(255, 182, 193);

	/// <summary>
	/// LightSalmon color (R:255,G:160,B:122,A:255).
	/// </summary>
	public static Color LightSalmon => new(255, 160, 122);

	/// <summary>
	/// LightSeaGreen color (R:32,G:178,B:170,A:255).
	/// </summary>
	public static Color LightSeaGreen => new(32, 178, 170);

	/// <summary>
	/// LightSkyBlue color (R:135,G:206,B:250,A:255).
	/// </summary>
	public static Color LightSkyBlue => new(135, 206, 250);

	/// <summary>
	/// LightSlateGray color (R:119,G:136,B:153,A:255).
	/// </summary>
	public static Color LightSlateGray => new(119, 136, 153);

	/// <summary>
	/// LightSteelBlue color (R:176,G:196,B:222,A:255).
	/// </summary>
	public static Color LightSteelBlue => new(179, 196, 222);

	/// <summary>
	/// LightYellow color (R:255,G:255,B:224,A:255).
	/// </summary>
	public static Color LightYellow => new(255, 255, 224);

	/// <summary>
	/// Lime color (R:0,G:255,B:0,A:255).
	/// </summary>
	public static Color Lime => new(0, 255, 0);

	/// <summary>
	/// LimeGreen color (R:50,G:205,B:50,A:255).
	/// </summary>
	public static Color LimeGreen => new(50, 205, 50);

	/// <summary>
	/// Linen color (R:250,G:240,B:230,A:255).
	/// </summary>
	public static Color Linen => new(250, 240, 230);

	/// <summary>
	/// Magenta color (R:255,G:0,B:255,A:255).
	/// </summary>
	public static Color Magenta => new(255, 0, 255);

	/// <summary>
	/// Maroon color (R:128,G:0,B:0,A:255).
	/// </summary>
	public static Color Maroon => new(128, 0, 0, 255);

	/// <summary>
	/// MediumAquamarine color (R:102,G:205,B:170,A:255).
	/// </summary>
	public static Color MediumAquamarine => new(102, 205, 170);

	/// <summary>
	/// MediumBlue color (R:0,G:0,B:205,A:255).
	/// </summary>
	public static Color MediumBlue => new(0, 0, 205);

	/// <summary>
	/// MediumOrchid color (R:186,G:85,B:211,A:255).
	/// </summary>
	public static Color MediumOrchid => new(186, 85, 211);

	/// <summary>
	/// MediumPurple color (R:147,G:112,B:219,A:255).
	/// </summary>
	public static Color MediumPurple => new(147, 112, 219);

	/// <summary>
	/// MediumSeaGreen color (R:60,G:179,B:113,A:255).
	/// </summary>
	public static Color MediumSeaGreen => new(60, 179, 113);

	/// <summary>
	/// MediumSlateBlue color (R:123,G:104,B:238,A:255).
	/// </summary>
	public static Color MediumSlateBlue => new(123, 104, 238);

	/// <summary>
	/// MediumSpringGreen color (R:0,G:250,B:154,A:255).
	/// </summary>
	public static Color MediumSpringGreen => new(0, 250, 154);

	/// <summary>
	/// MediumTurquoise color (R:72,G:209,B:204,A:255).
	/// </summary>
	public static Color MediumTurquoise => new(72, 209, 204);

	/// <summary>
	/// MediumVioletRed color (R:199,G:21,B:133,A:255).
	/// </summary>
	public static Color MediumVioletRed => new(199, 21, 133);

	/// <summary>
	/// MidnightBlue color (R:25,G:25,B:112,A:255).
	/// </summary>
	public static Color MidnightBlue => new(25, 25, 112);

	/// <summary>
	/// MintCream color (R:245,G:255,B:250,A:255).
	/// </summary>
	public static Color MintCream => new(245, 255, 250);

	/// <summary>
	/// MistyRose color (R:255,G:228,B:225,A:255).
	/// </summary>
	public static Color MistyRose => new(255, 228, 225);

	/// <summary>
	/// Moccasin color (R:255,G:228,B:181,A:255).
	/// </summary>
	public static Color Moccasin => new(255, 228, 181);

	/// <summary>
	/// NavajoWhite color (R:255,G:222,B:173,A:255).
	/// </summary>
	public static Color NavajoWhite => new(255, 222, 173);

	/// <summary>
	/// Navy color (R:0,G:0,B:128,A:255).
	/// </summary>
	public static Color Navy => new(0, 0, 128);

	/// <summary>
	/// OldLace color (R:253,G:245,B:230,A:255).
	/// </summary>
	public static Color OldLace => new(253, 245, 230);

	/// <summary>
	/// Olive color (R:128,G:128,B:0,A:255).
	/// </summary>
	public static Color Olive => new(128, 128, 0);

	/// <summary>
	/// OliveDrab color (R:107,G:142,B:35,A:255).
	/// </summary>
	public static Color OliveDrab => new(107, 142, 35);

	/// <summary>
	/// Orange color (R:255,G:165,B:0,A:255).
	/// </summary>
	public static Color Orange => new(255, 165, 0);

	/// <summary>
	/// OrangeRed color (R:255,G:69,B:0,A:255).
	/// </summary>
	public static Color OrangeRed => new(255, 69, 0);

	/// <summary>
	/// Orchid color (R:218,G:112,B:214,A:255).
	/// </summary>
	public static Color Orchid => new(218, 112, 214);

	/// <summary>
	/// PaleGoldenrod color (R:238,G:232,B:170,A:255).
	/// </summary>
	public static Color PaleGoldenrod => new(238, 232, 170);

	/// <summary>
	/// PaleGreen color (R:152,G:251,B:152,A:255).
	/// </summary>
	public static Color PaleGreen => new(152, 251, 152);

	/// <summary>
	/// PaleTurquoise color (R:175,G:238,B:238,A:255).
	/// </summary>
	public static Color PaleTurquoise => new(175, 238, 238);

	/// <summary>
	/// PaleVioletRed color (R:219,G:112,B:147,A:255).
	/// </summary>
	public static Color PaleVioletRed => new(219, 112, 147);

	/// <summary>
	/// PapayaWhip color (R:255,G:239,B:213,A:255).
	/// </summary>
	public static Color PapayaWhip => new(255, 239, 213);

	/// <summary>
	/// PeachPuff color (R:255,G:218,B:185,A:255).
	/// </summary>
	public static Color PeachPuff => new(255, 218, 185);

	/// <summary>
	/// Peru color (R:205,G:133,B:63,A:255).
	/// </summary>
	public static Color Peru => new(205, 133, 63);

	/// <summary>
	/// Pink color (R:255,G:192,B:203,A:255).
	/// </summary>
	public static Color Pink => new(255, 192, 203);

	/// <summary>
	/// Plum color (R:221,G:160,B:221,A:255).
	/// </summary>
	public static Color Plum => new(221, 160, 221);

	/// <summary>
	/// PowderBlue color (R:176,G:224,B:230,A:255).
	/// </summary>
	public static Color PowderBlue => new(176, 224, 230);

	/// <summary>
	/// Purple color (R:128,G:0,B:128,A:255).
	/// </summary>
	public static Color Purple => new(128, 0, 128);

	/// <summary>
	/// Red color (R:255,G:0,B:0,A:255).
	/// </summary>
	public static Color Red => new(255, 0, 0);

	/// <summary>
	/// RosyBrown color (R:188,G:143,B:143,A:255).
	/// </summary>
	public static Color RosyBrown => new(188, 143, 143);

	/// <summary>
	/// RoyalBlue color (R:65,G:105,B:225,A:255).
	/// </summary>
	public static Color RoyalBlue => new(65, 105, 225);

	/// <summary>
	/// SaddleBrown color (R:139,G:69,B:19,A:255).
	/// </summary>
	public static Color SaddleBrown => new(139, 69, 19);

	/// <summary>
	/// Salmon color (R:250,G:128,B:114,A:255).
	/// </summary>
	public static Color Salmon => new(250, 128, 114);

	/// <summary>
	/// SandyBrown color (R:244,G:164,B:96,A:255).
	/// </summary>
	public static Color SandyBrown => new(244, 164, 96);

	/// <summary>
	/// SeaGreen color (R:46,G:139,B:87,A:255).
	/// </summary>
	public static Color SeaGreen => new(46, 139, 87);

	/// <summary>
	/// SeaShell color (R:255,G:245,B:238,A:255).
	/// </summary>
	public static Color SeaShell => new(255, 245, 238);

	/// <summary>
	/// Sienna color (R:160,G:82,B:45,A:255).
	/// </summary>
	public static Color Sienna => new(160, 82, 45);

	/// <summary>
	/// Silver color (R:192,G:192,B:192,A:255).
	/// </summary>
	public static Color Silver => new(192, 192, 192);

	/// <summary>
	/// SkyBlue color (R:135,G:206,B:235,A:255).
	/// </summary>
	public static Color SkyBlue => new(135, 206, 235);

	/// <summary>
	/// SlateBlue color (R:106,G:90,B:205,A:255).
	/// </summary>
	public static Color SlateBlue => new(106, 90, 205);

	/// <summary>
	/// SlateGray color (R:112,G:128,B:144,A:255).
	/// </summary>
	public static Color SlateGray => new(112, 128, 144);

	/// <summary>
	/// Snow color (R:255,G:250,B:250,A:255).
	/// </summary>
	public static Color Snow => new(255, 250, 250);

	/// <summary>
	/// SpringGreen color (R:0,G:255,B:127,A:255).
	/// </summary>
	public static Color SpringGreen => new(0, 255, 127);

	/// <summary>
	/// SteelBlue color (R:70,G:130,B:180,A:255).
	/// </summary>
	public static Color SteelBlue => new(70, 130, 180);

	/// <summary>
	/// Tan color (R:210,G:180,B:140,A:255).
	/// </summary>
	public static Color Tan => new(210, 180, 140);

	/// <summary>
	/// Teal color (R:0,G:128,B:128,A:255).
	/// </summary>
	public static Color Teal => new(0, 128, 128);

	/// <summary>
	/// Thistle color (R:216,G:191,B:216,A:255).
	/// </summary>
	public static Color Thistle => new(216, 191, 216);

	/// <summary>
	/// Tomato color (R:255,G:99,B:71,A:255).
	/// </summary>
	public static Color Tomato => new(255, 99, 71);

	/// <summary>
	/// Turquoise color (R:64,G:224,B:208,A:255).
	/// </summary>
	public static Color Turquoise => new(64, 224, 208);

	/// <summary>
	/// Violet color (R:238,G:130,B:238,A:255).
	/// </summary>
	public static Color Violet => new(238, 130, 238);

	/// <summary>
	/// Wheat color (R:245,G:222,B:179,A:255).
	/// </summary>
	public static Color Wheat => new(245, 222, 179);

	/// <summary>
	/// White color (R:255,G:255,B:255,A:255).
	/// </summary>
	public static Color White => new(255, 255, 255);

	/// <summary>
	/// WhiteSmoke color (R:245,G:245,B:245,A:255).
	/// </summary>
	public static Color WhiteSmoke => new(245, 245, 245);

	/// <summary>
	/// Yellow color (R:255,G:255,B:0,A:255).
	/// </summary>
	public static Color Yellow => new(255, 255, 0);

	/// <summary>
	/// YellowGreen color (R:154,G:205,B:50,A:255).
	/// </summary>
	public static Color YellowGreen => new(154, 205, 50);

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
	public static Color FromNonPremultiplied(Vector4 vector)
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
	/// Returns a <see cref="string"/> representation of this <see cref="Color"/> in the format:
	/// {R:[red] G:[green] B:[blue] A:[alpha]}
	/// </summary>
	/// <returns><see cref="string"/> representation of this <see cref="Color"/>.</returns>
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
	/// Implicitly converts a <see cref="Color"/> struct to  <see cref="VertexStructs.Ubyte4Norm"/> struct.
	/// </summary>
	/// <param name="color">The <see cref="Color"/> struct to convert.</param>
	public static implicit operator VertexStructs.Ubyte4Norm(Color color) => new VertexStructs.Ubyte4Norm
	{
		X = color.R,
		Y = color.G,
		Z = color.B,
		W = color.A
	};

	/// <summary>
	/// Implicitly converts a <see cref="Color"/> struct to a <see cref="FColor"/> struct.
	/// </summary>
	/// <param name="color">The <see cref="Color"/> struct to convert.</param>
	public static implicit operator FColor(Color color) => new FColor
	{
		R = color.R / 255f,
		G = color.G / 255f,
		B = color.B / 255f,
		A = color.A / 255f
	};
}
