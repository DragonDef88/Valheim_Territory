using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainOp : MonoBehaviour
{
	[Serializable]
	public class Settings
	{
		public float m_levelOffset;

		[Header("Level")]
		public bool m_level;

		public float m_levelRadius = 2f;

		public bool m_square = true;

		[Header("Raise")]
		public bool m_raise;

		public float m_raiseRadius = 2f;

		public float m_raisePower;

		public float m_raiseDelta;

		[Header("Smooth")]
		public bool m_smooth;

		public float m_smoothRadius = 2f;

		public float m_smoothPower = 3f;

		[Header("Paint")]
		public bool m_paintCleared = true;

		public bool m_paintHeightCheck;

		public TerrainModifier.PaintType m_paintType;

		public float m_paintRadius = 2f;

		public void Serialize(ZPackage pkg)
		{
			pkg.Write(m_levelOffset);
			pkg.Write(m_level);
			pkg.Write(m_levelRadius);
			pkg.Write(m_square);
			pkg.Write(m_raise);
			pkg.Write(m_raiseRadius);
			pkg.Write(m_raisePower);
			pkg.Write(m_raiseDelta);
			pkg.Write(m_smooth);
			pkg.Write(m_smoothRadius);
			pkg.Write(m_smoothPower);
			pkg.Write(m_paintCleared);
			pkg.Write(m_paintHeightCheck);
			pkg.Write((int)m_paintType);
			pkg.Write(m_paintRadius);
		}

		public void Deserialize(ZPackage pkg)
		{
			m_levelOffset = pkg.ReadSingle();
			m_level = pkg.ReadBool();
			m_levelRadius = pkg.ReadSingle();
			m_square = pkg.ReadBool();
			m_raise = pkg.ReadBool();
			m_raiseRadius = pkg.ReadSingle();
			m_raisePower = pkg.ReadSingle();
			m_raiseDelta = pkg.ReadSingle();
			m_smooth = pkg.ReadBool();
			m_smoothRadius = pkg.ReadSingle();
			m_smoothPower = pkg.ReadSingle();
			m_paintCleared = pkg.ReadBool();
			m_paintHeightCheck = pkg.ReadBool();
			m_paintType = (TerrainModifier.PaintType)pkg.ReadInt();
			m_paintRadius = pkg.ReadSingle();
		}

		public float GetRadius()
		{
			float num = 0f;
			if (m_level && m_levelRadius > num)
			{
				num = m_levelRadius;
			}
			if (m_raise && m_raiseRadius > num)
			{
				num = m_raiseRadius;
			}
			if (m_smooth && m_smoothRadius > num)
			{
				num = m_smoothRadius;
			}
			if (m_paintCleared && m_paintRadius > num)
			{
				num = m_paintRadius;
			}
			return num;
		}
	}

	public static bool m_forceDisableTerrainOps;

	public Settings m_settings = new Settings();

	[Header("Effects")]
	public EffectList m_onPlacedEffect = new EffectList();

	[Header("Spawn items")]
	public GameObject m_spawnOnPlaced;

	public float m_chanceToSpawn = 1f;

	public int m_maxSpawned = 1;

	public bool m_spawnAtMaxLevelDepth = true;

	private void Awake()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (m_forceDisableTerrainOps)
		{
			return;
		}
		List<Heightmap> list = new List<Heightmap>();
		Heightmap.FindHeightmap(((Component)this).transform.position, GetRadius(), list);
		foreach (Heightmap item in list)
		{
			item.GetAndCreateTerrainCompiler().ApplyOperation(this);
		}
		OnPlaced();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public float GetRadius()
	{
		return m_settings.GetRadius();
	}

	private void OnPlaced()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		m_onPlacedEffect.Create(((Component)this).transform.position, Quaternion.identity);
		if (Object.op_Implicit((Object)(object)m_spawnOnPlaced) && (m_spawnAtMaxLevelDepth || !Heightmap.AtMaxLevelDepth(((Component)this).transform.position + Vector3.up * m_settings.m_levelOffset)) && Random.value <= m_chanceToSpawn)
		{
			Vector3 val = Vector2.op_Implicit(Random.insideUnitCircle * 0.2f);
			GameObject obj = Object.Instantiate<GameObject>(m_spawnOnPlaced, ((Component)this).transform.position + Vector3.up * 0.5f + val, Quaternion.identity);
			obj.GetComponent<ItemDrop>().m_itemData.m_stack = Random.Range(1, m_maxSpawned + 1);
			obj.GetComponent<Rigidbody>().linearVelocity = Vector3.up * 4f;
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position + Vector3.up * m_settings.m_levelOffset, Quaternion.identity, new Vector3(1f, 0f, 1f));
		if (m_settings.m_level)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Vector3.zero, m_settings.m_levelRadius);
		}
		if (m_settings.m_smooth)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(Vector3.zero, m_settings.m_smoothRadius);
		}
		if (m_settings.m_paintCleared)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Vector3.zero, m_settings.m_paintRadius);
		}
		Gizmos.matrix = Matrix4x4.identity;
	}
}
