using System.Collections.Generic;
using UnityEngine;

public class TeleportAbility : MonoBehaviour, IProjectile
{
	public string m_targetTag = "";

	public string m_message = "";

	public float m_maxTeleportRange = 100f;

	private Character m_owner;

	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		m_owner = owner;
		GameObject val = FindTarget();
		if (Object.op_Implicit((Object)(object)val))
		{
			Vector3 position = val.transform.position;
			if (ZoneSystem.instance.FindFloor(position, out position.y))
			{
				((Component)m_owner).transform.position = position;
				((Component)m_owner).transform.rotation = val.transform.rotation;
				if (m_message.Length > 0)
				{
					Player.MessageAllInRange(((Component)this).transform.position, 100f, MessageHud.MessageType.Center, m_message);
				}
			}
		}
		ZNetScene.instance.Destroy(((Component)this).gameObject);
	}

	private GameObject FindTarget()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		GameObject[] array = GameObject.FindGameObjectsWithTag(m_targetTag);
		List<GameObject> list = new List<GameObject>();
		GameObject[] array2 = array;
		foreach (GameObject val in array2)
		{
			if (!(Vector3.Distance(val.transform.position, ((Component)m_owner).transform.position) > m_maxTeleportRange))
			{
				list.Add(val);
			}
		}
		if (list.Count == 0)
		{
			ZLog.Log((object)"No valid telport target in range");
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	public string GetTooltipString(int itemQuality)
	{
		return "";
	}
}
