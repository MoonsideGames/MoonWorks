using System.Numerics;

namespace MoonWorks.Audio;

public class Spatializer
{
    public enum AttenuationModel
    {
        Inverse,
        Linear,
        Exponential,
        None
    }

    public void Process(AudioListener listener, AudioEmitter emitter)
    {
        var gain = 1f;

        // transform emitter position and direction to listener space.
        var lookAt = Matrix4x4.CreateLookAt(listener.Position, listener.Forward, Vector3.UnitZ);
        var relativePosition = Vector3.Transform(emitter.Position, lookAt);
        var relativeDirection = Vector3.TransformNormal(emitter.Forward, lookAt);

        var distance = Vector3.Distance(listener.Position, emitter.Position);

        // TODO: distance attenuation models

        Vector3 relativePositionNormalized;
        if (distance < float.Epsilon)
        {
            distance = 0;
            relativePositionNormalized = Vector3.Zero;
        }
        else
        {
            relativePositionNormalized = relativePosition / distance;
        }

        // Angular attenuation
        if (distance > 0)
        {
            gain *= AngularGain(relativeDirection, -relativePositionNormalized, coneInnerAngle, coneOuterAngle, coneOuterGain);

            if (listener.ConeInnerAngleRadians < float.Pi * 2)
            {
                gain *= AngularGain(Vector3.UnitY, relativePositionNormalized, listener.ConeInnerAngleRadians, listener.ConeOuterAngleRadians, listener.ConeOuterGain);
            }
        }

        gain = float.Clamp(gain, emitter.MinGain, emitter.MaxGain);

        // TODO: channel map?

        if (distance > 0)
        {
            var unitPosition = relativePosition / distance;

            
        }
    }

    private static float AngularGain(Vector3 dirA, Vector3 dirB, float coneInnerAngleRadians, float coneOuterAngleRadians, float coneOuterGain)
    {
        if (coneInnerAngleRadians < float.Pi * 2)
        {
			var innerCutoff = float.Cos(coneInnerAngleRadians * 0.5f);
			var outerCutoff = float.Cos(coneOuterAngleRadians * 0.5f);

            var d = Vector3.Dot(dirA, dirB);

			if (d > innerCutoff)
			{
				return 1;
			}
			else
			{
				if (d > outerCutoff)
				{
					return float.Lerp(coneOuterGain, 1, (d - outerCutoff) / (innerCutoff - outerCutoff));
				}
				else
				{
					return coneOuterGain;
				}
			}
        }

        return 1;
    }
}
