using System;
using System.Collections.Generic;
using UnityEngine;

public class MaterialFader : MonoBehaviour
{
	[Serializable]
	public class FadeProperty
	{
		[Header("Settings")]
		public string m_propertyName;

		public AnimationCurve m_animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Tooltip("Single Vector Channel mode will work on Colors too, if you just want to affect alpha color for example.")]
		public PropertyType m_propertyType;

		[Min(0.1f)]
		public float m_fadeTime = 5f;

		[Min(0f)]
		public float m_delay;

		[NonSerialized]
		public int m_shaderID;

		[Header("Values")]
		[Tooltip("Used for floats and single vector channels")]
		public float m_finalFloatValue;

		public Color m_finalColorValue;

		public Vector3 m_finalVectorValue;

		[Tooltip("Only used for single vector channel mode.")]
		public VectorChannel m_vectorChannel;

		[NonSerialized]
		public float m_startFloatValue;

		[NonSerialized]
		public Color m_startColorValue;

		[NonSerialized]
		public Vector4 m_startVectorValue;

		private float m_fadeTimer;

		private bool m_startedFade;

		private bool m_finished;

		private Material m_originalMaterial;

		public void Initalize(Material mat)
		{
			m_shaderID = Shader.PropertyToID(m_propertyName);
			m_originalMaterial = mat;
		}

		public void Reset()
		{
			m_fadeTimer = 0f;
			m_finished = false;
			m_startedFade = false;
		}

		private void GetMaterialValues(MaterialPropertyBlock propertyBlock)
		{
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			m_startedFade = true;
			if (propertyBlock.HasProperty(m_shaderID))
			{
				switch (m_propertyType)
				{
				case PropertyType.Float:
					m_startFloatValue = propertyBlock.GetFloat(m_shaderID);
					break;
				case PropertyType.Color:
					m_startColorValue = propertyBlock.GetColor(m_shaderID);
					break;
				case PropertyType.Vector3:
					m_startVectorValue = propertyBlock.GetVector(m_shaderID);
					break;
				case PropertyType.SingleVectorChannel:
					m_startVectorValue = propertyBlock.GetVector(m_shaderID);
					m_startFloatValue = ((Vector4)(ref m_startVectorValue))[(int)m_vectorChannel];
					break;
				}
			}
			else
			{
				switch (m_propertyType)
				{
				case PropertyType.Float:
					m_startFloatValue = m_originalMaterial.GetFloat(m_shaderID);
					break;
				case PropertyType.Color:
					m_startColorValue = m_originalMaterial.GetColor(m_shaderID);
					break;
				case PropertyType.Vector3:
					m_startVectorValue = m_originalMaterial.GetVector(m_shaderID);
					break;
				case PropertyType.SingleVectorChannel:
					m_startVectorValue = m_originalMaterial.GetVector(m_shaderID);
					m_startFloatValue = ((Vector4)(ref m_startVectorValue))[(int)m_vectorChannel];
					break;
				}
			}
		}

		public void Update(float delta, ref MaterialPropertyBlock propertyBlock)
		{
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			m_fadeTimer += delta;
			if (!m_finished && !(m_fadeTimer < m_delay))
			{
				if (!m_startedFade)
				{
					GetMaterialValues(propertyBlock);
				}
				float num = Mathf.Clamp01((m_fadeTimer - m_delay) / m_fadeTime);
				num = m_animationCurve.Evaluate(num);
				switch (m_propertyType)
				{
				case PropertyType.Float:
					propertyBlock.SetFloat(m_shaderID, Mathf.Lerp(m_startFloatValue, m_finalFloatValue, num));
					break;
				case PropertyType.Color:
					propertyBlock.SetColor(m_shaderID, Color.Lerp(m_startColorValue, m_finalColorValue, num));
					break;
				case PropertyType.Vector3:
					propertyBlock.SetVector(m_shaderID, Vector4.op_Implicit(Vector3.Lerp(Vector4.op_Implicit(m_startVectorValue), m_finalVectorValue, num)));
					break;
				case PropertyType.SingleVectorChannel:
				{
					Vector4 startVectorValue = m_startVectorValue;
					((Vector4)(ref startVectorValue))[(int)m_vectorChannel] = Mathf.Lerp(m_startFloatValue, m_finalFloatValue, num);
					propertyBlock.SetVector(m_shaderID, startVectorValue);
					break;
				}
				}
				if (num == 1f)
				{
					m_finished = true;
				}
			}
		}
	}

	[Serializable]
	public enum PropertyType
	{
		Float,
		Color,
		SingleVectorChannel,
		Vector3
	}

	[Serializable]
	public enum VectorChannel
	{
		X,
		Y,
		Z,
		W
	}

	public List<Renderer> m_renderers;

	public bool m_triggerOnAwake = true;

	public List<FadeProperty> m_fadeProperties;

	private MaterialPropertyBlock m_propertyBlock;

	private bool m_started;

	private void Awake()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		if (m_renderers == null)
		{
			m_renderers = new List<Renderer>();
			Renderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<Renderer>();
			if (componentsInChildren != null)
			{
				m_renderers.AddRange(componentsInChildren);
			}
			Renderer component = ((Component)this).GetComponent<Renderer>();
			if ((Object)(object)component != (Object)null)
			{
				m_renderers.Add(component);
			}
		}
		if (m_renderers.Count == 0)
		{
			ZLog.LogError((object)"No MeshRenderer components assigned to MaterialFader!");
		}
		foreach (FadeProperty fadeProperty in m_fadeProperties)
		{
			fadeProperty.Initalize(m_renderers[0].material);
		}
		m_propertyBlock = new MaterialPropertyBlock();
		if (m_renderers[0].HasPropertyBlock())
		{
			m_renderers[0].GetPropertyBlock(m_propertyBlock);
		}
		m_started = m_triggerOnAwake;
	}

	public void TriggerFade()
	{
		if (m_started)
		{
			foreach (FadeProperty fadeProperty in m_fadeProperties)
			{
				fadeProperty.Reset();
			}
		}
		m_started = true;
	}

	private void Update()
	{
		if (!m_started)
		{
			return;
		}
		foreach (FadeProperty fadeProperty in m_fadeProperties)
		{
			fadeProperty.Update(Time.deltaTime, ref m_propertyBlock);
		}
		foreach (Renderer renderer in m_renderers)
		{
			renderer.SetPropertyBlock(m_propertyBlock);
		}
	}
}
