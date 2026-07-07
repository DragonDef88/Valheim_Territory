using UnityEngine;

public class MusicLocation : MonoBehaviour
{
	private float volume;

	public bool m_addRadiusFromLocation = true;

	public float m_radius = 10f;

	public bool m_oneTime = true;

	public bool m_notIfEnemies = true;

	public bool m_forceFade;

	private ZNetView m_nview;

	private AudioSource m_audioSource;

	private float m_baseVolume;

	private bool m_blockLoopAndFade;

	private void Awake()
	{
		m_audioSource = ((Component)this).GetComponent<AudioSource>();
		m_baseVolume = m_audioSource.volume;
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			m_nview.Register("SetPlayed", SetPlayed);
		}
		if (m_addRadiusFromLocation)
		{
			Location componentInParent = ((Component)this).GetComponentInParent<Location>();
			if (componentInParent != null)
			{
				m_radius += componentInParent.GetMaxRadius();
			}
		}
	}

	private void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return;
		}
		float num = Vector3.Distance(((Component)this).transform.position, ((Component)Player.m_localPlayer).transform.position);
		float num2 = 1f - Utils.SmoothStep(m_radius * 0.5f, m_radius, num);
		volume = Mathf.MoveTowards(volume, num2, Time.deltaTime);
		float num3 = volume * m_baseVolume * MusicMan.m_masterMusicVolume;
		if (volume > 0f && !m_audioSource.isPlaying && !m_blockLoopAndFade)
		{
			if ((m_oneTime && HasPlayed()) || (m_notIfEnemies && BaseAI.HaveEnemyInRange(Player.m_localPlayer, ((Component)this).transform.position, m_radius)))
			{
				return;
			}
			m_audioSource.time = 0f;
			m_audioSource.Play();
		}
		if (!Settings.ContinousMusic && m_audioSource.loop)
		{
			m_audioSource.loop = false;
			m_blockLoopAndFade = true;
		}
		if (m_blockLoopAndFade || m_forceFade)
		{
			float num4 = m_audioSource.time - m_audioSource.clip.length + 1.5f;
			if (num4 > 0f)
			{
				num3 *= 1f - num4 / 1.5f;
			}
			if (Terminal.m_showTests)
			{
				Terminal.m_testList["Music location fade"] = num4 + " " + (1f - num4 / 1.5f);
			}
		}
		m_audioSource.volume = num3;
		if (m_blockLoopAndFade && volume <= 0f)
		{
			m_blockLoopAndFade = false;
			m_audioSource.loop = true;
		}
		if (Terminal.m_showTests && m_audioSource.isPlaying)
		{
			Terminal.m_testList["Music location current"] = ((Object)m_audioSource).name;
			Terminal.m_testList["Music location vol / volume"] = num3 + " / " + volume;
			if (ZInput.GetKeyDown((KeyCode)110, true) && ZInput.GetKey((KeyCode)304, true))
			{
				m_audioSource.time = m_audioSource.clip.length - 4f;
			}
		}
		if (m_oneTime && volume > 0f && m_audioSource.time > m_audioSource.clip.length * 0.75f && !HasPlayed())
		{
			SetPlayed();
		}
	}

	private void SetPlayed()
	{
		m_nview.InvokeRPC("SetPlayed");
	}

	private void SetPlayed(long sender)
	{
		if (m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_played, value: true);
			ZLog.Log((object)"Setting location music as played");
		}
	}

	private bool HasPlayed()
	{
		return m_nview.GetZDO().GetBool(ZDOVars.s_played);
	}

	private void OnDrawGizmos()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.5f);
		Gizmos.DrawWireSphere(((Component)this).transform.position, m_radius);
	}
}
