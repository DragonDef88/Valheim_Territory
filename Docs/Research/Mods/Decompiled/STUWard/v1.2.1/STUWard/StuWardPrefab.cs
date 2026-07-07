using System;
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace STUWard;

internal static class StuWardPrefab
{
	private static bool _registered;

	private static GameObject? _stuWardPrefab;

	private static GameObject? _vanillaGuardStonePrefab;

	private static int _vanillaGuardStoneIndex = -1;

	private static Requirement[]? _defaultStuWardRequirements;

	private static string? _lastLoggedPieceIconState;

	internal static void Register()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		if (_registered || PieceManager.Instance.GetPiece("piece_stuward") != null)
		{
			_registered = true;
		}
		else
		{
			if ((Object)(object)PrefabManager.Instance.GetPrefab("guard_stone") == (Object)null)
			{
				return;
			}
			PieceConfig val = new PieceConfig
			{
				PieceTable = "Hammer",
				Name = "$stuw_piece_name",
				Description = "$stuw_piece_desc"
			};
			CustomPiece val2 = new CustomPiece("piece_stuward", "guard_stone", val);
			GameObject piecePrefab = val2.PiecePrefab;
			Piece piece = val2.Piece;
			PrivateArea val3 = (((Object)(object)piecePrefab != (Object)null) ? piecePrefab.GetComponent<PrivateArea>() : null);
			if ((Object)(object)piecePrefab == (Object)null || (Object)(object)piece == (Object)null || (Object)(object)val3 == (Object)null)
			{
				Plugin.Log.LogWarning((object)"Failed to create STUWard clone prefab from guard_stone.");
				return;
			}
			if ((Object)(object)piecePrefab.GetComponent<StuWardArea>() == (Object)null)
			{
				piecePrefab.AddComponent<StuWardArea>();
			}
			if ((Object)(object)piecePrefab.GetComponent<StuWardPlacedHook>() == (Object)null)
			{
				piecePrefab.AddComponent<StuWardPlacedHook>();
			}
			piece.m_name = "$stuw_piece_name";
			piece.m_description = "$stuw_piece_desc";
			piece.m_resources = CloneRequirements(piece.m_resources);
			val3.m_name = "$stuw_piece_name";
			val3.m_radius = 8f;
			if ((Object)(object)val3.m_areaMarker != (Object)null)
			{
				val3.m_areaMarker.m_radius = 8f;
			}
			_stuWardPrefab = piecePrefab;
			_defaultStuWardRequirements = CloneRequirements(piece.m_resources);
			PieceManager.Instance.AddPiece(val2);
			_registered = PieceManager.Instance.GetPiece("piece_stuward") != null;
			if (_registered)
			{
				Plugin.Log.LogInfo((object)"Registered STUWard clone piece.");
			}
		}
	}

	internal static void ApplyRecipeSettings()
	{
		ApplyVanillaGuardStoneRecipeSetting();
		ApplyStuWardRecipeSetting();
	}

	internal static Sprite? GetPieceIcon()
	{
		Piece stuWardPiece = GetStuWardPiece();
		if ((Object)(object)stuWardPiece != (Object)null && (Object)(object)stuWardPiece.m_icon != (Object)null)
		{
			return LogPieceIconResolution(stuWardPiece.m_icon, "stuWardPrefab piece icon '" + ((Object)stuWardPiece.m_icon).name + "'");
		}
		CustomPiece piece = PieceManager.Instance.GetPiece("piece_stuward");
		if ((Object)(object)((piece != null) ? piece.Piece : null) != (Object)null && (Object)(object)piece.Piece.m_icon != (Object)null)
		{
			return LogPieceIconResolution(piece.Piece.m_icon, "registered piece icon '" + ((Object)piece.Piece.m_icon).name + "'");
		}
		GameObject val = ((piece != null) ? piece.PiecePrefab : null) ?? PrefabManager.Instance.GetPrefab("piece_stuward") ?? PrefabManager.Instance.GetPrefab("guard_stone");
		Sprite val2 = ((!((Object)(object)val != (Object)null)) ? null : val.GetComponent<Piece>()?.m_icon);
		if ((Object)(object)val2 != (Object)null)
		{
			string text = (((Object)(object)val != (Object)null) ? ((Object)val).name : "null");
			return LogPieceIconResolution(val2, "prefab '" + text + "' piece icon '" + ((Object)val2).name + "'");
		}
		return LogMissingPieceIcon(string.Format("stuWardPrefabPresent={0}, registeredPiecePresent={1}, registeredPiecePrefabPresent={2}, fallbackPrefab='{3}'", (Object)(object)_stuWardPrefab != (Object)null, (Object)(object)((piece != null) ? piece.Piece : null) != (Object)null, (Object)(object)((piece != null) ? piece.PiecePrefab : null) != (Object)null, ((val != null) ? ((Object)val).name : null) ?? "null"));
	}

	internal static Requirement[] GetCurrentStuWardRequirements()
	{
		return CloneRequirements(GetStuWardPiece()?.m_resources);
	}

	private static Piece? GetStuWardPiece()
	{
		Piece val = (((Object)(object)_stuWardPrefab != (Object)null) ? _stuWardPrefab.GetComponent<Piece>() : null);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		CustomPiece piece = PieceManager.Instance.GetPiece("piece_stuward");
		if (piece == null)
		{
			return null;
		}
		return piece.Piece;
	}

	private static Sprite LogPieceIconResolution(Sprite icon, string source)
	{
		string text = "resolved:" + source;
		if (_lastLoggedPieceIconState != text)
		{
			_lastLoggedPieceIconState = text;
			Plugin.LogWardDiagnosticVerbose("WardPins.Icon", "Resolved piece_stuward icon from " + source + ".");
		}
		return icon;
	}

	private static Sprite? LogMissingPieceIcon(string context)
	{
		string text = "missing:" + context;
		if (_lastLoggedPieceIconState != text)
		{
			_lastLoggedPieceIconState = text;
			Plugin.LogWardDiagnosticFailure("WardPins.Icon", "Failed to resolve piece_stuward icon. " + context);
		}
		return null;
	}

	internal static void ApplyVanillaGuardStoneRecipeSetting()
	{
		PieceTable? hammerPieceTable = GetHammerPieceTable();
		List<GameObject> list = hammerPieceTable?.m_pieces;
		GameObject prefab = PrefabManager.Instance.GetPrefab("guard_stone");
		if ((Object)(object)hammerPieceTable == (Object)null || list == null || (Object)(object)prefab == (Object)null)
		{
			return;
		}
		if (_vanillaGuardStonePrefab == null)
		{
			_vanillaGuardStonePrefab = prefab;
		}
		List<int> matchingGuardStoneIndexes = GetMatchingGuardStoneIndexes(list, prefab);
		if (_vanillaGuardStoneIndex < 0 && matchingGuardStoneIndexes.Count > 0)
		{
			_vanillaGuardStoneIndex = matchingGuardStoneIndexes[0];
		}
		if (Plugin.DisableVanillaGuardStoneRecipe != null && Plugin.DisableVanillaGuardStoneRecipe.Value == Plugin.Toggle.On)
		{
			for (int num = matchingGuardStoneIndexes.Count - 1; num >= 0; num--)
			{
				list.RemoveAt(matchingGuardStoneIndexes[num]);
			}
		}
		else if (matchingGuardStoneIndexes.Count == 0 && (Object)(object)_vanillaGuardStonePrefab != (Object)null)
		{
			int index = ((_vanillaGuardStoneIndex >= 0) ? Mathf.Clamp(_vanillaGuardStoneIndex, 0, list.Count) : list.Count);
			list.Insert(index, _vanillaGuardStonePrefab);
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer != null)
		{
			localPlayer.UpdateAvailablePiecesList();
		}
	}

	private static void ApplyStuWardRecipeSetting()
	{
		Piece val = (((Object)(object)_stuWardPrefab != (Object)null) ? _stuWardPrefab.GetComponent<Piece>() : null);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		string text = Plugin.StuWardRecipe?.Value?.Trim() ?? string.Empty;
		Requirement[] requirements;
		if (string.IsNullOrWhiteSpace(text))
		{
			if (_defaultStuWardRequirements != null)
			{
				val.m_resources = CloneRequirements(_defaultStuWardRequirements);
				Player localPlayer = Player.m_localPlayer;
				if (localPlayer != null)
				{
					localPlayer.UpdateAvailablePiecesList();
				}
			}
		}
		else if (!TryParseRequirements(text, out requirements))
		{
			Plugin.Log.LogWarning((object)("Invalid STUWard recipe override '" + text + "'. Keeping previous recipe."));
		}
		else
		{
			val.m_resources = requirements;
			Player localPlayer2 = Player.m_localPlayer;
			if (localPlayer2 != null)
			{
				localPlayer2.UpdateAvailablePiecesList();
			}
		}
	}

	private static PieceTable? GetHammerPieceTable()
	{
		GameObject prefab = PrefabManager.Instance.GetPrefab("Hammer");
		return (((Object)(object)prefab != (Object)null) ? prefab.GetComponent<ItemDrop>() : null)?.m_itemData?.m_shared?.m_buildPieces;
	}

	private static List<int> GetMatchingGuardStoneIndexes(List<GameObject> pieces, GameObject guardStonePrefab)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < pieces.Count; i++)
		{
			GameObject val = pieces[i];
			if (!((Object)(object)val == (Object)null) && ((Object)(object)val == (Object)(object)guardStonePrefab || ((Object)val).name == "guard_stone"))
			{
				list.Add(i);
			}
		}
		return list;
	}

	private static Requirement[] CloneRequirements(Requirement[]? source)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		if (source == null || source.Length == 0)
		{
			return Array.Empty<Requirement>();
		}
		Requirement[] array = (Requirement[])(object)new Requirement[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			Requirement val = source[i];
			array[i] = new Requirement
			{
				m_resItem = val.m_resItem,
				m_amount = val.m_amount,
				m_extraAmountOnlyOneIngredient = val.m_extraAmountOnlyOneIngredient,
				m_amountPerLevel = val.m_amountPerLevel,
				m_recover = val.m_recover
			};
		}
		return array;
	}

	private static bool TryParseRequirements(string value, out Requirement[] requirements)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		requirements = Array.Empty<Requirement>();
		string[] array = value.Split(new char[4] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return false;
		}
		List<Requirement> list = new List<Requirement>(array.Length);
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = array2[i].Split(':');
			int num = array3.Length;
			if ((num < 2 || num > 3) ? true : false)
			{
				return false;
			}
			string text = array3[0].Trim();
			if (string.IsNullOrWhiteSpace(text) || !int.TryParse(array3[1], out var result) || result <= 0)
			{
				return false;
			}
			GameObject val = ResolveItemPrefab(text);
			ItemDrop val2 = (((Object)(object)val != (Object)null) ? val.GetComponent<ItemDrop>() : null);
			if ((Object)(object)val2 == (Object)null)
			{
				Plugin.Log.LogWarning((object)("Unable to resolve STUWard recipe item prefab '" + text + "'."));
				return false;
			}
			bool result2 = true;
			if (array3.Length == 3 && !TryParseBool(array3[2], out result2))
			{
				return false;
			}
			list.Add(new Requirement
			{
				m_resItem = val2,
				m_amount = result,
				m_amountPerLevel = 1,
				m_recover = result2
			});
		}
		requirements = list.ToArray();
		return true;
	}

	private static GameObject? ResolveItemPrefab(string prefabName)
	{
		if (string.IsNullOrWhiteSpace(prefabName))
		{
			return null;
		}
		ObjectDB instance = ObjectDB.instance;
		GameObject val = ((instance != null) ? instance.GetItemPrefab(prefabName) : null);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		return PrefabManager.Instance.GetPrefab(prefabName);
	}

	private static bool TryParseBool(string value, out bool result)
	{
		switch (value.Trim().ToLowerInvariant())
		{
		case "1":
		case "yes":
		case "on":
		case "true":
			result = true;
			return true;
		case "0":
		case "off":
		case "no":
		case "false":
			result = false;
			return true;
		default:
			result = false;
			return false;
		}
	}
}
