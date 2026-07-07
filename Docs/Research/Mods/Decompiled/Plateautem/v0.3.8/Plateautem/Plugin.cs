using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace Plateautem;

[BepInPlugin("com.erkle64.Plateautem", "Plateautem", "0.3.8")]
[BepInDependency(/*Could not decode attribute arguments.*/)]
[NetworkCompatibility(/*Could not decode attribute arguments.*/)]
internal class Plugin : BaseUnityPlugin
{
	public const string PluginName = "Plateautem";

	public const string PluginAuthor = "erkle64";

	public const string PluginGUID = "com.erkle64.Plateautem";

	public const string PluginVersion = "0.3.8";

	public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

	private static ConfigEntry<string> configRequirements;

	private void Awake()
	{
		Logger.LogInfo((object)"Plateautem loading");
		LoadConfig();
		Plateautem.RegisterRPCs();
		CustomLocalization localization = Localization;
		string text = "English";
		localization.AddTranslation(ref text, new Dictionary<string, string>
		{
			{ "piece_plateautem", "Plateautem" },
			{ "piece_plateautem_description", "Terrain flattening totem." },
			{ "$piece_plateautem_noFuel", "You have no $1" },
			{ "$piece_plateautem_hold", "Hold" },
			{ "$piece_plateautem_all", "all" },
			{ "$piece_plateautem_fuel", "Fuel" },
			{ "$piece_plateautem_tools", "Tools" },
			{ "$piece_plateautem_radius", "Radius" },
			{ "$piece_plateautem_reset", "Reset scan" },
			{ "$piece_plateautem_selectFuel", "Select item to insert" },
			{ "$piece_plateautem_eject", "Eject" },
			{ "$piece_plateautem_ejectFuel", "Eject fuel" },
			{ "$piece_plateautem_ejectStone", "Eject stone" },
			{ "$piece_plateautem_selectMode", "Insert item set to $1\nPress or hold E to insert $1." }
		});
		PrefabManager.OnVanillaPrefabsAvailable += OnVanillaPrefabsAvailable;
		Harmony.CreateAndPatchAll(((object)this).GetType().Assembly, (string)null);
	}

	private void LoadConfig()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		configRequirements = ((BaseUnityPlugin)this).Config.Bind<string>("Server", "Requirements", "Wood*5,Stone*2", new ConfigDescription("Requirements for crafting.  Use Prefab column at https://valheim-modding.github.io/Jotunn/data/objects/item-list.html", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		Plateautem.LoadConfig((BaseUnityPlugin)(object)this);
	}

	private void OnVanillaPrefabsAvailable()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Expected O, but got Unknown
		Plateautem.LoadFuelItems();
		AssetBundle val = AssetUtils.LoadAssetBundleFromResources("plateautem");
		try
		{
			PieceConfig val2 = new PieceConfig();
			val2.Name = "$piece_plateautem";
			val2.PieceTable = "Hammer";
			MatchCollection matchCollection = new Regex("\\s*([^*,\\s]+)(?:\\s*\\*\\s*(\\d+))?\\s*(?:,|$)").Matches(configRequirements.Value);
			if (matchCollection.Count > 0)
			{
				for (int i = 0; i < matchCollection.Count; i++)
				{
					Match match = matchCollection[i];
					if (match.Groups.Count < 2)
					{
						continue;
					}
					string value = match.Groups[1].Value;
					int num = 1;
					if (match.Groups.Count >= 3 && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
					{
						try
						{
							num = int.Parse(match.Groups[2].Value);
						}
						catch
						{
							try
							{
								num = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
							}
							catch
							{
								num = 1;
							}
						}
					}
					val2.AddRequirement(new RequirementConfig(value, num, 0, true));
					Logger.LogInfo((object)$"Adding {value}x{num} as requirement for Plateautem.");
				}
			}
			else
			{
				val2.AddRequirement(new RequirementConfig("Wood", 2, 0, true));
			}
			GameObject val3 = PrefabManager.Instance.CreateClonedPrefab("erkle_plateautem", "guard_stone");
			Piece component = val3.GetComponent<Piece>();
			component.m_name = "$piece_plateautem_name";
			component.m_description = "$piece_plateautem_description";
			component.m_icon = val.LoadAsset<Sprite>("plateautem");
			component.m_clipGround = true;
			component.m_groundOnly = true;
			component.m_noInWater = true;
			GuidePoint[] array = val3.GetComponentsInChildren<GuidePoint>().ToArray();
			for (int j = 0; j < array.Length; j++)
			{
				Object.DestroyImmediate((Object)(object)array[j]);
			}
			Plateautem plateautem = val3.AddComponent<Plateautem>();
			PrivateArea component2 = val3.GetComponent<PrivateArea>();
			if ((Object)(object)component2 != (Object)null)
			{
				plateautem.BuildPrefab(component2, val);
				Object.DestroyImmediate((Object)(object)component2);
			}
			PieceManager.Instance.AddPiece(new CustomPiece(val3, false, val2));
		}
		catch (Exception arg)
		{
			Logger.LogError((object)$"Error while adding cloned item: {arg}");
		}
		finally
		{
			PrefabManager.OnVanillaPrefabsAvailable -= OnVanillaPrefabsAvailable;
			if (val != null)
			{
				val.Unload(false);
			}
		}
	}
}
