using System;

namespace ClanTerritory.Domain.ValueObjects
{
    internal struct WorldPosition : IEquatable<WorldPosition>
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public WorldPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float DistanceTo(WorldPosition other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            float dz = Z - other.Z;

            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public bool Equals(WorldPosition other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is WorldPosition && Equals((WorldPosition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X.GetHashCode();
                hash = (hash * 397) ^ Y.GetHashCode();
                hash = (hash * 397) ^ Z.GetHashCode();
                return hash;
            }
        }
    }
}