using UnityEngine;

public class AnimationObjectToggle : MonoBehaviour
{
	public Transform m_parentTransform;

	private GameObject GetGameObject(string objectName)
	{
		if ((Object)(object)m_parentTransform == (Object)null)
		{
			return ((Component)((Component)this).transform.Find(objectName)).gameObject;
		}
		return ((Component)m_parentTransform.Find(objectName)).gameObject;
	}

	private void HideObject(string objectName)
	{
		GetGameObject(objectName).SetActive(false);
	}

	private void ShowObject(string objectName)
	{
		GetGameObject(objectName).SetActive(true);
	}
}
