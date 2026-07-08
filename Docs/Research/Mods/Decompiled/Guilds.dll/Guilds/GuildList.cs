using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;

namespace Guilds;

internal static class GuildList
{
	[HarmonyPatch(typeof(ZNet), "OnNewConnection")]
	private static class GuildListManipulationListener
	{
		[UsedImplicitly]
		private static void Postfix(ZNet __instance, ZNetPeer peer)
		{
			if (__instance.IsServer())
			{
				peer.m_rpc.Register<ZPackage>("Guild Create Guild", (Action<ZRpc, ZPackage>)AddGuild);
				peer.m_rpc.Register<ZPackage>("Guild Update Guild", (Action<ZRpc, ZPackage>)UpdateGuild);
				peer.m_rpc.Register<ZPackage>("Guild Remove Guild", (Action<ZRpc, ZPackage>)RemoveGuild);
				peer.m_rpc.Register<ZPackage>("Guild Rename Guild", (Action<ZRpc, ZPackage>)RenameGuild);
				peer.m_rpc.Register<ZPackage>("Guild Increase Achievement", (Action<ZRpc, ZPackage>)IncreaseAchievement);
			}
			else
			{
				peer.m_rpc.Register<int>("Guild Update Ack", (Action<ZRpc, int>)AckUpdate);
				peer.m_rpc.Register<string, string, string>("Guild Achievement Completed", (Action<ZRpc, string, string, string>)CompletedAchievement);
			}
		}

		private static void AckUpdate(ZRpc rpc, int id)
		{
			unappliedChanges.Remove(id);
		}

		public static void UpdateGuild(ZRpc? rpc, ZPackage zpkg)
		{
			int num = zpkg.ReadInt();
			string text = zpkg.ReadString();
			Dictionary<string[], object> dictionary = new Dictionary<string[], object>();
			int num2 = zpkg.ReadInt();
			for (int i = 0; i < num2; i++)
			{
				string[] array = new string[zpkg.ReadInt()];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = zpkg.ReadString();
				}
				object value = null;
				string text2 = zpkg.ReadString();
				if (text2 != "")
				{
					Type type = Type.GetType(text2);
					if ((object)type != null)
					{
						value = GuildSerialization.Deserialize(zpkg.ReadString(), type);
					}
				}
				dictionary.Add(array, value);
			}
			Guild baseTarget = guildList[text];
			HashSet<PlayerReference> hashSet = new HashSet<PlayerReference>(baseTarget.Members.Keys);
			ObjectDiff.ApplyDiff(ref baseTarget, dictionary);
			guildList[text] = baseTarget;
			foreach (PlayerReference item in baseTarget.Members.Keys.Except(hashSet))
			{
				API.InvokeGuildJoined(baseTarget, item);
			}
			foreach (PlayerReference item2 in hashSet.Except(baseTarget.Members.Keys))
			{
				API.InvokeGuildLeft(baseTarget, item2);
			}
			WriteGuild(text);
			if (rpc != null)
			{
				rpc.Invoke("Guild Update Ack", new object[1] { num });
			}
		}

		public static void AddGuild(ZRpc? rpc, ZPackage zpkg)
		{
			Guild guild = GuildSerialization.Deserialize<Guild>(zpkg.ReadString());
			if (!guildList.ContainsKey(guild.Name))
			{
				guildList[guild.Name] = guild;
			}
			WriteGuild(guild.Name);
		}

		public static void RemoveGuild(ZRpc? rpc, ZPackage zpkg)
		{
			string key = zpkg.ReadString();
			guildEntries.Value.Remove(key);
			guildEntries.Value = guildEntries.Value;
		}

		public static void RenameGuild(ZRpc? rpc, ZPackage zpkg)
		{
			string key = zpkg.ReadString();
			string key2 = zpkg.ReadString();
			if (guildEntries.Value.TryGetValue(key, out string value))
			{
				guildEntries.Value[key2] = value;
				guildEntries.Value.Remove(key);
				guildEntries.Value = guildEntries.Value;
			}
		}

