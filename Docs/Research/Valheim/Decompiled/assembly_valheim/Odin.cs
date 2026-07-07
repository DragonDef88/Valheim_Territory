using UnityEngine;

public class Odin : MonoBehaviour
{
	public float m_despawnCloseDistance = 20f;

	public float m_despawnFarDistance = 50f;

	public EffectList m_despawn = new EffectList();

	public float m_ttl = 300f;

	private float m_time;

	private ZNetView m_nview;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
	}

	private void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner())
		{
			return;
		}
		Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, m_despawnFarDistance);
		if ((Object)(object)closestPlayer == (Object)null)
		{
			m_despawn.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			m_nview.Destroy();
			ZLog.Log((object)"No player in range, despawning");
			return;
		}
		Vector3 val = ((Component)closestPlayer).transform.position - ((Component)this).transform.position;
		val.y = 0f;
		((Vector3)(ref val)).Normalize();
		((Component)this).transform.rotation = Quaternion.LookRotation(val);
		if (Vector3.Distance(((Component)closestPlayer).transform.position, ((Component)this).transform.position) < m_despawnCloseDistance)
		{
			m_despawn.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			m_nview.Destroy();
			ZLog.Log((object)"Player go too close,despawning");
			return;
		}
		m_time += Time.deltaTime;
		if (m_time > m_ttl)
		{
			m_despawn.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			m_nview.Destroy();
			ZLog.Log((object)("timeout " + m_time + " , despawning"));
		}
	}
}
