using System.Threading;
using UnityEngine;

public class TestSceneCharacter : MonoBehaviour
{
	public float m_speed = 5f;

	public float m_cameraDistance = 10f;

	private Rigidbody m_body;

	private Quaternion m_lookYaw = Quaternion.identity;

	private float m_lookPitch;

	private void Start()
	{
		m_body = ((Component)this).GetComponent<Rigidbody>();
	}

	private void Update()
	{
		Thread.Sleep(30);
		HandleInput(Time.deltaTime);
	}

	private void HandleInput(float dt)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if ((Object)(object)mainCamera == (Object)null)
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		zero = ZInput.GetMouseDelta();
		if (ZInput.GetKey((KeyCode)324, true) || (int)Cursor.lockState != 0)
		{
			m_lookYaw *= Quaternion.Euler(0f, zero.x, 0f);
			m_lookPitch = Mathf.Clamp(m_lookPitch - zero.y, -89f, 89f);
		}
		if (ZInput.GetKeyDown((KeyCode)282, true))
		{
			if ((int)Cursor.lockState == 0)
			{
				Cursor.lockState = (CursorLockMode)1;
			}
			else
			{
				Cursor.lockState = (CursorLockMode)0;
			}
		}
		Vector3 val = Vector3.zero;
		if (ZInput.GetKey((KeyCode)97, true))
		{
			val -= ((Component)this).transform.right * m_speed;
		}
		if (ZInput.GetKey((KeyCode)100, true))
		{
			val += ((Component)this).transform.right * m_speed;
		}
		if (ZInput.GetKey((KeyCode)119, true))
		{
			val += ((Component)this).transform.forward * m_speed;
		}
		if (ZInput.GetKey((KeyCode)115, true))
		{
			val -= ((Component)this).transform.forward * m_speed;
		}
		if (ZInput.GetKeyDown((KeyCode)32, true))
		{
			m_body.AddForce(Vector3.up * 10f, (ForceMode)2);
		}
		Vector3 val2 = val - m_body.linearVelocity;
		val2.y = 0f;
		m_body.AddForce(val2, (ForceMode)2);
		((Component)this).transform.rotation = m_lookYaw;
		Quaternion val3 = m_lookYaw * Quaternion.Euler(m_lookPitch, 0f, 0f);
		((Component)mainCamera).transform.position = ((Component)this).transform.position - val3 * Vector3.forward * m_cameraDistance;
		((Component)mainCamera).transform.LookAt(((Component)this).transform.position + Vector3.up);
	}
}
