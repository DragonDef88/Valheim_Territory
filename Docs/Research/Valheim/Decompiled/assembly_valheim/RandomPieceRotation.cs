using UnityEngine;

public class RandomPieceRotation : MonoBehaviour
{
	public bool m_rotateX;

	public bool m_rotateY;

	public bool m_rotateZ;

	public int m_stepsX = 4;

	public int m_stepsY = 4;

	public int m_stepsZ = 4;

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		int num = (int)position.x * (int)(position.y * 10f) * (int)(position.z * 100f);
		State state = Random.state;
		Random.InitState(num);
		float num2 = (m_rotateX ? ((float)Random.Range(0, m_stepsX) * 360f / (float)m_stepsX) : 0f);
		float num3 = (m_rotateY ? ((float)Random.Range(0, m_stepsY) * 360f / (float)m_stepsY) : 0f);
		float num4 = (m_rotateZ ? ((float)Random.Range(0, m_stepsZ) * 360f / (float)m_stepsZ) : 0f);
		((Component)this).transform.localRotation = Quaternion.Euler(num2, num3, num4);
		Random.state = state;
	}
}