		public static void IncreaseAchievement(ZRpc? rpc, ZPackage package)
		{
			int num = package.ReadInt();
			string achievement = package.ReadString();
			float num2 = package.ReadSingle();
			if (!guildsById.TryGetValue(num, out Guild value))
			{
				return;
			}
			AchievementConfig achievementConfig = Achievements.GetAchievementConfig(achievement);
			if (achievementConfig == null)
			{
				return;
			}
			if (!value.Achievements.TryGetValue(achievement, out AchievementData value2))
			{
				value2 = (value.Achievements[achievement] = new AchievementData());
				if (achievementConfig.first && API.GetGuilds().Any(delegate(Guild g)
				{
					if (g.Achievements.TryGetValue(achievement, out AchievementData value4))
					{
						float? progress2 = value4.progress;
						return !progress2.HasValue;
					}
					return false;
				}))
				{
					value2.progress = null;
				}
			}
			float? progress = value2.progress;
			if (!progress.HasValue || value2.completed.Count >= achievementConfig.progress.Count)
			{
				return;
			}
			value2.progress += num2;
			if (value2.progress >= achievementConfig.progress[value2.completed.Count])
			{
				if (achievementConfig.first)
				{
					foreach (Guild guild in API.GetGuilds())
					{
						if (guild.Achievements.TryGetValue(achievement, out AchievementData value3))
						{
							value3.progress = null;
						}
					}
				}
				value2.completed.Add(DateTime.Now);
				value.General.level += achievementConfig.GetLevel(value2.completed.Count);
				WriteGuild(num);
				PlayerReference playerReference = PlayerReference.fromRPC(rpc);
				foreach (ZNetPeer connectedPeer in ZNet.instance.GetConnectedPeers())
				{
					connectedPeer.m_rpc.Invoke("Guild Achievement Completed", new object[3] { playerReference.name, playerReference.id, achievement });
				}
				CompletedAchievement(null, playerReference.name, playerReference.id, achievement);
			}
			else
			{
				DelayedGuildUpdate(num);
			}
		}

