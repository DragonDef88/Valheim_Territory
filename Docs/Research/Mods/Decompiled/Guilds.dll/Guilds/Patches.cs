using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

public static class Patches
{
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
				if (val != null && hit.m_weakSpot != -23749)
				{
					Guild ownGuild = API.GetOwnGuild();
					if (ownGuild != null && ownGuild.Name == API.GetPlayerGuild(val)?.Name && Guilds.friendlyFire.Value == Toggle.Off)
					{
						return false;
					}
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

	[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
	public class DisplayGuildNameAbovePlayer
	{
		private static void Postfix(EnemyHud __instance, Character c, Dictionary<Character, HudData> ___m_huds)
		{
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			Player val = (Player)(object)((c is Player) ? c : null);
			if (val != null)
			{
				GameObject gui = ___m_huds[c].m_gui;
				TextMeshProUGUI component = ((Component)gui.transform.Find("Name")).GetComponent<TextMeshProUGUI>();
				Transform obj = gui.transform.Find("guildname");
				GameObject val2 = ((obj != null) ? ((Component)obj).gameObject : null);
				if (val2 == null)
				{
					val2 = new GameObject("guildname", new Type[1] { typeof(RectTransform) });
					val2.transform.SetParent(gui.transform, false);
					((RectTransform)val2.transform).sizeDelta = new Vector2(300f, 12f);
					TextMeshProUGUI obj2 = val2.AddComponent<TextMeshProUGUI>();
					((TMP_Text)obj2).font = ((TMP_Text)component).font;
					((TMP_Text)obj2).fontSize = 14f;
					((TMP_Text)obj2).alignment = ((TMP_Text)component).alignment;
					Outline obj3 = val2.AddComponent<Outline>();
					((Shadow)obj3).effectColor = Color.black;
					((Shadow)obj3).effectDistance = new Vector2(1f, -1f);
				}
				TextMeshProUGUI component2 = val2.GetComponent<TextMeshProUGUI>();
				Color color = ((Graphic)((Component)__instance.m_baseHudPlayer.transform.Find("Name")).GetComponent<TextMeshProUGUI>()).color;
				RectTransform component3 = ((Component)component).GetComponent<RectTransform>();
				Vector2 pivot = component3.pivot;
				Guild playerGuild = API.GetPlayerGuild(val);
				if (playerGuild != null)
				{
					Color val3 = default(Color);
					ColorUtility.TryParseHtmlString(playerGuild.General.color, ref val3);
					pivot.y = 0.2f;
					((TMP_Text)component2).text = "<" + playerGuild.Name + ((Guilds.displayGuildLevel.Value == Toggle.On) ? $" ({playerGuild.General.level})" : "") + ">";
					((Graphic)component2).color = ((Guilds.guildColors.Value == Toggle.On) ? val3 : color);
				}
				else
				{
					pivot.y = 0.5f;
					((TMP_Text)val2.GetComponent<TextMeshProUGUI>()).text = "";
				}
				component3.pivot = pivot;
			}
		}
	}

	[HarmonyPatch(typeof(FejdStartup), "Awake")]
	public static class FejdStartupFixMaterial
	{
		public static Material? originalMaterial;

		private static void Postfix()
		{
			AssetBundle[] array = Resources.FindObjectsOfTypeAll<AssetBundle>();
			foreach (AssetBundle val in array)
			{
				IEnumerable<Material> enumerable3;
				try
				{
					IEnumerable<Material> enumerable2;
					if (!val.isStreamedSceneAssetBundle)
					{
						IEnumerable<Material> enumerable = val.LoadAllAssets<Material>();
						enumerable2 = enumerable;
					}
					else
					{
						enumerable2 = from shader in ((IEnumerable<string>)val.GetAllAssetNames()).Select((Func<string, Material>)val.LoadAsset<Material>)
							where (Object)(object)shader != (Object)null
							select shader;
					}
					enumerable3 = enumerable2;
				}
				catch (Exception)
				{
					continue;
				}
				if (enumerable3 == null)
				{
					continue;
				}
				foreach (Material item in enumerable3)
				{
					if (Object.op_Implicit((Object)(object)item) && ((Object)item).name == "litpanel")
					{
						originalMaterial = item;
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(Hud), "Awake")]
	public static class AffixGuildMenu
	{
		private static void Prefix(Hud __instance)
		{
			Transform transform = __instance.m_rootObject.transform;
			Interface.NoGuildUIPrefab.SetActive(false);
			Interface.SearchGuildUIPrefab.SetActive(false);
			Interface.CreateGuildUIPrefab.SetActive(false);
			Interface.GuildManagementUIPrefab.SetActive(false);
			Interface.ApplicationsUIPrefab.SetActive(false);
			Interface.EditGuildUIPrefab.SetActive(false);
			Interface.AchievementUIPrefab.SetActive(false);
			Interface.AchievementPopupPrefab.SetActive(false);
			Interface.NoGuildUI = Object.Instantiate<GameObject>(Interface.NoGuildUIPrefab, transform, false);
			Interface.SearchGuildUI = Object.Instantiate<GameObject>(Interface.SearchGuildUIPrefab, transform, false);
			Interface.CreateGuildUI = Object.Instantiate<GameObject>(Interface.CreateGuildUIPrefab, transform, false);
			Interface.GuildManagementUI = Object.Instantiate<GameObject>(Interface.GuildManagementUIPrefab, transform, false);
			Interface.ApplicationsUI = Object.Instantiate<GameObject>(Interface.ApplicationsUIPrefab, transform, false);
			Interface.EditGuildUI = Object.Instantiate<GameObject>(Interface.EditGuildUIPrefab, transform, false);
			Interface.AchievementUI = Object.Instantiate<GameObject>(Interface.AchievementUIPrefab, transform, false);
			Interface.AchievementPopup = Object.Instantiate<GameObject>(Interface.AchievementPopupPrefab, transform, false);
			FixBkgMaterial(Interface.NoGuildUI);
			FixBkgMaterial(Interface.SearchGuildUI);
			FixBkgMaterial(Interface.CreateGuildUI);
			FixBkgMaterial(Interface.GuildManagementUI);
			FixBkgMaterial(Interface.ApplicationsUI);
			FixBkgMaterial(Interface.EditGuildUI);
			FixBkgMaterial(Interface.AchievementUI);
			FixBkgMaterial(Interface.AchievementPopup);
			((Component)UnifiedPopup.instance).transform.SetAsLastSibling();
		}

		private static void FixBkgMaterial(GameObject ui)
		{
			if (!((Object)(object)FejdStartupFixMaterial.originalMaterial == (Object)null))
			{
				Transform val = Utils.FindChild(ui.transform, "Background", (IterativeSearchType)0);
				if (val != null)
				{
					((Graphic)((Component)val).GetComponent<Image>()).material = FejdStartupFixMaterial.originalMaterial;
				}
				Transform val2 = Utils.FindChild(ui.transform, "BackgroundBack", (IterativeSearchType)0);
				if (val2 != null)
				{
					((Graphic)((Component)val2).GetComponent<Image>()).material = FejdStartupFixMaterial.originalMaterial;
				}
			}
		}
	}

	[HarmonyPatch]
	private class DisablePlayerInputInGuildMenu
	{
		private static IEnumerable<MethodInfo> TargetMethods()
		{
			return new MethodInfo[2]
			{
				AccessTools.DeclaredMethod(typeof(StoreGui), "IsVisible", (Type[])null, (Type[])null),
				AccessTools.DeclaredMethod(typeof(TextInput), "IsVisible", (Type[])null, (Type[])null)
			};
		}

		private static void Postfix(ref bool __result)
		{
			if (Interface.UIIsActive())
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(Menu), "Update")]
	internal class PreventMainMenu
	{
		public static bool AllowMainMenu = true;

		private static bool Prefix()
		{
			if (!Interface.UIIsActive())
			{
				return AllowMainMenu;
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(ZNet), "Disconnect")]
	private static class UpdateLastOnline
	{
		private static void Prefix(ZNet __instance, ZNetPeer peer)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			ZNetPeer peer2 = peer;
			if (!__instance.IsServer())
			{
				return;
			}
			PlayerInfo val = ((IEnumerable<PlayerInfo>)__instance.m_players).FirstOrDefault((Func<PlayerInfo, bool>)((PlayerInfo p) => p.m_characterID == peer2.m_characterID));
			if (val.m_characterID != ZDOID.None)
			{
				PlayerReference playerReference = PlayerReference.fromPlayerInfo(val);
				Guild playerGuild = API.GetPlayerGuild(playerReference);
				if (playerGuild != null)
				{
					playerGuild.Members[playerReference].lastOnline = DateTime.Now;
					API.SaveGuild(playerGuild);
				}
			}
		}
	}

	private const short IsFriendlyAoe = -23749;
}
