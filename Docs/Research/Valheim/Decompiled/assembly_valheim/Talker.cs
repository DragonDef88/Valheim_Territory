using UnityEngine;

public class Talker : MonoBehaviour
{
	public enum Type
	{
		Whisper,
		Normal,
		Shout,
		Ping
	}

	public float m_visperDistance = 4f;

	public float m_normalDistance = 15f;

	public float m_shoutDistance = 70f;

	private ZNetView m_nview;

	private Character m_character;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_character = ((Component)this).GetComponent<Character>();
		m_nview.Register<int, UserInfo, string>("Say", RPC_Say);
	}

	public void Say(Type type, string text)
	{
		ZLog.Log((object)("Saying " + type.ToString() + "  " + text));
		Chat.CheckPermissionsAndSendChatMessageRPCsAsync(delegate(long user, bool filterText)
		{
			Chat.GetChatMessageData(text, filterText, out var userInfoToSend, out var textToSend);
			m_nview.InvokeRPC(user, "Say", (int)type, userInfoToSend, textToSend);
		});
	}

	private void RPC_Say(long sender, int ctype, UserInfo user, string text)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Player.m_localPlayer == (Object)null))
		{
			float num = 0f;
			switch (ctype)
			{
			case 0:
				num = m_visperDistance;
				break;
			case 1:
				num = m_normalDistance;
				break;
			case 2:
				num = m_shoutDistance;
				break;
			}
			if (Vector3.Distance(((Component)this).transform.position, ((Component)Player.m_localPlayer).transform.position) < num && Object.op_Implicit((Object)(object)Chat.instance))
			{
				Vector3 headPoint = m_character.GetHeadPoint();
				Chat.instance.OnNewChatMessage(((Component)this).gameObject, sender, headPoint, (Type)ctype, user, text);
			}
		}
	}
}