		private static void CompletedAchievement(ZRpc? rpc, string name, string id, string achievement)
		{
			PlayerReference playerReference = default(PlayerReference);
			playerReference.id = id;
			playerReference.name = name;
			PlayerReference player = playerReference;
			foreach (API.AchievementCompleted achievementCompletedCallback in Achievements.achievementCompletedCallbacks)
			{
				achievementCompletedCallback(player, achievement);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003CDelayedGuildUpdate_003Eg__NotifyPendingChanges_007C11_0_003Ed : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		object IEnumerator<object>.Current
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
		public _003C_003CDelayedGuildUpdate_003Eg__NotifyPendingChanges_007C11_0_003Ed(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = (object)new WaitForSeconds(1f);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				HashSet<int> guildsWithPendingChanges = GuildList.guildsWithPendingChanges;
				GuildList.guildsWithPendingChanges = new HashSet<int>();
				foreach (int item in guildsWithPendingChanges)
				{
					WriteGuild(item);
				}
				return false;
			}
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
	}

	private static readonly CustomSyncedValue<Dictionary<string, string>> guildEntries = new CustomSyncedValue<Dictionary<string, string>>(Guilds.configSync, "guildEntries");

	private static Dictionary<string, string> oldEntries = new Dictionary<string, string>();

	private static readonly SortedDictionary<int, KeyValuePair<string, Dictionary<string[], object?>>> unappliedChanges = new SortedDictionary<int, KeyValuePair<string, Dictionary<string[], object>>>();

	private static int changeCount = 0;

	private static bool GuildIOActive = false;

	public static readonly Dictionary<string, Guild> guildList = new Dictionary<string, Guild>();

	public static Dictionary<int, Guild> guildsById = new Dictionary<int, Guild>();

	private static readonly Dictionary<string, Guild> guildBackup = new Dictionary<string, Guild>();

	private static HashSet<int> guildsWithPendingChanges = new HashSet<int>();

	private static bool truthSource = false;

	public static void Init()
	{
		guildEntries.ValueChanged += delegate
		{
			bool flag = truthSource == Guilds.configSync.IsSourceOfTruth;
			truthSource = Guilds.configSync.IsSourceOfTruth;
			HashSet<string> hashSet = new HashSet<string>();
			bool guildIOActive = GuildIOActive;
			GuildIOActive = true;
			if (Guilds.configSync.IsSourceOfTruth && !guildIOActive)
			{
				foreach (KeyValuePair<string, string> item in guildEntries.Value.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> kv) => !oldEntries.ContainsKey(kv.Key) || oldEntries[kv.Key] != kv.Value))
				{
					hashSet.Add(item.Key);
				}
				foreach (string item2 in oldEntries.Keys.Except(guildEntries.Value.Keys))
				{
					string guildsPath = Guilds.GuildsPath;
					char directorySeparatorChar = Path.DirectorySeparatorChar;
					File.Delete(guildsPath + directorySeparatorChar + item2 + ".yml");
				}
				oldEntries = guildEntries.Value.ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> kv) => kv.Key, (KeyValuePair<string, string> kv) => kv.Value);
			}
			Dictionary<int, Guild> dictionary = guildsById;
			guildsById = new Dictionary<int, Guild>();
			guildList.Clear();
			guildBackup.Clear();
			foreach (KeyValuePair<string, string> syncedEntry in guildEntries.Value)
			{
				try
				{
					string key = syncedEntry.Key;
					foreach (char c in key)
					{
						if (!Tools.ValidateChar(c))
						{
							throw new InvalidDataException($"Invalid character {(int)c} in guild name");
						}
					}
					Guild baseTarget = GuildSerialization.Deserialize<Guild>(syncedEntry.Value);
					if (baseTarget != null)
					{
						Guild baseTarget2 = GuildSerialization.Deserialize<Guild>(syncedEntry.Value);
						foreach (Dictionary<string[], object> item3 in from kv in unappliedChanges.Values
							where kv.Key == syncedEntry.Key
							select kv.Value)
						{
							ObjectDiff.ApplyDiff(ref baseTarget, item3);
							ObjectDiff.ApplyDiff(ref baseTarget2, item3);
						}
						guildsById.Add(baseTarget.General.id, baseTarget);
						guildList.Add(syncedEntry.Key, baseTarget);
						guildBackup.Add(syncedEntry.Key, baseTarget2);
						if (flag)
						{
							if (!dictionary.TryGetValue(baseTarget.General.id, out var value))
							{
								API.InvokeGuildCreated(baseTarget);
							}
							IEnumerable<PlayerReference> enumerable = value?.Members.Keys;
							HashSet<PlayerReference> hashSet2 = new HashSet<PlayerReference>(enumerable ?? Enumerable.Empty<PlayerReference>());
							foreach (PlayerReference item4 in baseTarget.Members.Keys.Except(hashSet2))
							{
								API.InvokeGuildJoined(baseTarget, item4);
							}
							foreach (PlayerReference item5 in hashSet2.Except(baseTarget.Members.Keys))
							{
								API.InvokeGuildLeft(baseTarget, item5);
							}
						}
						if (!guildIOActive && hashSet.Contains(syncedEntry.Key))
						{
							string guildsPath2 = Guilds.GuildsPath;
							char directorySeparatorChar = Path.DirectorySeparatorChar;
							File.WriteAllText(guildsPath2 + directorySeparatorChar + syncedEntry.Key + ".yml", GuildSerialization.Serialize(GuildConfigSerialized.fromGuildConfig(baseTarget)));
						}
					}
				}
				catch (Exception arg)
				{
					Debug.LogError((object)$"Failed to deserialize internally transferred guild file {syncedEntry.Key}: {arg}");
				}
			}
			if (flag)
			{
				foreach (KeyValuePair<int, Guild> item6 in dictionary)
				{
					if (!guildsById.ContainsKey(item6.Key))
					{
						foreach (PlayerReference key2 in item6.Value.Members.Keys)
						{
							API.InvokeGuildLeft(item6.Value, key2);
						}
						API.InvokeGuildDeleted(item6.Value);
					}
				}
			}
			GuildIOActive = guildIOActive;
			if (Object.op_Implicit((Object)(object)Interface.GuildManagementUI))
			{
				Interface.GuildManagementUI.GetComponent<GuildManagementUI>().UpdateRows();
				Interface.ApplicationsUI.GetComponent<ApplicationsUI>().UpdateRows();
				Interface.SearchGuildUI.GetComponent<SearchGuildUI>().UpdateRows();
				if (API.GetOwnGuild() != null)
				{
					if (Interface.NoGuildUI.activeSelf || Interface.CreateGuildUI.activeSelf)
					{
						Interface.SwitchUI(Interface.GuildManagementUI);
					}
					Map.UpdateMapPinColor();
				}
				else
				{
					GuildChat.ToggleGuildsChat(active: false);
				}
			}
		};
	}

	private static void DelayedGuildUpdate(int guild)
	{
		if (guildsWithPendingChanges.Count == 0)
		{
			((MonoBehaviour)Guilds.self).StartCoroutine(NotifyPendingChanges());
		}
		guildsWithPendingChanges.Add(guild);
		[IteratorStateMachine(typeof(_003C_003CDelayedGuildUpdate_003Eg__NotifyPendingChanges_007C11_0_003Ed))]
		static IEnumerator NotifyPendingChanges()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003C_003CDelayedGuildUpdate_003Eg__NotifyPendingChanges_007C11_0_003Ed(0);
		}
	}

	public static void readGuildFiles()
	{
		if (GuildIOActive)
		{
			return;
		}
		GuildIOActive = true;
		Directory.CreateDirectory(Guilds.GuildsPath);
		oldEntries.Clear();
		foreach (FileInfo item in from s in Directory.GetFiles(Guilds.GuildsPath)
			select new FileInfo(s) into file
			where file.Name.EndsWith(".yml", StringComparison.Ordinal)
			select file)
		{
			try
			{
				string text = item.Name.Replace(".yml", "");
				GuildConfigSerialized guildConfigSerialized = GuildSerialization.Deserialize<GuildConfigSerialized>(File.ReadAllText(item.FullName));
				if (guildConfigSerialized != null)
				{
					GuildConfigSerialized guildConfigSerialized2 = guildConfigSerialized;
					if (guildConfigSerialized2.members == null)
					{
						guildConfigSerialized2.members = new List<GuildMemberClass>();
					}
					guildConfigSerialized2 = guildConfigSerialized;
					if (guildConfigSerialized2.general == null)
					{
						guildConfigSerialized2.general = new GuildGeneral();
					}
					guildConfigSerialized2 = guildConfigSerialized;
					if (guildConfigSerialized2.applications == null)
					{
						guildConfigSerialized2.applications = new List<ApplicationClass>();
					}
					guildConfigSerialized2 = guildConfigSerialized;
					if (guildConfigSerialized2.customData == null)
					{
						guildConfigSerialized2.customData = new CustomData();
					}
					Guild guild = guildConfigSerialized.toGuild(text);
					oldEntries.Add(text, GuildSerialization.Serialize(guild));
				}
			}
			catch (Exception arg)
			{
				Debug.LogError((object)$"Failed to deserialize guild file {item.Name}: {arg}");
			}
		}
		guildEntries.AssignLocalValue(oldEntries.ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> kv) => kv.Key, (KeyValuePair<string, string> kv) => kv.Value));
		GuildIOActive = false;
	}

	private static void WriteGuild(string guildName)
	{
		guildsWithPendingChanges.Remove(guildList[guildName].General.id);
		guildEntries.Value[guildName] = GuildSerialization.Serialize(guildList[guildName]);
		guildEntries.Value = guildEntries.Value;
	}

	private static void WriteGuild(int guildId)
	{
		if (guildsById.TryGetValue(guildId, out Guild value))
		{
			WriteGuild(value.Name);
		}
	}

	public static void removeGuild(string guildName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		ZPackage val = new ZPackage();
		val.Write(guildName);
		guildBackup.Remove(guildName);
		ZRpc serverRPC = ZNet.m_instance.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Guild Remove Guild", new object[1] { val });
		}
		else
		{
			val.SetPos(0);
			GuildListManipulationListener.RemoveGuild(null, val);
		}
	}

	public static void renameGuild(string oldName, string newName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		ZPackage val = new ZPackage();
		val.Write(oldName);
		val.Write(newName);
		guildBackup[newName] = guildBackup[oldName];
		guildBackup.Remove(oldName);
		ZRpc serverRPC = ZNet.m_instance.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Guild Rename Guild", new object[1] { val });
		}
		else
		{
			val.SetPos(0);
			GuildListManipulationListener.RenameGuild(null, val);
		}
	}

	public static void updateGuild(string guildName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		ZPackage val = new ZPackage();
		if (guildBackup.ContainsKey(guildName))
		{
			val.Write(++changeCount);
			val.Write(guildName);
			Dictionary<string[], object> dictionary = ObjectDiff.diff(guildBackup[guildName], guildList[guildName]);
			val.Write(dictionary.Count);
			foreach (KeyValuePair<string[], object> item in dictionary)
			{
				val.Write(item.Key.Length);
				string[] key = item.Key;
				foreach (string text in key)
				{
					val.Write(text);
				}
				if (item.Value == null)
				{
					val.Write("");
					continue;
				}
				val.Write(item.Value.GetType().AssemblyQualifiedName);
				string text2 = GuildSerialization.Serialize(item.Value);
				val.Write(text2);
				Guild baseTarget = guildBackup[guildName];
				ObjectDiff.ApplyDiff(ref baseTarget, new Dictionary<string[], object> { 
				{
					item.Key,
					GuildSerialization.Deserialize(text2, item.Value.GetType())
				} });
				guildBackup[guildName] = baseTarget;
			}
			ZRpc serverRPC = ZNet.m_instance.GetServerRPC();
			if (serverRPC != null)
			{
				unappliedChanges.Add(changeCount, new KeyValuePair<string, Dictionary<string[], object>>(guildName, dictionary));
				serverRPC.Invoke("Guild Update Guild", new object[1] { val });
			}
			else
			{
				val.SetPos(0);
				GuildListManipulationListener.UpdateGuild(null, val);
			}
		}
		else
		{
			string text3 = GuildSerialization.Serialize(guildList[guildName]);
			val.Write(text3);
			guildBackup[guildName] = GuildSerialization.Deserialize<Guild>(text3);
			ZRpc serverRPC2 = ZNet.m_instance.GetServerRPC();
			if (serverRPC2 != null)
			{
				serverRPC2.Invoke("Guild Create Guild", new object[1] { val });
			}
			else
			{
				val.SetPos(0);
				GuildListManipulationListener.AddGuild(null, val);
			}
		}
	}

	public static void increaseAchievement(int guildId, string achievement, float increment)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		AchievementConfig achievementConfig = Achievements.GetAchievementConfig(achievement);
		if (achievementConfig == null)
		{
			return;
		}
		List<int> guild = achievementConfig.guild;
		if (guild == null || guild.Contains(guildId))
		{
			ZPackage val = new ZPackage();
			val.Write(guildId);
			val.Write(achievement);
			val.Write(increment);
			ZRpc serverRPC = ZNet.instance.GetServerRPC();
			if (serverRPC != null)
			{
				serverRPC.Invoke("Guild Increase Achievement", new object[1] { val });
			}
			else
			{
				val.SetPos(0);
				GuildListManipulationListener.IncreaseAchievement(null, val);
			}
		}
	}
}
