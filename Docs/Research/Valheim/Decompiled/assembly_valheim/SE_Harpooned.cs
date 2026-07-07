using UnityEngine;

public class SE_Harpooned : StatusEffect
{
	[Header("SE_Harpooned")]
	public float m_pullForce;

	public float m_forcePower = 2f;

	public float m_pullSpeed = 5f;

	public float m_smoothDistance = 2f;

	public float m_maxLineSlack = 0.3f;

	public float m_breakDistance = 4f;

	public float m_maxDistance = 30f;

	public float m_staminaDrain = 10f;

	public float m_staminaDrainInterval = 0.1f;

	private bool m_broken;

	private Character m_attacker;

	private float m_baseDistance = 999999f;

	private LineConnect m_line;

	private float m_drainStaminaTimer;

	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	public override void SetAttacker(Character attacker)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)("Setting attacker " + attacker.m_name));
		m_attacker = attacker;
		m_time = 0f;
		if (m_character.IsBoss())
		{
			m_broken = true;
			return;
		}
		float num = Vector3.Distance(((Component)m_attacker).transform.position, ((Component)m_character).transform.position);
		if (num > m_maxDistance)
		{
			m_attacker.Message(MessageHud.MessageType.Center, "$msg_harpoon_targettoofar");
			m_broken = true;
			return;
		}
		m_baseDistance = num;
		m_attacker.Message(MessageHud.MessageType.Center, m_character.m_name + " $msg_harpoon_harpooned");
		GameObject[] startEffectInstances = m_startEffectInstances;
		foreach (GameObject val in startEffectInstances)
		{
			if (Object.op_Implicit((Object)(object)val))
			{
				LineConnect component = val.GetComponent<LineConnect>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.SetPeer(((Component)m_attacker).GetComponent<ZNetView>());
					m_line = component;
				}
			}
		}
	}

	public override void UpdateStatusEffect(float dt)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateStatusEffect(dt);
		if (!Object.op_Implicit((Object)(object)m_attacker))
		{
			return;
		}
		Rigidbody component = ((Component)m_character).GetComponent<Rigidbody>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		float num = Vector3.Distance(((Component)m_attacker).transform.position, ((Component)m_character).transform.position);
		if ((Object)(object)m_character.GetStandingOnShip() == (Object)null && !m_character.IsAttached())
		{
			float num2 = Utils.Pull(component, ((Component)m_attacker).transform.position, m_baseDistance, m_pullSpeed, m_pullForce, m_smoothDistance, true, true, m_forcePower);
			m_drainStaminaTimer += dt;
			if (m_drainStaminaTimer > m_staminaDrainInterval && num2 > 0f)
			{
				m_drainStaminaTimer = 0f;
				float stamina = m_staminaDrain * num2 * m_character.GetMass();
				m_attacker.UseStamina(stamina);
			}
		}
		if (Object.op_Implicit((Object)(object)m_line))
		{
			m_line.SetSlack((1f - Utils.LerpStep(m_baseDistance / 2f, m_baseDistance, num)) * m_maxLineSlack);
		}
		if (num - m_baseDistance > m_breakDistance)
		{
			m_broken = true;
			m_attacker.Message(MessageHud.MessageType.Center, "$msg_harpoon_linebroke");
		}
		if (!m_attacker.HaveStamina())
		{
			m_broken = true;
			m_attacker.Message(MessageHud.MessageType.Center, m_character.m_name + " $msg_harpoon_released");
		}
	}

	public override bool IsDone()
	{
		if (base.IsDone())
		{
			return true;
		}
		if (m_broken)
		{
			return true;
		}
		if (!Object.op_Implicit((Object)(object)m_attacker))
		{
			return true;
		}
		if (m_time > 2f && (m_attacker.IsBlocking() || m_attacker.InAttack()))
		{
			m_attacker.Message(MessageHud.MessageType.Center, m_character.m_name + " released");
			return true;
		}
		return false;
	}
}
