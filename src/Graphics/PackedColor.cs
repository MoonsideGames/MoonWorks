using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Numerics;

namespace MoonWorks.Graphics;

/// <summary>
/// A <see cref="Color"/> packed as 4 bytes in an unsigned integer representation.
/// <para>This represensation exists to prevent footguns related to endianness.</para>
/// <para>Using static constructors, we guarantee that we can safely bit-cast this value into a <see cref="Color"/>.</para>
/// </summary>
public readonly record struct PackedColor()
{
    public readonly uint RGBA;

	/// <summary>
    /// Disable default constructor w/ parameter.
	/// We don't want people to shoot themselves in the foot by passing in
	/// an integer literal without thinking of endianness.
    /// </summary>
    private PackedColor(uint rgba) : this()
    {
        RGBA = rgba;
    }

    /// <summary>
    /// Returns a <see cref="PackedColor"/> representation of a <see cref="Color"/> value.
    /// </summary>
    /// <param name="color">The <see cref="Color"/> to convert.</param>
    /// <returns>A <see cref="PackedColor"/> representation of a <see cref="Color"/> value</returns>
    public static PackedColor FromColor(Color color) 
        => new PackedColor(Unsafe.BitCast<Color, uint>(color));

    /// <summary>
    /// Returns a <see cref="PackedColor"/> representation of a packed RGBA value 
    /// whose endianness matches the <see cref="Color"/> struct.
    /// <para>Example use-case: convert the result of <see cref="Color.PackedValue"/> 
    /// into a <see cref="PackedColor"/>, such that we can now explicitly bit-cast it 
    /// back into a <see cref="Color"/>.</para>
    /// <para>Of course, the result of <see cref="Color.PackedValue"/> could've 
    /// been directly bit-casted anyways, but using these facilities is good practice 
    /// since it forces the user to think about endianness for other cases.</para>
    /// </summary>
    /// <param name="color">The <see cref="Color"/> to convert.</param>
    /// <returns></returns>
    public static PackedColor FromCurrentEndianRGBA(uint rgba)
    {
        return new PackedColor(rgba);
    }
    
    /// <summary>
    /// Converts a big-endian-packed RGBA color into a <see cref="PackedColor"/> whose endianness matches the <see cref="Color"/> struct.
    /// <para>Example valid input: `0xF0F8FFFF`, aka AliceBlue.</para>
    /// </summary>
    /// <param name="bigEndian_RGBA">A big-endian packed color that specifies R, G, B, and A values.</param>
    /// <returns>A <see cref="PackedColor"/>, which can then be safely bit-cast to a <see cref="Color"/>.</returns>
    public static PackedColor FromBigEndianRGBA(uint bigEndian_RGBA)
    {
        if (BitConverter.IsLittleEndian)
        {
            // Reverse the byte order of the big-endian-packed color into little-endian format.
            // This is because our Color struct is currently little-endian, 
            // so we need to maintain convertability.
            return new PackedColor(ReversedEndiannessColor.ToLittleEndian(bigEndian_RGBA));
        }
        else
        {
            // Endianness matches, so we don't need to do anything special.
            // This is an uncommon case, since most computers are little-endian.
            return new PackedColor(bigEndian_RGBA);
        }
    }

    /// <summary>
    /// Converts a big-endian-packed RGB color into a <see cref="PackedColor"/> whose endianness matches the <see cref="Color"/> struct.
    /// <para>Example valid input: `0xF0F8FF`, aka AliceBlue.</para>
    /// <para>The above will be interpeted as a big-endian input of `0xF0F8FF(AlphaByte)`.</para>
    /// </summary>
    /// <param name="bigEndian_RGB">A big-endian packed color that specifies R, G, and B values.</param>
    /// <param name="alpha">The Alpha channel value for the color.</param>
    /// <returns>A <see cref="PackedColor"/>, which can then be safely bit-cast to a <see cref="Color"/>.</returns>
    public static PackedColor FromBigEndianRGB(int bigEndian_RGB, byte alpha = 255)
    {
        // Shift by 8 bits up, since `0xF0F8FF` is supposed to be interpreted as `0xF0F8FF(AlphaByte)`.
        var bigEndian_RGBA = ((uint)bigEndian_RGB << 8) | alpha;
        return FromBigEndianRGBA(bigEndian_RGBA);
    }

    public Color ToColor()
    {
        return Color.FromPacked(this);
    }

    private readonly record struct ReversedEndiannessColor(byte A, byte B, byte G, byte R)
    {
        /// <summary>
        /// Transforms a big-endian packed color value into little-endian.
        /// <para>There's no reason to call this if our architecture is big-endian; doing so is a bug.</para>
        /// </summary>
        /// <param name="bigEndian_RGBA">A big-endian packed color value that must be reversed into little-endian.</param>
        /// <returns>A little-endian RGBA packed value,such that it can be bit-cast into a <see cref="Color"/>.</returns>
        public static uint ToLittleEndian(uint bigEndian_RGBA)
        {
            var reversedColor = FromBigEndian_RGBA(bigEndian_RGBA);
            return reversedColor.PackedValue();
        }

        private static ReversedEndiannessColor FromBigEndian_RGBA(uint bigEndian_RGBA)
            => Unsafe.BitCast<uint, ReversedEndiannessColor>(bigEndian_RGBA);

        private Color ToRegularColor()
        {
            return new Color(R, G, B, A);
        }

        private uint PackedValue()
        {
            var color = ToRegularColor();
            return color.PackedValue();
        }
    }
}