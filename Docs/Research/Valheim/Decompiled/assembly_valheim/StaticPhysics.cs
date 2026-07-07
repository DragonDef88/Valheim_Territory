using UnityEngine;

public class StaticPhysics : SlowUpdate
{
	public bool m_pushUp = true;

	public bool m_fall = true;

	public bool m_checkSolids;

	public float m_fallCheckRadius;

	private ZNetView m_nview;

	private const float m_fallSpeed = 4f;

	private const float m_fallStep = 0.05f;

	private float m_updateTime;

	private bool m_falling;

	private int m_activeArea;

	public bool IsFalling => m_falling;

	public override void Awake()
	{
		base.Awake();
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_updateTime = Time.time + 20f;
		m_activeArea = ZoneSystem.instance.m_activeArea;
	}

	private bool ShouldUpdate(float time)
	{
		return time > m_updateTime;
	}

	public override void SUpdate(float time, Vector2i referenceZone)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!m_falling && ShouldUpdate(time) && !ZNetScene.OutsideActiveArea(((Component)this).transform.position, referenceZone, m_activeArea))
		{
			if (m_fall)
			{
				CheckFall();
			}
			if (m_pushUp)
			{
				PushUp();
			}
		}
	}

	private void CheckFall()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		float fallHeight = GetFallHeight();
		if (((Component)this).transform.position.y > fallHeight + 0.05f)
		{
			Fall();
		}
	}

	private float GetFallHeight()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (m_checkSolids)
		{
			if (ZoneSystem.instance.GetSolidHeight(((Component)this).transform.position, m_fallCheckRadius, out var height, ((Component)this).transform))
			{
				return height;
			}
			return ((Component)this).transform.position.y;
		}
		if (ZoneSystem.instance.GetGroundHeight(((Component)this).transform.position, out var height2))
		{
			return height2;
		}
		return ((Component)this).transform.position.y;
	}

	private void Fall()
	{
		m_falling = true;
		((Component)this).gameObject.isStatic = false;
		((MonoBehaviour)this).InvokeRepeating("FallUpdate", 0.05f, 0.05f);
	}

	private void FallUpdate()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		float fallHeight = GetFallHeight();
		Vector3 position = ((Component)this).transform.position;
		position.y -= 0.2f;
		if (position.y <= fallHeight)
		{
			position.y = fallHeight;
			StopFalling();
		}
		((Component)this).transform.position = position;
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid() && m_nview.IsOwner())
		{
			m_nview.GetZDO().SetPosition(((Component)this).transform.position);
		}
	}

	private void StopFalling()
	{
		((Component)this).gameObject.isStatic = true;
		m_falling = false;
		((MonoBehaviour)this).CancelInvoke("FallUpdate");
	}

	private void PushUp()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (ZoneSystem.instance.GetGroundHeight(((Component)this).transform.position, out var height) && ((Component)this).transform.position.y < height - 0.05f)
		{
			((Component)this).gameObject.isStatic = false;
			Vector3 position = ((Component)this).transform.position;
			position.y = height;
			((Component)this).transform.position = position;
			((Component)this).gameObject.isStatic = true;
			if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid() && m_nview.IsOwner())
			{
				m_nview.GetZDO().SetPosition(((Component)this).transform.position);
			}
		}
	}
}
