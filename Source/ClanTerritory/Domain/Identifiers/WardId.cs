using System;

namespace ClanTerritory.Domain.Identifiers
{
    internal struct WardId : IEquatable<WardId>
    {
        private readonly string _value;

        public WardId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("WardId cannot be empty.", "value");

            _value = value;
        }

        public bool Equals(WardId other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is WardId && Equals((WardId)obj);
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