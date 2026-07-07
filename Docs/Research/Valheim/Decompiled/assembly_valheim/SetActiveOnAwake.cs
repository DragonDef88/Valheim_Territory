using UnityEngine;

public class SetActiveOnAwake : MonoBehaviour
{
	[SerializeField]
	private GameObject m_objectToSetActive;

	private void Awake()
	{
		if ((Object)(object)m_objectToSetActive != (Object)null)
		{
			m_objectToSetActive.SetActive(true);
		}
	}
}
