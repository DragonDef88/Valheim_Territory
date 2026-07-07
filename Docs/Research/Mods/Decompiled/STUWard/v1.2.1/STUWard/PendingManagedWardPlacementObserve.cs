using System;

namespace STUWard;

internal readonly struct PendingManagedWardPlacementObserve
{
	internal ZDOID WardZdoId { get; }

	internal long SenderUid { get; }

	internal long RequesterId { get; }

	internal DateTime FirstSeenUtc { get; }

	internal PendingManagedWardPlacementObserve(ZDOID wardZdoId, long senderUid, long requesterId, DateTime firstSeenUtc)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		WardZdoId = wardZdoId;
		SenderUid = senderUid;
		RequesterId = requesterId;
		FirstSeenUtc = firstSeenUtc;
	}
}
