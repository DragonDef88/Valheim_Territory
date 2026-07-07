using System;
using System.IO;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
	private Vector3 m_playerPos = Vector3.zero;

	private Vector3 m_currentBaseOffset = Vector3.zero;

	private Vector3 m_offsetBaseVel = Vector3.zero;

	private Vector3 m_playerVel = Vector3.zero;

	public Vector3 m_3rdOffset = Vector3.zero;

	public Vector3 m_3rdCombatOffset = Vector3.zero;

	public Vector3 m_fpsOffset = Vector3.zero;

	public float m_flyingDistance = 15f;

	public LayerMask m_blockCameraMask;

	public float m_minDistance;

	public float m_maxDistance = 6f;

	public float m_maxDistanceBoat = 6f;

	public float m_raycastWidth = 0.35f;

	public bool m_smoothYTilt;

	public float m_zoomSens = 10f;

	public float m_inventoryOffset = 0.1f;

	public float m_nearClipPlaneMin = 0.1f;

	public float m_nearClipPlaneMax = 0.5f;

	public float m_fov = 65f;

	public float m_freeFlyMinFov = 5f;

	public float m_freeFlyMaxFov = 120f;

	public float m_tiltSmoothnessShipMin = 0.1f;

	public float m_tiltSmoothnessShipMax = 0.5f;

	public float m_shakeFreq = 10f;

	public float m_shakeMovement = 1f;

	public float m_smoothness = 0.1f;

	public float m_minWaterDistance = 0.3f;

	public Camera m_skyCamera;

	private float m_distance = 4f;

	private bool m_freeFly;

	private float m_shakeIntensity;

	private float m_shakeTimer;

	private bool m_cameraShakeEnabled = true;

	private bool m_mouseCapture;

	private Quaternion m_freeFlyRef = Quaternion.identity;

	private float m_freeFlyYaw;

	private float m_freeFlyPitch;

	private float m_freeFlySpeed = 20f;

	private float m_freeFlySmooth;

	private Vector3 m_freeFlySavedVel = Vector3.zero;

	private Transform m_freeFlyTarget;

	private Vector3 m_freeFlyTargetOffset = Vector3.zero;

	private Transform m_freeFlyLockon;

	private Vector3 m_freeFlyLockonOffset = Vector3.zero;

	private Vector3 m_freeFlyVel = Vector3.zero;

	private Vector3 m_freeFlyAcc = Vector3.zero;

	private Vector3 m_freeFlyTurnVel = Vector3.zero;

	private bool m_shipCameraTilt = true;

	private Vector3 m_smoothedCameraUp = Vector3.up;

	private Vector3 m_smoothedCameraUpVel = Vector3.zero;

	private AudioListener m_listner;

	private Camera m_camera;

	private bool m_waterClipping;

	private bool m_camZoomToggle;

	public HeatDistortImageEffect m_heatDistortImageEffect;

	private static GameCamera m_instance;

	public static GameCamera instance => m_instance;

	private void Awake()
	{
		m_instance = this;
		m_camera = ((Component)this).GetComponent<Camera>();
		m_listner = ((Component)this).GetComponentInChildren<AudioListener>();
		m_heatDistortImageEffect = ((Component)this).GetComponent<HeatDistortImageEffect>();
		m_camera.depthTextureMode = (DepthTextureMode)2;
		ApplySettings();
		if (!Application.isEditor)
		{
			m_mouseCapture = true;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_instance == (Object)(object)this)
		{
			m_instance = null;
		}
	}

	public void ApplySettings()
	{
		m_cameraShakeEnabled = PlatformPrefs.GetInt("CameraShake", 1) == 1;
		m_shipCameraTilt = PlatformPrefs.GetInt("ShipCameraTilt", 1) == 1;
	}

	private void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (ZInput.GetKeyDown((KeyCode)292, true) || (m_freeFly && ZInput.GetKeyDown((KeyCode)324, true)))
		{
			ScreenShot();
		}
		Player localPlayer = Player.m_localPlayer;
		if (Object.op_Implicit((Object)(object)localPlayer))
		{
			UpdateBaseOffset(localPlayer, deltaTime);
		}
		UpdateMouseCapture();
		UpdateCamera(Time.unscaledDeltaTime);
		UpdateListner();
	}

	public void UpdateMouseCapture()
	{
		if (ZInput.GetKey((KeyCode)306, true) && ZInput.GetKeyDown((KeyCode)282, true))
		{
			m_mouseCapture = !m_mouseCapture;
		}
		if (m_mouseCapture && !Hud.InRadial() && !InventoryGui.IsVisible() && !TextInput.IsVisible() && !Menu.IsVisible() && !Minimap.IsOpen() && !StoreGui.IsVisible() && !Hud.IsPieceSelectionVisible() && !PlayerCustomizaton.BarberBlocksLook() && !UnifiedPopup.IsVisible() && !ZNet.IsPasswordDialogShowing())
		{
			Cursor.lockState = (CursorLockMode)1;
			Cursor.visible = false;
		}
		else if (Hud.InRadial())
		{
			Cursor.lockState = (CursorLockMode)(!ZInput.IsMouseActive());
			Cursor.visible = ZInput.IsMouseActive();
		}
		else if (!Menu.IsVisible() || UnifiedPopup.IsVisible())
		{
			Cursor.lockState = (CursorLockMode)0;
			Cursor.visible = ZInput.IsMouseActive();
		}
	}

	public static void ScreenShot()
	{
		DateTime now = DateTime.Now;
		Directory.CreateDirectory(Utils.GetSaveDataPath((FileSource)1) + "/screenshots");
		string text = now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
		string text2 = now.ToString("yyyy-MM-dd");
		string text3 = Utils.GetSaveDataPath((FileSource)1) + "/screenshots/screenshot_" + text2 + "_" + text + ".png";
		if (!File.Exists(text3))
		{
			ScreenCapture.CaptureScreenshot(text3);
			ZLog.Log((object)("Screenshot saved:" + text3));
		}
	}

	private void UpdateListner()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if (Object.op_Implicit((Object)(object)localPlayer) && !m_freeFly)
		{
			((Component)m_listner).transform.position = localPlayer.m_eye.position;
		}
		else
		{
			((Component)m_listner).transform.localPosition = Vector3.zero;
		}
	}

	private void UpdateCamera(float dt)
	{
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		if (m_freeFly)
		{
			UpdateFreeFly(dt);
			UpdateCameraShake(dt);
			debugCamera(ZInput.GetMouseScrollWheel());
			return;
		}
		m_camera.fieldOfView = m_fov;
		m_skyCamera.fieldOfView = m_fov;
		Player localPlayer = Player.m_localPlayer;
		if (!Object.op_Implicit((Object)(object)localPlayer))
		{
			return;
		}
		if ((!Object.op_Implicit((Object)(object)Chat.instance) || !Chat.instance.HasFocus()) && !Console.IsVisible() && !InventoryGui.IsVisible() && !StoreGui.IsVisible() && !Menu.IsVisible() && !Minimap.IsOpen() && !Hud.IsPieceSelectionVisible() && !Hud.InRadial() && !localPlayer.InCutscene() && (!localPlayer.InPlaceMode() || localPlayer.InRepairMode() || !localPlayer.CanRotatePiece() || localPlayer.GetPlacementStatus() == Player.PlacementStatus.NoRayHits || ZInput.IsGamepadActive()))
		{
			float minDistance = m_minDistance;
			float mouseScrollWheel = ZInput.GetMouseScrollWheel();
			mouseScrollWheel = Mathf.Clamp(mouseScrollWheel, -0.05f, 0.05f);
			if (Player.m_debugMode)
			{
				mouseScrollWheel = debugCamera(mouseScrollWheel);
			}
			m_distance -= mouseScrollWheel * m_zoomSens;
			if (ZInput.GetButton("JoyAltKeys") && !Hud.InRadial())
			{
				if (ZInput.GetButton("JoyCamZoomIn"))
				{
					m_distance -= m_zoomSens * dt;
				}
				else if (ZInput.GetButton("JoyCamZoomOut"))
				{
					m_distance += m_zoomSens * dt;
				}
			}
			float num = (((Object)(object)localPlayer.GetControlledShip() != (Object)null) ? m_maxDistanceBoat : m_maxDistance);
			m_distance = Mathf.Clamp(m_distance, minDistance, num);
		}
		if (localPlayer.IsDead() && Object.op_Implicit((Object)(object)localPlayer.GetRagdoll()))
		{
			Vector3 averageBodyPosition = localPlayer.GetRagdoll().GetAverageBodyPosition();
			((Component)this).transform.LookAt(averageBodyPosition);
		}
		else if (localPlayer.IsAttached() && (Object)(object)localPlayer.GetAttachCameraPoint() != (Object)null)
		{
			Transform attachCameraPoint = localPlayer.GetAttachCameraPoint();
			((Component)this).transform.position = attachCameraPoint.position;
			((Component)this).transform.rotation = attachCameraPoint.rotation;
		}
		else
		{
			GetCameraPosition(dt, out var pos, out var rot);
			((Component)this).transform.position = pos;
			((Component)this).transform.rotation = rot;
		}
		UpdateCameraShake(dt);
		float debugCamera(float scroll)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			if (ZInput.GetKey((KeyCode)304, true) && ZInput.GetKey((KeyCode)99, true) && !Console.IsVisible())
			{
				Vector2 mouseDelta = ZInput.GetMouseDelta();
				EnvMan.instance.m_debugTimeOfDay = true;
				EnvMan.instance.m_debugTime = (EnvMan.instance.m_debugTime + mouseDelta.y * 0.005f) % 1f;
				if (EnvMan.instance.m_debugTime < 0f)
				{
					EnvMan.instance.m_debugTime += 1f;
				}
				m_fov += mouseDelta.x * 1f;
				m_fov = Mathf.Clamp(m_fov, 0.5f, 165f);
				m_camera.fieldOfView = m_fov;
				m_skyCamera.fieldOfView = m_fov;
				if (Object.op_Implicit((Object)(object)Player.m_localPlayer) && Player.m_localPlayer.IsDebugFlying())
				{
					if (scroll > 0f)
					{
						Character.m_debugFlySpeed = (int)Mathf.Clamp((float)Character.m_debugFlySpeed * 1.1f, (float)(Character.m_debugFlySpeed + 1), 300f);
					}
					else if (scroll < 0f && Character.m_debugFlySpeed > 1)
					{
						Character.m_debugFlySpeed = (int)Mathf.Min((float)Character.m_debugFlySpeed * 0.9f, (float)(Character.m_debugFlySpeed - 1));
					}
				}
				scroll = 0f;
			}
			return scroll;
		}
	}

	private void GetCameraPosition(float dt, out Vector3 pos, out Quaternion rot)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			pos = ((Component)this).transform.position;
			rot = ((Component)this).transform.rotation;
			return;
		}
		Vector3 val = GetOffsetedEyePos();
		float num = m_distance;
		if (localPlayer.InIntro())
		{
			val = ((Component)localPlayer).transform.position;
			num = m_flyingDistance;
		}
		Vector3 val2 = -((Component)localPlayer.m_eye).transform.forward;
		if (m_smoothYTilt && !localPlayer.InIntro())
		{
			num = Mathf.Lerp(num, 1.5f, Utils.SmoothStep(0f, -0.5f, val2.y));
		}
		Vector3 end = val + val2 * num;
		CollideRay2(localPlayer.m_eye.position, val, ref end);
		UpdateNearClipping(val, end, dt);
		float liquidLevel = Floating.GetLiquidLevel(end);
		if (end.y < liquidLevel + m_minWaterDistance)
		{
			end.y = liquidLevel + m_minWaterDistance;
			m_waterClipping = true;
		}
		else
		{
			m_waterClipping = false;
		}
		pos = end;
		rot = ((Component)localPlayer.m_eye).transform.rotation;
		if (m_shipCameraTilt)
		{
			ApplyCameraTilt(localPlayer, dt, ref rot);
		}
	}

	private void ApplyCameraTilt(Player player, float dt, ref Quaternion rot)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!player.InIntro())
		{
			Ship standingOnShip = player.GetStandingOnShip();
			float num = Mathf.Clamp01((m_distance - m_minDistance) / (m_maxDistanceBoat - m_minDistance));
			num = Mathf.Pow(num, 2f);
			float num2 = Mathf.Lerp(m_tiltSmoothnessShipMin, m_tiltSmoothnessShipMax, num);
			Vector3 up = Vector3.up;
			if ((Object)(object)standingOnShip != (Object)null && ((Component)standingOnShip).transform.up.y > 0f)
			{
				up = ((Component)standingOnShip).transform.up;
			}
			else if (player.IsAttached())
			{
				up = player.GetVisual().transform.up;
			}
			Vector3 forward = ((Component)player.m_eye).transform.forward;
			Vector3 val = Vector3.Lerp(up, Vector3.up, num * 0.5f);
			m_smoothedCameraUp = Vector3.SmoothDamp(m_smoothedCameraUp, val, ref m_smoothedCameraUpVel, num2, 99f, dt);
			rot = Quaternion.LookRotation(forward, m_smoothedCameraUp);
		}
	}

	private void UpdateNearClipping(Vector3 eyePos, Vector3 camPos, float dt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		float num = m_nearClipPlaneMax;
		Vector3 val = camPos - eyePos;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		if (m_waterClipping || Physics.CheckSphere(camPos - normalized * m_nearClipPlaneMax, m_nearClipPlaneMax, LayerMask.op_Implicit(m_blockCameraMask)))
		{
			num = m_nearClipPlaneMin;
		}
		if (m_camera.nearClipPlane != num)
		{
			m_camera.nearClipPlane = num;
		}
	}

	private void CollideRay2(Vector3 eyePos, Vector3 offsetedEyePos, ref Vector3 end)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - offsetedEyePos;
		if (RayTestPoint(eyePos, offsetedEyePos, ((Vector3)(ref val)).normalized, Vector3.Distance(eyePos, end), out var distance))
		{
			float num = Utils.LerpStep(0.5f, 2f, distance);
			val = end - eyePos;
			Vector3 val2 = eyePos + ((Vector3)(ref val)).normalized * distance;
			val = end - offsetedEyePos;
			Vector3 val3 = offsetedEyePos + ((Vector3)(ref val)).normalized * distance;
			end = Vector3.Lerp(val2, val3, num);
		}
	}

	private bool RayTestPoint(Vector3 point, Vector3 offsetedPoint, Vector3 dir, float maxDist, out float distance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		distance = maxDist;
		float num = ZoneSystem.instance.GetGroundOffset(point) * 1.6f;
		offsetedPoint += new Vector3(0f, 0f - num, 0f);
		RaycastHit val = default(RaycastHit);
		if (Physics.SphereCast(offsetedPoint, m_raycastWidth, dir, ref val, maxDist, LayerMask.op_Implicit(m_blockCameraMask)))
		{
			distance = ((RaycastHit)(ref val)).distance;
			flag = true;
		}
		_ = offsetedPoint + dir * distance;
		if (Physics.SphereCast(point, m_raycastWidth, dir, ref val, maxDist, LayerMask.op_Implicit(m_blockCameraMask)))
		{
			if (((RaycastHit)(ref val)).distance < distance)
			{
				distance = ((RaycastHit)(ref val)).distance;
			}
			flag = true;
		}
		if (Physics.Raycast(point - new Vector3(0f, num, 0f), dir, ref val, maxDist, LayerMask.op_Implicit(m_blockCameraMask)))
		{
			float num2 = ((RaycastHit)(ref val)).distance - m_nearClipPlaneMin;
			if (num2 < distance)
			{
				distance = num2;
			}
			flag = true;
		}
		if (flag)
		{
			Vector3 position = point + ((Vector3)(ref dir)).normalized * distance;
			float num3 = Mathf.Max(ZoneSystem.instance.GetGroundOffset(position) * 1.6f, num);
			if (num3 > 0f && Physics.Raycast(point + new Vector3(0f, 0f - num3, 0f), dir, ref val, maxDist, LayerMask.op_Implicit(m_blockCameraMask)))
			{
				float num4 = ((RaycastHit)(ref val)).distance - m_nearClipPlaneMin;
				if (num4 < distance)
				{
					distance = num4;
				}
			}
		}
		return flag;
	}

	private bool RayTestPoint(Vector3 point, Vector3 dir, float maxDist, out Vector3 hitPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.SphereCast(point, 0.2f, dir, ref val, maxDist, LayerMask.op_Implicit(m_blockCameraMask)))
		{
			hitPoint = point + dir * ((RaycastHit)(ref val)).distance;
			return true;
		}
		if (Physics.Raycast(point, dir, ref val, maxDist, LayerMask.op_Implicit(m_blockCameraMask)))
		{
			hitPoint = point + dir * (((RaycastHit)(ref val)).distance - 0.05f);
			return true;
		}
		hitPoint = Vector3.zero;
		return false;
	}

	private void UpdateFreeFly(float dt)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_061f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_0629: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_064f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_0642: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		if (Console.IsVisible())
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		zero = ZInput.GetMouseDelta();
		zero.x += ZInput.GetJoyRightStickX(true) * 110f * dt;
		zero.y += (0f - ZInput.GetJoyRightStickY(true)) * 110f * dt;
		m_freeFlyYaw += zero.x;
		m_freeFlyPitch -= zero.y;
		if (ZInput.GetMouseScrollWheel() < 0f)
		{
			m_freeFlySpeed *= 0.8f;
		}
		if (ZInput.GetMouseScrollWheel() > 0f)
		{
			m_freeFlySpeed *= 1.2f;
		}
		if (ZInput.GetMouseScrollWheel() > 0f)
		{
			m_freeFlySpeed *= 1.2f;
		}
		if (ZInput.GetButton("JoyTabLeft"))
		{
			m_camera.fieldOfView = Mathf.Max(m_freeFlyMinFov, m_camera.fieldOfView - dt * 20f);
		}
		if (ZInput.GetButton("JoyTabRight"))
		{
			m_camera.fieldOfView = Mathf.Min(m_freeFlyMaxFov, m_camera.fieldOfView + dt * 20f);
		}
		m_skyCamera.fieldOfView = m_camera.fieldOfView;
		if (ZInput.GetButton("JoyButtonY"))
		{
			m_freeFlySpeed += m_freeFlySpeed * 0.1f * dt * 10f;
		}
		if (ZInput.GetButton("JoyButtonX"))
		{
			m_freeFlySpeed -= m_freeFlySpeed * 0.1f * dt * 10f;
		}
		m_freeFlySpeed = Mathf.Clamp(m_freeFlySpeed, 1f, 1000f);
		if (ZInput.GetButtonDown("JoyLStick") || ZInput.GetButtonDown("SecondaryAttack"))
		{
			if (Object.op_Implicit((Object)(object)m_freeFlyLockon))
			{
				m_freeFlyLockon = null;
			}
			else
			{
				int mask = LayerMask.GetMask(new string[8] { "Default", "static_solid", "terrain", "vehicle", "character", "piece", "character_net", "viewblock" });
				RaycastHit val = default(RaycastHit);
				if (Physics.Raycast(((Component)this).transform.position, ((Component)this).transform.forward, ref val, 10000f, mask))
				{
					m_freeFlyLockon = ((Component)((RaycastHit)(ref val)).collider).transform;
					m_freeFlyLockonOffset = m_freeFlyLockon.InverseTransformPoint(((Component)this).transform.position);
				}
			}
		}
		Vector3 val2 = Vector3.zero;
		if (ZInput.GetButton("Left"))
		{
			val2 -= Vector3.right;
		}
		if (ZInput.GetButton("Right"))
		{
			val2 += Vector3.right;
		}
		if (ZInput.GetButton("Forward"))
		{
			val2 += Vector3.forward;
		}
		if (ZInput.GetButton("Backward"))
		{
			val2 -= Vector3.forward;
		}
		if (ZInput.GetButton("Jump"))
		{
			val2 += Vector3.up;
		}
		if (ZInput.GetButton("Crouch"))
		{
			val2 -= Vector3.up;
		}
		val2 += Vector3.up * ZInput.GetJoyRTrigger();
		val2 -= Vector3.up * ZInput.GetJoyLTrigger();
		val2 += Vector3.right * ZInput.GetJoyLeftStickX(false);
		val2 += -Vector3.forward * ZInput.GetJoyLeftStickY(true);
		if (ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("Block"))
		{
			m_freeFlySavedVel = val2;
		}
		float magnitude = ((Vector3)(ref m_freeFlySavedVel)).magnitude;
		if (magnitude > 0.001f)
		{
			val2 += m_freeFlySavedVel;
			if (((Vector3)(ref val2)).magnitude > magnitude)
			{
				val2 = ((Vector3)(ref val2)).normalized * magnitude;
			}
		}
		if (((Vector3)(ref val2)).magnitude > 1f)
		{
			((Vector3)(ref val2)).Normalize();
		}
		val2 = ((Component)this).transform.TransformVector(val2);
		val2 *= m_freeFlySpeed;
		if (m_freeFlySmooth <= 0f)
		{
			m_freeFlyVel = val2;
		}
		else
		{
			m_freeFlyVel = Vector3.SmoothDamp(m_freeFlyVel, val2, ref m_freeFlyAcc, m_freeFlySmooth, 99f, dt);
		}
		if (Object.op_Implicit((Object)(object)m_freeFlyLockon))
		{
			m_freeFlyLockonOffset += m_freeFlyLockon.InverseTransformVector(m_freeFlyVel * dt);
			((Component)this).transform.position = m_freeFlyLockon.TransformPoint(m_freeFlyLockonOffset);
		}
		else
		{
			((Component)this).transform.position = ((Component)this).transform.position + m_freeFlyVel * dt;
		}
		Quaternion val3 = Quaternion.Euler(0f, m_freeFlyYaw, 0f) * Quaternion.Euler(m_freeFlyPitch, 0f, 0f);
		if (Object.op_Implicit((Object)(object)m_freeFlyLockon))
		{
			val3 = m_freeFlyLockon.rotation * val3;
		}
		if ((ZInput.GetButtonDown("JoyRStick") && !ZInput.GetButton("JoyAltKeys")) || ZInput.GetButtonDown("Attack"))
		{
			if (Object.op_Implicit((Object)(object)m_freeFlyTarget))
			{
				m_freeFlyTarget = null;
			}
			else
			{
				int mask2 = LayerMask.GetMask(new string[8] { "Default", "static_solid", "terrain", "vehicle", "character", "piece", "character_net", "viewblock" });
				RaycastHit val4 = default(RaycastHit);
				if (Physics.Raycast(((Component)this).transform.position, ((Component)this).transform.forward, ref val4, 10000f, mask2))
				{
					m_freeFlyTarget = ((Component)((RaycastHit)(ref val4)).collider).transform;
					m_freeFlyTargetOffset = m_freeFlyTarget.InverseTransformPoint(((RaycastHit)(ref val4)).point);
				}
			}
		}
		if (Object.op_Implicit((Object)(object)m_freeFlyTarget))
		{
			Vector3 val5 = m_freeFlyTarget.TransformPoint(m_freeFlyTargetOffset) - ((Component)this).transform.position;
			val3 = Quaternion.LookRotation(((Vector3)(ref val5)).normalized, Vector3.up);
		}
		if (m_freeFlySmooth <= 0f)
		{
			((Component)this).transform.rotation = val3;
			return;
		}
		Quaternion rotation = Utils.SmoothDamp(((Component)this).transform.rotation, val3, ref m_freeFlyRef, m_freeFlySmooth, 9999f, dt);
		((Component)this).transform.rotation = rotation;
	}

	private void UpdateCameraShake(float dt)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		m_shakeIntensity -= dt;
		if (m_shakeIntensity <= 0f)
		{
			m_shakeIntensity = 0f;
			return;
		}
		float num = m_shakeIntensity * m_shakeIntensity * m_shakeIntensity;
		m_shakeTimer += dt * Mathf.Clamp01(m_shakeIntensity) * m_shakeFreq;
		Quaternion val = Quaternion.Euler(Mathf.Sin(m_shakeTimer) * num * m_shakeMovement, Mathf.Cos(m_shakeTimer * 0.9f) * num * m_shakeMovement, 0f);
		((Component)this).transform.rotation = ((Component)this).transform.rotation * val;
	}

	public void AddShake(Vector3 point, float range, float strength, bool continous)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!m_cameraShakeEnabled)
		{
			return;
		}
		float num = Vector3.Distance(point, ((Component)this).transform.position);
		if (num > range)
		{
			return;
		}
		num = Mathf.Max(1f, num);
		float num2 = 1f - num / range;
		float num3 = strength * num2;
		if (!(num3 < m_shakeIntensity))
		{
			m_shakeIntensity = num3;
			if (continous)
			{
				m_shakeTimer = Time.time * Mathf.Clamp01(strength) * m_shakeFreq;
			}
			else
			{
				m_shakeTimer = Time.time * Mathf.Clamp01(m_shakeIntensity) * m_shakeFreq;
			}
		}
	}

	private float RayTest(Vector3 point, Vector3 dir, float maxDist)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.SphereCast(point, 0.2f, dir, ref val, maxDist, LayerMask.op_Implicit(m_blockCameraMask)))
		{
			return ((RaycastHit)(ref val)).distance;
		}
		return maxDist;
	}

	private Vector3 GetCameraBaseOffset(Player player)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (player.InBed())
		{
			return player.GetHeadPoint() - ((Component)player).transform.position;
		}
		if (player.IsAttached() || player.IsSitting())
		{
			return player.GetHeadPoint() + Vector3.up * 0.3f - ((Component)player).transform.position;
		}
		return ((Component)player.m_eye).transform.position - ((Component)player).transform.position;
	}

	private void UpdateBaseOffset(Player player, float dt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector3 cameraBaseOffset = GetCameraBaseOffset(player);
		m_currentBaseOffset = Vector3.SmoothDamp(m_currentBaseOffset, cameraBaseOffset, ref m_offsetBaseVel, 0.5f, 999f, dt);
		if (Vector3.Distance(m_playerPos, ((Component)player).transform.position) > 20f)
		{
			m_playerPos = ((Component)player).transform.position;
		}
		m_playerPos = Vector3.SmoothDamp(m_playerPos, ((Component)player).transform.position, ref m_playerVel, m_smoothness, 999f, dt);
	}

	private Vector3 GetOffsetedEyePos()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if (Object.op_Implicit((Object)(object)localPlayer))
		{
			if ((Object)(object)localPlayer.GetStandingOnShip() != (Object)null || localPlayer.IsAttached())
			{
				return ((Component)localPlayer).transform.position + m_currentBaseOffset + GetCameraOffset(localPlayer);
			}
			return m_playerPos + m_currentBaseOffset + GetCameraOffset(localPlayer);
		}
		return ((Component)this).transform.position;
	}

	private Vector3 GetCameraOffset(Player player)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (m_distance <= 0f)
		{
			return ((Component)player.m_eye).transform.TransformVector(m_fpsOffset);
		}
		if (player.InBed())
		{
			return Vector3.zero;
		}
		Vector3 val = (player.UseMeleeCamera() ? m_3rdCombatOffset : m_3rdOffset);
		return ((Component)player.m_eye).transform.TransformVector(val);
	}

	public void ToggleFreeFly()
	{
		m_freeFly = !m_freeFly;
	}

	public void SetFreeFlySmoothness(float smooth)
	{
		m_freeFlySmooth = Mathf.Clamp(smooth, 0f, 1f);
	}

	public float GetFreeFlySmoothness()
	{
		return m_freeFlySmooth;
	}

	public static bool InFreeFly()
	{
		if (Object.op_Implicit((Object)(object)m_instance))
		{
			return m_instance.m_freeFly;
		}
		return false;
	}
}
