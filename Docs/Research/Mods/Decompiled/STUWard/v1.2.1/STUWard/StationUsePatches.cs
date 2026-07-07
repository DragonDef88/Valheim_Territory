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
internal static class StationUsePatches
{
	[CompilerGenerated]
	private sealed class _003CTargetMethods_003Ed__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

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
		public _003CTargetMethods_003Ed__0(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = AccessTools.DeclaredMethod(typeof(ArmorStand), "UseItem", (Type[])null, (Type[])null);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = AccessTools.DeclaredMethod(typeof(MapTable), "OnRead", new Type[3]
				{
					typeof(Switch),
					typeof(Humanoid),
					typeof(ItemData)
				}, (Type[])null);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = AccessTools.DeclaredMethod(typeof(MapTable), "OnRead", new Type[4]
				{
					typeof(Switch),
					typeof(Humanoid),
					typeof(ItemData),
					typeof(bool)
				}, (Type[])null);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = AccessTools.DeclaredMethod(typeof(MapTable), "OnWrite", (Type[])null, (Type[])null);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				return false;
			}
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
			return new _003CTargetMethods_003Ed__0(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[IteratorStateMachine(typeof(_003CTargetMethods_003Ed__0))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTargetMethods_003Ed__0(-2);
	}

	private static bool Prefix(Component __instance, Humanoid __1, ref bool __result, out WardCheckScopeState __state)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		__state = default(WardCheckScopeState);
		Player player = WardAccess.GetPlayer(__1);
		WardRestrictionOptions restriction;
		bool flag = (WardInteractionPatchTargets.TryGetRestriction(__instance, out restriction) ? WardAccess.TryBlockInteraction(restriction, __instance, player, ref __result) : WardAccess.TryBlockInteraction(__instance, player, ref __result));
		if (flag && restriction != 0)
		{
			__state.EnterRestriction(restriction);
		}
		if (Plugin.ShouldLogWardDiagnosticVerbose() && (Object)(object)__instance != (Object)null && (Object)(object)player != (Object)null)
		{
			Plugin.LogWardDiagnosticVerbose("Access.StationUse", $"Intercepted station interaction. targetType={((object)__instance).GetType().Name}, targetName='{((Object)__instance).name}', playerId={player.GetPlayerID()}, continueOriginal={flag}, result={__result}, position={__instance.transform.position}");
		}
		return flag;
	}

	private static void Postfix(WardCheckScopeState __state)
	{
		__state.Dispose();
	}
}
