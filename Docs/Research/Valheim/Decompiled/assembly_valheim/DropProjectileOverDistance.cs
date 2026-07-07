using UnityEngine;

public class DropProjectileOverDistance : MonoBehaviour
{
	public GameObject m_projectilePrefab;

	public float m_distancePerProjectile = 5f;

	public float m_spawnHeight = 1f;

	public bool m_snapToGround;

	[Tooltip("If higher than 0, will force a spawn if nothing has spawned in that amount of time.")]
	public float m_timeToForceSpawn = -1f;

	public float m_minVelocity;

	public float m_maxVelocity;

	private Character m_character;

	private ZNetView m_nview;

	private Vector3 lastPosition;

	private float m_distanceAccumulator;

	private float m_spawnTimer;

	private const int c_MaxSpawnsPerFrame = 3;

	private void Awake()
	{
		m_character = ((Component)this).GetComponent<Character>();
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_projectilePrefab == null)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void FixedUpdate()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		Vector3 val = VectorExtensions.Horizontal(((Component)this).transform.position);
		m_distanceAccumulator += Vector3.Distance(lastPosition, val);
		Vector3 val2 = VectorExtensions.DirTo(lastPosition, val);
		if (lastPosition != val)
		{
			lastPosition = val;
		}
		if (m_timeToForceSpawn > 0f)
		{
			m_spawnTimer += Time.deltaTime;
			if (m_spawnTimer > m_timeToForceSpawn)
			{
				SpawnProjectile(((Component)this).transform.position, val2);
				m_distanceAccumulator -= m_distancePerProjectile;
				m_distanceAccumulator = Mathf.Max(m_distanceAccumulator, 0f);
			}
		}
		if (!(m_distanceAccumulator < m_distancePerProjectile))
		{
			int num = Mathf.FloorToInt(m_distanceAccumulator / m_distancePerProjectile);
			for (int i = 0; i < Mathf.Min(3, num); i++)
			{
				SpawnProjectile(((Component)this).transform.position - val2 * (float)i, val2);
				m_distanceAccumulator -= m_distancePerProjectile;
				num--;
			}
			m_distanceAccumulator -= m_distancePerProjectile * (float)num;
		}
	}

	private void SpawnProjectile(Vector3 point, Vector3 travelDirection)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		m_spawnTimer = 0f;
		if (m_projectilePrefab.GetComponent<IProjectile>() == null)
		{
			ZLog.LogWarning((object)"Attempted to spawn non-projectile");
		}
		point.y += m_spawnHeight;
		if (m_snapToGround)
		{
			ZoneSystem.instance.GetSolidHeight(point, out var height);
			point.y = height;
		}
		Object.Instantiate<GameObject>(m_projectilePrefab, point, Quaternion.LookRotation(travelDirection)).GetComponent<IProjectile>().Setup(m_character, travelDirection * Random.Range(m_minVelocity, m_maxVelocity), -1f, null, null, null);
	}

	private void OnDrawGizmosSelected()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = new Color(0.76f, 0.52f, 0.55f);
		Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + Vector3.up * m_spawnHeight);
		Vector3 val = ((Component)this).transform.position + ((Component)this).transform.forward * m_distancePerProjectile;
		Gizmos.DrawLine(((Component)this).transform.position + Vector3.up * 0.5f * m_spawnHeight, val + Vector3.up * 0.5f * m_spawnHeight);
		Gizmos.DrawLine(val, val + Vector3.up * m_spawnHeight);
	}
}
