using UnityEngine;

public class FloatingTerrainDummy : MonoBehaviour
{
	public FloatingTerrain m_parent;

	private void OnCollisionStay(Collision collision)
	{
		if (!Object.op_Implicit((Object)(object)m_parent))
		{
			Object.Destroy((Object)(object)this);
		}
		m_parent.OnDummyCollision(collision);
	}
}
