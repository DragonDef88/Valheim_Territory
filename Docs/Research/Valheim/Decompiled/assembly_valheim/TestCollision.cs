using UnityEngine;

public class TestCollision : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void OnCollisionEnter(Collision info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)("Hit by " + ((Object)((Component)info.rigidbody).gameObject).name));
		Vector3 val = info.relativeVelocity;
		string? text = ((object)(Vector3)(ref val)).ToString();
		val = info.relativeVelocity;
		ZLog.Log((object)("rel vel " + text + " " + ((object)(Vector3)(ref val)).ToString()));
		val = info.rigidbody.linearVelocity;
		string? text2 = ((object)(Vector3)(ref val)).ToString();
		val = info.rigidbody.angularVelocity;
		ZLog.Log((object)("Vel " + text2 + "  " + ((object)(Vector3)(ref val)).ToString()));
	}
}
