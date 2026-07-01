using System;

namespace ClanTerritory.Domain.ValueObjects
{
    internal struct TerritoryRadius : IEquatable<TerritoryRadius>
    {
        public float Value { get; private set; }

        public TerritoryRadius(float value)
        {
            if (value <= 0f)
                throw new ArgumentOutOfRangeException("value", "Territory radius must be greater than zero.");

            Value = value;
        }

        public bool Equals(TerritoryRadius other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is TerritoryRadius && Equals((TerritoryRadius)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}