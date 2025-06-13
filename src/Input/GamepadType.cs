namespace MoonWorks.Input;

/// <summary>
/// This type does not necessarily map to first-party controllers from Microsoft/Sony/Nintendo; 
/// in many cases, third-party controllers can report as these, either because they were designed for a specific console, 
/// or they simply most closely match that console's controllers.
/// (Does it have A/B/X/Y buttons or X/O/Square/Triangle? Does it have a touchpad? etc).
/// </summary>
public enum GamepadType
{
    Unknown = 0,
    Standard,
    Xbox360,
    XboxOne,
    PS3,
    PS4,
    PS5,
    SwitchPro,
    SwitchJoyConLeft,
    SwitchJoyConRight,
    SwitchJoyConPair,
    GameCube
}

/// <summary>
/// Represents the grouping of the gamepad type (Microsoft, Sony, Nintendo) to more easily display button glyph types.
/// </summary>
public enum GamepadFamily
{
    Generic,
    Xbox,
    PlayStation,
    Nintendo
}
