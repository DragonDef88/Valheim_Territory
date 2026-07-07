using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch]
internal static class TameableCollectorCollectorItemPatch
{
	[CompilerGenerated]
	private sealed class _003CTargetMethods_003Ed__2 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Type _003CcollectorItemType_003E5__2;

		MethodBase IEnumerator<MethodBase>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CTargetMethods_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CcollectorItemType_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			MethodInfo methodInfo2;
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				_003CcollectorItemType_003E5__2 = GetCollectorItemType();
				if (_003CcollectorItemType_003E5__2 == null)
				{
					return false;
				}
				MethodInfo methodInfo = AccessTools.Method(_003CcollectorItemType_003E5__2, "TryCatch", (Type[])null, (Type[])null);
				if (methodInfo != null)
				{
					_003C_003E2__current = methodInfo;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0070;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_0070;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_0070:
				methodInfo2 = AccessTools.Method(_003CcollectorItemType_003E5__2, "Summon", (Type[])null, (Type[])null);
				if (methodInfo2 != null)
				{
					_003C_003E2__current = methodInfo2;
					_003C_003E1__state = 2;
					return true;
				}
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CTargetMethods_003Ed__2(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	private static Type? GetCollectorItemType()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Type type = assemblies[i].GetType("TameableCollector.Mechanics+CollectorItem", throwOnError: false);
			if (type != null)
			{
				return type;
			}
		}
		return null;
	}

	private static bool Prepare()
	{
		return GetCollectorItemType() != null;
	}

	[IteratorStateMachine(typeof(_003CTargetMethods_003Ed__2))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTargetMethods_003Ed__2(-2);
	}

	[HarmonyPriority(800)]
	private static bool Prefix(object __instance, MethodBase __originalMethod)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return true;
		}
		ItemData val = null;
		try
		{
			val = Traverse.Create(__instance).Property("Item", (object[])null).GetValue<ItemData>();
		}
		catch
		{
			return true;
		}
		if (__originalMethod.Name == "TryCatch" && (Object)(object)localPlayer.m_hoveringCreature != (Object)null)
		{
			return WardAccess.TryBlockItemUse(localPlayer, val, ((Component)localPlayer.m_hoveringCreature).transform.position);
		}
		return WardAccess.TryBlockItemUse(localPlayer, val);
	}
}
