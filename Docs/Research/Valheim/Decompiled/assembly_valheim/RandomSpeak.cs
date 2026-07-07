using UnityEngine;

public class RandomSpeak : MonoBehaviour
{
	public float m_interval = 5f;

	public float m_chance = 0.5f;

	public float m_triggerDistance = 5f;

	public float m_cullDistance = 10f;

	public float m_ttl = 10f;

	public Vector3 m_offset = new Vector3(0f, 0f, 0f);

	public EffectList m_speakEffects = new EffectList();

	public bool m_useLargeDialog;

	public bool m_onlyOnce;

	public bool m_onlyOnItemStand;

	public float m_minTOD;

	public float m_maxTOD = 1f;

	public bool m_invertTod;

	public bool m_indexFromDay;

	public string m_topic = "";

	public string[] m_texts = new string[0];

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("Speak", Random.Range(0f, m_interval), m_interval);
	}

	private void Speak()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		if (Random.value > m_chance || m_texts.Length == 0 || (Object)(object)Player.m_localPlayer == (Object)null || Vector3.Distance(((Component)this).transform.position, ((Component)Player.m_localPlayer).transform.position) > m_triggerDistance || (m_onlyOnItemStand && !Object.op_Implicit((Object)(object)((Component)this).gameObject.GetComponentInParent<ItemStand>())))
		{
			return;
		}
		float dayFraction = EnvMan.instance.GetDayFraction();
		if ((m_invertTod || (!(dayFraction < m_minTOD) && !(dayFraction > m_maxTOD))) && (!m_invertTod || !(dayFraction > m_minTOD) || !(dayFraction < m_maxTOD)))
		{
			m_speakEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			int num = (m_indexFromDay ? (EnvMan.instance.GetDay() % m_texts.Length) : Random.Range(0, m_texts.Length));
			string text = m_texts[num];
			Chat.instance.SetNpcText(((Component)this).gameObject, m_offset, m_cullDistance, m_ttl, m_topic, text, m_useLargeDialog);
			if (m_onlyOnce)
			{
				((MonoBehaviour)this).CancelInvoke("Speak");
			}
		}
	}
}
