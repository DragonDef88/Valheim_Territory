using System;

namespace ClanTerritory.Domain.Identifiers
{
    internal struct PlayerId : IEquatable<PlayerId>
    {
        private readonly long _value;

        public PlayerId(long value)
        {
            _value = value;
        }

        public bool Equals(PlayerId other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerId && Equals((PlayerId)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}