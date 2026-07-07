using System.Collections.Generic;
using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
	public GameObject m_OffObject;

	[Range(0f, 100f)]
	public float m_chanceToSpawn = 50f;

	public Room.Theme m_dungeonRequireTheme;

	public Heightmap.Biome m_requireBiome;

	public bool m_notInLava;

	[Header("Elevation span (water is 30)")]
	public int m_minElevation = -10000;

	public int m_maxElevation = 10000;

	private List<ZNetView> m_childNetViews;

	private ZNetView m_nview;

	public void Randomize(Vector3 pos, Location loc = null, DungeonGenerator dg = null)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		bool spawned = Random.Range(0f, 100f) <= m_chanceToSpawn;
		if ((Object)(object)dg != (Object)null && m_dungeonRequireTheme != 0 && !dg.m_themes.HasFlag(m_dungeonRequireTheme))
		{
			spawned = false;
		}
		if ((Object)(object)loc != (Object)null && m_requireBiome != 0)
		{
			if (loc.m_biome == Heightmap.Biome.None)
			{
				loc.m_biome = WorldGenerator.instance.GetBiome(pos);
			}
			if (!m_requireBiome.HasFlag(loc.m_biome))
			{
				spawned = false;
			}
		}
		if (m_notInLava && Object.op_Implicit((Object)(object)ZoneSystem.instance) && ZoneSystem.IsLavaPreHeightmap(pos))
		{
			spawned = false;
		}
		if (pos.y < (float)m_minElevation || pos.y > (float)m_maxElevation)
		{
			spawned = false;
		}
		SetSpawned(spawned);
	}

	public void Reset()
	{
		SetSpawned(doSpawn: true);
	}

	private void SetSpawned(bool doSpawn)
	{
		if (!doSpawn)
		{
			((Component)this).gameObject.SetActive(false);
			foreach (ZNetView childNetView in m_childNetViews)
			{
				((Component)childNetView).gameObject.SetActive(false);
			}
		}
		else if ((Object)(object)m_nview == (Object)null)
		{
			((Component)this).gameObject.SetActive(true);
		}
		if ((Object)(object)m_OffObject != (Object)null)
		{
			m_OffObject.SetActive(!doSpawn);
		}
	}

	public void Prepare()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_childNetViews = new List<ZNetView>();
		ZNetView[] componentsInChildren = ((Component)this).gameObject.GetComponentsInChildren<ZNetView>(true);
		foreach (ZNetView zNetView in componentsInChildren)
		{
			if (Utils.IsEnabledInheirarcy(((Component)zNetView).gameObject, ((Component)this).gameObject))
			{
				m_childNetViews.Add(zNetView);
			}
		}
	}
}
