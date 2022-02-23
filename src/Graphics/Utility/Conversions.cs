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
			return b == 0 ? false : true;
		}
	}
}
