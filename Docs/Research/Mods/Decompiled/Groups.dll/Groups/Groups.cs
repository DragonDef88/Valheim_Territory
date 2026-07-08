using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using APIManager;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using ServerSync;
using UnityEngine;

namespace Groups;

[BepInPlugin("org.bepinex.plugins.groups", "Groups", "1.2.10")]
[BepInIncompatibility("org.bepinex.plugins.valheim_plus")]
public class Groups : BaseUnityPlugin
{
	public enum Toggle
	{
		On = 1,
		Off = 0
	}

	public enum GroupLeaderDisplayOption
	{
		Disabled,
		Icon,
		Color
	}

	public enum BlockInvitation
	{
		Never,
		Always,
		[Description("While PvP enabled")]
		PvP,
		[Description("While Enemy Player Nearby")]
		Enemy
	}

	private class ConfigurationManagerAttributes
	{
		[UsedImplicitly]
		public int? Order;

		[UsedImplicitly]
		public bool? Browsable;
	}

	[HarmonyPatch(typeof(Aoe), "OnHit")]
	private static class TagFriendlyFireAoe
	{
		[CompilerGenerated]
		private sealed class _003CTranspiler_003Ed__3 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IDisposable, IEnumerator
		{
			private int _003C_003E1__state;

			private CodeInstruction _003C_003E2__current;

			private int _003C_003El__initialThreadId;

			private IEnumerable<CodeInstruction> instructions;

			public IEnumerable<CodeInstruction> _003C_003E3__instructions;

			private bool _003CpreDamage_003E5__2;

			private IEnumerator<CodeInstruction> _003C_003E7__wrap2;

			private CodeInstruction _003Cinstruction_003E5__4;

			CodeInstruction IEnumerator<CodeInstruction>.Current
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
			public _003CTranspiler_003Ed__3(int _003C_003E1__state)
			{
				this._003C_003E1__state = _003C_003E1__state;
				_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				int num = _003C_003E1__state;
				if (num == -3 || (uint)(num - 1) <= 2u)
				{
					try
					{
					}
					finally
					{
						_003C_003Em__Finally1();
					}
				}
				_003C_003E7__wrap2 = null;
				_003Cinstruction_003E5__4 = null;
				_003C_003E1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cf: Expected O, but got Unknown
				//IL_0089: Unknown result type (might be due to invalid IL or missing references)
				//IL_0093: Expected O, but got Unknown
				try
				{
					switch (_003C_003E1__state)
					{
					default:
						return false;
					case 0:
						_003C_003E1__state = -1;
						_003CpreDamage_003E5__2 = false;
						_003C_003E7__wrap2 = instructions.GetEnumerator();
						_003C_003E1__state = -3;
						goto IL_0123;
					case 1:
						_003C_003E1__state = -3;
						_003C_003E2__current = new CodeInstruction(OpCodes.Call, (object)AccessTools.DeclaredMethod(typeof(TagFriendlyFireAoe), "CheckAndTag", (Type[])null, (Type[])null));
						_003C_003E1__state = 2;
						return true;
					case 2:
						_003C_003E1__state = -3;
						goto IL_00fd;
					case 3:
						{
							_003C_003E1__state = -3;
							_003Cinstruction_003E5__4 = null;
							goto IL_0123;
						}
						IL_0123:
						if (_003C_003E7__wrap2.MoveNext())
						{
							_003Cinstruction_003E5__4 = _003C_003E7__wrap2.Current;
							if (_003CpreDamage_003E5__2 && CodeInstructionExtensions.Calls(_003Cinstruction_003E5__4, Damage))
							{
								_003CpreDamage_003E5__2 = false;
								_003C_003E2__current = new CodeInstruction(OpCodes.Ldarg_0, (object)null);
								_003C_003E1__state = 1;
								return true;
							}
							if (CodeInstructionExtensions.Calls(_003Cinstruction_003E5__4, ModifyHit))
							{
								_003CpreDamage_003E5__2 = true;
							}
							goto IL_00fd;
						}
						_003C_003Em__Finally1();
						_003C_003E7__wrap2 = null;
						return false;
						IL_00fd:
						_003C_003E2__current = _003Cinstruction_003E5__4;
						_003C_003E1__state = 3;
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
				if (_003C_003E7__wrap2 != null)
				{
					_003C_003E7__wrap2.Dispose();
				}
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<CodeInstruction> IEnumerable<CodeInstruction>.GetEnumerator()
			{
				_003CTranspiler_003Ed__3 _003CTranspiler_003Ed__;
				if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
				{
					_003C_003E1__state = 0;
					_003CTranspiler_003Ed__ = this;
				}
				else
				{
					_003CTranspiler_003Ed__ = new _003CTranspiler_003Ed__3(0);
				}
				_003CTranspiler_003Ed__.instructions = _003C_003E3__instructions;
				return _003CTranspiler_003Ed__;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<CodeInstruction>)this).GetEnumerator();
			}
		}

