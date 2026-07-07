using UnityEngine;

public class EggHatch : MonoBehaviour
{
	public float m_triggerDistance = 5f;

	[Range(0f, 1f)]
	public float m_chanceToHatch = 1f;

	public Vector3 m_spawnOffset = new Vector3(0f, 0.5f, 0f);

	public GameObject m_spawnPrefab;

	public EffectList m_hatchEffect;

	private ZNetView m_nview;

	private void Start()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (Random.value <= m_chanceToHatch)
		{
			((MonoBehaviour)this).InvokeRepeating("CheckSpawn", Random.Range(1f, 2f), 1f);
		}
	}

	private void CheckSpawn()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, m_triggerDistance);
			if (Object.op_Implicit((Object)(object)closestPlayer) && !closestPlayer.InGhostMode())
			{
				Hatch();
			}
		}
	}

	private void Hatch()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		m_hatchEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		Object.Instantiate<GameObject>(m_spawnPrefab, ((Component)this).transform.TransformPoint(m_spawnOffset), Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f));
		m_nview.Destroy();
	}
}
