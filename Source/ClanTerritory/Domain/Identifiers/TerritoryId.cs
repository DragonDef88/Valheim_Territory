using System;

namespace ClanTerritory.Domain.Identifiers
{
    internal struct TerritoryId : IEquatable<TerritoryId>
    {
        private readonly string _value;

        public TerritoryId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("TerritoryId cannot be empty.", "value");

            _value = value;
        }

        public bool Equals(TerritoryId other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is TerritoryId && Equals((TerritoryId)obj);
        }

        public override int GetHashCode()
        {
            return _value != null ? _value.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return _value;
        }
    }
}