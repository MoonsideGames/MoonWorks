// This code is largely translated from FAudio.
// We need to add a few extra spatialization features, but it's not really worth extending FAudio for those,
// since the 3D features are largely self-contained and require a call to do the spatial calculations.
// -cosmonaut

/* FAudio - XAudio Reimplementation for FNA
 *
 * Copyright (c) 2011-2024 Ethan Lee, Luigi Auriemma, and the MonoGame Team
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */

using System;
using System.Numerics;

namespace MoonWorks.Audio;

public static class Audio3D
{
	public record struct Cone(
		float InnerAngle,
		float OuterAngle,
		float InnerVolume,
		float OuterVolume,
		float InnerLPF,
		float OuterLPF,
		float InnerReverb,
		float OuterReverb);

	public class Listener(
		bool UsesCone,
		Cone Cone,
		Vector3 Forward,
		Vector3 Up,
		Vector3 Position,
		Vector3 Velocity)
	{
		public bool UsesCone { get; set; } = UsesCone;
		public Cone Cone { get; set; } = Cone;
		public Vector3 Forward { get; set; } = Forward;
		public Vector3 Up { get; set; } = Up;
		public Vector3 Position { get; set; } = Position;
		public Vector3 Velocity { get; set; } = Velocity;
	}

	public class Emitter(
		bool UsesCone,
		Cone Cone,
		Vector3 Forward,
		Vector3 Up,
		Vector3 Position,
		Vector3 Velocity,
		float InnerRadius,
		float InnerRadiusAngle,
		float CurveDistanceScaler,
		float DopplerScaler,
		float MinDistance,
		float MaxDistance,
		float Rolloff,
		AttenuationModel AttenuationModel)
	{
		public Emitter() : this(
			false,
			default,
			Vector3.UnitY,
			Vector3.UnitZ,
			Vector3.Zero,
			Vector3.Zero,
			0,
			0,
			1,
			1,
			1,
			float.MaxValue,
			1,
			AttenuationModel.Inverse)
		{

		}

		public bool UsesCone { get; set; } = UsesCone;
		public Cone Cone { get; set; } = Cone;
		public Vector3 Forward { get; set; } = Forward;
		public Vector3 Up { get; set; } = Up;
		public Vector3 Position { get; set; } = Position;
		public Vector3 Velocity { get; set; } = Velocity;
		public float InnerRadius { get; set; } = InnerRadius;
		public float InnerRadiusAngle { get; set; } = InnerRadiusAngle;
		public float CurveDistanceScaler { get; set; } = CurveDistanceScaler;
		public float DopplerScaler { get; set; } = DopplerScaler;
		public float MinDistance { get; set; } = MinDistance;
		public float MaxDistance { get; set; } = MaxDistance;
		public float Rolloff { get; set; } = Rolloff;
		public AttenuationModel AttenuationModel { get; set; } = AttenuationModel;
	}

	public unsafe struct DSPSettings
	{
		public float* MatrixCoefficients;
		public float[] DelayTimes;
		public uint SrcChannelCount;
		public uint DstChannelCount;
		public float LPFDirectCoefficient;
		public float LPFReverbCoefficient;
		public float ReverbLevel;
		public float DopplerFactor;
		public float EmitterToListenerAngle;
		public float EmitterToListenerDistance;
		public float EmitterVelocityComponent;
		public float ListenerVelocityComponent;

		public readonly Span<float> MatrixCoefficientsSpan => new(MatrixCoefficients, (int) (SrcChannelCount * DstChannelCount));
	};

	public record struct F3DAudio(
		uint SpeakerMask,
		uint SpeakerCount,
		uint SpeakerLFIndex,
		float SpeedOfSound,
		float SpeedOfSoundEpsilon);

	public enum AttenuationModel
	{
		None,
		Inverse,
		Linear,
		Exponential
	}

	public static F3DAudio F3DAudioInitialize(
		uint SpeakerChannelMask,
		float SpeedOfSound)
	{
		F3DAudio instance = default;
		instance.SpeakerMask = SpeakerChannelMask;
		instance.SpeedOfSound = SpeedOfSound;
		instance.SpeedOfSoundEpsilon = float.BitDecrement(SpeedOfSound);
		instance.SpeakerLFIndex = 0xFFFFFFFF;

		if ((SpeakerChannelMask & FAudio.SPEAKER_LOW_FREQUENCY) > 0)
		{
			if ((SpeakerChannelMask & FAudio.SPEAKER_FRONT_CENTER) > 0)
			{
				instance.SpeakerLFIndex = 3;
			}
			else
			{
				instance.SpeakerLFIndex = 2;
			}
		}

		uint speakerCount = 0;
		while (SpeakerChannelMask > 0)
		{
			speakerCount += 1;
			SpeakerChannelMask &= SpeakerChannelMask - 1;
		}
		instance.SpeakerCount = speakerCount;

		return instance;
	}

	/* This structure represent a tuple of vectors that form a left-handed basis.
	 * That is, all vectors are normal, orthogonal to each other, and taken in the
	 * order front, right, top they follow the left-hand rule.
	 * (https://en.wikipedia.org/wiki/Right-hand_rule)
	 */
	struct Basis
	{
		public Vector3 Front;
		public Vector3 Right;
		public Vector3 Top;
	};

