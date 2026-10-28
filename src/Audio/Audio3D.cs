// This code is largely translated from FAudio.
// We need to add a few extra spatialization features, but it's not really worth extending FAudio for those,
// since the 3D features are largely self-contained and require a call to initiate processing.
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

record struct Cone(
    float InnerAngle,
    float OuterAngle,
    float InnerVolume,
    float OuterVolume,
    float InnerLPF,
    float OuterLPF,
    float InnerReverb,
    float OuterReverb);

record struct Listener(
    Vector3 OrientFront,
    Vector3 OrientTop,
    Vector3 Position,
    Vector3 Velocity,
    bool UsesCone,
    Cone Cone);

record struct Emitter(
    bool UsesCone,
    Cone Cone,
    Vector3 OrientFront,
    Vector3 OrientTop,
    Vector3 Position,
    Vector3 Velocity,
    float InnerRadius,
    float InnerRadiusAngle,
    uint ChannelCount,
    float ChannelRadius,
    float CurveDistanceScaler,
    float DopplerScaler); // TODO: more fields?

public class Audio3D
{

/* Quote X3DAUDIO docs:
 * "To be considered orthonormal, a pair of vectors must have a magnitude of
 * 1 +- 1x10-5 and a dot product of 0 +- 1x10-5."
 * VECTOR_NORMAL_CHECK verifies that vectors are normal (i.e. have norm 1 +- 1x10-5)
 * VECTOR_BASE_CHECK verifies that a pair of vectors are orthogonal (i.e. their dot
 * product is 0 +- 1x10-5)
 */

/*************************************
 * F3DAudioInitialize Implementation *
 *************************************/

/* F3DAUDIO_HANDLE Structure */
#define SPEAKERMASK(Instance)		*((uint32_t*)	&Instance[0])
#define SPEAKERCOUNT(Instance)		*((uint32_t*)	&Instance[4])
#define SPEAKER_LF_INDEX(Instance)	*((uint32_t*)	&Instance[8])
#define SPEEDOFSOUND(Instance)		*((float*)	&Instance[12])
#define SPEEDOFSOUNDEPSILON(Instance)	*((float*)	&Instance[16])

void F3DAudioInitialize(
	uint32_t SpeakerChannelMask,
	float SpeedOfSound,
	F3DAUDIO_HANDLE Instance
) {
	F3DAudioInitialize8(SpeakerChannelMask, SpeedOfSound, Instance);
}

uint32_t F3DAudioInitialize8(
	uint32_t SpeakerChannelMask,
	float SpeedOfSound,
	F3DAUDIO_HANDLE Instance
) {
	union
	{
		float f;
		uint32_t i;
	} epsilonHack;
	uint32_t speakerCount = 0;

	if (!F3DAudioCheckInitParams(SpeakerChannelMask, SpeedOfSound, Instance))
	{
		return FAUDIO_E_INVALID_CALL;
	}

	SPEAKERMASK(Instance) = SpeakerChannelMask;
	SPEEDOFSOUND(Instance) = SpeedOfSound;

	/* "Convert" raw float to int... */
	epsilonHack.f = SpeedOfSound;
	/* ... Subtract epsilon value... */
	epsilonHack.i -= 1;
	/* ... Convert back to float. */
	SPEEDOFSOUNDEPSILON(Instance) = epsilonHack.f;

	SPEAKER_LF_INDEX(Instance) = 0xFFFFFFFF;
	if (SpeakerChannelMask & SPEAKER_LOW_FREQUENCY)
	{
		if (SpeakerChannelMask & SPEAKER_FRONT_CENTER)
		{
			SPEAKER_LF_INDEX(Instance) = 3;
		}
		else
		{
			SPEAKER_LF_INDEX(Instance) = 2;
		}
	}

	while (SpeakerChannelMask)
	{
		speakerCount += 1;
		SpeakerChannelMask &= SpeakerChannelMask - 1;
	}
	SPEAKERCOUNT(Instance) = speakerCount;

	return 0;
}


/************************************
 * F3DAudioCalculate Implementation *
 ************************************/

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

/*
 * MATRIX CALCULATION
 */

/* This function computes the distance either according to a curve if pCurve
 * isn't NULL, or according to the inverse distance law 1/d otherwise.
 */
static float ComputeDistanceAttenuation(
	float normalizedDistance
) {
	float res;
	float alpha;
	uint32_t n_points;
	size_t i;
	if (pCurve)
	{
		F3DAUDIO_DISTANCE_CURVE_POINT* points = pCurve->pPoints;
		n_points = pCurve->PointCount;

		/* By definition, the first point in the curve must be 0.0f
		 * -Adrien
		 */

		/* We advance i up until our normalizedDistance lies between the distances of
		 * the i_th and (i-1)_th points, or we reach the last point.
		 */
		for (i = 1; (i < n_points) && (normalizedDistance >= points[i].Distance); i += 1);
		if (i == n_points)
		{
			/* We've reached the last point, so we use its value directly.
			 * Quote X3DAUDIO docs:
			 * "If an emitter moves beyond a distance of (CurveDistanceScaler × 1.0f),
			 * the last point on the curve is used to compute the volume output level."
			 */
			res = points[n_points - 1].DSPSetting;
		}
		else
		{
			/* We're between two points: the distance attenuation is the linear interpolation of the DSPSetting
			 * values defined by our points, according to the distance.
			 */
			alpha = (points[i].Distance - normalizedDistance) / (points[i].Distance - points[i - 1].Distance);
			res = LERP(alpha, points[i].DSPSetting, points[i - 1].DSPSetting);
		}
	}
	else
	{
		res = 1.0f;
		if (normalizedDistance >= 1.0f)
		{
			res /= normalizedDistance;
		}
	}
	return res;
}

static float ComputeConeParameter(
	float distance,
	float angle,
	float innerAngle,
	float outerAngle,
	float innerParam,
	float outerParam
) {
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

const float SPEAKER_AZIMUTH_CENTER                = 0.0f;
const float SPEAKER_AZIMUTH_FRONT_RIGHT_OF_CENTER =	float.Pi *  1.0f / 8.0f;
const float SPEAKER_AZIMUTH_FRONT_RIGHT           = float.Pi *  1.0f / 4.0f;
const float SPEAKER_AZIMUTH_SIDE_RIGHT            = float.Pi *  1.0f / 2.0f;
const float SPEAKER_AZIMUTH_BACK_RIGHT            = float.Pi *  3.0f / 4.0f;
const float SPEAKER_AZIMUTH_BACK_CENTER           =	float.Pi;
const float SPEAKER_AZIMUTH_BACK_LEFT             = float.Pi *  5.0f / 4.0f;
const float SPEAKER_AZIMUTH_SIDE_LEFT             = float.Pi *  3.0f / 2.0f;
const float SPEAKER_AZIMUTH_FRONT_LEFT            = float.Pi *  7.0f / 4.0f;
const float SPEAKER_AZIMUTH_FRONT_LEFT_OF_CENTER  = float.Pi * 15.0f / 8.0f;

static readonly SpeakerInfo[] kMonoConfigSpeakers =
[
	new SpeakerInfo(SPEAKER_AZIMUTH_CENTER, 0)
];

static readonly SpeakerInfo[] kStereoConfigSpeakers =
[
	new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_RIGHT,	1),
	new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_LEFT,	0),
];

