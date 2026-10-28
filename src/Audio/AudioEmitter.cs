using System;
using System.Runtime.InteropServices;
using System.Numerics;

namespace MoonWorks.Audio
{
	/// <summary>
	/// An emitter for 3D spatial audio.
	/// </summary>
	public class AudioEmitter : AudioResource
	{
		internal FAudio.F3DAUDIO_EMITTER emitterData;

		public float MinDistance = 0;
		public float MaxDistance = float.MaxValue;
		public float Rolloff;
		public float DopplerFactor;

		public Vector3 Position;
		public Vector3 Forward;
		public Vector3 Up;
		public Vector3 Velocity;

		private static readonly float[] stereoAzimuth = new float[]
		{
			0.0f, 0.0f
		};

		private static readonly GCHandle stereoAzimuthHandle = GCHandle.Alloc(
			stereoAzimuth,
			GCHandleType.Pinned
		);

		public AudioEmitter(AudioDevice device) : base(device)
		{
			emitterData = new FAudio.F3DAUDIO_EMITTER();

			DopplerScale = 1f;
			Forward = -Vector3.UnitZ;
			Position = Vector3.Zero;
			Up = Vector3.UnitY;
			Velocity = Vector3.Zero;

			/* Unexposed variables, defaults based on XNA behavior */
			emitterData.pCone = IntPtr.Zero;
			emitterData.ChannelCount = 1;
			emitterData.ChannelRadius = 1.0f;
			emitterData.pChannelAzimuths = stereoAzimuthHandle.AddrOfPinnedObject();
			emitterData.pVolumeCurve = IntPtr.Zero;
			emitterData.pLFECurve = IntPtr.Zero;
			emitterData.pLPFDirectCurve = IntPtr.Zero;
			emitterData.pLPFReverbCurve = IntPtr.Zero;
			emitterData.pReverbCurve = IntPtr.Zero;
			emitterData.CurveDistanceScaler = 1.0f;
		}
	}
}
