using System;
using System.Collections.Generic;
using UnityEngine;

public class MaterialVariation : MonoBehaviour
{
	[Serializable]
	public class MaterialEntry
	{
		public Material m_material;

		public float m_weight = 1f;
	}

	public int m_materialIndex;

	public List<MaterialEntry> m_materials = new List<MaterialEntry>();

	private ZNetView m_nview;

	private Renderer m_renderer;

	private int m_variation = -1;

	private string m_matName;

	private bool m_isSet;

	private Piece m_piece;

	private int m_checks;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		m_piece = ((Component)this).GetComponentInParent<Piece>();
		m_renderer = (Renderer)(object)((Component)this).GetComponent<SkinnedMeshRenderer>();
		if (!Object.op_Implicit((Object)(object)m_renderer))
		{
			m_renderer = (Renderer)(object)((Component)this).GetComponent<MeshRenderer>();
		}
		if (!Object.op_Implicit((Object)(object)m_nview) || !Object.op_Implicit((Object)(object)m_renderer))
		{
			ZLog.LogError((object)("Missing nview or renderer on '" + ((Object)((Component)((Component)this).transform).gameObject).name + "'"));
		}
		m_nview.Register<int>("RPC_UpdateMaterial", RPC_UpdateMaterial);
		((MonoBehaviour)this).InvokeRepeating("CheckMaterial", 0f, 0.2f);
	}

	private void CheckMaterial()
	{
		if (((!m_isSet && m_variation < 0) || (m_isSet && ((Object)m_renderer.materials[m_materialIndex]).name != m_matName && (!Object.op_Implicit((Object)(object)m_piece) || !Player.IsPlacementGhost(((Component)m_piece).gameObject)))) && Object.op_Implicit((Object)(object)m_nview) && m_nview.GetZDO() != null && Object.op_Implicit((Object)(object)m_renderer))
		{
			m_variation = m_nview.GetZDO().GetInt("MatVar" + m_materialIndex, -1);
			if (m_variation < 0 && m_nview.IsOwner())
			{
				SetMaterial(GetWeightedVariation());
			}
			else if (m_variation >= 0)
			{
				UpdateMaterial();
			}
		}
		m_checks++;
		if (m_checks >= 5)
		{
			((MonoBehaviour)this).CancelInvoke("CheckMaterial");
		}
	}

	public void SetMaterial(int index)
	{
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsOwner())
		{
			m_nview.GetZDO().Set("MatVar" + m_materialIndex, index);
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateMaterial", index);
		}
		m_variation = index;
		UpdateMaterial();
	}

	public int GetMaterial()
	{
		return m_variation;
	}

	private void RPC_UpdateMaterial(long sender, int index)
	{
		m_variation = index;
		UpdateMaterial();
	}

	private void UpdateMaterial()
	{
		if (m_variation >= 0)
		{
			Material[] materials = m_renderer.materials;
			materials[m_materialIndex] = m_materials[m_variation].m_material;
			m_renderer.materials = materials;
			m_matName = ((Object)m_renderer.materials[m_materialIndex]).name;
			m_isSet = true;
		}
	}

	private int GetWeightedVariation()
	{
		float num = 0f;
		foreach (MaterialEntry material in m_materials)
		{
			num += material.m_weight;
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		for (int i = 0; i < m_materials.Count; i++)
		{
			num3 += m_materials[i].m_weight;
			if (num2 <= num3)
			{
				return i;
			}
		}
		return 0;
	}
}
