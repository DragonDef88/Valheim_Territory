using UnityEngine;

public class DrawBounds : MonoBehaviour
{
	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.magenta;
		MeshFilter[] componentsInChildren = ((Component)this).GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter obj in componentsInChildren)
		{
			Gizmos.matrix = ((Component)obj).transform.localToWorldMatrix;
			Mesh sharedMesh = obj.sharedMesh;
			Bounds bounds = sharedMesh.bounds;
			Vector3 center = ((Bounds)(ref bounds)).center;
			bounds = sharedMesh.bounds;
			Gizmos.DrawWireCube(center, ((Bounds)(ref bounds)).size);
		}
	}
}
