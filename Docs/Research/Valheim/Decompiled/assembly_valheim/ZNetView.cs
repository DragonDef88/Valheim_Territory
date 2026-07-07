using System;
using System.Collections.Generic;
using System.Reflection;
using SoftReferenceableAssets;
using UnityEngine;

public class ZNetView : MonoBehaviour, IReferenceHolder
{
	public const string CustomFieldsStr = "HasFields";

	public static long Everybody = 0L;

	public bool m_persistent;

	public bool m_distant;

	public ZDO.ObjectType m_type;

	public bool m_syncInitialScale;

	public static bool m_useInitZDO = false;

	public static ZDO m_initZDO = null;

	public static bool m_forceDisableInit = false;

	private ZDO m_zdo;

	private Rigidbody m_body;

	private Dictionary<int, RoutedMethodBase> m_functions = new Dictionary<int, RoutedMethodBase>();

	private bool m_ghost;

	private static List<MonoBehaviour> m_tempComponents = new List<MonoBehaviour>();

	private static bool m_ghostInit = false;

	private List<IReferenceCounted> m_heldReferences;

	private void Awake()
	{
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		if (m_forceDisableInit || ZDOMan.instance == null)
		{
			Object.Destroy((Object)(object)this);
			return;
		}
		m_body = ((Component)this).GetComponent<Rigidbody>();
		if (m_useInitZDO && m_initZDO == null)
		{
			ZLog.LogWarning((object)("Double ZNetview when initializing object " + ((Object)((Component)this).gameObject).name));
		}
		if (m_initZDO != null)
		{
			m_zdo = m_initZDO;
			m_initZDO = null;
			if (m_zdo.Type != m_type && m_zdo.IsOwner())
			{
				m_zdo.SetType(m_type);
			}
			if (m_zdo.Distant != m_distant && m_zdo.IsOwner())
			{
				m_zdo.SetDistant(m_distant);
			}
			if (m_syncInitialScale)
			{
				Vector3 vec = m_zdo.GetVec3(ZDOVars.s_scaleHash, Vector3.zero);
				if (vec != Vector3.zero)
				{
					((Component)this).transform.localScale = vec;
				}
				else
				{
					float @float = m_zdo.GetFloat(ZDOVars.s_scaleScalarHash, ((Component)this).transform.localScale.x);
					if (!((Component)this).transform.localScale.x.Equals(@float))
					{
						((Component)this).transform.localScale = new Vector3(@float, @float, @float);
					}
				}
			}
			if (Object.op_Implicit((Object)(object)m_body))
			{
				m_body.Sleep();
			}
		}
		else
		{
			string prefabName = GetPrefabName();
			m_zdo = ZDOMan.instance.CreateNewZDO(((Component)this).transform.position, StringExtensionMethods.GetStableHashCode(prefabName));
			m_zdo.Persistent = m_persistent;
			m_zdo.Type = m_type;
			m_zdo.Distant = m_distant;
			m_zdo.SetPrefab(StringExtensionMethods.GetStableHashCode(prefabName));
			m_zdo.SetRotation(((Component)this).transform.rotation);
			if (m_syncInitialScale)
			{
				SyncScale(skipOne: true);
			}
			if (m_ghostInit)
			{
				m_ghost = true;
				return;
			}
		}
		LoadFields();
		ZNetScene.instance.AddInstance(m_zdo, this);
	}