		private static readonly MethodInfo ModifyHit = AccessTools.DeclaredMethod(typeof(DamageTypes), "Modify", new Type[1] { typeof(float) }, (Type[])null);

		private static readonly MethodInfo Damage = AccessTools.DeclaredMethod(typeof(IDestructible), "Damage", (Type[])null, (Type[])null);

		private static HitData CheckAndTag(HitData hit, Aoe aoe)
		{
			if (aoe.m_hitFriendly && Object.op_Implicit((Object)(object)Projectile.FindHitObject(hit.m_hitCollider).GetComponent<Player>()))
			{
				hit.m_weakSpot = -23749;
			}
			return hit;
		}

		[IteratorStateMachine(typeof(_003CTranspiler_003Ed__3))]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003CTranspiler_003Ed__3(-2)
			{
				_003C_003E3__instructions = instructions
			};
		}
	}

	[HarmonyPatch(typeof(Character), "RPC_Damage")]
	public class FriendlyFirePatch
	{
		private static bool Prefix(Character __instance, HitData hit)
		{
			if ((Object)(object)__instance == (Object)(object)Player.m_localPlayer)
			{
				Character attacker = hit.GetAttacker();
				Player val = (Player)(object)((attacker is Player) ? attacker : null);
				if (val != null && hit.m_weakSpot != -23749 && friendlyFire.Value == Toggle.Off && ownGroup != null && ownGroup.playerStates.ContainsKey(PlayerReference.fromPlayer(val)))
				{
					return false;
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Character), "Damage")]
	private static class PreventFriendlyFireMarkerOverwrite
	{
		[CompilerGenerated]
		private sealed class _003CTranspiler_003Ed__2 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IDisposable, IEnumerator
		{
			private int _003C_003E1__state;

			private CodeInstruction _003C_003E2__current;

			private int _003C_003El__initialThreadId;

			private IEnumerable<CodeInstruction> instructions;

			public IEnumerable<CodeInstruction> _003C_003E3__instructions;

			private IEnumerator<CodeInstruction> _003C_003E7__wrap1;

			CodeInstruction IEnumerator<CodeInstruction>.Current
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
			public _003CTranspiler_003Ed__2(int _003C_003E1__state)
			{
				this._003C_003E1__state = _003C_003E1__state;
				_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				int num = _003C_003E1__state;
				if (num == -3 || (uint)(num - 1) <= 1u)
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
				//IL_0077: Unknown result type (might be due to invalid IL or missing references)
				//IL_0081: Expected O, but got Unknown
				try
				{
					switch (_003C_003E1__state)
					{
					default:
						return false;
					case 0:
						_003C_003E1__state = -1;
						_003C_003E7__wrap1 = instructions.GetEnumerator();
						_003C_003E1__state = -3;
						break;
					case 1:
						_003C_003E1__state = -3;
						break;
					case 2:
						_003C_003E1__state = -3;
						break;
					}
					if (_003C_003E7__wrap1.MoveNext())
					{
						CodeInstruction current = _003C_003E7__wrap1.Current;
						if (CodeInstructionExtensions.StoresField(current, weakSpotField))
						{
							_003C_003E2__current = new CodeInstruction(OpCodes.Call, (object)AccessTools.DeclaredMethod(typeof(PreventFriendlyFireMarkerOverwrite), "WriteWeakSpot", (Type[])null, (Type[])null));
							_003C_003E1__state = 1;
							return true;
						}
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
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
			IEnumerator<CodeInstruction> IEnumerable<CodeInstruction>.GetEnumerator()
			{
				_003CTranspiler_003Ed__2 _003CTranspiler_003Ed__;
				if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
				{
					_003C_003E1__state = 0;
					_003CTranspiler_003Ed__ = this;
				}
				else
				{
					_003CTranspiler_003Ed__ = new _003CTranspiler_003Ed__2(0);
				}
				_003CTranspiler_003Ed__.instructions = _003C_003E3__instructions;
				return _003CTranspiler_003Ed__;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<CodeInstruction>)this).GetEnumerator();
			}
		}

