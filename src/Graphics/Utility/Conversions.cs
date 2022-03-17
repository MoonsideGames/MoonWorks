namespace MoonWorks
{
	public static class Conversions
	{
		public static byte BoolToByte(bool b)
		{
			return (byte) (b ? 1 : 0);
		}

		public static bool ByteToBool(byte b)
		{
			return b != 0;
		}

		public static Graphics.VertexElementFormat TypeToVertexElementFormat(System.Type type)
		{
			if (type == typeof(uint))
			{
				return Graphics.VertexElementFormat.UInt;
			}
			if (type == typeof(float))
			{
				return Graphics.VertexElementFormat.Float;
			}
			else if (type == typeof(Math.Vector2))
			{
				return Graphics.VertexElementFormat.Vector2;
			}
			else if (type == typeof(Math.Vector3))
			{
				return Graphics.VertexElementFormat.Vector3;
			}
			else if (type == typeof(Math.Vector4))
			{
				return Graphics.VertexElementFormat.Vector4;
			}
			else if (type == typeof(Graphics.Color))
			{
				return Graphics.VertexElementFormat.Color;
			}
			else
			{
				throw new System.ArgumentException(
					"Cannot automatically convert this type to a VertexElementFormat!"
				);
			}
		}
	}
}
