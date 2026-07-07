using UnityEngine;

namespace LlamAcademy.Spring;

public class SpringVector3 : BaseSpring<Vector3>
{
	private FloatSpring XSpring = new FloatSpring();

	private FloatSpring YSpring = new FloatSpring();

	private FloatSpring ZSpring = new FloatSpring();

	public override float Damping
	{
		get
		{
			return base.Damping;
		}
		set
		{
			XSpring.Damping = value;
			YSpring.Damping = value;
			ZSpring.Damping = value;
			base.Damping = value;
		}
	}

	public override float Stiffness
	{
		get
		{
			return base.Stiffness;
		}
		set
		{
			XSpring.Stiffness = value;
			YSpring.Stiffness = value;
			ZSpring.Stiffness = value;
			base.Stiffness = value;
		}
	}

	public override Vector3 StartValue
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(XSpring.StartValue, YSpring.StartValue, ZSpring.StartValue);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			XSpring.StartValue = value.x;
			YSpring.StartValue = value.y;
			ZSpring.StartValue = value.z;
		}
	}

	public override Vector3 EndValue
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(XSpring.EndValue, YSpring.EndValue, ZSpring.EndValue);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			XSpring.EndValue = value.x;
			YSpring.EndValue = value.y;
			ZSpring.EndValue = value.z;
		}
	}

	public override Vector3 InitialVelocity
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(XSpring.InitialVelocity, YSpring.InitialVelocity, ZSpring.InitialVelocity);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			XSpring.InitialVelocity = value.x;
			YSpring.InitialVelocity = value.y;
			ZSpring.InitialVelocity = value.z;
		}
	}

	public override Vector3 CurrentVelocity
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(XSpring.CurrentVelocity, YSpring.CurrentVelocity, ZSpring.CurrentVelocity);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			XSpring.CurrentVelocity = value.x;
			YSpring.CurrentVelocity = value.y;
			ZSpring.CurrentVelocity = value.z;
		}
	}

	public override Vector3 CurrentValue
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(XSpring.CurrentValue, YSpring.CurrentValue, ZSpring.CurrentValue);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			XSpring.CurrentValue = value.x;
			YSpring.CurrentValue = value.y;
			ZSpring.CurrentValue = value.z;
		}
	}

	public override Vector3 Evaluate(float DeltaTime)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		CurrentValue = new Vector3(XSpring.Evaluate(DeltaTime), YSpring.Evaluate(DeltaTime), ZSpring.Evaluate(DeltaTime));
		CurrentVelocity = new Vector3(XSpring.CurrentVelocity, YSpring.CurrentVelocity, ZSpring.CurrentVelocity);
		return CurrentValue;
	}

	public override void Reset()
	{
		XSpring.Reset();
		YSpring.Reset();
		ZSpring.Reset();
	}

	public override void UpdateEndValue(Vector3 Value, Vector3 Velocity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		XSpring.UpdateEndValue(Value.x, Velocity.x);
		YSpring.UpdateEndValue(Value.y, Velocity.y);
		ZSpring.UpdateEndValue(Value.z, Velocity.z);
	}
}
