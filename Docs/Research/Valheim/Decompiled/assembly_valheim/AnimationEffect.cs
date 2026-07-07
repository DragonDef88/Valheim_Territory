using System.Collections.Generic;
using UnityEngine;

public class AnimationEffect : MonoBehaviour
{
	public Transform m_effectRoot;

	private Animator m_animator;

	private List<GameObject> m_attachments;

	private int m_attachStateHash;

	private void Start()
	{
		m_animator = ((Component)this).GetComponent<Animator>();
	}

	public void Effect(AnimationEvent e)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		string stringParameter = e.stringParameter;
		Object objectReferenceParameter = e.objectReferenceParameter;
		GameObject val = (GameObject)(object)((objectReferenceParameter is GameObject) ? objectReferenceParameter : null);
		if (!((Object)(object)val == (Object)null))
		{
			Transform val2 = null;
			if (stringParameter.Length > 0)
			{
				val2 = Utils.FindChild(((Component)this).transform, stringParameter, (IterativeSearchType)0);
			}
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = (Object.op_Implicit((Object)(object)m_effectRoot) ? m_effectRoot : ((Component)this).transform);
			}
			Object.Instantiate<GameObject>(val, val2.position, val2.rotation);
		}
	}

	public void Attach(AnimationEvent e)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		string stringParameter = e.stringParameter;
		Object objectReferenceParameter = e.objectReferenceParameter;
		GameObject val = (GameObject)(object)((objectReferenceParameter is GameObject) ? objectReferenceParameter : null);
		bool flag = e.intParameter < 0;
		int intParameter = e.intParameter;
		bool flag2 = intParameter == 10 || intParameter == -10;
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		AnimatorClipInfo animatorClipInfo;
		if (stringParameter == "")
		{
			animatorClipInfo = e.animatorClipInfo;
			ZLog.LogWarning((object)("No joint name specified for Attach in animation " + ((Object)((AnimatorClipInfo)(ref animatorClipInfo)).clip).name));
			return;
		}
		Transform val2 = Utils.FindChild(((Component)this).transform, stringParameter, (IterativeSearchType)0);
		if ((Object)(object)val2 == (Object)null)
		{
			animatorClipInfo = e.animatorClipInfo;
			ZLog.LogWarning((object)("Failed to find attach joint " + stringParameter + " for animation " + ((Object)((AnimatorClipInfo)(ref animatorClipInfo)).clip).name));
			return;
		}
		ClearAttachment(val2);
		GameObject val3 = Object.Instantiate<GameObject>(val, val2.position, val2.rotation);
		Vector3 localScale = val3.transform.localScale;
		val3.transform.SetParent(val2, true);
		if (flag2)
		{
			val3.transform.localScale = localScale;
		}
		if (!flag)
		{
			if (m_attachments == null)
			{
				m_attachments = new List<GameObject>();
			}
			m_attachments.Add(val3);
			AnimatorStateInfo animatorStateInfo = e.animatorStateInfo;
			m_attachStateHash = ((AnimatorStateInfo)(ref animatorStateInfo)).fullPathHash;
			((MonoBehaviour)this).CancelInvoke("UpdateAttachments");
			((MonoBehaviour)this).InvokeRepeating("UpdateAttachments", 0.1f, 0.1f);
		}
	}

	private void ClearAttachment(Transform parent)
	{
		if (m_attachments == null)
		{
			return;
		}
		foreach (GameObject attachment in m_attachments)
		{
			if (Object.op_Implicit((Object)(object)attachment) && (Object)(object)attachment.transform.parent == (Object)(object)parent)
			{
				m_attachments.Remove(attachment);
				Object.Destroy((Object)(object)attachment);
				break;
			}
		}
	}

	public void RemoveAttachments()
	{
		if (m_attachments == null)
		{
			return;
		}
		foreach (GameObject attachment in m_attachments)
		{
			Object.Destroy((Object)(object)attachment);
		}
		m_attachments.Clear();
	}

	private void UpdateAttachments()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (m_attachments != null && m_attachments.Count > 0)
		{
			int attachStateHash = m_attachStateHash;
			AnimatorStateInfo val = m_animator.GetCurrentAnimatorStateInfo(0);
			if (attachStateHash != ((AnimatorStateInfo)(ref val)).fullPathHash)
			{
				int attachStateHash2 = m_attachStateHash;
				val = m_animator.GetNextAnimatorStateInfo(0);
				if (attachStateHash2 != ((AnimatorStateInfo)(ref val)).fullPathHash)
				{
					RemoveAttachments();
				}
			}
		}
		else
		{
			((MonoBehaviour)this).CancelInvoke("UpdateAttachments");
		}
	}
}
