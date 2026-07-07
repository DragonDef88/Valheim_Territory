using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Utils
{
	public enum IterativeSearchType
	{
		DepthFirst,
		BreadthFirst
	}

	public delegate void ChildHandler(GameObject go);

	[StructLayout(LayoutKind.Explicit)]
	private struct IntFloat
	{
		[FieldOffset(0)]
		private float f;

		[FieldOffset(0)]
		private uint i;

		public static uint Convert(float value)
		{
			IntFloat intFloat = default(IntFloat);
			intFloat.f = value;
			return intFloat.i;
		}

		public static float Convert(uint value)
		{
			IntFloat intFloat = default(IntFloat);
			intFloat.i = value;
			return intFloat.f;
		}
	}

	private static string m_saveDataOverride = null;

	public static string persistantDataPath = Application.persistentDataPath;

	private static readonly char[] extraCharacters = new char[2] { '(', ' ' };

	private static readonly Plane[] s_mainPlanes = (Plane[])(object)new Plane[6];

	private static int s_lastPlaneFrame = -1;

	private static int lastFrameCheck = 0;

	private static Camera lastMainCamera = null;

	public static string GetSaveDataPath(FileHelpers.FileSource fileSource)
	{
		if (FileHelpers.CloudStorageEnabled && (fileSource == FileHelpers.FileSource.Auto || fileSource == FileHelpers.FileSource.Cloud))
		{
			return "";
		}
		if (m_saveDataOverride != null)
		{
			return m_saveDataOverride;
		}
		return persistantDataPath;
	}

	public static void SetSaveDataPath(string path)
	{
		m_saveDataOverride = path;
	}

	public static void ResetSaveDataPath()
	{
		m_saveDataOverride = null;
	}

	public static string GetPrefabName(GameObject gameObject)
	{
		return GetPrefabName(((Object)gameObject).name);
	}

	public static string GetPrefabName(string name)
	{
		int num = name.IndexOfAny(extraCharacters);
		if (num != -1)
		{
			return name.Remove(num);
		}
		return name;
	}

	public static bool InsideMainCamera(Bounds bounds)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Plane[] mainCameraFrustumPlanes = GetMainCameraFrustumPlanes();
		if (mainCameraFrustumPlanes == null)
		{
			return false;
		}
		return GeometryUtility.TestPlanesAABB(mainCameraFrustumPlanes, bounds);
	}

	public static bool InsideMainCamera(BoundingSphere bounds)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Plane[] mainCameraFrustumPlanes = GetMainCameraFrustumPlanes();
		if (mainCameraFrustumPlanes == null)
		{
			return false;
		}
		for (int i = 0; i < mainCameraFrustumPlanes.Length; i++)
		{
			if (((Plane)(ref mainCameraFrustumPlanes[i])).GetDistanceToPoint(bounds.position) < 0f - bounds.radius)
			{
				return false;
			}
		}
		return true;
	}

	public static Plane[] GetMainCameraFrustumPlanes()
	{
		Camera mainCamera = GetMainCamera();
		if (Object.op_Implicit((Object)(object)mainCamera))
		{
			if (Time.frameCount != s_lastPlaneFrame)
			{
				GeometryUtility.CalculateFrustumPlanes(mainCamera, s_mainPlanes);
				s_lastPlaneFrame = Time.frameCount;
			}
			return s_mainPlanes;
		}
		return null;
	}

	public static Camera GetMainCamera()
	{
		int frameCount = Time.frameCount;
		if (lastFrameCheck == frameCount)
		{
			return lastMainCamera;
		}
		lastMainCamera = Camera.main;
		lastFrameCheck = frameCount;
		return lastMainCamera;
	}

	public static Color Vec3ToColor(Vector3 c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new Color(c.x, c.y, c.z);
	}

	public static Vector3 ColorToVec3(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(c.r, c.g, c.b);
	}

	public static float LerpStep(float l, float h, float v)
	{
		return Mathf.Clamp01((v - l) / (h - l));
	}

	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * Clamp1(t);
	}

	public static float SmoothStep(float p_Min, float p_Max, float p_X)
	{
		float num = Clamp01((p_X - p_Min) / (p_Max - p_Min));
		return num * num * (3f - 2f * num);
	}

	public static double LerpStep(double l, double h, double v)
	{
		return Clamp01((v - l) / (h - l));
	}

	public static float LerpSmooth(float a, float b, float dt, float h)
	{
		return b + (a - b) * Mathf.Pow(2f, (0f - dt) / h);
	}

	public static double Clamp01(double v)
	{
		if (v > 1.0)
		{
			return 1.0;
		}
		if (v < 0.0)
		{
			return 0.0;
		}
		return v;
	}

	public static float Remap(float value, float inLow, float inHigh, float outLow, float outHigh)
	{
		return Mathf.Lerp(outLow, outHigh, Mathf.InverseLerp(inLow, inHigh, value));
	}

	public static float Fbm(Vector3 p, int octaves, float lacunarity, float gain)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Utils.Fbm(new Vector2(p.x, p.z), octaves, lacunarity, gain);
	}

	public static float FbmMaxValue(int octaves, float gain)
	{
		float num = 0f;
		float num2 = 1f;
		for (int i = 0; i < octaves; i++)
		{
			num += num2;
			num2 *= gain;
		}
		return num;
	}

	public static float Fbm(Vector2 p, int octaves, float lacunarity, float gain)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 1f;
		Vector2 val = p;
		for (int i = 0; i < octaves; i++)
		{
			num += num2 * Mathf.PerlinNoise(val.x, val.y);
			num2 *= gain;
			val *= lacunarity;
		}
		return num;
	}

	public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float smoothTime, float maxSpeed, float deltaTime)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Quaternion.Dot(rot, target) > 0f) ? 1f : (-1f));
		target.x *= num;
		target.y *= num;
		target.z *= num;
		target.w *= num;
		Vector4 val = new Vector4(Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, smoothTime, maxSpeed, deltaTime), Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, smoothTime, maxSpeed, deltaTime), Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, smoothTime, maxSpeed, deltaTime), Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, smoothTime, maxSpeed, deltaTime));
		Vector4 normalized = ((Vector4)(ref val)).normalized;
		float num2 = 1f / deltaTime;
		deriv.x = (normalized.x - rot.x) * num2;
		deriv.y = (normalized.y - rot.y) * num2;
		deriv.z = (normalized.z - rot.z) * num2;
		deriv.w = (normalized.w - rot.w) * num2;
		return new Quaternion(normalized.x, normalized.y, normalized.z, normalized.w);
	}

	public static long GenerateUID()
	{
		string text = null;
		string text2 = null;
		try
		{
			IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
			if (iPGlobalProperties != null && iPGlobalProperties.HostName != null)
			{
				text = iPGlobalProperties.HostName;
			}
			if (iPGlobalProperties != null && iPGlobalProperties.DomainName != null)
			{
				text2 = iPGlobalProperties.DomainName;
			}
		}
		catch
		{
			if (text == null)
			{
				text = "unkown";
			}
			if (text2 == null)
			{
				text2 = "domain";
			}
		}
		return (long)(text + ":" + text2).GetHashCode() + (long)Random.Range(1, int.MaxValue);
	}

	public static bool TestPointInViewFrustum(Camera camera, Vector3 worldPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = camera.WorldToViewportPoint(worldPos);
		if (val.x >= 0f && val.x <= 1f && val.y >= 0f)
		{
			return val.y <= 1f;
		}
		return false;
	}

	public static Vector3 ParseVector3(string rString)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		string[] array = rString.Substring(1, rString.Length - 2).Split(',');
		float num = float.Parse(array[0]);
		float num2 = float.Parse(array[1]);
		float num3 = float.Parse(array[2]);
		return new Vector3(num, num2, num3);
	}

	public static int GetMinPow2(int val)
	{
		if (val <= 1)
		{
			return 1;
		}
		if (val <= 2)
		{
			return 2;
		}
		if (val <= 4)
		{
			return 4;
		}
		if (val <= 8)
		{
			return 8;
		}
		if (val <= 16)
		{
			return 16;
		}
		if (val <= 32)
		{
			return 32;
		}
		if (val <= 64)
		{
			return 64;
		}
		if (val <= 128)
		{
			return 128;
		}
		if (val <= 256)
		{
			return 256;
		}
		if (val <= 512)
		{
			return 512;
		}
		if (val <= 1024)
		{
			return 1024;
		}
		if (val <= 2048)
		{
			return 2048;
		}
		if (val <= 4096)
		{
			return 4096;
		}
		return 1;
	}

	public static void NormalizeQuaternion(ref Quaternion q)
	{
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			num += ((Quaternion)(ref q))[i] * ((Quaternion)(ref q))[i];
		}
		float num2 = 1f / Mathf.Sqrt(num);
		for (int j = 0; j < 4; j++)
		{
			int num3 = j;
			((Quaternion)(ref q))[num3] = ((Quaternion)(ref q))[num3] * num2;
		}
	}

	public static Vector3 Project(Vector3 v, Vector3 onTo)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(onTo, v);
		return onTo * num;
	}

	public static float Length(float x, float y)
	{
		return Mathf.Sqrt(x * x + y * y);
	}

	public static float DistanceSqr(Vector3 v0, Vector3 v1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float num = v1.x - v0.x;
		float num2 = v1.y - v0.y;
		float num3 = v1.z - v0.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public static float DistanceXZ(Vector3 v0, Vector3 v1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		float num = v1.x - v0.x;
		float num2 = v1.z - v0.z;
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	public static float LengthXZ(Vector3 v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Sqrt(v.x * v.x + v.z * v.z);
	}

	public static Vector3 DirectionXZ(Vector3 dir)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		dir.y = 0f;
		((Vector3)(ref dir)).Normalize();
		return dir;
	}

	public static Vector3 Bezier2(Vector3 Start, Vector3 Control, Vector3 End, float delta)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		return (1f - delta) * (1f - delta) * Start + 2f * delta * (1f - delta) * Control + delta * delta * End;
	}

	public static float FixDegAngle(float p_Angle)
	{
		while (p_Angle >= 360f)
		{
			p_Angle -= 360f;
		}
		while (p_Angle < 0f)
		{
			p_Angle += 360f;
		}
		return p_Angle;
	}

	public static float DegDistance(float p_a, float p_b)
	{
		if (p_a == p_b)
		{
			return 0f;
		}
		p_a = FixDegAngle(p_a);
		p_b = FixDegAngle(p_b);
		float num = Mathf.Abs(p_b - p_a);
		if (num > 180f)
		{
			num = Mathf.Abs(num - 360f);
		}
		return num;
	}

	public static float GetYawDeltaAngle(Quaternion q1, Quaternion q2)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		float y = ((Quaternion)(ref q1)).eulerAngles.y;
		float y2 = ((Quaternion)(ref q2)).eulerAngles.y;
		return Mathf.DeltaAngle(y, y2);
	}

	public static float YawFromDirection(Vector3 dir)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Atan2(dir.x, dir.z);
		return FixDegAngle(57.29578f * num);
	}

	public static float DegDirection(float p_a, float p_b)
	{
		if (p_a == p_b)
		{
			return 0f;
		}
		p_a = FixDegAngle(p_a);
		p_b = FixDegAngle(p_b);
		float num = p_a - p_b;
		float num2 = ((num > 0f) ? 1f : (-1f));
		if (Mathf.Abs(num) > 180f)
		{
			num2 *= -1f;
		}
		return num2;
	}

	public static void RotateBodyTo(Rigidbody body, Quaternion rot, float alpha)
	{
	}

	public static bool IsEnabledInheirarcy(GameObject go, GameObject root)
	{
		do
		{
			if (!go.activeSelf)
			{
				return false;
			}
			if ((Object)(object)go == (Object)(object)root)
			{
				return true;
			}
			go = ((Component)go.transform.parent).gameObject;
		}
		while ((Object)(object)go != (Object)null);
		return true;
	}

	public static bool IsParent(Transform go, Transform parent)
	{
		do
		{
			if ((Object)(object)go == (Object)(object)parent)
			{
				return true;
			}
			go = go.parent;
		}
		while ((Object)(object)go != (Object)null);
		return false;
	}

	public static Transform FindChild(Transform aParent, string aName, IterativeSearchType searchType = IterativeSearchType.DepthFirst)
	{
		switch (searchType)
		{
		case IterativeSearchType.DepthFirst:
		{
			Stack<Transform> stack = new Stack<Transform>();
			Transform val2 = aParent;
			while (true)
			{
				for (int num = val2.childCount - 1; num >= 0; num--)
				{
					stack.Push(val2.GetChild(num));
				}
				if (stack.Count <= 0)
				{
					break;
				}
				val2 = stack.Pop();
				if (((Object)val2).name == aName)
				{
					return val2;
				}
			}
			return null;
		}
		case IterativeSearchType.BreadthFirst:
		{
			Queue<Transform> queue = new Queue<Transform>();
			Transform val = aParent;
			while (true)
			{
				int childCount = val.childCount;
				for (int i = 0; i < childCount; i++)
				{
					queue.Enqueue(val.GetChild(i));
				}
				if (queue.Count <= 0)
				{
					break;
				}
				val = queue.Dequeue();
				if (((Object)val).name == aName)
				{
					return val;
				}
			}
			return null;
		}
		default:
			ZLog.LogError("Search type not implemented!");
			return null;
		}
	}

	public static Transform GetBoneTransform(Animator animator, HumanBodyBones humanBoneId)
	{
		return FindChild(((Component)animator).transform, ((object)(HumanBodyBones)(ref humanBoneId)).ToString());
	}

	public static void RecreateComponent<T>(ref T component) where T : Component
	{
		FieldInfo[] fields = ((object)component).GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		bool[] array = new bool[fields.Length];
		object[] array2 = new object[fields.Length];
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].IsPublic || FieldHasCustomAttribute(fields[i], typeof(SerializeField)))
			{
				array2[i] = fields[i].GetValue(component);
				array[i] = true;
			}
			else
			{
				array[i] = false;
			}
		}
		PropertyInfo[] properties = ((object)component).GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		object[] array3 = new object[properties.Length];
		for (int j = 0; j < properties.Length; j++)
		{
			if (properties[j].CanRead && properties[j].CanWrite)
			{
				array3[j] = properties[j].GetValue(component);
			}
		}
		GameObject gameObject = ((Component)component).gameObject;
		Object.DestroyImmediate((Object)(object)component);
		component = gameObject.AddComponent<T>();
		for (int k = 0; k < fields.Length; k++)
		{
			if (array[k])
			{
				fields[k].SetValue(component, array2[k]);
			}
		}
		for (int l = 0; l < properties.Length; l++)
		{
			if (properties[l].CanRead && properties[l].CanWrite)
			{
				properties[l].SetValue(component, array3[l]);
			}
		}
	}

	public static bool FieldHasCustomAttribute(FieldInfo field, Type customAttributeType)
	{
		foreach (CustomAttributeData customAttribute in field.CustomAttributes)
		{
			if (customAttribute.AttributeType == customAttributeType)
			{
				return true;
			}
		}
		return false;
	}

	public static void RecreateComponent(ref Cloth component)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		List<uint> list = new List<uint>();
		component.GetSelfAndInterCollisionIndices(list);
		List<uint> list2 = new List<uint>();
		component.GetVirtualParticleIndices(list2);
		List<Vector3> list3 = new List<Vector3>();
		component.GetVirtualParticleWeights(list3);
		List<object> list4 = new List<object>
		{
			component.bendingStiffness, component.capsuleColliders, component.clothSolverFrequency, component.coefficients, component.collisionMassScale, component.enableContinuousCollision, component.enabled, component.externalAcceleration, component.friction, component.randomAcceleration,
			component.selfCollisionDistance, component.selfCollisionStiffness, component.sleepThreshold, component.sphereColliders, component.stiffnessFrequency, component.stretchingStiffness, component.useGravity, component.useTethers, component.useVirtualParticles, component.worldAccelerationScale,
			component.worldVelocityScale, list, list2, list3
		};
		GameObject gameObject = ((Component)component).gameObject;
		Object.DestroyImmediate((Object)(object)component);
		component = gameObject.AddComponent<Cloth>();
		component.bendingStiffness = (float)list4[0];
		component.capsuleColliders = (CapsuleCollider[])list4[1];
		component.clothSolverFrequency = (float)list4[2];
		component.coefficients = (ClothSkinningCoefficient[])list4[3];
		component.collisionMassScale = (float)list4[4];
		component.enableContinuousCollision = (bool)list4[5];
		component.enabled = (bool)list4[6];
		component.externalAcceleration = (Vector3)list4[7];
		component.friction = (float)list4[8];
		component.randomAcceleration = (Vector3)list4[9];
		component.selfCollisionDistance = (float)list4[10];
		component.selfCollisionStiffness = (float)list4[11];
		component.sleepThreshold = (float)list4[12];
		component.sphereColliders = (ClothSphereColliderPair[])list4[13];
		component.stiffnessFrequency = (float)list4[14];
		component.stretchingStiffness = (float)list4[15];
		component.useGravity = (bool)list4[16];
		component.useTethers = (bool)list4[17];
		component.useVirtualParticles = (float)list4[18];
		component.worldAccelerationScale = (float)list4[19];
		component.worldVelocityScale = (float)list4[20];
		component.SetSelfAndInterCollisionIndices((List<uint>)list4[21]);
		component.SetVirtualParticleIndices((List<uint>)list4[22]);
		component.SetVirtualParticleWeights((List<Vector3>)list4[23]);
	}

	public static void AddToLodgroup(LODGroup lg, GameObject toAdd)
	{
		List<Renderer> list = new List<Renderer>(lg.GetLODs()[0].renderers);
		Renderer[] componentsInChildren = toAdd.GetComponentsInChildren<Renderer>();
		list.AddRange(componentsInChildren);
		lg.GetLODs()[0].renderers = list.ToArray();
	}

	public static void RemoveFromLodgroup(LODGroup lg, GameObject toRemove)
	{
		List<Renderer> list = new List<Renderer>(lg.GetLODs()[0].renderers);
		Renderer[] componentsInChildren = toRemove.GetComponentsInChildren<Renderer>();
		foreach (Renderer item in componentsInChildren)
		{
			list.Remove(item);
		}
		lg.GetLODs()[0].renderers = list.ToArray();
	}

	public static void DrawGizmoCylinder(Vector3 center, float radius, float height, int steps)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Math.PI * 2f / (float)steps;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0f, height, 0f);
		Vector3 val2 = center + new Vector3(Mathf.Cos(0f) * radius, 0f, Mathf.Sin(0f) * radius);
		Vector3 val3 = val2;
		for (float num2 = num; num2 <= (float)Math.PI * 2f; num2 += num)
		{
			Vector3 val4 = center + new Vector3(Mathf.Cos(num2) * radius, 0f, Mathf.Sin(num2) * radius);
			Gizmos.DrawLine(val4, val3);
			Gizmos.DrawLine(val4 + val, val3 + val);
			Gizmos.DrawLine(val4, val4 + val);
			val3 = val4;
		}
		Gizmos.DrawLine(val3, val2);
		Gizmos.DrawLine(val2, val2 + val);
		Gizmos.DrawLine(val3 + val, val2 + val);
	}

	public static void DrawGizmoCircle(Vector3 center, float radius, int steps)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Math.PI * 2f / (float)steps;
		Vector3 val = center + new Vector3(Mathf.Cos(0f) * radius, 0f, Mathf.Sin(0f) * radius);
		Vector3 val2 = val;
		for (float num2 = num; num2 <= (float)Math.PI * 2f; num2 += num)
		{
			Vector3 val3 = center + new Vector3(Mathf.Cos(num2) * radius, 0f, Mathf.Sin(num2) * radius);
			Gizmos.DrawLine(val3, val2);
			val2 = val3;
		}
		Gizmos.DrawLine(val2, val);
	}

	public static void DrawGizmoCapsule(Vector3 p1, Vector3 p2, float radius)
	{
	}

	public static void DrawGizmoCapsule(CapsuleCollider capsule, Transform t = null)
	{
	}

	public static void ClampUIToScreen(RectTransform transform)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		transform.GetWorldCorners(array);
		if (!((Object)(object)GetMainCamera() == (Object)null))
		{
			float num = 0f;
			float num2 = 0f;
			if (array[2].x > (float)Screen.width)
			{
				num -= array[2].x - (float)Screen.width;
			}
			if (array[0].x < 0f)
			{
				num -= array[0].x;
			}
			if (array[2].y > (float)Screen.height)
			{
				num2 -= array[2].y - (float)Screen.height;
			}
			if (array[0].y < 0f)
			{
				num2 -= array[0].y;
			}
			Vector3 position = ((Transform)transform).position;
			position.x += num;
			position.y += num2;
			((Transform)transform).position = position;
		}
	}

	public static float Pull(Rigidbody body, Vector3 target, float targetDistance, float speed, float force, float smoothDistance, bool noUpForce = false, bool useForce = false, float power = 1f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = target - body.position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude < targetDistance)
		{
			return 0f;
		}
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = Mathf.Clamp01((magnitude - targetDistance) / smoothDistance);
		num = (float)Math.Pow(num, power);
		Vector3 val2 = Vector3.Project(body.linearVelocity, ((Vector3)(ref normalized)).normalized);
		Vector3 val3 = ((Vector3)(ref normalized)).normalized * speed - val2;
		if (noUpForce && val3.y > 0f)
		{
			val3.y = 0f;
		}
		ForceMode val4 = (ForceMode)(useForce ? 1 : 2);
		Vector3 val5 = val3 * num * Mathf.Clamp01(force);
		body.AddForce(val5, val4);
		return num;
	}

	public static byte[] Compress(byte[] inputArray)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionLevel.Fastest))
		{
			gZipStream.Write(inputArray, 0, inputArray.Length);
		}
		return memoryStream.ToArray();
	}

	public static byte[] Decompress(byte[] inputArray)
	{
		using MemoryStream stream = new MemoryStream(inputArray);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		gZipStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public static string GetPath(this Transform obj)
	{
		if ((Object)(object)obj.parent == (Object)null)
		{
			return "/" + ((Object)obj).name;
		}
		return obj.parent.GetPath() + "/" + ((Object)obj).name;
	}

	public static int CompareFloats(float a, float b)
	{
		if (!(a > b))
		{
			if (!(a < b))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public static void IterateHierarchy(GameObject gameObject, ChildHandler childHandler, bool deepestFirst = false)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		foreach (Transform item in gameObject.transform)
		{
			Transform val = item;
			if (!deepestFirst)
			{
				childHandler(((Component)val).gameObject);
			}
			IterateHierarchy(((Component)val).gameObject, childHandler);
			if (deepestFirst)
			{
				childHandler(((Component)val).gameObject);
			}
		}
	}

	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - pivot;
		val = rotation * val;
		point = val + pivot;
		return point;
	}

	public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
	{
		object[] customAttributes = enumVal.GetType().GetMember(enumVal.ToString())[0].GetCustomAttributes(typeof(T), inherit: false);
		if (customAttributes.Length == 0)
		{
			return null;
		}
		return (T)customAttributes[0];
	}

	public static Quaternion RotateTorwardsSmooth(Quaternion from, Quaternion to, Quaternion last, float maxDegreesDelta, float acceleration = 1.2f, float deacceleration = 0.05f, float minDegreesDelta = 0.005f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (last == default(Quaternion))
		{
			last = from;
		}
		Vector3 val = from * Vector3.forward * 100f;
		float num = Vector3.Distance(last * Vector3.forward * 100f, val);
		float num2 = Mathf.Clamp(Vector3.Distance(to * Vector3.forward * 100f, val) * deacceleration, minDegreesDelta, 1f);
		float num3 = ((num2 < 1f) ? num2 : Mathf.Clamp(num * acceleration, minDegreesDelta, 1f));
		return Quaternion.RotateTowards(from, to, maxDegreesDelta * num3);
	}

	public static void IncrementOrSet<T>(this Dictionary<T, int> dict, T key, int amountToAdd = 1)
	{
		if (dict.TryGetValue(key, out var value))
		{
			dict[key] = value + amountToAdd;
		}
		else
		{
			dict[key] = amountToAdd;
		}
	}

	public static void IncrementOrSet<T>(this Dictionary<T, float> dict, T key, float amountToAdd = 1f)
	{
		if (dict.TryGetValue(key, out var value))
		{
			dict[key] = value + amountToAdd;
		}
		else
		{
			dict[key] = amountToAdd;
		}
	}

	public static void Write(this BinaryWriter bw, Vector3 value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		bw.Write(value.x);
		bw.Write(value.y);
		bw.Write(value.z);
	}

	public static Vector3 ReadVector3(this BinaryReader bw)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(bw.ReadSingle(), bw.ReadSingle(), bw.ReadSingle());
	}

	public static void Write(this BinaryWriter bw, Quaternion value)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		bw.Write(((Quaternion)(ref value)).eulerAngles);
	}

	public static Quaternion ReadQuaternion(this BinaryReader bw)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(bw.ReadVector3());
	}

	public static bool SignDiffers(float a, float b)
	{
		return (IntFloat.Convert(a) & 0x80000000u) != (IntFloat.Convert(b) & 0x80000000u);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Sign(float f)
	{
		return IntFloat.Convert(0x3F800000u | (IntFloat.Convert(f) & 0x80000000u));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Abs(float f)
	{
		return IntFloat.Convert(IntFloat.Convert(f) & 0x7FFFFFFFu);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FloorToInt(float f)
	{
		return (int)(f + 64000f) - 64000;
	}

	public static int Clamp(int num, int min, int max)
	{
		return Math.Max(min, Math.Min(num, max));
	}

	public static float Clamp(float num, float min, float max)
	{
		if (num > max)
		{
			return max;
		}
		if (num < min)
		{
			return min;
		}
		return num;
	}

	public static float Clamp01(float num)
	{
		if (num > 1f)
		{
			return 1f;
		}
		if (num < 0f)
		{
			return 0f;
		}
		return num;
	}

	public static float Clamp0(float num)
	{
		if (!(num < 0f))
		{
			return num;
		}
		return 0f;
	}

	public static float Clamp1(float num)
	{
		if (!(num > 1f))
		{
			return num;
		}
		return 1f;
	}

	public static short ClampToShort(this int num)
	{
		return (short)Math.Max(-32768, Math.Min(num, 32767));
	}

	public static Vector2s ClampToShort(this Vector2i vec)
	{
		short x = (short)Math.Max(-32768, Math.Min(vec.x, 32767));
		short y = (short)Math.Max(-32768, Math.Min(vec.y, 32767));
		return new Vector2s(x, y);
	}

	public static int Mod(int value, int dividend)
	{
		value %= dividend;
		if (value < 0)
		{
			value += dividend;
		}
		return value;
	}

	public static bool CustomEndsWith(this string a, string b)
	{
		int num = a.Length - 1;
		int num2 = b.Length - 1;
		while (num >= 0 && num2 >= 0 && a[num] == b[num2])
		{
			num--;
			num2--;
		}
		return num2 < 0;
	}

	public static bool CustomStartsWith(this string a, string b)
	{
		int length = a.Length;
		int length2 = b.Length;
		int num = 0;
		int num2 = 0;
		while (num < length && num2 < length2 && a[num] == b[num2])
		{
			num++;
			num2++;
		}
		return num2 == length2;
	}

	public static float BlendOverlay(float a, float b)
	{
		float result = 2f * a * b;
		float result2 = 1f - 2f * (1f - a) * (1f - b);
		if (!((double)a < 0.5))
		{
			return result2;
		}
		return result;
	}

	public static Vector3 Vec3(float value)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(value, value, value);
	}

	public static Vector3 RandomVector3(float min, float max)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Random.RandomRange(min, max), Random.RandomRange(min, max), Random.RandomRange(min, max));
	}

	public static Vector3 WorldToScreenPointScaled(this Camera camera, Vector3 worldPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = camera.WorldToScreenPoint(worldPos);
		result.x *= (float)Screen.width / (float)camera.pixelWidth;
		result.y *= (float)Screen.height / (float)camera.pixelHeight;
		return result;
	}

	public static T[] GetEnabledComponentsInChildren<T>(GameObject root) where T : Component
	{
		T[] componentsInChildren = root.GetComponentsInChildren<T>();
		List<T> list = new List<T>(componentsInChildren.Length);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!((Object)(object)((Component)componentsInChildren[i]).transform == (Object)(object)root.transform) && IsEnabledInheirarcy(((Component)componentsInChildren[i]).gameObject, root))
			{
				list.Add(componentsInChildren[i]);
			}
		}
		return list.ToArray();
	}

	public static void Shuffle<T>(this T[] array)
	{
		Random random = new Random();
		for (int i = 0; i < array.Length; i++)
		{
			int num = random.Next(i, array.Length);
			if (num != i)
			{
				T val = array[num];
				Array.Copy(array, i, array, i + 1, num - i);
				array[i] = val;
			}
		}
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		Random random = new Random();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			int num2 = random.Next(0, num + 1);
			if (num2 != num)
			{
				T item = list[num2];
				list.RemoveAt(num2);
				list.Insert(num, item);
			}
		}
	}

	public static void InsertSortNoAlloc<TType>(List<TType> collection, TType item, List<float> distances, float distance)
	{
		int num = distances.BinarySearch(distance);
		if (num < 0)
		{
			num = ~num;
		}
		collection.Insert(num, item);
		distances.Insert(num, distance);
	}

	public static string ToGlobalInvariantString(this float value)
	{
		return value.ToString(CultureInfo.InvariantCulture.NumberFormat);
	}
}
