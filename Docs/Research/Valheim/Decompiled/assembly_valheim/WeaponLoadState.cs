using UnityEngine;

public class WeaponLoadState : MonoBehaviour
{
	public GameObject m_unloaded;

	public GameObject m_loaded;

	private Player m_owner;

	private void Start()
	{
		m_owner = ((Component)this).GetComponentInParent<Player>();
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)m_owner))
		{
			bool flag = m_owner.IsWeaponLoaded();
			m_unloaded.SetActive(!flag);
			m_loaded.SetActive(flag);
		}
	}
}
