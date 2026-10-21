namespace MoonWorks.Audio;

public readonly record struct AudioDeviceParameters(
    float CurveDistanceScalar = 1f, 
    float SpeedOfSound = 343.5f);