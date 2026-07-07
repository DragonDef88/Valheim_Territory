using UnityEngine;

namespace Valheim.UI;

public class RadialKeyHints : MonoBehaviour
{
	[SerializeField]
	protected GameObject m_Next;

	[SerializeField]
	protected GameObject m_Prev;

	private void Update()
	{
		if ((Object)(object)m_Next != (Object)null)
		{
			m_Next.SetActive(ZInput.IsGamepadActive());
		}
		if ((Object)(object)m_Prev != (Object)null)
		{
			m_Prev.SetActive(ZInput.IsGamepadActive());
		}
	}
}
