using System.Collections.Generic;
using System.Linq;

namespace Groups;

public class Group
{
	public class PlayerState
	{
		public float health;

		public float maxHealth;

		public long playerId;

		public static PlayerState read(ZPackage group)
		{
			return new PlayerState
			{
				health = group.ReadSingle(),
				maxHealth = group.ReadSingle(),
				playerId = group.ReadLong()
			};
		}

		public void write(ZPackage group)
		{
			group.Write(health);
			group.Write(maxHealth);
			group.Write(playerId);
		}

		public static PlayerState fromLocal()
		{
			return new PlayerState
			{
				health = ((Character)Player.m_localPlayer).GetHealth(),
				maxHealth = ((Character)Player.m_localPlayer).GetMaxHealth(),
				playerId = Game.instance.m_playerProfile.GetPlayerID()
			};
		}
	}

	private PlayerReference _leader;

	public readonly Dictionary<PlayerReference, PlayerState> playerStates;

	public PlayerReference leader
	{
		get
		{
			return _leader;
		}
		set
		{
			if (value != _leader)
			{
				_leader = value;
				API.InvokeLeaderChanged(value);
			}
		}
	}

	public Group(PlayerReference leader, PlayerState playerState)
	{
		this.leader = leader;
		playerStates = new Dictionary<PlayerReference, PlayerState> { { leader, playerState } };
	}

	private bool CheckGroupFull()
	{
		return playerStates.Count >= Groups.maximumGroupSize.Value;
	}

	public bool AddMember(PlayerReference player, PlayerState playerState)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		if (playerStates.ContainsKey(player))
		{
			return false;
		}
		if (CheckGroupFull())
		{
			return false;
		}
		PlayerReference[] array = playerStates.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			PlayerReference playerReference = array[i];
			if (playerReference != player)
			{
				ZPackage val = new ZPackage();
				playerState.write(val);
				ZRoutedRpc.instance.InvokeRoutedRPC(playerReference.peerId, "Groups AddMember", new object[2]
				{
					player.ToString(),
					val
				});
			}
			ZRoutedRpc.instance.InvokeRoutedRPC(playerReference.peerId, "Groups AddMessage", new object[1] { Localization.instance.Localize("$groups_player_joined", new string[1] { player.name }) });
		}
		return true;
	}

	public bool RemoveMember(PlayerReference player, bool self = false)
	{
		if (playerStates.ContainsKey(player))
		{
			PlayerReference[] array = playerStates.Keys.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				PlayerReference playerReference = array[i];
				ZRoutedRpc.instance.InvokeRoutedRPC(playerReference.peerId, "Groups UpdateGroup", new object[2]
				{
					player.ToString(),
					"Member Removed"
				});
				if (playerReference != player)
				{
					string text = Localization.instance.Localize(self ? "$groups_player_left" : "$groups_player_removed", new string[1] { player.name });
					ZRoutedRpc.instance.InvokeRoutedRPC(playerReference.peerId, "Groups AddMessage", new object[1] { text });
				}
			}
			if (!self)
			{
				ZRoutedRpc.instance.InvokeRoutedRPC(player.peerId, "Groups AddMessage", new object[1] { Localization.instance.Localize("$groups_self_removed") });
			}
			return true;
		}
		return false;
	}

	public bool PromoteMember(PlayerReference player, bool sendToLeader = false)
	{
		if (player == leader)
		{
			return false;
		}
		if (playerStates.ContainsKey(player))
		{
			foreach (PlayerReference key in playerStates.Keys)
			{
				if (key != leader || sendToLeader)
				{
					ZRoutedRpc.instance.InvokeRoutedRPC(key.peerId, "Groups UpdateGroup", new object[2]
					{
						player.ToString(),
						"Member Promoted"
					});
				}
				ZRoutedRpc.instance.InvokeRoutedRPC(key.peerId, "Groups AddMessage", new object[1] { Localization.instance.Localize("$groups_player_promoted", new string[1] { player.name }) });
			}
			leader = player;
			return true;
		}
		return false;
	}

	public void Leave()
	{
		API.InvokeGroupLeft();
		ChatCommands.ToggleGroupsChat(active: false);
		PlayerReference ownReference = PlayerReference.fromPlayer(Player.m_localPlayer);
		if (leader == ownReference && playerStates.Count > 1)
		{
			PromoteMember(playerStates.Keys.First((PlayerReference p) => p != ownReference));
		}
		RemoveMember(PlayerReference.fromPlayer(Player.m_localPlayer), self: true);
	}
}
