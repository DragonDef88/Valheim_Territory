using System;
using UnityEngine;

public class Thunder : MonoBehaviour
{
	public float m_strikeIntervalMin = 3f;

	public float m_strikeIntervalMax = 10f;

	public float m_thunderDelayMin = 3f;

	public float m_thunderDelayMax = 5f;

	public float m_flashDistanceMin = 50f;

	public float m_flashDistanceMax = 200f;

	public float m_flashAltitude = 100f;

	public EffectList m_flashEffect = new EffectList();

	public EffectList m_thunderEffect = new EffectList();

	[Header("Thor")]
	public bool m_spawnThor;

	public string m_requiredGlobalKey = "";

	public GameObject m_thorPrefab;

	public float m_thorSpawnDistance = 300f;

	public float m_thorSpawnAltitudeMax = 100f;

	public float m_thorSpawnAltitudeMin = 100f;

	public float m_thorInterval = 10f;

	public float m_thorChance = 1f;

	private Vector3 m_flashPos = Vector3.zero;

	private float m_strikeTimer = -1f;

	private float m_thunderTimer = -1f;

	private float m_thorTimer;

	private void Start()
	{
		m_strikeTimer = Random.Range(m_strikeIntervalMin, m_strikeIntervalMax);
	}

	private void Update()
	{
		if (m_strikeTimer > 0f)
		{
			m_strikeTimer -= Time.deltaTime;
			if (m_strikeTimer <= 0f)
			{
				DoFlash();
			}
		}
		if (m_thunderTimer > 0f)
		{
			m_thunderTimer -= Time.deltaTime;
			if (m_thunderTimer <= 0f)
			{
				DoThunder();
				m_strikeTimer = Random.Range(m_strikeIntervalMin, m_strikeIntervalMax);
			}
		}
		if (!m_spawnThor)
		{
			return;
		}
		m_thorTimer += Time.deltaTime;
		if (m_thorTimer > m_thorInterval)
		{
			m_thorTimer = 0f;
			if (Random.value <= m_thorChance && (m_requiredGlobalKey == "" || ZoneSystem.instance.GetGlobalKey(m_requiredGlobalKey)))
			{
				SpawnThor();
			}
		}
	}

	private void SpawnThor()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.value * ((float)Math.PI * 2f);
		Vector3 val = ((Component)this).transform.position + new Vector3(Mathf.Sin(num), 0f, Mathf.Cos(num)) * m_thorSpawnDistance;
		val.y += Random.Range(m_thorSpawnAltitudeMin, m_thorSpawnAltitudeMax);
		float groundHeight = ZoneSystem.instance.GetGroundHeight(val);
		if (val.y < groundHeight)
		{
			val.y = groundHeight + 50f;
		}
		float num2 = num + 180f + (float)Random.Range(-45, 45);
		Vector3 val2 = ((Component)this).transform.position + new Vector3(Mathf.Sin(num2), 0f, Mathf.Cos(num2)) * m_thorSpawnDistance;
		val2.y += Random.Range(m_thorSpawnAltitudeMin, m_thorSpawnAltitudeMax);
		float groundHeight2 = ZoneSystem.instance.GetGroundHeight(val2);
		if (val.y < groundHeight2)
		{
			val.y = groundHeight2 + 50f;
		}
		Vector3 val3 = val2 - val;
		Vector3 normalized = ((Vector3)(ref val3)).normalized;
		Object.Instantiate<GameObject>(m_thorPrefab, val, Quaternion.LookRotation(normalized));
	}

	private void DoFlash()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.value * ((float)Math.PI * 2f);
		float num2 = Random.Range(m_flashDistanceMin, m_flashDistanceMax);
		m_flashPos = ((Component)this).transform.position + new Vector3(Mathf.Sin(num), 0f, Mathf.Cos(num)) * num2;
		m_flashPos.y += m_flashAltitude;
		Vector3 val = ((Component)this).transform.position - m_flashPos;
		Quaternion rotation = Quaternion.LookRotation(((Vector3)(ref val)).normalized);
		GameObject[] array = m_flashEffect.Create(m_flashPos, Quaternion.identity);
		for (int i = 0; i < array.Length; i++)
		{
			Light[] componentsInChildren = array[i].GetComponentsInChildren<Light>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				((Component)componentsInChildren[j]).transform.rotation = rotation;
			}
		}
		m_thunderTimer = Random.Range(m_thunderDelayMin, m_thunderDelayMax);
	}

	private void DoThunder()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_thunderEffect.Create(m_flashPos, Quaternion.identity);
	}
}
