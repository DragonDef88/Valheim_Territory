using SoftReferenceableAssets;
using UnityEngine;

public class SoftReferencePrefabSpawner : MonoBehaviour
{
	[SerializeField]
	private SoftReference<GameObject> m_prefab;

	[SerializeField]
	private bool m_spawnAsynchronously;

	private bool m_currentlyLoading;

	private void Awake()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		if (m_spawnAsynchronously)
		{
			m_currentlyLoading = true;
			m_prefab.LoadAsync((LoadedHandler)delegate(AssetID assetID, LoadResult result)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				m_currentlyLoading = false;
				if ((int)result == 0)
				{
					SpawnPrefab();
				}
				m_prefab.Release();
				Object.Destroy((Object)(object)((Component)this).gameObject);
			});
		}
		else
		{
			SpawnPrefab();
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnDestroy()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (m_currentlyLoading)
		{
			m_prefab.Load();
			m_prefab.Release();
		}
	}

	private void SpawnPrefab()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		((Object)Utils.Instantiate(m_prefab, ((Component)this).transform.parent)).name = m_prefab.Name;
	}
}
