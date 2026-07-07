using UnityEngine;

public class HideWhenRunning : MonoBehaviour
{
	private void Awake()
	{
		if (Application.isPlaying)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
