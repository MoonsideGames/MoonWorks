using System.Collections.Generic;
using MoonWorks.Math;

namespace MoonWorks.Collision
{
	/// <summary>
	/// Used to quickly check if two shapes are potentially overlapping.
	/// </summary>
	/// <typeparam name="T">The type that will be used to uniquely identify shape-transform pairs.</typeparam>
	public class SpatialHash2D<T> where T : System.IEquatable<T>
	{
		private readonly int cellSize;

		private readonly Dictionary<long, HashSet<T>> hashDictionary = new Dictionary<long, HashSet<T>>();
		private readonly Dictionary<T, (ICollidable, Transform2D, uint)> IDLookup = new Dictionary<T, (ICollidable, Transform2D, uint)>();

		public int MinX { get; private set; } = 0;
		public int MaxX { get; private set; } = 0;
		public int MinY { get; private set; } = 0;
		public int MaxY { get; private set; } = 0;

		private Queue<HashSet<T>> hashSetPool = new Queue<HashSet<T>>();

		public SpatialHash2D(int cellSize)
		{
			this.cellSize = cellSize;
		}

		private (int, int) Hash(Vector2 position)
		{
			return ((int) System.Math.Floor(position.X / cellSize), (int) System.Math.Floor(position.Y / cellSize));
		}

		/// <summary>
		/// Inserts an element into the SpatialHash.
		/// </summary>
		/// <param name="id">A unique ID for the shape-transform pair.</param>
		/// <param name="shape"></param>
		/// <param name="transform2D"></param>
		/// <param name="collisionGroups">A bitmask value specifying the groups this object belongs to.</param>
		public void Insert(T id, ICollidable shape, Transform2D transform2D, uint collisionGroups = uint.MaxValue)
		{
			var box = shape.TransformedAABB(transform2D);
			var minHash = Hash(box.Min);
			var maxHash = Hash(box.Max);

			for (var i = minHash.Item1; i <= maxHash.Item1; i++)
			{
				for (var j = minHash.Item2; j <= maxHash.Item2; j++)
				{
					var key = MakeLong(i, j);
					if (!hashDictionary.ContainsKey(key))
					{
						hashDictionary.Add(key, new HashSet<T>());
					}

					hashDictionary[key].Add(id);
					IDLookup[id] = (shape, transform2D, collisionGroups);
				}
			}

			MinX = System.Math.Min(MinX, minHash.Item1);
			MinY = System.Math.Min(MinY, minHash.Item2);
			MaxX = System.Math.Max(MaxX, maxHash.Item1);
			MaxY = System.Math.Max(MaxY, maxHash.Item2);
		}

		/// <summary>
		/// Retrieves all the potential collisions of a shape-transform pair. Excludes any shape-transforms with the given ID.
		/// </summary>
		public IEnumerable<(T, ICollidable, Transform2D, uint)> Retrieve(T id, ICollidable shape, Transform2D transform2D, uint collisionMask = uint.MaxValue)
		{
			var returned = AcquireHashSet();

			var box = shape.TransformedAABB(transform2D);
			var (minX, minY) = Hash(box.Min);
			var (maxX, maxY) = Hash(box.Max);

			if (minX < MinX) { minX = MinX; }
			if (maxX > MaxX) { maxX = MaxX; }
			if (minY < MinY) { minY = MinY; }
			if (maxY > MaxY) { maxY = MaxY; }

			for (var i = minX; i <= maxX; i++)
			{
				for (var j = minY; j <= maxY; j++)
				{
					var key = MakeLong(i, j);
					if (hashDictionary.ContainsKey(key))
					{
						foreach (var t in hashDictionary[key])
						{
							if (!returned.Contains(t))
							{
								var (otherShape, otherTransform, collisionGroups) = IDLookup[t];
								if (!id.Equals(t) && ((collisionGroups & collisionMask) > 0) && AABB2D.TestOverlap(box, otherShape.TransformedAABB(otherTransform)))
								{
									returned.Add(t);
									yield return (t, otherShape, otherTransform, collisionGroups);
								}
							}
						}
					}
				}
			}

			FreeHashSet(returned);
		}


		/// <summary>
		/// Retrieves objects based on a pre-transformed AABB.
		/// </summary>
		/// <param name="aabb">A transformed AABB.</param>
		/// <returns></returns>
		public IEnumerable<(T, ICollidable, Transform2D, uint)> Retrieve(AABB2D aabb, uint collisionMask = uint.MaxValue)
		{
			var returned = AcquireHashSet();

			var (minX, minY) = Hash(aabb.Min);
			var (maxX, maxY) = Hash(aabb.Max);

			if (minX < MinX) { minX = MinX; }
			if (maxX > MaxX) { maxX = MaxX; }
			if (minY < MinY) { minY = MinY; }
			if (maxY > MaxY) { maxY = MaxY; }

			for (var i = minX; i <= maxX; i++)
			{
				for (var j = minY; j <= maxY; j++)
				{
					var key = MakeLong(i, j);
					if (hashDictionary.ContainsKey(key))
					{
						foreach (var t in hashDictionary[key])
						{
							if (!returned.Contains(t))
							{
								var (otherShape, otherTransform, collisionGroups) = IDLookup[t];
								if (((collisionGroups & collisionMask) > 0) && AABB2D.TestOverlap(aabb, otherShape.TransformedAABB(otherTransform)))
								{
									yield return (t, otherShape, otherTransform, collisionGroups);
								}
							}
						}
					}
				}
			}

			FreeHashSet(returned);
		}

		/// <summary>
		/// Removes a specific ID from the SpatialHash.
		/// </summary>
		public void Remove(T id)
		{

		}

		/// <summary>
		/// Removes everything that has been inserted into the SpatialHash.
		/// </summary>
		public void Clear()
		{
			foreach (var hash in hashDictionary.Values)
			{
				hash.Clear();
			}

			IDLookup.Clear();
		}

		private static long MakeLong(int left, int right)
		{
			return ((long) left << 32) | ((uint) right);
		}

		private HashSet<T> AcquireHashSet()
		{
			if (hashSetPool.Count == 0)
			{
				hashSetPool.Enqueue(new HashSet<T>());
			}

			var hashSet = hashSetPool.Dequeue();
			hashSet.Clear();
			return hashSet;
		}

		private void FreeHashSet(HashSet<T> hashSet)
		{
			hashSetPool.Enqueue(hashSet);
		}
	}
}
