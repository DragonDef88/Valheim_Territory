using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
	public GameObject m_prefab;

	private ZNetView m_nview;

	private void Start()
	{
		m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		if ((Object)(object)m_nview == (Object)null)
		{
			ZLog.LogWarning((object)("SpawnerPrefab cant find netview " + ((Object)((Component)this).gameObject).name));
		}
		else
		{
			((MonoBehaviour)this).InvokeRepeating("TrySpawn", 1f, 1f);
		}
	}

	private void TrySpawn()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			string name = "HasSpawned_" + ((Object)((Component)this).gameObject).name;
			if (!m_nview.GetZDO().GetBool(name))
			{
				ZLog.Log((object)("SpawnPrefab " + ((Object)((Component)this).gameObject).name + " SPAWNING " + ((Object)m_prefab).name));
				Object.Instantiate<GameObject>(m_prefab, ((Component)this).transform.position, ((Component)this).transform.rotation);
				m_nview.GetZDO().Set(name, value: true);
			}
			((MonoBehaviour)this).CancelInvoke("TrySpawn");
		}
	}

	private void OnDrawGizmos()
	{
	}
}
