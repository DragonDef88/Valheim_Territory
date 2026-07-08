using UnityEngine;
using UnityEngine.EventSystems;

namespace Groups;

public class DragNDrop : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public RectTransform target;

	public bool shouldReturn;

	private bool isMouseDown;

	private Vector3 startMousePosition;

	private Vector3 startPosition;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		target = (RectTransform)((Component)this).transform;
	}

	private void Update()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (isMouseDown)
		{
			Vector3 val = Input.mousePosition - startMousePosition;
			Vector3 position = startPosition + val;
			SetPosition(position);
		}
	}

	public void SetPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = target.sizeDelta;
		position.x = Mathf.Clamp(position.x, sizeDelta.x / 2f, (float)Screen.width - sizeDelta.x / 2f);
		position.y = Mathf.Clamp(position.y, sizeDelta.y / 2f, (float)Screen.height - sizeDelta.y / 2f);
		((Transform)target).position = position;
		Groups.groupInterfaceAnchor.Value = Vector2.op_Implicit(((Transform)target).localPosition);
	}

	public void OnPointerDown(PointerEventData dt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		isMouseDown = true;
		startPosition = ((Transform)target).position;
		((Transform)target).position = startPosition;
		startMousePosition = Input.mousePosition;
	}

	public void OnPointerUp(PointerEventData dt)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		isMouseDown = false;
		if (shouldReturn)
		{
			((Transform)target).position = startPosition;
			Groups.groupInterfaceAnchor.Value = Vector2.op_Implicit(((Transform)target).localPosition);
		}
	}
}
