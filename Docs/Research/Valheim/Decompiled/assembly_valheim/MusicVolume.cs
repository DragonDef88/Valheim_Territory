using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicVolume : MonoBehaviour
{
	private ZNetView m_nview;

	public static List<MusicVolume> m_proximityMusicVolumes = new List<MusicVolume>();

	private static MusicVolume m_lastProximityVolume;

	private static List<MusicVolume> m_close = new List<MusicVolume>();

	public bool m_addRadiusFromLocation = true;

	public float m_radius = 10f;

	public float m_outerRadiusExtra = 0.5f;

	public float m_surroundingPlayersAdditionalRadius = 50f;

	public Bounds m_boundsInner;

	[Tooltip("Takes dimension from the room it's a part of and sets bounds to it's size.")]
	public Room m_sizeFromRoom;

	[Header("Music")]
	public string m_musicName = "";

	public float m_musicChance = 0.7f;

	[Tooltip("If the music can play again before playing a different location music first.")]
	public bool m_musicCanRepeat = true;

	public bool m_loopMusic;

	public bool m_stopMusicOnExit;

	public int m_maxPlaysPerActivation;

	[Tooltip("Makes the music fade by distance between inner/outer bounds. With this enabled loop, repeat, stoponexit, chance, etc is ignored.")]
	public bool m_fadeByProximity;

	[HideInInspector]
	public int m_PlayCount;

	private double m_lastEnterCheck;

	private bool m_lastWasInside;

	private bool m_lastWasInsideWide;

	private bool m_isLooping;

	private float m_proximity;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			m_PlayCount = m_nview.GetZDO().GetInt(ZDOVars.s_plays);
			m_nview.Register("RPC_PlayMusic", RPC_PlayMusic);
		}
		if (m_addRadiusFromLocation)
		{
			Location componentInParent = ((Component)this).GetComponentInParent<Location>();
			if (componentInParent != null)
			{
				m_radius += componentInParent.GetMaxRadius();
			}
		}
		if (m_fadeByProximity)
		{
			m_proximityMusicVolumes.Add(this);
		}
	}

	private void OnDestroy()
	{
		m_proximityMusicVolumes.Remove(this);
	}

	private void RPC_PlayMusic(long sender)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Vector3.Distance(((Component)Player.m_localPlayer).transform.position, ((Component)this).transform.position) < m_radius + m_surroundingPlayersAdditionalRadius;
		if (flag)
		{
			PlayMusic();
		}
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid() && m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_plays, flag ? m_PlayCount : (m_PlayCount + 1));
		}
	}

	private void PlayMusic()
	{
		ZLog.Log((object)("MusicLocation '" + ((Object)this).name + "' Playing Music: " + m_musicName));
		m_PlayCount++;
		MusicMan.instance.LocationMusic(m_musicName);
		if (m_loopMusic)
		{
			m_isLooping = true;
		}
	}

	private void Update()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.m_localPlayer == (Object)null || m_fadeByProximity)
		{
			return;
		}
		double timeSeconds = ZNet.instance.GetTimeSeconds();
		if (timeSeconds > m_lastEnterCheck + 1.0)
		{
			m_lastEnterCheck = timeSeconds;
			if (IsInside(((Component)Player.m_localPlayer).transform.position))
			{
				if (!m_lastWasInside)
				{
					m_lastWasInside = (m_lastWasInsideWide = true);
					OnEnter();
				}
			}
			else
			{
				if (m_lastWasInside)
				{
					m_lastWasInside = false;
					OnExit();
				}
				if (m_lastWasInsideWide && !IsInside(((Component)Player.m_localPlayer).transform.position, checkOuter: true))
				{
					m_lastWasInsideWide = false;
					OnExitWide();
				}
			}
		}
		if (m_isLooping && m_lastWasInside && !string.IsNullOrEmpty(m_musicName))
		{
			MusicMan.instance.LocationMusic(m_musicName);
		}
	}

	private void OnEnter()
	{
		ZLog.Log((object)("MusicLocation.OnEnter: " + ((Object)this).name));
		if (!string.IsNullOrEmpty(m_musicName) && (m_maxPlaysPerActivation == 0 || m_PlayCount < m_maxPlaysPerActivation) && Random.Range(0f, 1f) <= m_musicChance && (m_musicCanRepeat || MusicMan.instance.m_lastLocationMusic != m_musicName))
		{
			if (Object.op_Implicit((Object)(object)m_nview))
			{
				m_nview.InvokeRPC(ZNetView.Everybody, "RPC_PlayMusic");
			}
			else
			{
				PlayMusic();
			}
		}
	}

	private void OnExit()
	{
		ZLog.Log((object)("MusicLocation.OnExit: " + ((Object)this).name));
	}

	private void OnExitWide()
	{
		ZLog.Log((object)("MusicLocation.OnExitWide: " + ((Object)this).name));
		if (MusicMan.instance.m_lastLocationMusic == m_musicName && (m_stopMusicOnExit || m_loopMusic))
		{
			MusicMan.instance.LocationMusic(null);
		}
		m_isLooping = false;
	}

	public bool IsInside(Vector3 point, bool checkOuter = false)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (IsBox())
		{
			Bounds val;
			if (!checkOuter)
			{
				val = GetInnerBounds();
				return ((Bounds)(ref val)).Contains(point);
			}
			val = GetOuterBounds();
			return ((Bounds)(ref val)).Contains(point);
		}
		float num = Vector3.Distance(((Component)this).transform.position, point);
		if (checkOuter)
		{
			return num < m_radius + m_outerRadiusExtra;
		}
		return num < m_radius;
	}

	private void OnDrawGizmos()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!IsBox())
		{
			Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.5f);
			Gizmos.DrawWireSphere(((Component)this).transform.position, m_radius);
			Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.25f);
			Gizmos.DrawWireSphere(((Component)this).transform.position, m_radius + m_outerRadiusExtra);
			return;
		}
		Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.5f);
		Bounds val = GetInnerBounds();
		Vector3 center = ((Bounds)(ref val)).center;
		val = GetBox();
		Gizmos.DrawWireCube(center, ((Bounds)(ref val)).size);
		Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.25f);
		val = GetOuterBounds();
		Vector3 center2 = ((Bounds)(ref val)).center;
		val = GetOuterBounds();
		Gizmos.DrawWireCube(center2, ((Bounds)(ref val)).size);
	}

	private bool IsBox()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Bounds box = GetBox();
		return ((Bounds)(ref box)).size.x != 0f;
	}

	private Bounds GetBox()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_sizeFromRoom))
		{
			return m_boundsInner;
		}
		return new Bounds(Vector3.zero, Vector3Int.op_Implicit(m_sizeFromRoom.m_size));
	}

	private Bounds GetInnerBounds()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Bounds box = GetBox();
		return new Bounds(((Bounds)(ref box)).center + ((Component)this).transform.position, ((Bounds)(ref box)).size);
	}

	private Bounds GetOuterBounds()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Bounds box = GetBox();
		return new Bounds(((Bounds)(ref box)).center + ((Component)this).transform.position, ((Bounds)(ref box)).size + new Vector3(m_outerRadiusExtra, m_outerRadiusExtra, m_outerRadiusExtra));
	}

	private float MinBoundDimension()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Bounds box = GetBox();
		if (!(((Bounds)(ref box)).size.x < ((Bounds)(ref box)).size.y) || !(((Bounds)(ref box)).size.x < ((Bounds)(ref box)).size.z))
		{
			if (!(((Bounds)(ref box)).size.y < ((Bounds)(ref box)).size.z))
			{
				return ((Bounds)(ref box)).size.z;
			}
			return ((Bounds)(ref box)).size.y;
		}
		return ((Bounds)(ref box)).size.x;
	}

	public static float UpdateProximityVolumes(AudioSource musicSource)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			return 1f;
		}
		float num = 0f;
		Bounds innerBounds;
		if ((Object)(object)m_lastProximityVolume != (Object)null)
		{
			innerBounds = m_lastProximityVolume.GetInnerBounds();
			if (((Bounds)(ref innerBounds)).Contains(((Component)Player.m_localPlayer).transform.position))
			{
				num = 1f;
				goto IL_024a;
			}
		}
		m_lastProximityVolume = null;
		m_close.Clear();
		foreach (MusicVolume proximityMusicVolume in m_proximityMusicVolumes)
		{
			if (Object.op_Implicit((Object)(object)proximityMusicVolume) && proximityMusicVolume.IsInside(((Component)Player.m_localPlayer).transform.position, checkOuter: true))
			{
				m_close.Add(proximityMusicVolume);
			}
		}
		if (m_close.Count == 0)
		{
			MusicMan.instance.LocationMusic(null);
			return 1f;
		}
		foreach (MusicVolume item in m_close)
		{
			if (item.IsInside(((Component)Player.m_localPlayer).transform.position))
			{
				m_lastProximityVolume = item;
				num = 1f;
			}
		}
		if (num == 0f)
		{
			MusicVolume musicVolume = null;
			foreach (MusicVolume item2 in m_close)
			{
				float num2;
				float num3;
				if (item2.IsBox())
				{
					innerBounds = item2.GetInnerBounds();
					num2 = Vector3.Distance(((Bounds)(ref innerBounds)).ClosestPoint(((Component)Player.m_localPlayer).transform.position), ((Component)Player.m_localPlayer).transform.position);
					num3 = item2.m_outerRadiusExtra - num2;
				}
				else
				{
					float num4 = Vector3.Distance(((Component)item2).transform.position, ((Component)Player.m_localPlayer).transform.position);
					num2 = num4 - item2.m_radius;
					num3 = item2.m_radius + item2.m_outerRadiusExtra - num4;
				}
				item2.m_proximity = 1f - Math.Min(1f, num2 / (num2 + num3));
				if ((Object)(object)musicVolume == (Object)null || item2.m_proximity > musicVolume.m_proximity)
				{
					musicVolume = item2;
				}
			}
			m_lastProximityVolume = musicVolume;
			num = musicVolume.m_proximity;
		}
		goto IL_024a;
		IL_024a:
		MusicMan.instance.LocationMusic(m_lastProximityVolume.m_musicName);
		return num;
	}
}
