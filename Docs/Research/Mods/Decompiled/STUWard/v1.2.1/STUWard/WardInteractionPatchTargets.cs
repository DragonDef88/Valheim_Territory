using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

internal static class WardInteractionPatchTargets
{
	[CompilerGenerated]
	private sealed class _003CGetCommonTargets_003Ed__1 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string methodName;

		public string _003C_003E3__methodName;

		private int _003Cindex_003E5__2;

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
		public _003CGetCommonTargets_003Ed__1(int _003C_003E1__state)
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
				_003Cindex_003E5__2 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Cindex_003E5__2++;
				break;
			}
			if (_003Cindex_003E5__2 < CommonTargetTypes.Length)
			{
				_003C_003E2__current = RequireDeclaredMethod(CommonTargetTypes[_003Cindex_003E5__2], methodName);
				_003C_003E1__state = 1;
				return true;
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
			_003CGetCommonTargets_003Ed__1 _003CGetCommonTargets_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetCommonTargets_003Ed__ = this;
			}
			else
			{
				_003CGetCommonTargets_003Ed__ = new _003CGetCommonTargets_003Ed__1(0);
			}
			_003CGetCommonTargets_003Ed__.methodName = _003C_003E3__methodName;
			return _003CGetCommonTargets_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetDirectInteractTargets_003Ed__2 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerator<MethodBase> _003C_003E7__wrap1;

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
		public _003CGetDirectInteractTargets_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = GetCommonTargets("Interact").GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_006c;
				case 1:
					_003C_003E1__state = -3;
					goto IL_006c;
				case 2:
					{
						_003C_003E1__state = -1;
						return false;
					}
					IL_006c:
					if (_003C_003E7__wrap1.MoveNext())
					{
						MethodBase current = _003C_003E7__wrap1.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					_003C_003E2__current = RequireDeclaredMethod(typeof(ItemDrop), "Interact");
					_003C_003E1__state = 2;
					return true;
				}
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
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
			return new _003CGetDirectInteractTargets_003Ed__2(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	private static readonly Type[] CommonTargetTypes = new Type[16]
	{
		typeof(Container),
		typeof(Door),
		typeof(ShipControlls),
		typeof(Vagon),
		typeof(Sign),
		typeof(ItemStand),
		typeof(Beehive),
		typeof(CraftingStation),
		typeof(Fermenter),
		typeof(SapCollector),
		typeof(Trap),
		typeof(Tameable),
		typeof(Sadle),
		typeof(TeleportWorld),
		typeof(Feast),
		typeof(Pickable)
	};

	[IteratorStateMachine(typeof(_003CGetCommonTargets_003Ed__1))]
	internal static IEnumerable<MethodBase> GetCommonTargets(string methodName)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetCommonTargets_003Ed__1(-2)
		{
			_003C_003E3__methodName = methodName
		};
	}

	[IteratorStateMachine(typeof(_003CGetDirectInteractTargets_003Ed__2))]
	internal static IEnumerable<MethodBase> GetDirectInteractTargets()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetDirectInteractTargets_003Ed__2(-2);
	}

	internal static bool TryGetRestriction(Component? target, out WardRestrictionOptions restriction)
	{
		WardRestrictionOptions wardRestrictionOptions = ((target is Door) ? WardRestrictionOptions.Doors : ((target is TeleportWorld) ? WardRestrictionOptions.Portals : ((target is Feast) ? WardRestrictionOptions.PlacedConsumables : ((target is ItemStand) ? WardRestrictionOptions.ItemStands : ((target is ArmorStand) ? WardRestrictionOptions.ArmorStands : ((target is Container) ? WardRestrictionOptions.Containers : ((target is CraftingStation) ? WardRestrictionOptions.CraftingStations : ((target is Tameable) ? WardRestrictionOptions.TameablesAndSaddles : ((target is Sadle) ? WardRestrictionOptions.TameablesAndSaddles : WardRestrictionOptions.None)))))))));
		restriction = wardRestrictionOptions;
		return restriction != WardRestrictionOptions.None;
	}

	private static MethodBase RequireDeclaredMethod(Type type, string methodName)
	{
		MethodInfo methodInfo = AccessTools.DeclaredMethod(type, methodName, (Type[])null, (Type[])null);
		if (methodInfo == null)
		{
			throw new MissingMethodException(type.FullName, methodName);
		}
		return methodInfo;
	}
}
