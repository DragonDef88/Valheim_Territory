using UnityEngine;

public class MaterialManNotifier : MonoBehaviour
{
	private void OnDestroy()
	{
		MaterialMan.instance.UnregisterRenderers(((Component)this).gameObject);
	}
}
