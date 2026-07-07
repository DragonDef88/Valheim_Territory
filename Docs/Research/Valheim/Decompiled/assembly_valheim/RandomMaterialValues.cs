using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomMaterialValues : MonoBehaviour
{
	[Serializable]
	public abstract class MaterialVariationProperty<T>
	{
		public List<string> m_propertyNames;

		protected Random m_random;

		public abstract T GetValue(int seed);
	}

	[Serializable]
	public class VectorVariationProperty : MaterialVariationProperty<Vector4>
	{
		public Vector4 m_minimum;

		public Vector4 m_maximum;

		public override Vector4 GetValue(int seed)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			m_random = new Random(seed);
			Vector4 result = default(Vector4);
			result.x = Mathf.Lerp(m_minimum.x, m_maximum.x, (float)m_random.NextDouble());
			result.y = Mathf.Lerp(m_minimum.y, m_maximum.y, (float)m_random.NextDouble());
			result.z = Mathf.Lerp(m_minimum.z, m_maximum.z, (float)m_random.NextDouble());
			result.w = Mathf.Lerp(m_minimum.w, m_maximum.w, (float)m_random.NextDouble());
			return result;
		}
	}

	public List<VectorVariationProperty> m_vectorProperties = new List<VectorVariationProperty>();

	private ZNetView m_nview;

	private int m_randomSeed = -1;

	private string m_matName;

	private bool m_isSet;

	private Piece m_piece;

	private int m_checks;

	private static readonly string s_randSeedString = "RandMatSeed";

	private void Start()
	{
		m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		m_piece = ((Component)this).GetComponentInParent<Piece>();
		if (!Object.op_Implicit((Object)(object)m_nview))
		{
			ZLog.LogError((object)("Missing nview on '" + ((Object)((Component)((Component)this).transform).gameObject).name + "'"));
		}
		((MonoBehaviour)this).InvokeRepeating("CheckMaterial", 0f, 0.2f);
	}

	private void CheckMaterial()
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if (((!m_isSet && m_randomSeed < 0) || (m_isSet && (!Object.op_Implicit((Object)(object)m_piece) || !Player.IsPlacementGhost(((Component)m_piece).gameObject)))) && Object.op_Implicit((Object)(object)m_nview) && m_nview.GetZDO() != null)
		{
			m_randomSeed = m_nview.GetZDO().GetInt(s_randSeedString, -1);
			if (m_randomSeed < 0 && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(s_randSeedString, Random.Range(0, 12345));
			}
			if (m_randomSeed >= 0)
			{
				for (int i = 0; i < m_vectorProperties.Count; i++)
				{
					VectorVariationProperty vectorVariationProperty = m_vectorProperties[i];
					foreach (string propertyName in vectorVariationProperty.m_propertyNames)
					{
						MaterialMan.instance.SetValue<Vector4>(((Component)this).gameObject, Shader.PropertyToID(propertyName), vectorVariationProperty.GetValue(m_randomSeed + i));
					}
				}
				m_isSet = true;
			}
		}
		m_checks++;
		if (m_checks >= 5)
		{
			((MonoBehaviour)this).CancelInvoke("CheckMaterial");
		}
	}
}