static readonly SpeakerInfo[] k2Point1ConfigSpeakers =
[
	new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_RIGHT,	1),
	new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_LEFT,	0),
];

static readonly SpeakerInfo[] kSurroundConfigSpeakers =
[
	new SpeakerInfo(SPEAKER_AZIMUTH_CENTER,	2),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
	new SpeakerInfo(SPEAKER_AZIMUTH_BACK_CENTER, 3),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,	0)
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
	new SpeakerInfo(SPEAKER_AZIMUTH_BACK_RIGHT,	 4),
	new SpeakerInfo(SPEAKER_AZIMUTH_BACK_LEFT,	 3),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,	 0),
];

static readonly SpeakerInfo[] k5Point1ConfigSpeakers =
[
	new SpeakerInfo(SPEAKER_AZIMUTH_CENTER,	     2),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
	new SpeakerInfo(SPEAKER_AZIMUTH_BACK_RIGHT,	 5),
	new SpeakerInfo(SPEAKER_AZIMUTH_BACK_LEFT,	 4),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,	 0),
];

static readonly SpeakerInfo[] k7Point1ConfigSpeakers =
[
	new SpeakerInfo(SPEAKER_AZIMUTH_CENTER,	               2),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT_OF_CENTER, 7),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT,		   1),
	new SpeakerInfo(SPEAKER_AZIMUTH_BACK_RIGHT,			   5),
	new SpeakerInfo(SPEAKER_AZIMUTH_BACK_LEFT,			   4),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,			   0),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT_OF_CENTER,  6),
];