		private static readonly FieldInfo weakSpotField = AccessTools.DeclaredField(typeof(HitData), "m_weakSpot");

		private static void WriteWeakSpot(HitData hit, short index)
		{
			if (hit.m_weakSpot >= -1)
			{
				hit.m_weakSpot = index;
			}
		}

		[IteratorStateMachine(typeof(_003CTranspiler_003Ed__2))]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003CTranspiler_003Ed__2(-2)
			{
				_003C_003E3__instructions = instructions
			};
		}
	}

	[HarmonyPatch(typeof(Game), "Shutdown")]
	public class LeaveGroupOnLogout
	{
		private static void Postfix()
		{
			ownGroup = null;
		}
	}

	private const string ModName = "Groups";

	private const string ModVersion = "1.2.10";

	private const string ModGUID = "org.bepinex.plugins.groups";

	public static Group? ownGroup;

	private static ConfigEntry<Toggle> serverConfigLocked = null;

	public static ConfigEntry<int> maximumGroupSize = null;

	public static ConfigEntry<Toggle> friendlyFire = null;

	public static ConfigEntry<Color> friendlyNameColor = null;

	public static ConfigEntry<string> ignoreList = null;

	public static ConfigEntry<Color> groupChatColor = null;

	public static ConfigEntry<Vector2> groupInterfaceAnchor = null;

	public static ConfigEntry<KeyboardShortcut> groupPingHotkey = null;

	public static ConfigEntry<Toggle> horizontalGroupInterface = null;

	public static ConfigEntry<GroupLeaderDisplayOption> groupLeaderDisplay = null;

	public static ConfigEntry<Color> groupLeaderColor = null;

	public static ConfigEntry<float> spaceBetweenGroupMembers = null;

	public static ConfigEntry<BlockInvitation> blockInvitations = null;

	private static int configOrder = 0;

	private static readonly ConfigSync configSync = new ConfigSync("Groups")
	{
		CurrentVersion = "1.2.10",
		MinimumRequiredVersion = "1.2.10"
	};

	private const short IsFriendlyAoe = -23749;

	private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		ConfigEntry<T> val = ((BaseUnityPlugin)this).Config.Bind<T>(group, name, value, new ConfigDescription(description.Description, description.AcceptableValues, (description.Tags.Length != 0) ? description.Tags : new object[1]
		{
			new ConfigurationManagerAttributes
			{
				Order = configOrder--
			}
		}));
		configSync.AddConfigEntry<T>(val).SynchronizedConfig = synchronizedSetting;
		return val;
	}