	public void SetLocalScale(Vector3 scale)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!(((Component)this).transform.localScale == scale))
		{
			((Component)this).transform.localScale = scale;
			if (m_zdo != null && m_syncInitialScale && IsOwner())
			{
				SyncScale();
			}
		}
	}

	private void SyncScale(bool skipOne = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Approximately(((Component)this).transform.localScale.x, ((Component)this).transform.localScale.y) && Mathf.Approximately(((Component)this).transform.localScale.x, ((Component)this).transform.localScale.z))
		{
			if (!skipOne || !Mathf.Approximately(((Component)this).transform.localScale.x, 1f))
			{
				m_zdo.Set(ZDOVars.s_scaleScalarHash, ((Component)this).transform.localScale.x);
			}
		}
		else
		{
			m_zdo.Set(ZDOVars.s_scaleHash, ((Component)this).transform.localScale);
		}
	}

	private void OnDestroy()
	{
		Object.op_Implicit((Object)(object)ZNetScene.instance);
		if (m_heldReferences != null)
		{
			for (int i = 0; i < m_heldReferences.Count; i++)
			{
				m_heldReferences[i].Release();
			}
			m_heldReferences.Clear();
		}
	}

	public void LoadFields()
	{
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		ZDO zDO = GetZDO();
		if (!zDO.GetBool("HasFields"))
		{
			return;
		}
		((Component)this).gameObject.GetComponentsInChildren<MonoBehaviour>(m_tempComponents);
		foreach (MonoBehaviour tempComponent in m_tempComponents)
		{
			string name = ((object)tempComponent).GetType().Name;
			StringExtensionMethods.GetStableHashCode("HasFields" + name);
			if (!zDO.GetBool("HasFields" + name))
			{
				continue;
			}
			FieldInfo[] fields = ((object)tempComponent).GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				name = ((object)tempComponent).GetType().Name + "." + fieldInfo.Name;
				float value2;
				bool value3;
				Vector3 value4;
				string value5;
				string value6;
				if (fieldInfo.FieldType == typeof(int) && zDO.GetInt(name, out var value))
				{
					fieldInfo.SetValue(tempComponent, value);
				}
				else if (fieldInfo.FieldType == typeof(float) && zDO.GetFloat(name, out value2))
				{
					fieldInfo.SetValue(tempComponent, value2);
				}
				else if (fieldInfo.FieldType == typeof(bool) && zDO.GetBool(name, out value3))
				{
					fieldInfo.SetValue(tempComponent, value3);
				}
				else if (fieldInfo.FieldType == typeof(Vector3) && zDO.GetVec3(name, out value4))
				{
					fieldInfo.SetValue(tempComponent, value4);
				}
				else if (fieldInfo.FieldType == typeof(string) && zDO.GetString(name, out value5))
				{
					fieldInfo.SetValue(tempComponent, value5);
				}
				else if (fieldInfo.FieldType == typeof(GameObject) && zDO.GetString(name, out value6))
				{
					GameObject prefab = ZNetScene.instance.GetPrefab(value6);
					if (prefab != null)
					{
						fieldInfo.SetValue(tempComponent, prefab);
					}
				}
				else
				{
					if (!(fieldInfo.FieldType == typeof(ItemDrop)) || !zDO.GetString(name, out var value7))
					{
						continue;
					}
					GameObject prefab2 = ZNetScene.instance.GetPrefab(value7);
					if (prefab2 != null)
					{
						ItemDrop component = prefab2.GetComponent<ItemDrop>();
						if (component != null)
						{
							fieldInfo.SetValue(tempComponent, component);
						}
					}
				}
			}
		}
	}

	private string GetPrefabName()
	{
		return Utils.GetPrefabName(((Component)this).gameObject);
	}

	public void Destroy()
	{
		ZNetScene.instance.Destroy(((Component)this).gameObject);
	}

	public bool IsOwner()
	{
		if (!IsValid())
		{
			return false;
		}
		return m_zdo.IsOwner();
	}

	public bool HasOwner()
	{
		if (!IsValid())
		{
			return false;
		}
		return m_zdo.HasOwner();
	}

	public void ClaimOwnership()
	{
		if (!IsOwner())
		{
			m_zdo.SetOwner(ZDOMan.GetSessionID());
		}
	}

	public ZDO GetZDO()
	{
		return m_zdo;
	}

	public bool IsValid()
	{
		if (m_zdo != null)
		{
			return m_zdo.IsValid();
		}
		return false;
	}

	public void ResetZDO()
	{
		m_zdo.Created = false;
		m_zdo = null;
	}

	public void Register(string name, Action<long> f)
	{
		m_functions.Add(StringExtensionMethods.GetStableHashCode(name), new RoutedMethod(f));
	}

	public void Register<T>(string name, Action<long, T> f)
	{
		m_functions.Add(StringExtensionMethods.GetStableHashCode(name), new RoutedMethod<T>(f));
	}

	public void Register<T, U>(string name, Action<long, T, U> f)
	{
		m_functions.Add(StringExtensionMethods.GetStableHashCode(name), new RoutedMethod<T, U>(f));
	}

	public void Register<T, U, V>(string name, Action<long, T, U, V> f)
	{
		m_functions.Add(StringExtensionMethods.GetStableHashCode(name), new RoutedMethod<T, U, V>(f));
	}

	public void Register<T, U, V, B>(string name, RoutedMethod<T, U, V, B>.Method f)
	{
		m_functions.Add(StringExtensionMethods.GetStableHashCode(name), new RoutedMethod<T, U, V, B>(f));
	}

	public void Unregister(string name)
	{
		int stableHashCode = StringExtensionMethods.GetStableHashCode(name);
		m_functions.Remove(stableHashCode);
	}

	public void HandleRoutedRPC(ZRoutedRpc.RoutedRPCData rpcData)
	{
		if (m_functions.TryGetValue(rpcData.m_methodHash, out var value))
		{
			value.Invoke(rpcData.m_senderPeerID, rpcData.m_parameters);
		}
		else
		{
			ZLog.LogWarning((object)("Failed to find rpc method " + rpcData.m_methodHash));
		}
	}

	public void InvokeRPC(long targetID, string method, params object[] parameters)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(targetID, m_zdo.m_uid, method, parameters);
	}

	public void InvokeRPC(string method, params object[] parameters)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(m_zdo.GetOwner(), m_zdo.m_uid, method, parameters);
	}

	public static object[] Deserialize(long callerID, ParameterInfo[] paramInfo, ZPackage pkg)
	{
		List<object> parameters = new List<object>();
		parameters.Add(callerID);
		ZRpc.Deserialize(paramInfo, pkg, ref parameters);
		return parameters.ToArray();
	}

	public static void StartGhostInit()
	{
		m_ghostInit = true;
	}

	public static void FinishGhostInit()
	{
		m_ghostInit = false;
	}

	public void HoldReferenceTo(IReferenceCounted reference)
	{
		if (m_heldReferences == null)
		{
			m_heldReferences = new List<IReferenceCounted>(1);
		}
		reference.HoldReference();
		m_heldReferences.Add(reference);
	}

	public void ReleaseReferenceTo(IReferenceCounted reference)
	{
		if (m_heldReferences != null && m_heldReferences.Remove(reference))
		{
			reference.Release();
		}
	}
}
