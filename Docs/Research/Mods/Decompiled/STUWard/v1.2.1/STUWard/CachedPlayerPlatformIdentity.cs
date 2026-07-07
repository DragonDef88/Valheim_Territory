using System;

namespace STUWard;

internal readonly struct CachedPlayerPlatformIdentity
{
	internal bool HasPlatformId { get; }

	internal string PlatformId { get; }

	internal DateTime ExpiresAtUtc { get; }

	internal CachedPlayerPlatformIdentity(bool hasPlatformId, string platformId, DateTime expiresAtUtc)
	{
		HasPlatformId = hasPlatformId;
		PlatformId = platformId;
		ExpiresAtUtc = expiresAtUtc;
	}
}
