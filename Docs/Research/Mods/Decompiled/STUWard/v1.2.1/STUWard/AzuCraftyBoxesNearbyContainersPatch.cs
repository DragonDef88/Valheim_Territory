using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch]
internal static class AzuCraftyBoxesNearbyContainersPatch
{
	private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		internal static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

		public new bool Equals(object? x, object? y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	[CompilerGenerated]
	private sealed class _003CTargetMethods_003Ed__6 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerator<Type> _003C_003E7__wrap1;

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
		public _003CTargetMethods_003Ed__6(int _003C_003E1__state)
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
					if (ContainerInterfaceType == null)
					{
						return false;
					}
					_003C_003E7__wrap1 = GetLoadableTypes(ContainerInterfaceType.Assembly).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					Type current = _003C_003E7__wrap1.Current;
					if (!(current == null) && !current.IsAbstract && !current.IsInterface && ContainerInterfaceType.IsAssignableFrom(current))
					{
						MethodInfo methodInfo = AccessTools.DeclaredMethod(current, "ItemCount", new Type[1] { typeof(string) }, (Type[])null);
						if (methodInfo != null)
						{
							_003C_003E2__current = methodInfo;
							_003C_003E1__state = 1;
							return true;
						}
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
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
			return new _003CTargetMethods_003Ed__6(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	private const string AzuCraftyBoxesPluginGuid = "Azumatt.AzuCraftyBoxes";

	private static readonly Dictionary<Type, Func<object, Vector3>?> GetPositionDelegates = new Dictionary<Type, Func<object, Vector3>>();

	private static readonly Dictionary<object, bool> FrameBlockCache = new Dictionary<object, bool>(ReferenceEqualityComparer.Instance);

	private static readonly Type? ContainerInterfaceType = ResolveContainerInterfaceType();

	private static int _cachedFrame = -1;

	private static bool Prepare()
	{
		return ContainerInterfaceType != null;
	}

	[IteratorStateMachine(typeof(_003CTargetMethods_003Ed__6))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTargetMethods_003Ed__6(-2);
	}

	private static bool Prefix(object __instance, ref int __result)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null || !WardAccess.HasEnabledManagedWards())
		{
			return true;
		}
		ResetFrameCacheIfNeeded();
		if (FrameBlockCache.TryGetValue(__instance, out var value))
		{
			if (!value)
			{
				return true;
			}
			__result = 0;
			return false;
		}
		if (!TryGetContainerPosition(__instance, out var position))
		{
			FrameBlockCache[__instance] = false;
			return true;
		}
		value = WardAccess.ShouldBlockRestriction(WardRestrictionOptions.Containers, position, 0f, localPlayer, flash: false);
		FrameBlockCache[__instance] = value;
		if (!value)
		{
			return true;
		}
		__result = 0;
		return false;
	}

	private static Type? ResolveContainerInterfaceType()
	{
		if (!Chainloader.PluginInfos.TryGetValue("Azumatt.AzuCraftyBoxes", out var value))
		{
			return null;
		}
		return ((object)value.Instance)?.GetType().Assembly.GetType("AzuCraftyBoxes.IContainers.IContainer");
	}

	private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			if (ex.Types == null)
			{
				return Array.Empty<Type>();
			}
			List<Type> list = new List<Type>(ex.Types.Length);
			for (int i = 0; i < ex.Types.Length; i++)
			{
				if (ex.Types[i] != null)
				{
					list.Add(ex.Types[i]);
				}
			}
			return list;
		}
	}

	private static void ResetFrameCacheIfNeeded()
	{
		int frameCount = Time.frameCount;
		if (_cachedFrame != frameCount)
		{
			_cachedFrame = frameCount;
			FrameBlockCache.Clear();
		}
	}

	private static bool TryGetContainerPosition(object container, out Vector3 position)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Type type = container.GetType();
		if (!GetPositionDelegates.TryGetValue(type, out Func<object, Vector3> value))
		{
			value = CreateGetPositionDelegate(type);
			GetPositionDelegates[type] = value;
		}
		if (value == null)
		{
			position = default(Vector3);
			return false;
		}
		try
		{
			position = value(container);
			return true;
		}
		catch
		{
		}
		position = default(Vector3);
		return false;
	}

	private static Func<object, Vector3>? CreateGetPositionDelegate(Type type)
	{
		MethodInfo methodInfo = AccessTools.Method(type, "GetPosition", (Type[])null, (Type[])null);
		if (methodInfo == null || methodInfo.IsStatic || methodInfo.ReturnType != typeof(Vector3) || methodInfo.GetParameters().Length != 0)
		{
			return null;
		}
		try
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "target");
			return Expression.Lambda<Func<object, Vector3>>(Expression.Call(Expression.Convert(parameterExpression, type), methodInfo), new ParameterExpression[1] { parameterExpression }).Compile();
		}
		catch
		{
			return null;
		}
	}
}