static readonly SpeakerInfo[] k5Point1SurroundConfigSpeakers =
[
	new SpeakerInfo(SPEAKER_AZIMUTH_CENTER,	     2),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
	new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_RIGHT,	 5),
	new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_LEFT,	 4),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,	 0),
];

static readonly SpeakerInfo[] k7Point1SurroundConfigSpeakers =
[
	new SpeakerInfo(SPEAKER_AZIMUTH_CENTER,	     2),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_RIGHT, 1),
	new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_RIGHT,	 7),
	new SpeakerInfo(SPEAKER_AZIMUTH_BACK_RIGHT,	 5),
	new SpeakerInfo(SPEAKER_AZIMUTH_BACK_LEFT,	 4),
	new SpeakerInfo(SPEAKER_AZIMUTH_SIDE_LEFT,	 6),
	new SpeakerInfo(SPEAKER_AZIMUTH_FRONT_LEFT,	 0),
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
	new ConfigInfo(FAudio.SPEAKER_2POINT1,          k2Point1ConfigSpeakers,			 2),
	new ConfigInfo(FAudio.SPEAKER_SURROUND,	        kSurroundConfigSpeakers,		-1),
	new ConfigInfo(FAudio.SPEAKER_QUAD,			    kQuadConfigSpeakers,			-1),
	new ConfigInfo(FAudio.SPEAKER_4POINT1,		    k4Point1ConfigSpeakers,			 2),
	new ConfigInfo(FAudio.SPEAKER_5POINT1,		    k5Point1ConfigSpeakers,			 3),
	new ConfigInfo(FAudio.SPEAKER_7POINT1,		    k7Point1ConfigSpeakers,			 3),
	new ConfigInfo(FAudio.SPEAKER_5POINT1_SURROUND,	k5Point1SurroundConfigSpeakers,	 3),
	new ConfigInfo(FAudio.SPEAKER_7POINT1_SURROUND,	k7Point1SurroundConfigSpeakers,  3),
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
) {
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
) {
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
) {
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

			matrixCoefficients[curSpeakerIdx * numSrcChannels + currentChannel] += energyPerChannel;
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

		matrixCoefficients[i0 * numSrcChannels + currentChannel] += (1.0f - val) * totalEnergy;
		matrixCoefficients[i1 * numSrcChannels + currentChannel] += (       val) * totalEnergy;
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

		matrixCoefficients[i0 * numSrcChannels + currentChannel] += (1.0f - val) * totalEnergy;
		matrixCoefficients[i1 * numSrcChannels + currentChannel] += (       val) * totalEnergy;
	}

	if ((flags & FAudio.F3DAUDIO_CALCULATE_REDIRECT_TO_LFE) > 0)
	{
		matrixCoefficients[curConfig.LFSpeakerIdx * numSrcChannels + currentChannel] += LFEattenuation / numSrcChannels;
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
	float normalizedDistance,
	Span<float> matrixCoefficients
) {
	uint iEC;
	float curEmAzimuth;
	ConfigInfo curConfig = GetConfigInfo(channelMask);

    // TODO: we don't want anything to do with this volume curve design
	float attenuation = ComputeDistanceAttenuation(
		normalizedDistance,
		pEmitter->pVolumeCurve
	);

	/* TODO: same here */
	float LFEattenuation = ComputeDistanceAttenuation(
		normalizedDistance,
		pEmitter->pLFECurve
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
			Vector3.Dot(listener.OrientFront, emitterToListener) /
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
	if (emitter.UsesCone && emitter.ChannelCount == 1)
	{
		var angle = float.Acos(
			Vector3.Dot(emitter.OrientFront, emitterToListener) /
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
		for (iEC = 0; iEC < emitter.ChannelCount; iEC += 1)
		{
			curEmAzimuth = 0.0f;
			if (pEmitter->pChannelAzimuths)
			{
				curEmAzimuth = pEmitter->pChannelAzimuths[iEC];
			}

			/* The MONO setup doesn't have an LFE speaker. */
			if (curEmAzimuth != float.Pi * 2)
			{
				matrixCoefficients[iEC] = attenuation;
			}
		}
	}
	else if (curConfig != null)
	{
		listenerToEmitter = emitterToListener * -1.0f;

		/* Remember here that the coordinate system is Left-Handed. */
		listenerBasis.Front = listener.OrientFront;
		listenerBasis.Right = Vector3.Cross(listener.OrientTop, listener.OrientFront);
		listenerBasis.Top = listener.OrientTop;

		/* Handling the mono-channel emitter case separately is easier
		 * than having it as a separate case of a for-loop; indeed, in
		 * this case, we need to ignore the non-relevant values from the
		 * emitter, _even if they're set_.
		 */
		if (emitter.ChannelCount == 1)
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
			var emitterRight = Vector3.Cross(emitter.OrientTop, emitter.OrientFront);

			for (iEC = 0; iEC < emitter.ChannelCount; iEC += 1)
			{
				const float emChAzimuth = emitter.ChannelAzimuths[iEC];

				/* LFEs are easy enough to deal with; we can
				 * just do them separately.
				 */
				if (emChAzimuth == float.Pi * 2)
				{
					matrixCoefficients[curConfig.LFSpeakerIdx * emitter.ChannelCount + iEC] = LFEattenuation;
				}
				else
				{
					/* First compute the emitter channel
					 * vector relative to the emitter base...
					 */
					var emitterBaseToChannel = 
						emitter.OrientFront * emitter.ChannelRadius * float.Cos(emChAzimuth) +
						emitterRight * emitter.ChannelRadius * float.Sin(emChAzimuth);

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
						emitter.ChannelCount,
						matrixCoefficients
					);
				}
			}
		}
	}
	else
	{
		FAudio_assert(0 && "Config info not found!");
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
) {
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

void F3DAudioCalculate(
	const F3DAUDIO_HANDLE Instance,
	const F3DAUDIO_LISTENER *pListener,
	const F3DAUDIO_EMITTER *pEmitter,
	uint32_t Flags,
	F3DAUDIO_DSP_SETTINGS *pDSPSettings
) {
	uint32_t i;
	F3DAUDIO_VECTOR emitterToListener;
	float eToLDistance, normalizedDistance, dp;

	/* Clang treats #pragma pack(push, 1) as both pack and align attributes.
	 * 1 byte alignment can produce UB or even errors with modern Apple linker:
	 * https://github.com/FNA-XNA/FAudio/issues/362
	 * - F3DAUDIO_DISTANCE_CURVE_POINT requires 4 bytes alignment for float values
	 * - F3DAUDIO_DISTANCE_CURVE requires 8 bytes alignment for 64-bit pointer
	 */
	#define DEFAULT_POINTS(name, x1, y1, x2, y2) \
		static ALIGN(F3DAUDIO_DISTANCE_CURVE_POINT, 4) name##Points[2] = \
		{ \
			{ x1, y1 }, \
			{ x2, y2 } \
		}; \
		static ALIGN(F3DAUDIO_DISTANCE_CURVE, 8) name##Default = \
		{ \
			(F3DAUDIO_DISTANCE_CURVE_POINT*) &name##Points[0], 2 \
		};
	DEFAULT_POINTS(lpfDirect, 0.0f, 1.0f, 1.0f, 0.75f)
	DEFAULT_POINTS(lpfReverb, 0.0f, 0.75f, 1.0f, 0.75f)
	DEFAULT_POINTS(reverb, 0.0f, 1.0f, 1.0f, 0.0f)
	#undef DEFAULT_POINTS

	/* For XACT, this calculates "Distance" */
	emitterToListener = VectorSub(pListener->Position, pEmitter->Position);
	eToLDistance = VectorLength(emitterToListener);
	pDSPSettings->EmitterToListenerDistance = eToLDistance;

	F3DAudioCheckCalculateParams(Instance, pListener, pEmitter, Flags, pDSPSettings);

	/* This is used by MATRIX, LPF, and REVERB */
	normalizedDistance = eToLDistance / pEmitter->CurveDistanceScaler;

	if (Flags & F3DAUDIO_CALCULATE_MATRIX)
	{
		CalculateMatrix(
			SPEAKERMASK(Instance),
			Flags,
			pListener,
			pEmitter,
			pDSPSettings->SrcChannelCount,
			pDSPSettings->DstChannelCount,
			emitterToListener,
			eToLDistance,
			normalizedDistance,
			pDSPSettings->pMatrixCoefficients
		);
	}

	if (Flags & F3DAUDIO_CALCULATE_LPF_DIRECT)
	{
		pDSPSettings->LPFDirectCoefficient = ComputeDistanceAttenuation(
			normalizedDistance,
			(pEmitter->pLPFDirectCurve != NULL) ?
				pEmitter->pLPFDirectCurve :
				&lpfDirectDefault
		);
	}

	if (Flags & F3DAUDIO_CALCULATE_LPF_REVERB)
	{
		pDSPSettings->LPFReverbCoefficient = ComputeDistanceAttenuation(
			normalizedDistance,
			(pEmitter->pLPFReverbCurve != NULL) ?
				pEmitter->pLPFReverbCurve :
				&lpfReverbDefault
		);
	}

	if (Flags & F3DAUDIO_CALCULATE_REVERB)
	{
		pDSPSettings->ReverbLevel = ComputeDistanceAttenuation(
			normalizedDistance,
			(pEmitter->pReverbCurve != NULL) ?
				pEmitter->pReverbCurve :
				&reverbDefault
		);
	}

	/* For XACT, this calculates "DopplerPitchScalar" */
	if (Flags & F3DAUDIO_CALCULATE_DOPPLER)
	{
		CalculateDoppler(
			SPEEDOFSOUND(Instance),
			SPEEDOFSOUNDEPSILON(Instance),
			pListener,
			pEmitter,
			emitterToListener,
			eToLDistance,
			&pDSPSettings->ListenerVelocityComponent,
			&pDSPSettings->EmitterVelocityComponent,
			&pDSPSettings->DopplerFactor
		);
	}

	/* For XACT, this calculates "OrientationAngle" */
	if (Flags & F3DAUDIO_CALCULATE_EMITTER_ANGLE)
	{
		/* Determined roughly.
		 * Below that distance, the emitter angle is considered to be PI/2.
		 */
		if (eToLDistance < FLT_EPSILON)
		{
			pDSPSettings->EmitterToListenerAngle = F3DAUDIO_PI / 2.0f;
		}
		else
		{
			/* Note: pEmitter->OrientFront is normalized. */
			dp = VectorDot(emitterToListener, pEmitter->OrientFront) / eToLDistance;
			pDSPSettings->EmitterToListenerAngle = FAudio_acosf(dp);
		}
	}

	/* Unimplemented Flags */
	if (	(Flags & F3DAUDIO_CALCULATE_DELAY) &&
		SPEAKERMASK(Instance) == SPEAKER_STEREO	)
	{
		for (i = 0; i < pDSPSettings->DstChannelCount; i += 1)
		{
			pDSPSettings->pDelayTimes[i] = 0.0f;
		}
		FAudio_assert(0 && "DELAY not implemented!");
	}
}

/* vim: set noexpandtab shiftwidth=8 tabstop=8: */

}

