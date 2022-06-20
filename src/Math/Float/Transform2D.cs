namespace MoonWorks.Math.Float
{
	public struct Transform2D : System.IEquatable<Transform2D>
	{
		public Vector2 Position { get; }
		public float Rotation { get; }
		public Vector2 Scale { get; }

		private bool transformMatrixCalculated;
		private Matrix3x2 transformMatrix;

		public Matrix3x2 TransformMatrix
		{
			get
			{
				if (!transformMatrixCalculated)
				{
					transformMatrix = CreateTransformMatrix(Position, Rotation, Scale);
					transformMatrixCalculated = true;
				}

				return transformMatrix;
			}
		}

		public bool IsAxisAligned => Rotation % MathHelper.PiOver2 == 0;
		public bool IsUniformScale => Scale.X == Scale.Y;

		public static readonly Transform2D Identity = new Transform2D(Vector2.Zero, 0, Vector2.One);

		public Transform2D(Vector2 position)
		{
			Position = position;
			Rotation = 0;
			Scale = Vector2.One;
			transformMatrixCalculated = false;
			transformMatrix = Matrix3x2.Identity;
		}

		public Transform2D(Vector2 position, float rotation)
		{
			Position = position;
			Rotation = rotation;
			Scale = Vector2.One;
			transformMatrixCalculated = false;
			transformMatrix = Matrix3x2.Identity;
		}

		public Transform2D(Vector2 position, float rotation, Vector2 scale)
		{
			Position = position;
			Rotation = rotation;
			Scale = scale;
			transformMatrixCalculated = false;
			transformMatrix = Matrix3x2.Identity;
		}

		public Transform2D Compose(Transform2D other)
		{
			return new Transform2D(Position + other.Position, Rotation + other.Rotation, Scale * other.Scale);
		}

		private static Matrix3x2 CreateTransformMatrix(Vector2 position, float rotation, Vector2 scale)
		{
			return
				Matrix3x2.CreateScale(scale) *
				Matrix3x2.CreateRotation(rotation) *
				Matrix3x2.CreateTranslation(position);
		}

		public bool Equals(Transform2D other)
		{
			return
				Position == other.Position &&
				Rotation == other.Rotation &&
				Scale == other.Scale;
		}


        public override bool Equals(System.Object other)
        {
            if (other is Transform2D otherTransform)
            {
                return Equals(otherTransform);
            }

            return false;
        }

		public override int GetHashCode()
		{
			return System.HashCode.Combine(Position, Rotation, Scale);
		}

		public static bool operator ==(Transform2D a, Transform2D b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Transform2D a, Transform2D b)
		{
			return !a.Equals(b);
		}
	}
}
