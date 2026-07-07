using System.Collections.Generic;
using UnityEngine;

public class MaterialVariationWorld : MonoBehaviour
{
	public List<MaterialVariationSettings> m_variations = new List<MaterialVariationSettings>();

	private static List<MeshRenderer> mrs = new List<MeshRenderer>();

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		Location zoneLocation = Location.GetZoneLocation(((Component)this).transform.position);
		if (!Object.op_Implicit((Object)(object)zoneLocation))
		{
			return;
		}
		((Component)this).GetComponentsInChildren<MeshRenderer>(mrs);
		DungeonGenerator generator = zoneLocation.m_generator;
		foreach (MaterialVariationSettings variation in m_variations)
		{
			if (Object.op_Implicit((Object)(object)generator) && variation.m_dungeonThemeCondition != 0 && generator.m_themes.HasFlag(variation.m_dungeonThemeCondition))
			{
				change(variation);
			}
			if (Object.op_Implicit((Object)(object)zoneLocation) && variation.m_biomeCondition != 0)
			{
				if (zoneLocation.m_biome == Heightmap.Biome.None)
				{
					zoneLocation.m_biome = WorldGenerator.instance.GetBiome(((Component)zoneLocation).transform.position);
				}
				if (variation.m_biomeCondition == zoneLocation.m_biome)
				{
					change(variation);
				}
			}
		}
		((Behaviour)this).enabled = false;
		void change(MaterialVariationSettings mvs)
		{
			foreach (MeshRenderer mr in mrs)
			{
				((Renderer)mr).materials = mvs.m_materials;
				Terminal.Log($"Replaced material on {((Object)((Component)this).gameObject).name} for dungeon {mvs.m_dungeonThemeCondition} or {mvs.m_biomeCondition}");
			}
		}
	}
}
