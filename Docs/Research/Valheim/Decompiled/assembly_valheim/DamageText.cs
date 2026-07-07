using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
	public enum TextType
	{
		Normal,
		Resistant,
		Weak,
		Immune,
		Heal,
		TooHard,
		Blocked,
		Bonus
	}

	private class WorldTextInstance
	{
		public Vector3 m_worldPos;

		public GameObject m_gui;

		public float m_timer;

		public TMP_Text m_textField;

		public float m_duration;
	}

	private static DamageText m_instance;

	public float m_textDuration = 1.5f;

	public float m_maxTextDistance = 30f;

	public int m_largeFontSize = 16;

	public int m_smallFontSize = 8;

	public float m_smallFontDistance = 10f;

	public GameObject m_worldTextBase;

	private List<WorldTextInstance> m_worldTexts = new List<WorldTextInstance>();

	public static DamageText instance => m_instance;

	private void Awake()
	{
		m_instance = this;
		ZRoutedRpc.instance.Register<ZPackage>("RPC_DamageText", RPC_DamageText);
	}

	private void LateUpdate()
	{
		UpdateWorldTexts(Time.deltaTime);
	}

	private void UpdateWorldTexts(float dt)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		WorldTextInstance worldTextInstance = null;
		Camera mainCamera = Utils.GetMainCamera();
		foreach (WorldTextInstance worldText in m_worldTexts)
		{
			worldText.m_timer += dt;
			if (worldText.m_timer > worldText.m_duration && worldTextInstance == null)
			{
				worldTextInstance = worldText;
			}
			worldText.m_worldPos.y += dt;
			float num = Mathf.Clamp01(worldText.m_timer / worldText.m_duration);
			Color color = ((Graphic)worldText.m_textField).color;
			color.a = 1f - Mathf.Pow(num, 3f);
			((Graphic)worldText.m_textField).color = color;
			Vector3 val = Utils.WorldToScreenPointScaled(mainCamera, worldText.m_worldPos);
			if (val.x < 0f || val.x > (float)Screen.width || val.y < 0f || val.y > (float)Screen.height || val.z < 0f)
			{
				worldText.m_gui.SetActive(false);
				continue;
			}
			worldText.m_gui.SetActive(true);
			worldText.m_gui.transform.position = val;
		}
		if (worldTextInstance != null)
		{
			Object.Destroy((Object)(object)worldTextInstance.m_gui);
			m_worldTexts.Remove(worldTextInstance);
		}
	}

	private void AddInworldText(TextType type, Vector3 pos, float distance, string text, bool mySelf)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		if (text == "0" && m_worldTexts.Count > 200)
		{
			return;
		}
		WorldTextInstance worldTextInstance = new WorldTextInstance();
		worldTextInstance.m_duration = m_textDuration;
		worldTextInstance.m_worldPos = pos + Random.insideUnitSphere * 0.5f;
		worldTextInstance.m_gui = Object.Instantiate<GameObject>(m_worldTextBase, ((Component)this).transform);
		worldTextInstance.m_textField = worldTextInstance.m_gui.GetComponent<TMP_Text>();
		m_worldTexts.Add(worldTextInstance);
		text = Localization.instance.Localize(text);
		Color white = default(Color);
		if (mySelf && type <= TextType.Immune)
		{
			if (text == "0")
			{
				((Color)(ref white))._002Ector(0.5f, 0.5f, 0.5f, 1f);
			}
			else
			{
				((Color)(ref white))._002Ector(1f, 0f, 0f, 1f);
			}
		}
		else
		{
			switch (type)
			{
			case TextType.Normal:
				((Color)(ref white))._002Ector(1f, 1f, 1f, 1f);
				break;
			case TextType.Resistant:
				((Color)(ref white))._002Ector(0.6f, 0.6f, 0.6f, 1f);
				break;
			case TextType.Weak:
				((Color)(ref white))._002Ector(1f, 1f, 0f, 1f);
				break;
			case TextType.Immune:
				((Color)(ref white))._002Ector(0.6f, 0.6f, 0.6f, 1f);
				break;
			case TextType.TooHard:
				((Color)(ref white))._002Ector(0.8f, 0.7f, 0.7f, 1f);
				break;
			case TextType.Bonus:
				((Color)(ref white))._002Ector(1f, 0.63f, 0.24f, 1f);
				break;
			case TextType.Heal:
				((Color)(ref white))._002Ector(0.5f, 1f, 0.5f, 0.7f);
				break;
			default:
				white = Color.white;
				break;
			}
		}
		((Graphic)worldTextInstance.m_textField).color = white;
		if (distance > m_smallFontDistance)
		{
			worldTextInstance.m_textField.fontSize = m_smallFontSize;
		}
		else
		{
			worldTextInstance.m_textField.fontSize = m_largeFontSize;
		}
		switch (type)
		{
		case TextType.TooHard:
			text = Localization.instance.Localize("$msg_toohard");
			break;
		case TextType.Heal:
			text = "+" + text;
			break;
		case TextType.Blocked:
			text = Localization.instance.Localize("$msg_blocked: " + text);
			break;
		case TextType.Bonus:
		{
			TMP_Text textField = worldTextInstance.m_textField;
			textField.fontSize *= 1.5f;
			worldTextInstance.m_duration = 3f;
			break;
		}
		}
		worldTextInstance.m_textField.text = text;
		worldTextInstance.m_timer = 0f;
	}

	public void ShowText(HitData.DamageModifier type, Vector3 pos, float dmg, bool player = false)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		TextType type2 = TextType.Normal;
		switch (type)
		{
		case HitData.DamageModifier.Normal:
			type2 = TextType.Normal;
			break;
		case HitData.DamageModifier.Immune:
			type2 = TextType.Immune;
			break;
		case HitData.DamageModifier.SlightlyResistant:
			type2 = TextType.Resistant;
			break;
		case HitData.DamageModifier.Resistant:
			type2 = TextType.Resistant;
			break;
		case HitData.DamageModifier.VeryResistant:
			type2 = TextType.Resistant;
			break;
		case HitData.DamageModifier.SlightlyWeak:
			type2 = TextType.Weak;
			break;
		case HitData.DamageModifier.Weak:
			type2 = TextType.Weak;
			break;
		case HitData.DamageModifier.VeryWeak:
			type2 = TextType.Weak;
			break;
		}
		ShowText(type2, pos, dmg, player);
	}

	public void ShowText(TextType type, Vector3 pos, float dmg, bool player = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ShowText(type, pos, dmg.ToString("0.#", CultureInfo.InvariantCulture), player);
	}

	public void ShowText(TextType type, Vector3 pos, string text, bool player = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		ZPackage zPackage = new ZPackage();
		zPackage.Write((int)type);
		zPackage.Write(pos);
		zPackage.Write(text);
		zPackage.Write(player);
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "RPC_DamageText", zPackage);
	}

	private void RPC_DamageText(long sender, ZPackage pkg)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (Object.op_Implicit((Object)(object)mainCamera) && !Hud.IsUserHidden())
		{
			TextType type = (TextType)pkg.ReadInt();
			Vector3 val = pkg.ReadVector3();
			string text = pkg.ReadString();
			bool flag = pkg.ReadBool();
			float num = Vector3.Distance(((Component)mainCamera).transform.position, val);
			if (!(num > m_maxTextDistance))
			{
				bool mySelf = flag && sender == ZNet.GetUID();
				AddInworldText(type, val, num, text, mySelf);
			}
		}
	}
}
