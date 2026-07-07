using UnityEngine;

public class EnvZone : MonoBehaviour
{
	public string m_environment = "";

	public bool m_force = true;

	public MeshRenderer m_exteriorMesh;

	private static EnvZone s_triggered;

	private void Awake()
	{
		if (Object.op_Implicit((Object)(object)m_exteriorMesh))
		{
			((Renderer)m_exteriorMesh).forceRenderingOff = true;
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		Player component = ((Component)collider).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)Player.m_localPlayer != (Object)(object)component))
		{
			if (m_force && string.IsNullOrEmpty(EnvMan.instance.m_debugEnv))
			{
				EnvMan.instance.SetForceEnvironment(m_environment);
			}
			s_triggered = this;
			if (Object.op_Implicit((Object)(object)m_exteriorMesh))
			{
				((Renderer)m_exteriorMesh).forceRenderingOff = false;
			}
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if ((Object)(object)s_triggered != (Object)(object)this)
		{
			return;
		}
		Player component = ((Component)collider).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)Player.m_localPlayer != (Object)(object)component))
		{
			if (m_force)
			{
				EnvMan.instance.SetForceEnvironment("");
			}
			s_triggered = null;
		}
	}

	public static string GetEnvironment()
	{
		if (Object.op_Implicit((Object)(object)s_triggered) && !s_triggered.m_force)
		{
			return s_triggered.m_environment;
		}
		return null;
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)m_exteriorMesh))
		{
			((Renderer)m_exteriorMesh).forceRenderingOff = (Object)(object)s_triggered != (Object)(object)this;
		}
	}
}
