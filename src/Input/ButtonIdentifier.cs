namespace MoonWorks.Input
{
    // Blittable identifier that can be used for button state lookups.
    public struct ButtonIdentifier
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
	}
}
