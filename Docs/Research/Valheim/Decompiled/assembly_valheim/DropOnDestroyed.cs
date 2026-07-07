using System;
using System.Collections.Generic;
using UnityEngine;

public class DropOnDestroyed : MonoBehaviour
{
	[Header("Drops")]
	public DropTable m_dropWhenDestroyed = new DropTable();

	public float m_spawnYOffset = 0.5f;

	public float m_spawnYStep = 0.3f;

	private void Awake()
	{
		IDestructible component = ((Component)this).GetComponent<IDestructible>();
		Destructible destructible = component as Destructible;
		if (Object.op_Implicit((Object)(object)destructible))
		{
			destructible.m_onDestroyed = (Action)Delegate.Combine(destructible.m_onDestroyed, new Action(OnDestroyed));
		}
		WearNTear wearNTear = component as WearNTear;
		if (Object.op_Implicit((Object)(object)wearNTear))
		{
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(OnDestroyed));
		}
	}

	private void OnDestroyed()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		float groundHeight = ZoneSystem.instance.GetGroundHeight(((Component)this).transform.position);
		Vector3 position = ((Component)this).transform.position;
		if (position.y < groundHeight)
		{
			position.y = groundHeight + 0.1f;
		}
		List<GameObject> dropList = m_dropWhenDestroyed.GetDropList();
		for (int i = 0; i < dropList.Count; i++)
		{
			Vector2 val = Random.insideUnitCircle * 0.5f;
			Vector3 val2 = position + Vector3.up * m_spawnYOffset + new Vector3(val.x, m_spawnYStep * (float)i, val.y);
			Quaternion val3 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
			ItemDrop.OnCreateNew(Object.Instantiate<GameObject>(dropList[i], val2, val3));
		}
	}
}
