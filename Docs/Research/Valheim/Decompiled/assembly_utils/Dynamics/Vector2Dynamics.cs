using UnityEngine;

namespace Dynamics;

public class Vector2Dynamics : SecondOrderDynamicsBase<Vector2>
{
	public Vector2Dynamics(float f, float z, float r, Vector2 x0)
		: base(f, z, r, x0)
	{
	}//IL_0004: Unknown result type (might be due to invalid IL or missing references)


	public Vector2Dynamics(Vector2 x0)
		: base(x0)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)


	public Vector2Dynamics(DynamicsParameters parameters, Vector2 x0)
		: base(parameters, x0)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


	public Vector2 Update(float dt, Vector2 target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		_velocity = (target - _previousInput) / dt;
		_previousInput = target;
		float num = Mathf.Max(k2, Mathf.Max(dt * dt / 2f + dt * k1 / 2f, dt * k1));
		_currentValue += dt * _velocity;
		_velocity += dt * (target + k3 * _velocity - _currentValue - k1 * _velocity) / num;
		return _currentValue;
	}
}
