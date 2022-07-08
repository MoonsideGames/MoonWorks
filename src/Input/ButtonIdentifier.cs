namespace MoonWorks.Input
{
    // Blittable identifier that can be used for button state lookups.
    public struct ButtonIdentifier : System.IEquatable<ButtonIdentifier>
    {
        public DeviceKind DeviceKind { get; }
        public int Index { get; } // 1-4 for gamepads, 0 otherwise
		public int Code { get; }

        public ButtonIdentifier(Gamepad gamepad, ButtonCode buttonCode)
        {
			DeviceKind = DeviceKind.Gamepad;
			Index = gamepad.Index;
			Code = (int) buttonCode;
		}

        public ButtonIdentifier(KeyCode keyCode)
        {
			DeviceKind = DeviceKind.Keyboard;
			Index = 0;
			Code = (int) keyCode;
		}

        public ButtonIdentifier(MouseButtonCode mouseCode)
        {
			DeviceKind = DeviceKind.Mouse;
			Index = 0;
			Code = (int) mouseCode;
		}

		public override int GetHashCode()
		{
			return System.HashCode.Combine(DeviceKind, Index, Code);
		}

		public override bool Equals(object obj)
		{
			return obj is ButtonIdentifier identifier && Equals(identifier);
		}

		public bool Equals(ButtonIdentifier identifier)
		{
			return
				DeviceKind == identifier.DeviceKind &&
				Index == identifier.Index &&
				Code == identifier.Code;
		}

		public static bool operator ==(ButtonIdentifier a, ButtonIdentifier b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(ButtonIdentifier a, ButtonIdentifier b)
		{
			return !(a == b);
		}
	}
}
