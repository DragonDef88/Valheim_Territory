using UnityEngine;

namespace LlamAcademy.Spring;

public class SpringVector2 : BaseSpring<Vector2>
{
	private FloatSpring XSpring = new FloatSpring();

	private FloatSpring YSpring = new FloatSpring();

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
			base.Stiffness = value;
		}
	}

	public override Vector2 InitialVelocity
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(XSpring.InitialVelocity, YSpring.InitialVelocity);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			XSpring.InitialVelocity = value.x;
			YSpring.InitialVelocity = value.y;
		}
	}

	public override Vector2 StartValue
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(XSpring.StartValue, YSpring.StartValue);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			XSpring.StartValue = value.x;
			YSpring.StartValue = value.y;
		}
	}

	public override Vector2 EndValue
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(XSpring.EndValue, YSpring.EndValue);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			XSpring.EndValue = value.x;
			YSpring.EndValue = value.y;
		}
	}

	public override Vector2 CurrentVelocity
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(XSpring.CurrentVelocity, YSpring.CurrentVelocity);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			XSpring.CurrentVelocity = value.x;
			YSpring.CurrentVelocity = value.y;
		}
	}

	public override Vector2 CurrentValue
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(XSpring.CurrentValue, YSpring.CurrentValue);
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			XSpring.CurrentValue = value.x;
			YSpring.CurrentValue = value.y;
		}
	}

	public override Vector2 Evaluate(float DeltaTime)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		CurrentValue = new Vector2(XSpring.Evaluate(DeltaTime), YSpring.Evaluate(DeltaTime));
		CurrentVelocity = new Vector2(XSpring.CurrentVelocity, YSpring.CurrentVelocity);
		return CurrentValue;
	}

	public override void Reset()
	{
		XSpring.Reset();
		YSpring.Reset();
	}

	public override void UpdateEndValue(Vector2 Value, Vector2 Velocity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		XSpring.UpdateEndValue(Value.x, Velocity.x);
		YSpring.UpdateEndValue(Value.y, Velocity.y);
	}
}