	static float LinearAttenuation(float distance, float minDistance, float maxDistance, float rolloff)
	{
		if (minDistance >= maxDistance)
		{
			return 1; // avoid divide-by-zero
		}

		return 1 - rolloff * (float.Clamp(distance, minDistance, maxDistance) - minDistance) / (maxDistance - minDistance);
	}

	static float InverseAttenuation(float distance, float minDistance, float maxDistance, float rolloff)
	{
		if (minDistance >= maxDistance)
		{
			return 1; // avoid divide-by-zero
		}

		return minDistance / (minDistance + rolloff * (float.Clamp(distance, minDistance, maxDistance) - minDistance));
	}

	static float ExponentialAttenuation(float distance, float minDistance, float maxDistance, float rolloff)
	{
		if (minDistance >= maxDistance)
		{
			return 1; // avoid divide-by-zero
		}

		return float.Pow(float.Clamp(distance, minDistance, maxDistance) / minDistance, -rolloff);
	}

	// This function computes the distance attenuation according to the model given.
	static float ComputeDistanceAttenuation(
		float distance,
		float minDistance,
		float maxDistance,
		float rolloff,
		AttenuationModel attenuationModel)
	{
		return attenuationModel switch
		{
			AttenuationModel.Inverse => InverseAttenuation(distance, minDistance, maxDistance, rolloff),
			AttenuationModel.Linear => LinearAttenuation(distance, minDistance, maxDistance, rolloff),
			AttenuationModel.Exponential => ExponentialAttenuation(distance, minDistance, maxDistance, rolloff),
			_ => 1,
		};
	}

	static float ComputeConeParameter(
		float distance,
		float angle,
		float innerAngle,
		float outerAngle,
		float innerParam,
		float outerParam)
	{
		/* When computing whether a point lies inside a cone, X3DAUDIO first determines
		 * whether the point is close enough to the apex of the cone.
		 * If it is, the innerParam is used.
		 * The following constant is the one that is used for this distance check;
		 * It is an approximation, found by manual binary search.
		 * TODO: find the exact value of the constant via automated binary search. */
		const float CONE_NULL_DISTANCE_TOLERANCE = 0.00000001f;

		float halfInnerAngle, halfOuterAngle, alpha;

		/* Quote X3DAudio.h:
		 * "Set both cone angles to 0 or X3DAUDIO_2PI for omnidirectionality using
		 * only the outer or inner values respectively."
		 */
		if (innerAngle == 0.0f && outerAngle == 0.0f)
		{
			return outerParam;
		}
		if (innerAngle == float.Pi * 2 && outerAngle == float.Pi * 2)
		{
			return innerParam;
		}

		/* If we're within the inner angle, or close enough to the apex, we use
		 * the innerParam. */
		halfInnerAngle = innerAngle / 2.0f;
		if (distance <= CONE_NULL_DISTANCE_TOLERANCE || angle <= halfInnerAngle)
		{
			return innerParam;
		}

		/* If we're between the inner angle and the outer angle, we must use
		 * some interpolation of the innerParam and outerParam according to the
		 * distance between our angle and the inner and outer angles.
		 */
		halfOuterAngle = outerAngle / 2.0f;
		if (angle <= halfOuterAngle)
		{
			alpha = (angle - halfInnerAngle) / (halfOuterAngle - halfInnerAngle);

			/* Sooo... This is awkward. MSDN doesn't say anything, but
			 * X3DAudio.h says that this should be lerped. However in
			 * practice the behaviour of X3DAudio isn't a lerp at all. It's
			 * easy to see with big (InnerAngle / OuterAngle) values. If we
			 * want accurate emulation, we'll need to either find what
			 * formula they use, or use a more advanced interpolation, like
			 * tricubic.
			 *
			 * TODO: HIGH_ACCURACY version.
			 * -Adrien
			 */
			return float.Lerp(alpha, innerParam, outerParam);
		}

		/* Otherwise, we're outside the outer angle, so we just return the outer param. */
		return outerParam;
	}

	/* Here we declare the azimuths of every speaker for every speaker
	 * configuration, ordered by increasing angle, as well as the index to which
	 * they map in the final matrix for their respective configuration. It had to be
	 * reverse engineered by looking at the data from various X3DAudioCalculate()
	 * matrix results for the various speaker configurations; *in particular*, the
	 * azimuths are different from the ones in F3DAudio.h (and X3DAudio.h) for
	 * SPEAKER_STEREO (which is declared has having front L and R speakers in the
	 * bit mask, but in fact has L and R *side* speakers). LF speakers are
	 * deliberately not included in the SpeakerInfo list, rather, we store the index
	 * into a separate field (with a -1 sentinel value if it has no LF speaker).
	 * -Adrien
	 */
	readonly record struct SpeakerInfo(float Azimuth, uint MatrixIdx);

	record class ConfigInfo(uint ConfigMask, SpeakerInfo[] Speakers, int LFSpeakerIdx);

	/* It is absolutely necessary that these are stored in increasing, *positive*
	 * azimuth order (i.e. all angles between [0; 2PI]), as we'll do a linear
	 * interval search inside FindSpeakerAzimuths.
	 * -Adrien
	 */

