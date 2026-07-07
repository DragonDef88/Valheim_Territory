using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[Serializable]
public class EffectList
{
	[Serializable]
	public class EffectData
	{
		public GameObject m_prefab;

		public bool m_enabled = true;

		public int m_variant = -1;

		public bool m_attach;

		public bool m_follow;

		public bool m_inheritParentRotation;

		public bool m_inheritParentScale;

		public bool m_multiplyParentVisualScale;

		public bool m_randomRotation;

		public bool m_scale;

		public string m_childTransform;
	}

	public EffectData[] m_effectPrefabs = new EffectData[0];

	public GameObject[] Create(Vector3 basePos, Quaternion baseRot, Transform baseParent = null, float scale = 1f, int variant = -1)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < m_effectPrefabs.Length; i++)
		{
			EffectData effectData = m_effectPrefabs[i];
			if (!effectData.m_enabled || (variant >= 0 && effectData.m_variant >= 0 && variant != effectData.m_variant))
			{
				continue;
			}
			Transform val = baseParent;
			Vector3 val2 = basePos;
			Quaternion val3 = baseRot;
			if (!string.IsNullOrEmpty(effectData.m_childTransform) && (Object)(object)baseParent != (Object)null)
			{
				Transform val4 = Utils.FindChild(val, effectData.m_childTransform, (IterativeSearchType)0);
				if (Object.op_Implicit((Object)(object)val4))
				{
					val = val4;
					val2 = val.position;
				}
			}
			if (Object.op_Implicit((Object)(object)val) && effectData.m_inheritParentRotation)
			{
				val3 = val.rotation;
			}
			if (effectData.m_randomRotation)
			{
				val3 = Random.rotation;
			}
			GameObject val5 = Object.Instantiate<GameObject>(effectData.m_prefab, val2, val3);
			if (effectData.m_scale)
			{
				if (Object.op_Implicit((Object)(object)baseParent) && effectData.m_inheritParentScale)
				{
					Vector3 localScale = baseParent.localScale * scale;
					val5.transform.localScale = localScale;
				}
				else
				{
					val5.transform.localScale = new Vector3(scale, scale, scale);
				}
			}
			else if (Object.op_Implicit((Object)(object)baseParent))
			{
				if (effectData.m_multiplyParentVisualScale)
				{
					Transform val6 = baseParent.Find("Visual");
					if (val6 != null)
					{
						val5.transform.localScale = Vector3.Scale(val5.transform.localScale, val6.localScale);
						goto IL_0166;
					}
				}
				if (effectData.m_inheritParentScale)
				{
					val5.transform.localScale = baseParent.localScale;
				}
			}
			goto IL_0166;
			IL_0166:
			if (effectData.m_attach && (Object)(object)val != (Object)null)
			{
				val5.transform.SetParent(val);
			}
			if (effectData.m_follow)
			{
				ParentConstraint obj = val5.AddComponent<ParentConstraint>();
				ConstraintSource val7 = default(ConstraintSource);
				((ConstraintSource)(ref val7)).sourceTransform = val;
				((ConstraintSource)(ref val7)).weight = 1f;
				ConstraintSource val8 = val7;
				obj.AddSource(val8);
				obj.locked = true;
				obj.SetTranslationOffset(0, effectData.m_prefab.transform.position);
				Quaternion rotation = effectData.m_prefab.transform.rotation;
				obj.SetRotationOffset(0, ((Quaternion)(ref rotation)).eulerAngles);
				obj.constraintActive = true;
			}
			list.Add(val5);
		}
		return list.ToArray();
	}

	public bool HasEffects()
	{
		if (m_effectPrefabs == null || m_effectPrefabs.Length == 0)
		{
			return false;
		}
		EffectData[] effectPrefabs = m_effectPrefabs;
		for (int i = 0; i < effectPrefabs.Length; i++)
		{
			if (effectPrefabs[i].m_enabled)
			{
				return true;
			}
		}
		return false;
	}
}
