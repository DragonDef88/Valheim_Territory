using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

public class CameraEffects : MonoBehaviour
{
	private static CameraEffects m_instance;

	public bool m_forceDof;

	public LayerMask m_dofRayMask;

	public bool m_dofAutoFocus;

	public float m_dofMinDistance = 50f;

	public float m_dofMinDistanceShip = 50f;

	public float m_dofMaxDistance = 3000f;

	private PostProcessingBehaviour m_postProcessing;

	private DepthOfField m_dof;

	public static CameraEffects instance => m_instance;

	private void Awake()
	{
		m_instance = this;
		m_postProcessing = ((Component)this).GetComponent<PostProcessingBehaviour>();
		m_dof = ((Component)this).GetComponent<DepthOfField>();
		GraphicsSettingsManager.GraphicsSettingsChanged += ApplySettings;
		ApplySettings();
	}

	private void OnDestroy()
	{
		GraphicsSettingsManager.GraphicsSettingsChanged -= ApplySettings;
		if ((Object)(object)m_instance == (Object)(object)this)
		{
			m_instance = null;
		}
	}

	private void ApplySettings()
	{
		GraphicsSettingsState currentSettingsWithCurrentPresetApplied = GraphicsSettingsManager.Instance.GetCurrentSettingsWithCurrentPresetApplied(includeBackground: true);
		SetDof(currentSettingsWithCurrentPresetApplied.m_depthOfField);
		SetBloom(currentSettingsWithCurrentPresetApplied.m_bloom);
		SetSSAO(currentSettingsWithCurrentPresetApplied.m_ssao);
		SetSunShafts(currentSettingsWithCurrentPresetApplied.m_sunShafts);
		SetAntiAliasing(currentSettingsWithCurrentPresetApplied.m_antiAliasing);
		SetCA(currentSettingsWithCurrentPresetApplied.m_chromaticAberration);
		SetMotionBlur(currentSettingsWithCurrentPresetApplied.m_motionBlur);
	}

	public void SetSunShafts(bool enabled)
	{
		SunShafts component = ((Component)this).GetComponent<SunShafts>();
		if ((Object)(object)component != (Object)null)
		{
			((Behaviour)component).enabled = enabled;
		}
	}

	private void SetBloom(bool enabled)
	{
		((PostProcessingModel)m_postProcessing.profile.bloom).enabled = enabled;
	}

	private void SetSSAO(int value)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		PostProcessingProfile profile = m_postProcessing.profile;
		Settings settings = profile.ambientOcclusion.settings;
		switch (value)
		{
		case 0:
			((PostProcessingModel)profile.ambientOcclusion).enabled = false;
			break;
		case 1:
			((PostProcessingModel)profile.ambientOcclusion).enabled = true;
			settings.downsampling = true;
			settings.farDistance = 100f;
			settings.sampleCount = (SampleCount)6;
			break;
		default:
			((PostProcessingModel)profile.ambientOcclusion).enabled = true;
			settings.downsampling = false;
			settings.farDistance = 150f;
			settings.sampleCount = (SampleCount)10;
			break;
		}
		profile.ambientOcclusion.settings = settings;
	}

	private void SetMotionBlur(bool enabled)
	{
		((PostProcessingModel)m_postProcessing.profile.motionBlur).enabled = enabled;
	}

	private void SetAntiAliasing(bool enabled)
	{
		((PostProcessingModel)m_postProcessing.profile.antialiasing).enabled = enabled;
	}

	private void SetCA(bool enabled)
	{
		((PostProcessingModel)m_postProcessing.profile.chromaticAberration).enabled = enabled;
	}

	private void SetDof(bool enabled)
	{
		((Behaviour)m_dof).enabled = enabled || m_forceDof;
	}

	private void LateUpdate()
	{
		UpdateDOF();
	}

	private bool ControllingShip()
	{
		if ((Object)(object)Player.m_localPlayer == (Object)null || (Object)(object)Player.m_localPlayer.GetControlledShip() != (Object)null)
		{
			return true;
		}
		return false;
	}

	private void UpdateDOF()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)m_dof).enabled && m_dofAutoFocus)
		{
			float num = m_dofMaxDistance;
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(((Component)this).transform.position, ((Component)this).transform.forward, ref val, m_dofMaxDistance, LayerMask.op_Implicit(m_dofRayMask)))
			{
				num = ((RaycastHit)(ref val)).distance;
			}
			if (ControllingShip() && num < m_dofMinDistanceShip)
			{
				num = m_dofMinDistanceShip;
			}
			if (num < m_dofMinDistance)
			{
				num = m_dofMinDistance;
			}
			m_dof.focalLength = Mathf.Lerp(m_dof.focalLength, num, 0.2f);
		}
	}
}