	const float SPEAKER_AZIMUTH_CENTER = 0.0f;
	const float SPEAKER_AZIMUTH_FRONT_RIGHT_OF_CENTER = float.Pi * 1.0f / 8.0f;
	const float SPEAKER_AZIMUTH_FRONT_RIGHT = float.Pi * 1.0f / 4.0f;
	const float SPEAKER_AZIMUTH_SIDE_RIGHT = float.Pi * 1.0f / 2.0f;
	const float SPEAKER_AZIMUTH_BACK_RIGHT = float.Pi * 3.0f / 4.0f;
	const float SPEAKER_AZIMUTH_BACK_CENTER = float.Pi;
	const float SPEAKER_AZIMUTH_BACK_LEFT = float.Pi * 5.0f / 4.0f;
	const float SPEAKER_AZIMUTH_SIDE_LEFT = float.Pi * 3.0f / 2.0f;
	const float SPEAKER_AZIMUTH_FRONT_LEFT = float.Pi * 7.0f / 4.0f;
	const float SPEAKER_AZIMUTH_FRONT_LEFT_OF_CENTER = float.Pi * 15.0f / 8.0f;

	static readonly SpeakerInfo[] kMonoConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_CENTER, 0)
	];

	static readonly SpeakerInfo[] kStereoConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_RIGHT, 1),
		new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_LEFT,  0),
	];

	static readonly SpeakerInfo[] k2Point1ConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_RIGHT, 1),
		new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_LEFT,  0),
	];

	static readonly SpeakerInfo[] kSurroundConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_CENTER, 2),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_CENTER, 3),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT, 0)
		];

	static readonly SpeakerInfo[] kQuadConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_RIGHT,  3),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_LEFT,   2),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,  0),
	];

	static readonly SpeakerInfo[] k4Point1ConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_RIGHT,  4),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_LEFT,   3),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,  0),
	];

	static readonly SpeakerInfo[] k5Point1ConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_CENTER,      2),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_RIGHT,  5),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_LEFT,   4),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,  0),
	];

	static readonly SpeakerInfo[] k7Point1ConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_CENTER,                2),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT_OF_CENTER, 7),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT,           1),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_RIGHT,            5),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_LEFT,             4),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,            0),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT_OF_CENTER,  6),
	];

	static readonly SpeakerInfo[] k5Point1SurroundConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_CENTER,      2),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
		new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_RIGHT,  5),
		new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_LEFT,   4),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,  0),
	];

	static readonly SpeakerInfo[] k7Point1SurroundConfigSpeakers =
	[
		new SpeakerInfo(SPEAKER_AZIMUTH_CENTER,      2),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
		new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_RIGHT,  7),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_RIGHT,  5),
		new SpeakerInfo(SPEAKER_AZIMUTH_BACK_LEFT,   4),
		new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_LEFT,   6),
		new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,  0),
	];

	/* With that organization, the index of the LF speaker into the matrix array
	 * strangely looks *exactly* like the mystery field in the F3DAUDIO_HANDLE!!
	 * We're keeping a separate field within ConfigInfo because it makes the code
	 * much cleaner, though.
	 * -Adrien
	 */
	static readonly ConfigInfo[] kSpeakersConfigInfo =
	[
		new ConfigInfo(FAudio.SPEAKER_MONO,             kMonoConfigSpeakers,            -1),
		new ConfigInfo(FAudio.SPEAKER_STEREO,           kStereoConfigSpeakers,          -1),
		new ConfigInfo(FAudio.SPEAKER_2POINT1,          k2Point1ConfigSpeakers,          2),
		new ConfigInfo(FAudio.SPEAKER_SURROUND,         kSurroundConfigSpeakers,        -1),
		new ConfigInfo(FAudio.SPEAKER_QUAD,             kQuadConfigSpeakers,            -1),
		new ConfigInfo(FAudio.SPEAKER_4POINT1,          k4Point1ConfigSpeakers,          2),
		new ConfigInfo(FAudio.SPEAKER_5POINT1,          k5Point1ConfigSpeakers,          3),
		new ConfigInfo(FAudio.SPEAKER_7POINT1,          k7Point1ConfigSpeakers,          3),
		new ConfigInfo(FAudio.SPEAKER_5POINT1_SURROUND, k5Point1SurroundConfigSpeakers,  3),
		new ConfigInfo(FAudio.SPEAKER_7POINT1_SURROUND, k7Point1SurroundConfigSpeakers,  3),
	];

	/* A simple linear search is absolutely OK for 10 elements. */
	static ConfigInfo GetConfigInfo(uint speakerConfigMask)
	{
		uint i;
		for (i = 0; i < kSpeakersConfigInfo.Length; i += 1)
		{
			if (kSpeakersConfigInfo[i].ConfigMask == speakerConfigMask)
			{
				return kSpeakersConfigInfo[i];
			}
		}

		return null;
	}

	/* Given a configuration, this function finds the azimuths of the two speakers
	 * between which the emitter lies. All the azimuths here are relative to the
	 * listener's base, since that's where the speakers are defined.
	 */
	static (SpeakerInfo, SpeakerInfo) FindSpeakerAzimuths(
		ConfigInfo config,
		float emitterAzimuth,
		bool skipCenter
	)
	{
		uint i, nexti = 0;
		float a0 = 0.0f, a1 = 0.0f;

		/* We want to find, given an azimuth, which speakers are the closest
		 * ones (in terms of angle) to that azimuth.
		 * This is done by iterating through the list of speaker azimuths, as
		 * given to us by the current ConfigInfo (which stores speaker azimuths
		 * in increasing order of azimuth for each possible speaker configuration;
		 * each speaker azimuth is defined to be between 0 and 2PI by construction).
		 */
		for (i = 0; i < config.Speakers.Length; i += 1)
		{
			/* a0 and a1 are the azimuths of candidate speakers */
			a0 = config.Speakers[i].Azimuth;
			nexti = (uint) ((i + 1) % config.Speakers.Length);
			a1 = config.Speakers[nexti].Azimuth;

			if (a0 < a1)
			{
				if (emitterAzimuth >= a0 && emitterAzimuth < a1)
				{
					break;
				}
			}
			/* It is possible for a speaker pair to enclose the singulary at 0 == 2PI:
			 * consider for example the quad config, which has a front left speaker
			 * at 7PI/4 and a front right speaker at PI/4. In that case a0 = 7PI/4 and
			 * a1 = PI/4, and the way we know whether our current azimuth lies between
			 * that pair is by checking whether the azimuth is greather than 7PI/4 or
			 * whether it's less than PI/4. (By contract, currentAzimuth is always less
			 * than 2PI.)
			 */
			else
			{
				if (emitterAzimuth >= a0 || emitterAzimuth < a1)
				{
					break;
				}
			}
		}

		/* skipCenter means that we don't want to use the center speaker.
		 * The easiest way to deal with this is to check whether either of our candidate
		 * speakers are the center, which always has an azimuth of 0.0. If that is the case
		 * we just replace it with either the previous one or the next one.
		 */
		if (skipCenter)
		{
			if (a0 == 0.0f)
			{
				if (i == 0)
				{
					i = (uint) (config.Speakers.Length - 1);
				}
				else
				{
					i -= 1;
				}
			}
			else if (a1 == 0.0f)
			{
				nexti += 1;
				if (nexti >= config.Speakers.Length)
				{
					nexti -= (uint) config.Speakers.Length;
				}
			}
		}

		return (config.Speakers[i], config.Speakers[nexti]);
	}

	readonly record struct DiffusionSpeakerFactors(float All, float Matching, float Opposite);

	/* ComputeInnerRadiusDiffusionFactors is a utility function that returns how
	 * energy dissipates to the speakers, given the radial distance between the
	 * emitter and the listener and the (optionally 0) InnerRadius distance. It
	 * returns 3 floats, via the diffusionFactors array, that say how much energy
	 * (after distance attenuation) will need to be distributed between each of the
	 * following cases:
	 *
	 * - SPEAKERS_ALL for all (non-LF) speakers, _INCLUDING_ the MATCHING and OPPOSITE.
	 * - SPEAKERS_OPPOSITE corresponds to the two speakers OPPOSITE the emitter.
	 * - SPEAKERS_MATCHING corresponds to the two speakers closest to the emitter.
	 *
	 * For a distance below a certain threshold (DISTANCE_EQUAL_ENERGY), all
	 * speakers receive equal energy.
	 *
	 * Above that, the amount that all speakers receive decreases linearly as radial
	 * distance increases, up until InnerRadius / 2. (If InnerRadius is null, we use
	 * MINIMUM_INNER_RADIUS.)
	 *
	 * At the same time, both opposite and matching speakers start to receive sound
	 * (in addition to the energy they receive from the aforementioned "all
	 * speakers" linear law) according to some unknown as of now law,
	 * that is currently emulated with a LERP. This is true up until InnerRadius.
	 *
	 * Above InnerRadius, only the two matching speakers receive sound.
	 *
	 * For more detail, see the "Inner Radius and Inner Radius Angle" in the
	 * MSDN docs for the X3DAUDIO_EMITTER structure.
	 * https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.x3daudio.x3daudio_emitter(v=vs.85).aspx
	 */
	static DiffusionSpeakerFactors ComputeInnerRadiusDiffusionFactors(
		float radialDistance,
		float InnerRadius
	)
	{
		/* Determined experimentally; this is the midpoint value, i.e. the
		 * value at 0.5 for the matching speakers, used for the standard
		 * diffusion curve.
		 *
		 * Note: It is SUSPICIOUSLY close to 1/sqrt(2), but I haven't figured out why.
		 * -Adrien
		 */
		const float DIFFUSION_LERP_MIDPOINT_VALUE = 0.707107f;

		/* X3DAudio always uses an InnerRadius-like behaviour (i.e. diffusing sound to more than
		 * a pair of speakers) even if InnerRadius is set to 0.0f.
		 * This constant determines the distance at which this behaviour is produced in that case. */
		/* This constant was determined by manual binary search. TODO: get a more accurate version
		 * via an automated binary search. */
		const float DIFFUSION_DISTANCE_MINIMUM_INNER_RADIUS = 0.0000004f;
		float actualInnerRadius = float.Max(InnerRadius, DIFFUSION_DISTANCE_MINIMUM_INNER_RADIUS);
		float normalizedRadialDist;
		float a, ms, os;

		normalizedRadialDist = radialDistance / actualInnerRadius;

		/* X3DAudio does another check for small radial distances before applying any InnerRadius-like
		 * behaviour. This is the constant that determines the threshold: below this distance we simply
		 * diffuse to all speakers equally. */
		const float DIFFUSION_DISTANCE_EQUAL_ENERGY = 0.0000001f;
		if (radialDistance <= DIFFUSION_DISTANCE_EQUAL_ENERGY)
		{
			a = 1.0f;
			ms = 0.0f;
			os = 0.0f;
		}
		else if (normalizedRadialDist <= 0.5f)
		{
			/* Determined experimentally that this is indeed a linear law,
			 * with 100% confidence.
			 * -Adrien
			 */
			a = 1.0f - 2.0f * normalizedRadialDist;

			/* Lerping here is an approximation.
			 * TODO: High accuracy version. Having stared at the curves long
			 * enough, I'm pretty sure this is a quadratic, but trying to
			 * polyfit with numpy didn't give nice, round polynomial
			 * coefficients...
			 * -Adrien
			 */
			ms = float.Lerp(2.0f * normalizedRadialDist, 0.0f, DIFFUSION_LERP_MIDPOINT_VALUE);
			os = 1.0f - a - ms;
		}
		else if (normalizedRadialDist <= 1.0f)
		{
			a = 0.0f;

			/* Similarly, this is a lerp based on the midpoint value; the
			 * real, high-accuracy curve also looks like a quadratic.
			 * -Adrien
			 */
			ms = float.Lerp(2.0f * (normalizedRadialDist - 0.5f), DIFFUSION_LERP_MIDPOINT_VALUE, 1.0f);
			os = 1.0f - a - ms;
		}
		else
		{
			a = 0.0f;
			ms = 1.0f;
			os = 0.0f;
		}

		return new DiffusionSpeakerFactors(a, ms, os);
	}

	/* ComputeEmitterChannelCoefficients handles the coefficients calculation for 1
	 * column of the matrix. It uses ComputeInnerRadiusDiffusionFactors to separate
	 * into three discrete cases; and for each case does the right repartition of
	 * the energy after attenuation to the right speakers, in particular in the
	 * MATCHING and OPPOSITE cases, it gives each of the two speakers found a linear
	 * amount of the energy, according to the angular distance between the emitter
	 * and the speaker azimuth.
	 */
	static void ComputeEmitterChannelCoefficients(
		ConfigInfo curConfig,
		Basis listenerBasis,
		float innerRadius,
		Vector3 channelPosition,
		float attenuation,
		float LFEattenuation,
		uint flags,
		uint currentChannel,
		uint numSrcChannels,
		Span<float> matrixCoefficients
	)
	{
		bool skipCenter = (flags & FAudio.F3DAUDIO_CALCULATE_ZEROCENTER) > 0;
		DiffusionSpeakerFactors diffusionFactors = default;
		uint iS, centerChannelIdx = uint.MaxValue;

		/* We project against the listener basis' top vector to get the elevation of the
		 * current emitter channel position.
		 */
		var elevation = Vector3.Dot(listenerBasis.Top, channelPosition);

		/* To obtain the projection in the front-right plane of the listener's basis of the
		 * emitter channel position, we simply remove the projection against the top vector.
		 * The radial distance is then the length of the projected vector.
		 */
		var projTopVec = listenerBasis.Top * elevation;
		var projPlane = channelPosition - projTopVec;
		var radialDistance = projPlane.Length();

		diffusionFactors = ComputeInnerRadiusDiffusionFactors(
			radialDistance,
			innerRadius
		);

		/* See the ComputeInnerRadiusDiffusionFactors comment above for more context. */
		/* DIFFUSION_SPEAKERS_ALL corresponds to diffusing part of the sound to all of the
		 * speakers, equally. The amount of sound is determined by the float value
		 * diffusionFactors[DIFFUSION_SPEAKERS_ALL]. */
		if (diffusionFactors.All > 0.0f)
		{
			var nChannelsToDiffuseTo = (uint) curConfig.Speakers.Length;
			var totalEnergy = diffusionFactors.All * attenuation;

			if (skipCenter)
			{
				nChannelsToDiffuseTo -= 1;
				centerChannelIdx = curConfig.Speakers[0].MatrixIdx;
			}

			var energyPerChannel = totalEnergy / nChannelsToDiffuseTo;

			for (iS = 0; iS < curConfig.Speakers.Length; iS += 1)
			{
				uint curSpeakerIdx = curConfig.Speakers[iS].MatrixIdx;
				if (skipCenter && curSpeakerIdx == centerChannelIdx)
				{
					continue;
				}

				matrixCoefficients[(int) (curSpeakerIdx * numSrcChannels + currentChannel)] += energyPerChannel;
			}
		}

		/* DIFFUSION_SPEAKERS_MATCHING corresponds to sending part of the sound to the speakers closest
		 * (in terms of azimuth) to the current position of the emitter. The amount of sound we shoud send
		 * corresponds here to diffusionFactors[DIFFUSION_SPEAKERS_MATCHING].
		 * We use the FindSpeakerAzimuths function to find the speakers that match. */
		if (diffusionFactors.Matching > 0.0f)
		{
			var totalEnergy = diffusionFactors.Matching * attenuation;

			var x = Vector3.Dot(listenerBasis.Front, projPlane);
			var y = Vector3.Dot(listenerBasis.Right, projPlane);

			/* Now, a critical point: We shouldn't be sending sound to
			 * matching speakers when x and y are close to 0. That's the
			 * contract we get from ComputeInnerRadiusDiffusionFactors,
			 * which checks that we're not too close to the zero distance.
			 * This allows the atan2 calculation to give good results.
			 */

			/* atan2 returns [-PI, PI], but we want [0, 2PI] */
			var emitterAzimuth = float.Atan2(y, x);
			if (emitterAzimuth < 0.0f)
			{
				emitterAzimuth += float.Pi * 2;
			}

			var (info0, info1) = FindSpeakerAzimuths(curConfig, emitterAzimuth, skipCenter);
			var a0 = info0.Azimuth;
			var a1 = info1.Azimuth;

			/* The following code is necessary to handle the singularity in
			 * (0 == 2PI). It'll give us a nice, well ordered interval.
			 */
			if (a0 > a1)
			{
				if (emitterAzimuth >= a0)
				{
					emitterAzimuth -= float.Pi * 2;
				}
				a0 -= float.Pi * 2;
			}

			var val = (emitterAzimuth - a0) / (a1 - a0);

			var i0 = info0.MatrixIdx;
			var i1 = info1.MatrixIdx;

			matrixCoefficients[(int) (i0 * numSrcChannels + currentChannel)] += (1.0f - val) * totalEnergy;
			matrixCoefficients[(int) (i1 * numSrcChannels + currentChannel)] += (val) * totalEnergy;
		}

		/* DIFFUSION_SPEAKERS_OPPOSITE corresponds to sending part of the sound to the speakers
		 * _opposite_ the ones that are the closest to the current emitter position.
		 * To find these, we simply find the ones that are closest to the current emitter's azimuth + PI
		 * using the FindSpeakerAzimuth function. */
		if (diffusionFactors.Opposite > 0.0f)
		{
			/* This code is similar to the matching speakers code above. */
			var totalEnergy = diffusionFactors.Opposite * attenuation;

			var x = Vector3.Dot(listenerBasis.Front, projPlane);
			var y = Vector3.Dot(listenerBasis.Right, projPlane);

			/* Similarly, we expect atan2 to be well behaved here. */
			var emitterAzimuth = float.Atan2(y, x);

			/* Opposite speakers lie at azimuth + PI */
			emitterAzimuth += float.Pi;

			/* Normalize to [0; 2PI) range. */
			if (emitterAzimuth < 0.0f)
			{
				emitterAzimuth += float.Pi * 2;
			}
			else if (emitterAzimuth > float.Pi * 2)
			{
				emitterAzimuth -= float.Pi * 2;
			}

			var (info0, info1) = FindSpeakerAzimuths(curConfig, emitterAzimuth, skipCenter);
			var a0 = info0.Azimuth;
			var a1 = info1.Azimuth;

			/* The following code is necessary to handle the singularity in
			 * (0 == 2PI). It'll give us a nice, well ordered interval.
			 */
			if (a0 > a1)
			{
				if (emitterAzimuth >= a0)
				{
					emitterAzimuth -= float.Pi * 2;
				}
				a0 -= float.Pi * 2;
			}

			var val = (emitterAzimuth - a0) / (a1 - a0);

			var i0 = info0.MatrixIdx;
			var i1 = info1.MatrixIdx;

			matrixCoefficients[(int) (i0 * numSrcChannels + currentChannel)] += (1.0f - val) * totalEnergy;
			matrixCoefficients[(int) (i1 * numSrcChannels + currentChannel)] += (val) * totalEnergy;
		}

		if ((flags & FAudio.F3DAUDIO_CALCULATE_REDIRECT_TO_LFE) > 0)
		{
			matrixCoefficients[(int) (curConfig.LFSpeakerIdx * numSrcChannels + currentChannel)] += LFEattenuation / numSrcChannels;
		}
	}

	/* Calculations consist of several orthogonal steps that compose multiplicatively:
	 *
	 * First, we compute the attenuations (volume and LFE) due to distance, which
	 * may involve an optional volume and/or LFE volume curve.
	 *
	 * Then, we compute those due to optional cones.
	 *
	 * We then compute how much energy is diffuse w.r.t InnerRadius. If InnerRadius
	 * is 0.0f, this step is computed as if it was InnerRadius was
	 * NON_NULL_DISTANCE_DISK_RADIUS. The way this works is, we look at the radial
	 * distance of the current emitter channel to the listener, with regard to the
	 * listener's top orientation (i.e. this distance is independant of the
	 * emitter's elevation!). If this distance is less than NULL_DISTANCE_RADIUS,
	 * energy is diffused equally between all channels. If it's greater than
	 * InnerRadius (or NON_NULL_DISTANCE_RADIUS, if InnerRadius is 0.0f, as
	 * mentioned above), the two closest speakers, by azimuth, receive all the
	 * energy. Between InnerRadius/2.0f and InnerRadius, the energy starts bleeding
	 * into the opposite speakers. Once we go below InnerRadius/2.0f, the energy
	 * also starts to bleed into the other (non-opposite) channels, if there are
	 * any. This computation is handled by the ComputeInnerRadiusDiffusionFactors
	 * function. (TODO: High-accuracy version of this.)
	 *
	 * Finally, if we're not in the equal diffusion case, we find out the azimuths
	 * of the two closest speakers (with azimuth being defined with respect to the
	 * listener's front orientation, in the plane normal to the listener's top
	 * vector), as well as the azimuths of the two opposite speakers, if necessary,
	 * and linearly interpolate with respect to the angular distance. In the equal
	 * diffusion case, each channel receives the same value.
	 *
	 * Note: in the case of multi-channel emitters, the distance attenuation is only
	 * compted once, but all the azimuths and InnerRadius calculations are done per
	 * emitter channel.
	 *
	 * TODO: Handle InnerRadiusAngle. But honestly the X3DAudio default behaviour is
	 * so wacky that I wonder if anybody has ever used it.
	 * -Adrien
	 */
	static void CalculateMatrix(
		uint channelMask,
		uint flags,
		in Listener listener,
		in Emitter emitter,
		uint srcChannelCount,
		uint dstChannelCount,
		Vector3 emitterToListener,
		float eToLDistance,
		float channelRadius,
		float[] channelAzimuths,
		Span<float> matrixCoefficients)
	{
		uint iEC;
		float curEmAzimuth;
		ConfigInfo curConfig = GetConfigInfo(channelMask);

		float attenuation = ComputeDistanceAttenuation(
			eToLDistance,
			emitter.MinDistance,
			emitter.MaxDistance,
			emitter.Rolloff,
			emitter.AttenuationModel
		);

		float LFEattenuation = ComputeDistanceAttenuation(
			eToLDistance,
			emitter.MinDistance,
			emitter.MaxDistance,
			emitter.Rolloff,
			emitter.AttenuationModel
		);

		Vector3 listenerToEmitter;
		Vector3 listenerToEmChannel;
		Basis listenerBasis;

		/* Note: For both cone calculations, the angle might be NaN or infinite
		 * if distance == 0... ComputeConeParameter *does* check for this
		 * special case. It is necessary that we still go through the
		 * ComputeConeParameter function, because omnidirectional cones might
		 * give either InnerVolume or OuterVolume.
		 * -Adrien
		 */
		if (listener.UsesCone)
		{
			/* Negate the dot product because we need listenerToEmitter in
			 * this case
			 * -Adrien
			 */
			var angle = -float.Acos(
				Vector3.Dot(listener.Forward, emitterToListener) /
				eToLDistance
			);

			var listenerConeParam = ComputeConeParameter(
				eToLDistance,
				angle,
				listener.Cone.InnerAngle,
				listener.Cone.OuterAngle,
				listener.Cone.InnerVolume,
				listener.Cone.OuterVolume
			);
			attenuation *= listenerConeParam;
			LFEattenuation *= listenerConeParam;
		}

		/* See note above. */
		if (emitter.UsesCone && srcChannelCount == 1)
		{
			var angle = float.Acos(
				Vector3.Dot(emitter.Forward, emitterToListener) /
				eToLDistance
			);

			var emitterConeParam = ComputeConeParameter(
				eToLDistance,
				angle,
				emitter.Cone.InnerAngle,
				emitter.Cone.OuterAngle,
				emitter.Cone.InnerVolume,
				emitter.Cone.OuterVolume
			);
			attenuation *= emitterConeParam;
		}

		matrixCoefficients.Clear();

		/* In the SPEAKER_MONO case, we can skip all energy diffusion calculation. */
		if (dstChannelCount == 1)
		{
			for (iEC = 0; iEC < srcChannelCount; iEC += 1)
			{
				curEmAzimuth = 0.0f;
				if (channelAzimuths != null)
				{
					curEmAzimuth = channelAzimuths[iEC];
				}

				/* The MONO setup doesn't have an LFE speaker. */
				if (curEmAzimuth != float.Pi * 2)
				{
					matrixCoefficients[(int) iEC] = attenuation;
				}
			}
		}
		else if (curConfig != null)
		{
			listenerToEmitter = emitterToListener * -1.0f;

			// FAudio expects a left-handed coordinate system so let's convert.
			listenerBasis.Front = -listener.Forward;
			listenerBasis.Right = -Vector3.Cross(listener.Up, listener.Forward);
			listenerBasis.Top = listener.Up;

			/* Handling the mono-channel emitter case separately is easier
			 * than having it as a separate case of a for-loop; indeed, in
			 * this case, we need to ignore the non-relevant values from the
			 * emitter, _even if they're set_.
			 */
			if (srcChannelCount == 1)
			{
				listenerToEmChannel = listenerToEmitter;

				ComputeEmitterChannelCoefficients(
					curConfig,
					listenerBasis,
					emitter.InnerRadius,
					listenerToEmChannel,
					attenuation,
					LFEattenuation,
					flags,
					0 /* currentChannel */,
					1 /* numSrcChannels */,
					matrixCoefficients
				);
			}
			else /* Multi-channel emitter case. */
			{
				var emitterRight = Vector3.Cross(emitter.Up, emitter.Forward);

				for (iEC = 0; iEC < srcChannelCount; iEC += 1)
				{
					var emChAzimuth = channelAzimuths[iEC];

					/* LFEs are easy enough to deal with; we can
					 * just do them separately.
					 */
					if (emChAzimuth == float.Pi * 2)
					{
						matrixCoefficients[(int) (curConfig.LFSpeakerIdx * srcChannelCount + iEC)] = LFEattenuation;
					}
					else
					{
						/* First compute the emitter channel
						 * vector relative to the emitter base...
						 */
						var emitterBaseToChannel =
							emitter.Forward * channelRadius * float.Cos(emChAzimuth) +
							emitterRight * channelRadius * float.Sin(emChAzimuth);

						/* ... then translate. */
						listenerToEmChannel = listenerToEmitter + emitterBaseToChannel;

						ComputeEmitterChannelCoefficients(
							curConfig,
							listenerBasis,
							emitter.InnerRadius,
							listenerToEmChannel,
							attenuation,
							LFEattenuation,
							flags,
							iEC,
							srcChannelCount,
							matrixCoefficients
						);
					}
				}
			}
		}
		else
		{
			Logger.LogWarn("FAudio: config info not found!");
		}
	}

	/*
	 * OTHER CALCULATIONS
	 */

	/* DopplerPitchScalar
	 * Adapted from algorithm published as a part of the webaudio specification:
	 * https://dvcs.w3.org/hg/audio/raw-file/tip/webaudio/specification.html#Spatialization-doppler-shift
	 * -Chad
	 */
	static void CalculateDoppler(
		float speedOfSound,
		float speedOfSoundEpsilon,
		Listener listener,
		Emitter emitter,
		Vector3 emitterToListener,
		float eToLDistance,
		out float listenerVelocityComponent,
		out float emitterVelocityComponent,
		out float DopplerFactor
	)
	{
		DopplerFactor = 1.0f;

		/* Project... */
		if (eToLDistance > float.Epsilon)
		{
			listenerVelocityComponent =
				Vector3.Dot(emitterToListener, listener.Velocity) / eToLDistance;
			emitterVelocityComponent =
				Vector3.Dot(emitterToListener, emitter.Velocity) / eToLDistance;

			if (emitter.DopplerScaler > 0.0f)
			{
				/* Multiply... */
				float scaledListenerVelocityComponent = listenerVelocityComponent * emitter.DopplerScaler;
				float scaledEmitterVelocityComponent = emitterVelocityComponent * emitter.DopplerScaler;

				/* ... then Clamp. */
				DopplerFactor = (
					speedOfSound - float.Clamp(scaledListenerVelocityComponent, -speedOfSoundEpsilon, speedOfSoundEpsilon)
				) / (
					speedOfSound - float.Clamp(scaledEmitterVelocityComponent, -speedOfSoundEpsilon, speedOfSoundEpsilon)
				);

				/* Limit the pitch shifting to 2 octaves up and 1 octave down */
				DopplerFactor = float.Clamp(
					DopplerFactor,
					0.5f,
					4.0f
				);
			}
		}
		else
		{
			listenerVelocityComponent = 0.0f;
			emitterVelocityComponent = 0.0f;
		}
	}

	public static void Calculate(
		F3DAudio Instance,
		in Listener listener,
		in Emitter emitter,
		uint flags,
		float channelRadius,
		float[] channelAzimuths,
		ref DSPSettings dspSettings)
	{
		uint i;
		Vector3 emitterToListener;
		float eToLDistance, dp;

		emitterToListener = (listener.Position - emitter.Position) * emitter.CurveDistanceScaler;
		eToLDistance = emitterToListener.Length();
		dspSettings.EmitterToListenerDistance = eToLDistance;

		if ((flags & FAudio.F3DAUDIO_CALCULATE_MATRIX) > 0)
		{
			CalculateMatrix(
				Instance.SpeakerMask,
				flags,
				listener,
				emitter,
				dspSettings.SrcChannelCount,
				dspSettings.DstChannelCount,
				emitterToListener,
				eToLDistance,
				channelRadius,
				channelAzimuths,
				dspSettings.MatrixCoefficientsSpan
			);
		}

		// TODO: could optimize these
		if ((flags & FAudio.F3DAUDIO_CALCULATE_LPF_DIRECT) > 0)
		{
			dspSettings.LPFDirectCoefficient = ComputeDistanceAttenuation(
				eToLDistance,
				emitter.MinDistance,
				emitter.MaxDistance,
				emitter.Rolloff,
				emitter.AttenuationModel
			);
		}

		if ((flags & FAudio.F3DAUDIO_CALCULATE_LPF_REVERB) > 0)
		{
			dspSettings.LPFReverbCoefficient = ComputeDistanceAttenuation(
				eToLDistance,
				emitter.MinDistance,
				emitter.MaxDistance,
				emitter.Rolloff,
				emitter.AttenuationModel
			);
		}

		if ((flags & FAudio.F3DAUDIO_CALCULATE_REVERB) > 0)
		{
			dspSettings.ReverbLevel = ComputeDistanceAttenuation(
				eToLDistance,
				emitter.MinDistance,
				emitter.MaxDistance,
				emitter.Rolloff,
				emitter.AttenuationModel
			);
		}

		if ((flags & FAudio.F3DAUDIO_CALCULATE_DOPPLER) > 0)
		{
			CalculateDoppler(
				Instance.SpeedOfSound,
				Instance.SpeedOfSoundEpsilon,
				listener,
				emitter,
				emitterToListener,
				eToLDistance,
				out dspSettings.ListenerVelocityComponent,
				out dspSettings.EmitterVelocityComponent,
				out dspSettings.DopplerFactor
			);
		}

		if ((flags & FAudio.F3DAUDIO_CALCULATE_EMITTER_ANGLE) > 0)
		{
			/* Determined roughly.
			 * Below that distance, the emitter angle is considered to be PI/2.
			 */
			if (eToLDistance < float.Epsilon)
			{
				dspSettings.EmitterToListenerAngle = float.Pi / 2.0f;
			}
			else
			{
				/* Note: pEmitter->OrientFront is normalized. */
				dp = Vector3.Dot(emitterToListener, emitter.Forward) / eToLDistance;
				dspSettings.EmitterToListenerAngle = float.Acos(dp);
			}
		}

		/* Unimplemented Flags */
		if ((flags & FAudio.F3DAUDIO_CALCULATE_DELAY) > 0 &&
			Instance.SpeakerMask == FAudio.SPEAKER_STEREO)
		{
			for (i = 0; i < dspSettings.DstChannelCount; i += 1)
			{
				dspSettings.DelayTimes[i] = 0.0f;
			}

			Logger.LogWarn("FAUDIO DELAY not implemented!");
		}
	}
}
