using System;
using UnityEngine;

public class RoomConnection : MonoBehaviour
{
	public string m_type = "";

	public bool m_entrance;

	public bool m_allowDoor = true;

	public bool m_doorOnlyIfOtherAlsoAllowsDoor;

	[NonSerialized]
	public int m_placeOrder;

	private void OnDrawGizmos()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (m_entrance)
		{
			Gizmos.color = Color.white;
		}
		else
		{
			Gizmos.color = new Color(1f, 1f, 0f, 1f);
		}
		Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position, ((Component)this).transform.rotation, new Vector3(1f, 1f, 1f));
		Gizmos.DrawCube(Vector3.zero, new Vector3(2f, 0.02f, 0.2f));
		Gizmos.DrawCube(new Vector3(0f, 0f, 0.35f), new Vector3(0.2f, 0.02f, 0.5f));
		Gizmos.matrix = Matrix4x4.identity;
	}

	public bool TestContact(RoomConnection other)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(((Component)this).transform.position, ((Component)other).transform.position) < 0.1f;
	}
}
