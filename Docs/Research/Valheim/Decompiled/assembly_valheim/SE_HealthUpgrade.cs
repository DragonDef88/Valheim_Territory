using UnityEngine;

public class SE_HealthUpgrade : StatusEffect
{
	[Header("Health")]
	public float m_moreHealth;

	[Header("Stamina")]
	public float m_moreStamina;

	public EffectList m_upgradeEffect = new EffectList();

	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	public override void Stop()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		base.Stop();
		Player player = m_character as Player;
		if (Object.op_Implicit((Object)(object)player))
		{
			if (m_moreHealth > 0f)
			{
				player.SetMaxHealth(m_character.GetMaxHealth() + m_moreHealth, flashBar: true);
				player.SetHealth(m_character.GetMaxHealth());
			}
			if (m_moreStamina > 0f)
			{
				player.SetMaxStamina(m_character.GetMaxStamina() + m_moreStamina, flashBar: true);
			}
			m_upgradeEffect.Create(((Component)m_character).transform.position, Quaternion.identity);
		}
	}
}