	private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		return config(group, name, value, new ConfigDescription(description, (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting);
	}

	public void Awake()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Expected O, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Expected O, but got Unknown
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Expected O, but got Unknown
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Expected O, but got Unknown
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Expected O, but got Unknown
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Expected O, but got Unknown
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Expected O, but got Unknown
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Expected O, but got Unknown
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Expected O, but got Unknown
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		Patcher.Patch();
		Localizer.Load();
		Type configManagerType = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "ConfigurationManager")?.GetType("ConfigurationManager.ConfigurationManager");
		object configManager = ((configManagerType == null) ? null : Chainloader.ManagerObject.GetComponent(configManagerType));
		serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, new ConfigDescription("If on, only server admins can change the configuration.", (AcceptableValueBase)null, Array.Empty<object>()));
		configSync.AddLockingConfigEntry<Toggle>(serverConfigLocked);
		maximumGroupSize = config("1 - General", "Maximum size for groups", 5, new ConfigDescription("Maximum size for groups.", (AcceptableValueBase)(object)new AcceptableValueRange<int>(2, 10), Array.Empty<object>()));
		friendlyFire = config("1 - General", "Friendly fire in groups", Toggle.Off, new ConfigDescription("If members of the same group can damage each other in PvP.", (AcceptableValueBase)null, Array.Empty<object>()));
		groupChatColor = config<Color>("2 - Display", "Color of the group chat", new Color(0f, 1f, 0f), new ConfigDescription("The color for messages in your group.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		friendlyNameColor = config<Color>("2 - Display", "Name color for group members", new Color(0f, 1f, 0f), new ConfigDescription("The color for names of members of the own group, if you see them in the world.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		friendlyNameColor.SettingChanged += delegate
		{
			Map.UpdateMapPinColor();
		};
		groupInterfaceAnchor = config<Vector2>("2 - Display", "Position of the group interface", new Vector2(-875f, 310f), new ConfigDescription("Sets the anchor position of the group interface.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		groupInterfaceAnchor.SettingChanged += Interface.AnchorGroupInterface;
		horizontalGroupInterface = config("2 - Display", "Horizontal group interface", Toggle.Off, new ConfigDescription("Aligns the group interface horizontally, instead of vertically.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		horizontalGroupInterface.SettingChanged += Interface.UpdateGroupInterfaceSpacing;
		groupLeaderDisplay = config("2 - Display", "Group leader display", GroupLeaderDisplayOption.Icon, new ConfigDescription("How the leader of the group is displayed.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		ConfigurationManagerAttributes colorDisplay = new ConfigurationManagerAttributes
		{
			Order = (configOrder -= 1),
			Browsable = (groupLeaderDisplay.Value == GroupLeaderDisplayOption.Color)
		};
		groupLeaderDisplay.SettingChanged += delegate
		{
			colorDisplay.Browsable = groupLeaderDisplay.Value == GroupLeaderDisplayOption.Color;
			reloadConfigDisplay();
		};
		groupLeaderColor = config<Color>("2 - Display", "Group leader color", new Color(0.6f, 0.6f, 0.2f), new ConfigDescription("Color of the group leader, if using the group leader color display option.", (AcceptableValueBase)null, new object[1] { colorDisplay }), synchronizedSetting: false);
		spaceBetweenGroupMembers = config("2 - Display", "Space between group members", 75f, new ConfigDescription("The space between group members in the group display.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		spaceBetweenGroupMembers.SettingChanged += Interface.UpdateGroupInterfaceSpacing;
		groupPingHotkey = config<KeyboardShortcut>("3 - Other", "Group ping modifier key", new KeyboardShortcut((KeyCode)308, Array.Empty<KeyCode>()), new ConfigDescription("Modifier key that has to be pressed while pinging the map, to make the map ping visible to group members only.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		ignoreList = config("3 - Other", "Names of people who cannot invite you", "", new ConfigDescription("Ignore group invitations from people on this list. Comma separated.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		blockInvitations = config("3 - Other", "Block all invitations", BlockInvitation.Never, new ConfigDescription("Can be used to block all invitations. Optionally, only block invitations while PvP is enabled.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		new Harmony("org.bepinex.plugins.groups").PatchAll(executingAssembly);
		Interface.Init();
		Map.Init();
		((MonoBehaviour)this).InvokeRepeating("updatePositon", 0f, 2f);
		void reloadConfigDisplay()
		{
			configManagerType?.GetMethod("BuildSettingList").Invoke(configManager, Array.Empty<object>());
		}
	}

	private void updatePositon()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null || ownGroup == null || ZNet.instance.m_publicReferencePosition)
		{
			return;
		}
		foreach (PlayerReference item in ownGroup.playerStates.Keys.Where((PlayerReference r) => r.peerId != ZDOMan.GetSessionID()))
		{
			ZRoutedRpc.instance.InvokeRoutedRPC(item.peerId, "Groups UpdatePosition", new object[1] { ((Component)localPlayer).transform.position });
		}
	}
}
