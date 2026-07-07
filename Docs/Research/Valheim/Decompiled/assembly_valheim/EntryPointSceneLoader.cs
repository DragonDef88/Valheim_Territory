using System.Collections;
using SoftReferenceableAssets.SceneManagement;
using UnityEngine;

public class EntryPointSceneLoader : MonoBehaviour
{
	[SerializeField]
	private SceneReference m_scene;

	private void Start()
	{
		((MonoBehaviour)this).StartCoroutine(LoadSceneAndWaitForPrefs());
	}

	private IEnumerator LoadSceneAndWaitForPrefs()
	{
		ZLog.Log((object)"Loading first scene!");
		ILoadSceneAsyncOperation op = SceneManager.LoadSceneAsync(m_scene, (LoadSceneMode)0);
		op.AllowSceneActivation = false;
		while (!PlatformInitializer.PreferencesInitialized)
		{
			yield return null;
		}
		ZLog.Log((object)"Preferences initialized! Activating first scene!");
		op.AllowSceneActivation = true;
	}
}
