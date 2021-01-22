namespace MoonWorks.Input
{
    internal class Key
    {
        public Keycode Keycode { get; }
        public ButtonState InputState { get; internal set; }

        public Key(Keycode keycode)
        {
            Keycode = keycode;
            InputState = ButtonState.Released;
        }
    }
}
