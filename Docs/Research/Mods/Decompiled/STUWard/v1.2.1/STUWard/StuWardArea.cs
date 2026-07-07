using UnityEngine;

namespace STUWard;

[DisallowMultipleComponent]
internal sealed class StuWardArea : MonoBehaviour
{
	internal const string PrefabName = "piece_stuward";

	internal const string BasePrefabName = "guard_stone";

	internal const string DisplayName = "$stuw_piece_name";

	internal const string Description = "$stuw_piece_desc";

	internal static bool IsManaged(PrivateArea? area)
	{
		if ((Object)(object)area != (Object)null)
		{
			return (Object)(object)((Component)area).GetComponent<StuWardArea>() != (Object)null;
		}
		return false;
	}
}
