using System;
using MoonWorks.Math.Float;

namespace MoonWorks.Audio
{
	public class AudioListener : AudioResource
	{
		internal FAudio.F3DAUDIO_LISTENER listenerData;

		public Vector3 Forward
		{
			get
			{
				return new Vector3(
					listenerData.OrientFront.x,
					listenerData.OrientFront.y,
					-listenerData.OrientFront.z
				);
			}
			set
			{
				listenerData.OrientFront.x = value.X;
				listenerData.OrientFront.y = value.Y;
				listenerData.OrientFront.z = -value.Z;
			}
		}

		public Vector3 Position
		{
			get
			{
				return new Vector3(
					listenerData.Position.x,
					listenerData.Position.y,
					-listenerData.Position.z
				);
			}
			set
			{
				listenerData.Position.x = value.X;
				listenerData.Position.y = value.Y;
				listenerData.Position.z = -value.Z;
			}
		}


		public Vector3 Up
		{
			get
			{
				return new Vector3(
					listenerData.OrientTop.x,
					listenerData.OrientTop.y,
					-listenerData.OrientTop.z
				);
			}
			set
			{
				listenerData.OrientTop.x = value.X;
				listenerData.OrientTop.y = value.Y;
				listenerData.OrientTop.z = -value.Z;
			}
		}

		public Vector3 Velocity
		{
			get
			{
				return new Vector3(
					listenerData.Velocity.x,
					listenerData.Velocity.y,
					-listenerData.Velocity.z
				);
			}
			set
			{
				listenerData.Velocity.x = value.X;
				listenerData.Velocity.y = value.Y;
				listenerData.Velocity.z = -value.Z;
			}
		}

		public AudioListener(AudioDevice device) : base(device)
		{
			listenerData = new FAudio.F3DAUDIO_LISTENER();
			Forward = Vector3.Forward;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;

			/* Unexposed variables, defaults based on XNA behavior */
			listenerData.pCone = IntPtr.Zero;
		}

		protected override void Destroy() { }
	}
}
