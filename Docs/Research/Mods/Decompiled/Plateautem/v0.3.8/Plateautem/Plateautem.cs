using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Plateautem;

public class Plateautem : MonoBehaviour, Interactable, Hoverable
{
	private struct FuelItem
	{
		public string prefabName;

		public string displayName;

		public float fuelValue;
	}

	[HarmonyPatch(typeof(TerrainComp))]
	public static class Patch
	{
		[HarmonyPrefix]
		[HarmonyPatch("LevelTerrain")]
		public static bool LevelTerrain(TerrainComp __instance, Vector3 worldPos, float radius, bool square)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			int num = default(int);
			int num2 = default(int);
			__instance.m_hmap.WorldToVertex(worldPos, ref num, ref num2);
			Vector3 val = worldPos - ((Component)__instance).transform.position;
			float num3 = radius / __instance.m_hmap.m_scale;
			int num4 = Mathf.CeilToInt(num3);
			int num5 = __instance.m_width + 1;
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector((float)num, (float)num2);
			for (int i = num2 - num4; i <= num2 + num4; i++)
			{
				for (int j = num - num4; j <= num + num4; j++)
				{
					float num6 = Vector2.Distance(val2, new Vector2((float)j, (float)i));
					if ((square || num6 <= num3) && j >= 0 && i >= 0 && j < num5 && i < num5)
					{
						float num7 = 1f - num6 / num3;
						float height = __instance.m_hmap.GetHeight(j, i);
						float num8 = (val.y - height) * ((num7 > 0.5f) ? 1f : (num7 / 0.5f));
						int num9 = i * num5 + j;
						float num10 = num8 + __instance.m_smoothDelta[num9];
						__instance.m_smoothDelta[num9] = 0f;
						__instance.m_levelDelta[num9] += num10;
						__instance.m_modifiedHeight[num9] = true;
					}
				}
			}
			return false;
		}
	}

	private static TerrainOp flattenTerrainOp;

	private EffectList flattenPlaceEffect;

	[SerializeField]
	private CircleProjector circleProjector;

	[SerializeField]
	private GameObject enabledEffect;

	[SerializeField]
	private MeshRenderer model;

	private ZNetView netView;

	[SerializeField]
	private Transform lightTransform;

	[SerializeField]
	private Transform flareTransform;

	[SerializeField]
	private Transform droneAudioSourceTransform;

	[SerializeField]
	private AudioSource droneAudioSource;

	private float targetDroneAudioVolume;

	private static List<FuelItem> fuelItems = new List<FuelItem>();

	private const string stoneItemPrefabName = "Stone";

	private const string stoneItemDisplayName = "$item_stone";

	private static string[] lowerToolPrefabNames = new string[4] { "PickaxeAntler", "PickaxeBronze", "PickaxeIron", "PickaxeBlackMetal" };

	private static string[] lowerToolDisplayNames = new string[4] { "$item_pickaxe_antler", "$item_pickaxe_bronze", "$item_pickaxe_iron", "$item_pickaxe_blackmetal" };

	private static string[] raiseToolPrefabNames = new string[1] { "Hoe" };

	private static string[] raiseToolDisplayNames = new string[1] { "$item_hoe" };

	private const float placementSpacing = 2.6f;

	private const float circlePadding = 1f;

	private int currentFuelItemIndex;

	private float lastUseTime;

	private float previousScanTime;

	private float previousScanTime2;

	private float previousScanProgress;

	private float targetScanProgress;

	private static ConfigEntry<string> configFuelItems;

	private static ConfigEntry<float> configFuelPerScan;

	private static ConfigEntry<float> configFuelPerRaise;

	private static ConfigEntry<float> configFuelPerLower;

	private static ConfigEntry<int> configMaximumFuel;

	private static ConfigEntry<float> configStonePerRaise;

	private static ConfigEntry<float> configStonePerLower;

	private static ConfigEntry<int> configMaximumStone;

	private static ConfigEntry<float> configDefaultFlatteningRadius;

	private static ConfigEntry<float> configMaximumFlatteningRadius;

	private static ConfigEntry<float> configMinFlatteningTime;

	private static ConfigEntry<float> configMaxFlatteningTime;

	private static ConfigEntry<float> configScanningTime;

	private static ConfigEntry<bool> configDoPainting;

	private static ConfigEntry<bool> configRequireLowerTool;

	private static ConfigEntry<bool> configRequireRaiseTool;

	private static ConfigEntry<float>[] configLowerToolBonus;

	private static ConfigEntry<float>[] configRaiseToolBonus;

	private static ConfigEntry<KeyboardShortcut> configIncreaseRadiusKey;

	private static ConfigEntry<KeyboardShortcut> configDecreaseRadiusKey;

	private static ConfigEntry<KeyboardShortcut> configResetScanKey;

	private static ConfigEntry<KeyboardShortcut> configEjectFuelKey;

	private static ConfigEntry<KeyboardShortcut> configEjectStoneKey;

	private static ConfigEntry<KeyboardShortcut> configEjectToolsKey;

	private static ConfigEntry<bool> configShowMainKeys;

	private static ConfigEntry<bool> configShowExtraKeys;

	private static ConfigEntry<bool> configShowFillBars;

	private static ConfigEntry<bool> configShowFillNumbers;

	private static ConfigEntry<bool> configShowTools;

	private static ConfigEntry<bool> configShowSelection;

	public const string msgNoFuel = "$piece_plateautem_noFuel";

	public const string msgHold = "$piece_plateautem_hold";

	public const string msgAll = "$piece_plateautem_all";

	public const string msgFuel = "$piece_plateautem_fuel";

	public const string msgTools = "$piece_plateautem_tools";

	public const string msgRadius = "$piece_plateautem_radius";

	public const string msgResetScan = "$piece_plateautem_reset";

	public const string msgSelectFuel = "$piece_plateautem_selectFuel";

	public const string msgEject = "$piece_plateautem_eject";

	public const string msgEjectFuel = "$piece_plateautem_ejectFuel";

	public const string msgEjectStone = "$piece_plateautem_ejectStone";

	public const string msgSelectMode = "$piece_plateautem_selectMode";

	private const string zdonCurrentRadius = "current_radius";

	private int zdoidCurrentRadius;

	private const string zdonFuelStored = "fuel_stored";

	private int zdoidFuelStored;

	private const string zdonStoneStored = "stone_stored";

	private int zdoidStoneStored;

	private const string zdonScanProgress = "scan_progress";

	private int zdoidScanProgress;

	private const string zdonScanIndex = "scan_index";

	private int zdoidScanIndex;

	private const string zdonScanSpeed = "scan_speed";

	private int zdoidScanSpeed;

	private const string zdonLowerToolIndex = "lower_tool_index";

	private int zdoidLowerToolIndex;

	private const string zdonRaiseToolIndex = "raise_item_index";

	private int zdoidRaiseToolIndex;

	private static CustomRPC rpcLevelTerrain;

	private float currentRadius
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
			{
				return configDefaultFlatteningRadius.Value;
			}
			return netView.GetZDO().GetFloat(zdoidCurrentRadius, configDefaultFlatteningRadius.Value);
		}
		set
		{
			if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
			{
				netView.GetZDO().Set(zdoidCurrentRadius, value);
			}
		}
	}

	private float currentFuelStored
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
			{
				return 0f;
			}
			return netView.GetZDO().GetFloat(zdoidFuelStored, 0f);
		}
		set
		{
			if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
			{
				netView.GetZDO().Set(zdoidFuelStored, value);
			}
		}
	}

	private float currentStoneStored
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
			{
				return 0f;
			}
			return netView.GetZDO().GetFloat(zdoidStoneStored, 0f);
		}
		set
		{
			if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
			{
				netView.GetZDO().Set(zdoidStoneStored, value);
			}
		}
	}

	private float currentScanProgress
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
			{
				return 0f;
			}
			return netView.GetZDO().GetFloat(zdoidScanProgress, 0f);
		}
		set
		{
			if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
			{
				netView.GetZDO().Set(zdoidScanProgress, value);
			}
		}
	}

	private int currentScanIndex
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
			{
				return 0;
			}
			return netView.GetZDO().GetInt(zdoidScanIndex, 0);
		}
		set
		{
			if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
			{
				netView.GetZDO().Set(zdoidScanIndex, value, false);
			}
		}
	}

	private float currentScanSpeed
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
			{
				return 0f;
			}
			return netView.GetZDO().GetFloat(zdoidScanSpeed, (configScanningTime.Value <= float.Epsilon) ? 1f : (1f / configScanningTime.Value));
		}
		set
		{
			if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
			{
				netView.GetZDO().Set(zdoidScanSpeed, value);
			}
		}
	}

	private int currentLowerToolIndex
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
			{
				return -1;
			}
			return netView.GetZDO().GetInt(zdoidLowerToolIndex, -1);
		}
		set
		{
			if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
			{
				netView.GetZDO().Set(zdoidLowerToolIndex, value, false);
			}
		}
	}

	private int currentRaiseToolIndex
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
			{
				return -1;
			}
			return netView.GetZDO().GetInt(zdoidRaiseToolIndex, -1);
		}
		set
		{
			if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
			{
				netView.GetZDO().Set(zdoidRaiseToolIndex, value, false);
			}
		}
	}

	private string CurrentFuelItemName
	{
		get
		{
			if (currentFuelItemIndex < 0)
			{
				return "$item_stone";
			}
			if (fuelItems == null || fuelItems.Count == 0)
			{
				return "";
			}
			return GetFuelItemDisplayName(currentFuelItemIndex);
		}
	}

	private string CurrentLowerToolName
	{
		get
		{
			if (currentLowerToolIndex < 0)
			{
				return "";
			}
			return lowerToolDisplayNames[currentLowerToolIndex];
		}
	}

	private string CurrentRaiseToolName
	{
		get
		{
			if (currentRaiseToolIndex < 0)
			{
				return "";
			}
			return raiseToolDisplayNames[currentRaiseToolIndex];
		}
	}

	public static void RegisterRPCs()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		rpcLevelTerrain = NetworkManager.Instance.AddRPC("LevelTerrain", new CoroutineHandler(RPCS_LevelTerrain), (CoroutineHandler)null);
	}

	public static void LoadConfig(BaseUnityPlugin plugin)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Expected O, but got Unknown
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected O, but got Unknown
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Expected O, but got Unknown
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Expected O, but got Unknown
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Expected O, but got Unknown
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Expected O, but got Unknown
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Expected O, but got Unknown
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Expected O, but got Unknown
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Expected O, but got Unknown
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Expected O, but got Unknown
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Expected O, but got Unknown
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Expected O, but got Unknown
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Expected O, but got Unknown
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Expected O, but got Unknown
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Expected O, but got Unknown
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Expected O, but got Unknown
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Expected O, but got Unknown
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Expected O, but got Unknown
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Expected O, but got Unknown
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Expected O, but got Unknown
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Expected O, but got Unknown
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Expected O, but got Unknown
		//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ba: Expected O, but got Unknown
		//IL_06ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c4: Expected O, but got Unknown
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f5: Expected O, but got Unknown
		//IL_06f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Expected O, but got Unknown
		//IL_0723: Unknown result type (might be due to invalid IL or missing references)
		//IL_0728: Unknown result type (might be due to invalid IL or missing references)
		//IL_0730: Expected O, but got Unknown
		//IL_0730: Unknown result type (might be due to invalid IL or missing references)
		//IL_073a: Expected O, but got Unknown
		//IL_075e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0763: Unknown result type (might be due to invalid IL or missing references)
		//IL_076b: Expected O, but got Unknown
		//IL_076b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0775: Expected O, but got Unknown
		//IL_0799: Unknown result type (might be due to invalid IL or missing references)
		//IL_079e: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a6: Expected O, but got Unknown
		//IL_07a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b0: Expected O, but got Unknown
		//IL_07d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e1: Expected O, but got Unknown
		//IL_07e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07eb: Expected O, but got Unknown
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Expected O, but got Unknown
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Expected O, but got Unknown
		configFuelItems = plugin.Config.Bind<string>("Server", "Fuel Item List", "Wood=1,Coal=2.5,Resin=5", new ConfigDescription("Prefab name and fuel value list.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configFuelPerScan = plugin.Config.Bind<float>("Server", "Fuel Per Scan", 0.01f, new ConfigDescription("Amount of fuel to use for each scan action.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configFuelPerRaise = plugin.Config.Bind<float>("Server", "Fuel Per Raise", 0.01f, new ConfigDescription("Amount of fuel to use for raising terrain.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configFuelPerLower = plugin.Config.Bind<float>("Server", "Fuel Per Lower", 0.01f, new ConfigDescription("Amount of fuel to use for lowering terrain.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configMaximumFuel = plugin.Config.Bind<int>("Server", "Maximum Fuel", 250, new ConfigDescription("Maximum amount of fuel stored in the totem.", (AcceptableValueBase)(object)new AcceptableValueRange<int>(1, 10000), new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configStonePerRaise = plugin.Config.Bind<float>("Server", "Stone Per Raise", 0.05f, new ConfigDescription("Amount of stone to use for raising terrain.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configStonePerLower = plugin.Config.Bind<float>("Server", "Stone Per Lower", 0.05f, new ConfigDescription("Amount of stone gained from lowering terrain.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configMaximumStone = plugin.Config.Bind<int>("Server", "Maximum Stone", 250, new ConfigDescription("Maximum amount of stone stored in the totem.", (AcceptableValueBase)(object)new AcceptableValueRange<int>(1, 10000), new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configDefaultFlatteningRadius = plugin.Config.Bind<float>("Server", "Default Flattening Radius", 10f, new ConfigDescription("Default radius of the area to be flattened.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(2f, 100f), new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configMaximumFlatteningRadius = plugin.Config.Bind<float>("Server", "Maximum Flattening Radius", 40f, new ConfigDescription("Maximum radius of the area to be flattened.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(2f, 100f), new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configMinFlatteningTime = plugin.Config.Bind<float>("Server", "Min Flattening Time", 0.2f, new ConfigDescription("Time taken for a flattening action with maximum bonus.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.1f, 60f), new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configMaxFlatteningTime = plugin.Config.Bind<float>("Server", "Max Flattening Time", 1f, new ConfigDescription("Time taken for a flattening action with no bonus.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.1f, 60f), new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configScanningTime = plugin.Config.Bind<float>("Server", "Scanning Time", 0.2f, new ConfigDescription("Time taken for a scanning action.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.1f, 60f), new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configDoPainting = plugin.Config.Bind<bool>("Server", "Do Painting", true, new ConfigDescription("Paint dirt onto terrain.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configRequireLowerTool = plugin.Config.Bind<bool>("Server", "Require Lower Tool", true, new ConfigDescription("Require pickaxe to lower terrain.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configRequireRaiseTool = plugin.Config.Bind<bool>("Server", "Require Raise Tool", true, new ConfigDescription("Require hoe to raise terrain.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = true
		} }));
		configLowerToolBonus = new ConfigEntry<float>[lowerToolPrefabNames.Length];
		for (int i = 0; i < lowerToolPrefabNames.Length; i++)
		{
			configLowerToolBonus[i] = plugin.Config.Bind<float>("Server", lowerToolPrefabNames[i] + " Bonus", (float)i / (float)((lowerToolPrefabNames.Length <= 1) ? 1 : (lowerToolPrefabNames.Length - 1)), new ConfigDescription("Bonus applied for " + lowerToolPrefabNames[i] + ". 0 = Max Flattening Time. 1 = Min Flattening Time.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0f, 1f), new object[1] { (object)new ConfigurationManagerAttributes
			{
				IsAdminOnly = true
			} }));
		}
		configRaiseToolBonus = new ConfigEntry<float>[raiseToolPrefabNames.Length];
		for (int j = 0; j < raiseToolPrefabNames.Length; j++)
		{
			configRaiseToolBonus[j] = plugin.Config.Bind<float>("Server", raiseToolPrefabNames[j] + " Bonus", (float)j / (float)((raiseToolPrefabNames.Length <= 1) ? 1 : (lowerToolPrefabNames.Length - 1)), new ConfigDescription("Bonus applied for " + raiseToolPrefabNames[j] + ". 0 = Max Flattening Time. 1 = Min Flattening Time.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0f, 1f), new object[1] { (object)new ConfigurationManagerAttributes
			{
				IsAdminOnly = true
			} }));
		}
		configIncreaseRadiusKey = plugin.Config.Bind<KeyboardShortcut>("Input", "Increase flattening radius", new KeyboardShortcut((KeyCode)270, Array.Empty<KeyCode>()), (ConfigDescription)null);
		configDecreaseRadiusKey = plugin.Config.Bind<KeyboardShortcut>("Input", "Decrease flattening radius", new KeyboardShortcut((KeyCode)269, Array.Empty<KeyCode>()), (ConfigDescription)null);
		configResetScanKey = plugin.Config.Bind<KeyboardShortcut>("Input", "Reset scan position", new KeyboardShortcut((KeyCode)271, Array.Empty<KeyCode>()), (ConfigDescription)null);
		configEjectFuelKey = plugin.Config.Bind<KeyboardShortcut>("Input", "Eject fuel", new KeyboardShortcut((KeyCode)267, Array.Empty<KeyCode>()), (ConfigDescription)null);
		configEjectStoneKey = plugin.Config.Bind<KeyboardShortcut>("Input", "Eject stone", new KeyboardShortcut((KeyCode)268, Array.Empty<KeyCode>()), (ConfigDescription)null);
		configEjectToolsKey = plugin.Config.Bind<KeyboardShortcut>("Input", "Eject tools", new KeyboardShortcut((KeyCode)266, Array.Empty<KeyCode>()), (ConfigDescription)null);
		configShowMainKeys = plugin.Config.Bind<bool>("Client", "Show Main Keys", true, new ConfigDescription("Show main keys in hover text.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = false
		} }));
		configShowExtraKeys = plugin.Config.Bind<bool>("Client", "Show Extra Keys", true, new ConfigDescription("Show extra keys in hover text.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = false
		} }));
		configShowFillBars = plugin.Config.Bind<bool>("Client", "Show Fill Bars", true, new ConfigDescription("Show fill bars in hover text.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = false
		} }));
		configShowFillNumbers = plugin.Config.Bind<bool>("Client", "Show Fill Numbers", true, new ConfigDescription("Show fill numbers in hover text.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = false
		} }));
		configShowTools = plugin.Config.Bind<bool>("Client", "Show Tools", true, new ConfigDescription("Show tools in hover text.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = false
		} }));
		configShowSelection = plugin.Config.Bind<bool>("Client", "Show Selection", true, new ConfigDescription("Show selection in hover text.", (AcceptableValueBase)null, new object[1] { (object)new ConfigurationManagerAttributes
		{
			IsAdminOnly = false
		} }));
		configDefaultFlatteningRadius.SettingChanged += OnConfigChanged;
		configMaximumFlatteningRadius.SettingChanged += OnConfigChanged;
		configFuelItems.SettingChanged += OnFuelItemsChanged;
		UpdateConfig();
	}

	public static void OnConfigChanged(object sender, EventArgs eventArgs)
	{
		UpdateConfig();
	}

	public static void OnFuelItemsChanged(object sender, EventArgs eventArgs)
	{
		LoadFuelItems();
	}

	public static void UpdateConfig()
	{
		Plateautem[] array = Object.FindObjectsOfType<Plateautem>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SettingsUpdated();
		}
	}

	public static void LoadFuelItems()
	{
		fuelItems.Clear();
		MatchCollection matchCollection = new Regex("\\s*([^=,\\s]+)(?:\\s*\\=\\s*(\\d+(?:\\.\\d+)?))?\\s*(?:,|$)").Matches(configFuelItems.Value);
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
				float num = 1f;
				if (match.Groups.Count >= 3 && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
				{
					try
					{
						num = float.Parse(match.Groups[2].Value);
					}
					catch
					{
						try
						{
							num = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
						}
						catch
						{
							num = 1f;
						}
					}
				}
				fuelItems.Add(new FuelItem
				{
					prefabName = value,
					displayName = null,
					fuelValue = num
				});
				Logger.LogInfo((object)$"Adding fuel item {value} with value {num} to Plateautem.");
			}
		}
		if (fuelItems.Count == 0)
		{
			fuelItems.Add(new FuelItem
			{
				prefabName = "Wood",
				displayName = "$item_wood",
				fuelValue = 1f
			});
		}
	}

	private void Awake()
	{
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Expected O, but got Unknown
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Expected O, but got Unknown
		netView = ((Component)this).GetComponent<ZNetView>();
		zdoidCurrentRadius = StringExtensionMethods.GetStableHashCode("current_radius");
		zdoidFuelStored = StringExtensionMethods.GetStableHashCode("fuel_stored");
		zdoidStoneStored = StringExtensionMethods.GetStableHashCode("stone_stored");
		zdoidScanProgress = StringExtensionMethods.GetStableHashCode("scan_progress");
		zdoidScanIndex = StringExtensionMethods.GetStableHashCode("scan_index");
		zdoidScanSpeed = StringExtensionMethods.GetStableHashCode("scan_speed");
		zdoidLowerToolIndex = StringExtensionMethods.GetStableHashCode("lower_tool_index");
		zdoidRaiseToolIndex = StringExtensionMethods.GetStableHashCode("raise_item_index");
		if ((Object)(object)flattenTerrainOp == (Object)null || !Object.op_Implicit((Object)(object)flattenTerrainOp))
		{
			GameObject prefab = PrefabManager.Instance.GetPrefab("plateautem_flatten_op");
			if ((Object)(object)prefab == (Object)null)
			{
				prefab = PrefabManager.Instance.CreateClonedPrefab("plateautem_flatten_op", "mud_road_v2");
				flattenTerrainOp = prefab.GetComponent<TerrainOp>();
				flattenTerrainOp.m_settings.m_levelOffset = 0f;
				flattenTerrainOp.m_settings.m_level = true;
				flattenTerrainOp.m_settings.m_levelRadius = 4f;
				flattenTerrainOp.m_settings.m_square = false;
				flattenTerrainOp.m_settings.m_raise = false;
				flattenTerrainOp.m_settings.m_raiseRadius = 3f;
				flattenTerrainOp.m_settings.m_raisePower = 3f;
				flattenTerrainOp.m_settings.m_raiseDelta = 0f;
				flattenTerrainOp.m_settings.m_smooth = false;
				flattenTerrainOp.m_settings.m_smoothRadius = 3f;
				flattenTerrainOp.m_settings.m_smoothPower = 3f;
				flattenTerrainOp.m_settings.m_paintCleared = configDoPainting.Value;
				flattenTerrainOp.m_settings.m_paintHeightCheck = false;
				flattenTerrainOp.m_settings.m_paintType = (PaintType)0;
				flattenTerrainOp.m_settings.m_paintRadius = 2.5f;
			}
			else
			{
				flattenTerrainOp = prefab.GetComponent<TerrainOp>();
			}
		}
		EffectList val = new EffectList();
		val.m_effectPrefabs = (EffectData[])(object)new EffectData[2]
		{
			new EffectData
			{
				m_prefab = PrefabManager.Instance.GetPrefab("vfx_Place_mud_road")
			},
			new EffectData
			{
				m_prefab = PrefabManager.Instance.GetPrefab("sfx_build_hoe")
			}
		};
		flattenPlaceEffect = val;
		WearNTear component = ((Component)this).GetComponent<WearNTear>();
		component.m_onDestroyed = (Action)Delegate.Combine(component.m_onDestroyed, new Action(OnDestroyed));
		if ((Object)(object)netView != (Object)null)
		{
			netView.Register<int, int>("AddFuelItem", (Action<long, int, int>)RPC_AddFuelItem);
			netView.Register<ZPackage>("AddLowerTool", (Action<long, ZPackage>)RPC_AddLowerTool);
			netView.Register<ZPackage>("AddRaiseTool", (Action<long, ZPackage>)RPC_AddRaiseTool);
			netView.Register<int>("AddStone", (Action<long, int>)RPC_AddStone);
			netView.Register<float>("ChangeRadius", (Action<long, float>)RPC_ChangeRadius);
			netView.Register("ResetScan", (Action<long>)RPC_ResetScan);
			netView.Register("EjectFuel", (Action<long>)RPC_EjectFuel);
			netView.Register("EjectStone", (Action<long>)RPC_EjectStone);
			netView.Register("EjectTools", (Action<long>)RPC_EjectTools);
		}
		UpdateCircle(currentRadius);
	}

	private void OnDestroyed()
	{
		EjectFuel(clearStorage: false);
		EjectStone(clearStorage: false);
		EjectTools(ejectLowerTool: true, ejectRaiseTool: true);
	}

	public void RPC_AddFuelItem(long sender, int fuelItemIndex, int fuelItemsToAdd)
	{
		if (netView.IsOwner())
		{
			SetFuelItemStorage(fuelItemIndex, GetFuelItemStorage(fuelItemIndex) + fuelItemsToAdd);
		}
	}

	public void RPC_AddLowerTool(long sender, ZPackage package)
	{
		if (netView.IsOwner())
		{
			EjectTools(ejectLowerTool: true, ejectRaiseTool: false);
			currentLowerToolIndex = package.ReadInt();
			ReadItemToZDO(1, package, netView.GetZDO());
		}
	}

	public void RPC_AddRaiseTool(long sender, ZPackage package)
	{
		if (netView.IsOwner())
		{
			EjectTools(ejectLowerTool: false, ejectRaiseTool: true);
			currentRaiseToolIndex = package.ReadInt();
			ReadItemToZDO(2, package, netView.GetZDO());
		}
	}

	public void RPC_AddStone(long sender, int stoneToAdd)
	{
		if (netView.IsOwner())
		{
			currentStoneStored += stoneToAdd;
		}
	}

	public void RPC_ChangeRadius(long sender, float delta)
	{
		if (netView.IsOwner())
		{
			currentRadius = Mathf.Clamp(currentRadius + delta, 1f, configMaximumFlatteningRadius.Value);
		}
	}

	public void RPC_ResetScan(long sender)
	{
		if (netView.IsOwner())
		{
			currentScanProgress = 0f;
			currentScanIndex = 0;
			currentScanSpeed = 1f / configScanningTime.Value;
		}
	}

	public void RPC_EjectFuel(long sender)
	{
		if (netView.IsOwner())
		{
			EjectFuel(clearStorage: true);
		}
	}

	public void RPC_EjectStone(long sender)
	{
		if (netView.IsOwner())
		{
			EjectStone(clearStorage: true);
		}
	}

	public void RPC_EjectTools(long sender)
	{
		if (netView.IsOwner())
		{
			EjectTools(ejectLowerTool: true, ejectRaiseTool: true);
		}
	}

	private static IEnumerator RPCS_LevelTerrain(long sender, ZPackage package)
	{
		Vector3 position = package.ReadVector3();
		while (true)
		{
			bool flag = false;
			Vector2i zone = ZoneSystem.GetZone(position);
			for (int i = zone.y - 1; i <= zone.y + 1; i++)
			{
				for (int j = zone.x - 1; j <= zone.x + 1; j++)
				{
					if (ZoneSystem.instance.PokeLocalZone(zone))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				break;
			}
			yield return null;
		}
		Object.Instantiate<GameObject>(((Component)flattenTerrainOp).gameObject, position, Quaternion.identity);
	}

	private void Update()
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid())
		{
			return;
		}
		UpdateCircle(currentRadius);
		if (!netView.IsOwner())
		{
			UpdateOrb(isOwner: false);
			return;
		}
		bool flag = !configRequireLowerTool.Value || currentLowerToolIndex >= 0;
		bool flag2 = !configRequireRaiseTool.Value || currentRaiseToolIndex >= 0;
		if (flag || flag2)
		{
			int num = CountPointsInSpiral(currentRadius + 2.6f, 2.6f);
			if (currentScanProgress > (float)num)
			{
				currentScanProgress = 0f;
				currentScanIndex = 0;
				currentScanSpeed = 1f / configScanningTime.Value;
			}
			if (currentScanProgress >= (float)currentScanIndex)
			{
				PolarPointOnSpiral(currentScanProgress, 2.6f, out var angle, out var radius);
				float num2 = Mathf.Min(radius, currentRadius);
				Vector3 val = default(Vector3);
				((Vector3)(ref val))._002Ector(Mathf.Sin(angle) * num2, 0f, Mathf.Cos(angle) * num2);
				Vector3 val2 = ((Component)this).transform.position + val;
				float num3 = 0f;
				float num4 = 0f;
				float num5 = 0f;
				float num6 = 0f;
				float num7 = 0f;
				if (!IsInsideNoBuildLocation(val2, 4f) && IsInLoadedArea(val2, 5f))
				{
					foreach (Vector4 item in EachGroundPointOnSpiral(val2, 2f))
					{
						if (item.y > val2.y + 0.1f)
						{
							float num8 = (item.y - val2.y) * 0.25f;
							num4 += num8;
							num5 += num8 * configFuelPerLower.Value;
							num6 -= num8 * configStonePerLower.Value;
							num7 += num8 * Mathf.Pow(Mathf.Clamp01(1f - item.w / 2f), 0.3f);
						}
						else if (item.y < val2.y - 0.1f)
						{
							float num9 = (val2.y - item.y) * 0.25f;
							num3 += num9;
							num5 += num9 * configFuelPerRaise.Value;
							num6 += num9 * configStonePerRaise.Value;
							num7 += num9 * Mathf.Pow(Mathf.Clamp01(1f - item.w / 2f), 0.3f);
						}
					}
				}
				float num10 = num3 - num4;
				bool flag3 = num7 > 0.75f && (flag || num10 > 0f) && (flag2 || num10 < 0f);
				if (flag3 && num6 > 0f && currentStoneStored < num6)
				{
					flag3 = false;
				}
				if (flag3 && num6 < 0f && (float)configMaximumStone.Value - currentStoneStored < 0f - num6)
				{
					flag3 = false;
				}
				float num11 = 1f / configScanningTime.Value;
				if (flag3)
				{
					num5 += configFuelPerScan.Value;
				}
				else
				{
					num5 = configFuelPerScan.Value;
					num6 = 0f;
				}
				bool flag4 = num7 < 0.1f;
				if (GetTotalFuelStored() >= num5)
				{
					ConsumeFuel(num5);
					currentStoneStored -= num6;
					if (flag3)
					{
						float num12 = 0f;
						if (num10 < 0f && currentLowerToolIndex >= 0)
						{
							num12 = configLowerToolBonus[currentLowerToolIndex].Value;
						}
						else if (num10 > 0f && currentRaiseToolIndex >= 0)
						{
							num12 = configRaiseToolBonus[currentRaiseToolIndex].Value;
						}
						num11 = 1f / (flag4 ? configScanningTime.Value : Mathf.Lerp(configMaxFlatteningTime.Value, configMinFlatteningTime.Value, num12));
						Object.Instantiate<GameObject>(((Component)flattenTerrainOp).gameObject, val2, Quaternion.identity);
						if (!flag4)
						{
							EffectList obj = flattenPlaceEffect;
							if (obj != null)
							{
								obj.Create(val2, Quaternion.identity, ((Component)this).transform, 1f, -1);
							}
						}
					}
					targetDroneAudioVolume = 1f;
					currentScanIndex++;
					if (currentScanIndex < num)
					{
						currentScanProgress += Time.deltaTime * currentScanSpeed;
					}
					else
					{
						currentScanIndex = 0;
						currentScanProgress = 0f;
					}
				}
				else
				{
					targetDroneAudioVolume = 0f;
				}
				currentScanSpeed = num11;
			}
			else
			{
				currentScanProgress += Time.deltaTime * currentScanSpeed;
			}
		}
		UpdateOrb(isOwner: true);
	}

	private void UpdateOrb(bool isOwner)
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		float num = currentScanProgress;
		if (!isOwner)
		{
			if (previousScanTime == 0f)
			{
				previousScanTime = Time.time;
			}
			if (num > targetScanProgress)
			{
				previousScanTime2 = previousScanTime;
				previousScanTime = Time.time;
				previousScanProgress = targetScanProgress;
				targetScanProgress = num;
			}
			else if (num < previousScanProgress)
			{
				previousScanTime2 = previousScanTime;
				previousScanTime = Time.time;
				targetScanProgress = (previousScanProgress = num);
			}
			float num2 = (targetScanProgress - previousScanProgress) / Mathf.Max(0.001f, previousScanTime - previousScanTime2);
			num = Mathf.Lerp(previousScanProgress, targetScanProgress, Mathf.Clamp01((Time.time - previousScanTime) * num2));
		}
		PolarPointOnSpiral(num, 2.6f, out var angle, out var radius);
		radius = Mathf.Min(radius, currentRadius);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(Mathf.Sin(angle) * radius, 0.5f, Mathf.Cos(angle) * radius);
		Vector3 position = ((Component)this).transform.position + val;
		if ((Object)(object)lightTransform != (Object)null)
		{
			lightTransform.position = position;
		}
		if ((Object)(object)flareTransform != (Object)null)
		{
			flareTransform.position = position;
		}
		if ((Object)(object)droneAudioSourceTransform != (Object)null)
		{
			droneAudioSourceTransform.position = position;
		}
		if ((Object)(object)droneAudioSource != (Object)null)
		{
			if (!droneAudioSource.isPlaying)
			{
				((Component)droneAudioSource).gameObject.SetActive(true);
				((Behaviour)droneAudioSource).enabled = true;
				droneAudioSource.loop = true;
				droneAudioSource.Play();
			}
			droneAudioSource.volume = Mathf.MoveTowards(droneAudioSource.volume, targetDroneAudioVolume, Time.deltaTime);
		}
	}

	private string GetFuelItemDisplayName(int index)
	{
		if (index == -1)
		{
			return "$item_stone";
		}
		FuelItem value = fuelItems[index];
		if (value.displayName == null)
		{
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(value.prefabName);
			ItemDrop val = ((itemPrefab != null) ? itemPrefab.GetComponent<ItemDrop>() : null);
			if ((Object)(object)val != (Object)null)
			{
				value.displayName = val.m_itemData.m_shared.m_name;
				fuelItems[index] = value;
			}
		}
		return value.displayName;
	}

	private static IEnumerable<Vector4> EachGroundPointOnSpiral(Vector3 origin, float radius, float sampleSpacing = 0.5f)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		float num = default(float);
		foreach (Vector4 item in EachPointOnSpiral(radius, sampleSpacing))
		{
			((Vector3)(ref val))._002Ector(origin.x + item.x, origin.y, origin.z + item.y);
			if (ZoneSystem.instance.GetGroundHeight(val, ref num))
			{
				yield return new Vector4(val.x, num, val.z, item.w);
			}
		}
	}

	public static IEnumerable<Vector4> EachPointOnSpiral(float radius, float sampleSpacing)
	{
		int count = CountPointsInSpiral(radius, sampleSpacing);
		for (int i = 0; i < count; i++)
		{
			PolarPointOnSpiral(i, sampleSpacing, out var angle, out var radius2);
			yield return new Vector4(Mathf.Sin(angle) * radius2, Mathf.Cos(angle) * radius2, angle, radius2);
		}
	}

	public static IEnumerable<Vector2> EachPointOnSpiralPolar(float radius, float sampleSpacing)
	{
		int count = CountPointsInSpiral(radius, sampleSpacing);
		for (int i = 0; i < count; i++)
		{
			PolarPointOnSpiral(i, sampleSpacing, out var angle, out var radius2);
			yield return new Vector2(angle, radius2);
		}
	}

	public static void PolarPointOnSpiral(float t, float spacing, out float angle, out float radius)
	{
		angle = Mathf.Sqrt(t) * 3.542f;
		radius = angle * spacing / ((float)Math.PI * 2f);
	}

	public static int CountPointsInSpiral(float radius, float spacing)
	{
		float num = radius / spacing;
		return Mathf.CeilToInt(num * num * 3.146755f);
	}

	internal void BuildPrefab(PrivateArea privateArea, AssetBundle assetBundle)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		circleProjector = privateArea.m_areaMarker;
		((Component)circleProjector).gameObject.SetActive(true);
		enabledEffect = privateArea.m_enabledEffect;
		model = privateArea.m_model;
		Material[] materials = ((Renderer)model).materials;
		Color val = default(Color);
		((Color)(ref val))._002Ector(0.3f, 0.3f, 1f);
		Material[] array = materials;
		foreach (Material val2 in array)
		{
			if (((Object)val2).name.StartsWith("Guardstone_OdenGlow_mat"))
			{
				val2.SetColor("_EmissionColor", val);
			}
		}
		ParticleSystem component = ((Component)enabledEffect.transform.Find("sparcs")).gameObject.GetComponent<ParticleSystem>();
		ShapeModule shape = component.shape;
		((ShapeModule)(ref shape)).scale = new Vector3(8f, 0.5f, 8f);
		MainModule main = component.main;
		((MainModule)(ref main)).startColor = new MinMaxGradient(val, val * 0.15f);
		GameObject gameObject = ((Component)enabledEffect.transform.Find("Point light")).gameObject;
		lightTransform = gameObject.transform;
		Light component2 = gameObject.GetComponent<Light>();
		component2.color = new Color(0f, 0f, 0.8f, 0.4f);
		component2.intensity = 3f;
		component2.range = 9f;
		circleProjector.m_radius = currentRadius + 1f;
		circleProjector.m_nrOfSegments = Mathf.CeilToInt(circleProjector.m_radius * 4f);
		GameObject gameObject2 = ((Component)enabledEffect.transform.Find("flare")).gameObject;
		flareTransform = gameObject2.transform;
		MainModule main2 = gameObject2.GetComponent<ParticleSystem>().main;
		((MainModule)(ref main2)).startColor = new MinMaxGradient(val);
		((MainModule)(ref main2)).startSize = new MinMaxCurve(1f);
		GameObject val3 = Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("DroneAudioSource"), Vector3.zero, Quaternion.identity, ((Component)this).transform);
		droneAudioSourceTransform = val3.transform;
		droneAudioSource = val3.GetComponent<AudioSource>();
	}

	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
		{
			return false;
		}
		if (((Character)Player.m_localPlayer).InPlaceMode())
		{
			return false;
		}
		if (hold)
		{
			if (Time.time - lastUseTime < 1f)
			{
				return false;
			}
			lastUseTime = Time.time;
			if (currentFuelItemIndex == -1)
			{
				return TakeStoneFromUser(user, takeAll: true);
			}
			return TakeFuelItemFromUser(user, currentFuelItemIndex, takeAll: true);
		}
		lastUseTime = Time.time;
		if (currentFuelItemIndex == -1)
		{
			return TakeStoneFromUser(user, takeAll: false);
		}
		return TakeFuelItemFromUser(user, currentFuelItemIndex, takeAll: false);
	}

	public bool UseItem(Humanoid user, ItemData item)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
		{
			return false;
		}
		if (((Character)Player.m_localPlayer).InPlaceMode())
		{
			return false;
		}
		if (item == null)
		{
			return false;
		}
		string name = item.m_shared.m_name;
		for (int i = -1; i < fuelItems.Count; i++)
		{
			if (name == GetFuelItemDisplayName(i))
			{
				currentFuelItemIndex = i;
				((Character)user).Message((MessageType)2, Localization.instance.Localize("$piece_plateautem_selectMode", new string[1] { name }), 0, (Sprite)null);
				return true;
			}
		}
		Inventory inventory = user.GetInventory();
		if (inventory == null)
		{
			return false;
		}
		for (int j = 0; j < lowerToolDisplayNames.Length; j++)
		{
			if (name == lowerToolDisplayNames[j])
			{
				ZPackage val = new ZPackage();
				val.Write(j);
				WriteItem(val, item);
				netView.InvokeRPC("AddLowerTool", new object[1] { val });
				user.UnequipItem(item, true);
				inventory.RemoveOneItem(item);
				return true;
			}
		}
		for (int k = 0; k < raiseToolDisplayNames.Length; k++)
		{
			if (name == raiseToolDisplayNames[k])
			{
				ZPackage val2 = new ZPackage();
				val2.Write(k);
				WriteItem(val2, item);
				netView.InvokeRPC("AddRaiseTool", new object[1] { val2 });
				user.UnequipItem(item, true);
				inventory.RemoveOneItem(item);
				return true;
			}
		}
		return false;
	}

	public string GetHoverName()
	{
		return "$piece_plateautem";
	}

	public string GetHoverText()
	{
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		float totalFuelStored = GetTotalFuelStored();
		StringBuilder stringBuilder = new StringBuilder();
		if (configShowTools.Value)
		{
			stringBuilder.Append("$piece_plateautem_tools: " + GetToolNamesText(" + ") + "\n");
		}
		if (configShowFillBars.Value)
		{
			stringBuilder.Append("[<color=orange>" + TextProgressBar(totalFuelStored / (float)configMaximumFuel.Value, 12) + "</color>]\n");
		}
		if (configShowFillNumbers.Value)
		{
			stringBuilder.Append(string.Format("{0}: {1:0.0}/{2}\n", "$piece_plateautem_fuel", totalFuelStored, configMaximumFuel.Value));
		}
		if (configShowFillBars.Value)
		{
			stringBuilder.Append("[<color=#7F7F7FFF>" + TextProgressBar(currentStoneStored / (float)configMaximumStone.Value, 12) + "</color>]\n");
		}
		if (configShowFillNumbers.Value)
		{
			stringBuilder.Append(string.Format("{0}: {1:0.0}/{2}\n", "$item_stone", currentStoneStored, configMaximumStone.Value));
		}
		if (configShowSelection.Value)
		{
			stringBuilder.Append(string.Format(" {0} {1}: {2}\n", (currentFuelItemIndex == -1) ? '●' : '○', "$item_stone", Mathf.FloorToInt(currentStoneStored)));
			if (fuelItems != null)
			{
				for (int i = 0; i < fuelItems.Count; i++)
				{
					stringBuilder.Append($" {((currentFuelItemIndex == i) ? '●' : '○')} {GetFuelItemDisplayName(i)}: {GetFuelItemStorage(i)}\n");
				}
			}
		}
		if (configShowMainKeys.Value)
		{
			stringBuilder.Append("[<color=yellow><b>1-8</b></color>] $piece_plateautem_selectFuel\n");
			stringBuilder.Append("[<color=yellow><b>$KEY_Use</b></color>] $piece_smelter_add " + CurrentFuelItemName + "\n");
			stringBuilder.Append("[$piece_plateautem_hold <color=yellow><b>$KEY_Use</b></color>] $piece_smelter_add $piece_plateautem_all " + CurrentFuelItemName + "\n");
			stringBuilder.Append(string.Format("[<color=yellow>{0}</color>/<color=yellow>{1}</color>] {2}: {3}\n", configIncreaseRadiusKey.Value, configDecreaseRadiusKey.Value, "$piece_plateautem_radius", Mathf.RoundToInt(currentRadius)));
		}
		if (configShowExtraKeys.Value)
		{
			stringBuilder.Append(string.Format("[<color=yellow>{0}</color>] {1}\n", configEjectFuelKey.Value, "$piece_plateautem_ejectFuel"));
			stringBuilder.Append(string.Format("[<color=yellow>{0}</color>] {1}\n", configEjectStoneKey.Value, "$piece_plateautem_ejectStone"));
			if (currentLowerToolIndex >= 0 || currentRaiseToolIndex >= 0)
			{
				stringBuilder.Append(string.Format("[<color=yellow>{0}</color>] {1} {2}\n", configEjectToolsKey.Value, "$piece_plateautem_eject", GetToolNamesText(" + ")));
			}
			stringBuilder.Append(string.Format("[<color=yellow>{0}</color>] {1}\n", configResetScanKey.Value, "$piece_plateautem_reset"));
		}
		HoverUpdate();
		return Localization.instance.Localize(stringBuilder.ToString());
	}

	private string GetToolNamesText(string separator)
	{
		if (currentLowerToolIndex >= 0)
		{
			if (currentRaiseToolIndex >= 0)
			{
				return lowerToolDisplayNames[currentLowerToolIndex] + separator + raiseToolDisplayNames[currentRaiseToolIndex];
			}
			return lowerToolDisplayNames[currentLowerToolIndex];
		}
		if (currentRaiseToolIndex >= 0)
		{
			return raiseToolDisplayNames[currentRaiseToolIndex];
		}
		return "$piece_smelter_empty";
	}

	private void HoverUpdate()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
		{
			KeyboardShortcut value = configIncreaseRadiusKey.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				netView.InvokeRPC("ChangeRadius", new object[1] { 1f });
			}
			value = configDecreaseRadiusKey.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				netView.InvokeRPC("ChangeRadius", new object[1] { -1f });
			}
			value = configResetScanKey.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				netView.InvokeRPC("ResetScan", Array.Empty<object>());
			}
			value = configEjectFuelKey.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				netView.InvokeRPC("EjectFuel", Array.Empty<object>());
			}
			value = configEjectStoneKey.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				netView.InvokeRPC("EjectStone", Array.Empty<object>());
			}
			value = configEjectToolsKey.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				netView.InvokeRPC("EjectTools", Array.Empty<object>());
			}
		}
	}

	private static void WriteItem(ZPackage package, ItemData item)
	{
		package.Write(item.m_durability);
		package.Write(item.m_stack);
		package.Write(item.m_quality);
		package.Write(item.m_variant);
		package.Write(item.m_crafterID);
		package.Write(item.m_crafterName);
		package.Write(item.m_customData.Count);
		foreach (KeyValuePair<string, string> customDatum in item.m_customData)
		{
			package.Write(customDatum.Key);
			package.Write(customDatum.Value);
		}
		package.Write((byte)item.m_worldLevel);
		package.Write(item.m_pickedUp);
	}

	private static void ReadItem(ZPackage package, ItemData item)
	{
		item.m_durability = package.ReadSingle();
		item.m_stack = package.ReadInt();
		item.m_quality = package.ReadInt();
		item.m_variant = package.ReadInt();
		item.m_crafterID = package.ReadLong();
		item.m_crafterName = package.ReadString();
		int num = package.ReadInt();
		item.m_customData.Clear();
		for (int i = 0; i < num; i++)
		{
			string key = package.ReadString();
			string value = package.ReadString();
			item.m_customData[key] = value;
		}
		item.m_worldLevel = package.ReadByte();
		item.m_pickedUp = package.ReadBool();
	}

	private static void ReadItemToZDO(int index, ZPackage package, ZDO zdo)
	{
		string text = index.ToString();
		zdo.Set(text + "_durability", package.ReadSingle());
		zdo.Set(text + "_stack", package.ReadInt());
		zdo.Set(text + "_quality", package.ReadInt());
		zdo.Set(text + "_variant", package.ReadInt());
		zdo.Set(text + "_crafterID", package.ReadLong());
		zdo.Set(text + "_crafterName", package.ReadString());
		int num;
		zdo.Set(text + "_dataCount", num = package.ReadInt());
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			zdo.Set($"{index}_data_{num2}", package.ReadString());
			zdo.Set($"{index}_data__{num2++}", package.ReadString());
		}
		zdo.Set(index + "_worldLevel", (int)package.ReadByte());
		zdo.Set(index + "_pickedUp", package.ReadBool());
	}

	public static void LoadFromZDO(int index, ItemData itemData, ZDO zdo)
	{
		string text = index.ToString();
		itemData.m_durability = zdo.GetFloat(text + "_durability", itemData.m_durability);
		itemData.m_stack = zdo.GetInt(text + "_stack", itemData.m_stack);
		itemData.m_quality = zdo.GetInt(text + "_quality", itemData.m_quality);
		itemData.m_variant = zdo.GetInt(text + "_variant", itemData.m_variant);
		itemData.m_crafterID = zdo.GetLong(text + "_crafterID", itemData.m_crafterID);
		itemData.m_crafterName = zdo.GetString(text + "_crafterName", itemData.m_crafterName);
		int @int = zdo.GetInt(text + "_dataCount", 0);
		itemData.m_customData.Clear();
		for (int i = 0; i < @int; i++)
		{
			itemData.m_customData[zdo.GetString(text + $"_data_{i}", "")] = zdo.GetString(text + $"_data__{i}", "");
		}
		itemData.m_worldLevel = (byte)zdo.GetInt(index + "_worldLevel", itemData.m_worldLevel);
		itemData.m_pickedUp = zdo.GetBool(index + "_pickedUp", itemData.m_pickedUp);
	}

	private void EjectFuel(bool clearStorage)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if (fuelItems != null)
		{
			for (int i = 0; i < fuelItems.Count; i++)
			{
				FuelItem fuelItem = fuelItems[i];
				ObjectDB instance = ObjectDB.instance;
				object obj;
				if (instance == null)
				{
					obj = null;
				}
				else
				{
					GameObject itemPrefab = instance.GetItemPrefab(fuelItem.prefabName);
					obj = ((itemPrefab != null) ? itemPrefab.GetComponent<ItemDrop>() : null);
				}
				ItemDrop val = (ItemDrop)obj;
				if (!((Object)(object)val != (Object)null))
				{
					continue;
				}
				int maxStackSize = val.m_itemData.m_shared.m_maxStackSize;
				if (maxStackSize <= 0)
				{
					continue;
				}
				int num2 = GetFuelItemStorage(i);
				if (clearStorage)
				{
					SetFuelItemStorage(i, 0);
				}
				while (num2 > 0)
				{
					int num3 = Mathf.Min(num2, maxStackSize);
					num2 -= num3;
					Vector3 val2 = ((Component)this).transform.position + Vector3.up * 1.2f + Random.insideUnitSphere * 0.25f;
					Quaternion val3 = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
					ItemDrop component = Object.Instantiate<GameObject>(((Component)val).gameObject, val2, val3).GetComponent<ItemDrop>();
					if ((Object)(object)component != (Object)null)
					{
						component.m_itemData.m_stack = num3;
						num += num3;
					}
				}
			}
		}
		if (num == 0 && clearStorage)
		{
			currentFuelStored = 0f;
		}
	}

	private void EjectStone(bool clearStorage)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		ObjectDB instance = ObjectDB.instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			GameObject itemPrefab = instance.GetItemPrefab("Stone");
			obj = ((itemPrefab != null) ? itemPrefab.GetComponent<ItemDrop>() : null);
		}
		ItemDrop val = (ItemDrop)obj;
		if ((Object)(object)val != (Object)null)
		{
			int maxStackSize = val.m_itemData.m_shared.m_maxStackSize;
			if (maxStackSize > 0)
			{
				int num2 = Mathf.FloorToInt(currentStoneStored);
				if (clearStorage)
				{
					currentStoneStored -= num2;
				}
				while (num2 > 0)
				{
					int num3 = Mathf.Min(num2, maxStackSize);
					num2 -= num3;
					Vector3 val2 = ((Component)this).transform.position + Vector3.up * 1.2f + Random.insideUnitSphere * 0.25f;
					Quaternion val3 = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
					ItemDrop component = Object.Instantiate<GameObject>(((Component)val).gameObject, val2, val3).GetComponent<ItemDrop>();
					if ((Object)(object)component != (Object)null)
					{
						component.m_itemData.m_stack = num3;
						num += num3;
					}
				}
			}
		}
		if (num == 0 && clearStorage)
		{
			currentStoneStored = 0f;
		}
	}

	private void EjectTools(bool ejectLowerTool, bool ejectRaiseTool)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		if (ejectLowerTool && currentLowerToolIndex >= 0)
		{
			ObjectDB instance = ObjectDB.instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				GameObject itemPrefab = instance.GetItemPrefab(lowerToolPrefabNames[currentLowerToolIndex]);
				obj = ((itemPrefab != null) ? itemPrefab.GetComponent<ItemDrop>() : null);
			}
			ItemDrop val = (ItemDrop)obj;
			if ((Object)(object)val != (Object)null)
			{
				Vector3 val2 = ((Component)this).transform.position + Vector3.up * 1.2f + Random.insideUnitSphere * 0.25f;
				Quaternion val3 = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
				ItemDrop component = Object.Instantiate<GameObject>(((Component)val).gameObject, val2, val3).GetComponent<ItemDrop>();
				if ((Object)(object)component != (Object)null)
				{
					ItemDrop.LoadFromZDO(1, component.m_itemData, netView.GetZDO());
				}
			}
			currentLowerToolIndex = -1;
		}
		if (!ejectRaiseTool || currentRaiseToolIndex < 0)
		{
			return;
		}
		ObjectDB instance2 = ObjectDB.instance;
		object obj2;
		if (instance2 == null)
		{
			obj2 = null;
		}
		else
		{
			GameObject itemPrefab2 = instance2.GetItemPrefab(raiseToolPrefabNames[currentRaiseToolIndex]);
			obj2 = ((itemPrefab2 != null) ? itemPrefab2.GetComponent<ItemDrop>() : null);
		}
		ItemDrop val4 = (ItemDrop)obj2;
		if ((Object)(object)val4 != (Object)null)
		{
			Vector3 val5 = ((Component)this).transform.position + Vector3.up * 1.2f + Random.insideUnitSphere * 0.25f;
			Quaternion val6 = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
			ItemDrop component2 = Object.Instantiate<GameObject>(((Component)val4).gameObject, val5, val6).GetComponent<ItemDrop>();
			if ((Object)(object)component2 != (Object)null)
			{
				ItemDrop.LoadFromZDO(2, component2.m_itemData, netView.GetZDO());
			}
		}
		currentRaiseToolIndex = -1;
	}

	private void UpdateCircle(float radius)
	{
		if ((Object)(object)circleProjector != (Object)null && circleProjector.m_radius != radius + 1f)
		{
			circleProjector.m_radius = radius + 1f;
			circleProjector.m_nrOfSegments = Mathf.CeilToInt(circleProjector.m_radius * 4f);
		}
	}

	private void SettingsUpdated()
	{
		if (currentRadius > configMaximumFlatteningRadius.Value)
		{
			currentRadius = configMaximumFlatteningRadius.Value;
		}
		UpdateCircle(currentRadius);
	}

	private string TextProgressBar(float fraction, int length)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = Mathf.RoundToInt(fraction * (float)length * 2f);
		for (int i = 0; i < length; i++)
		{
			if (num >= 2)
			{
				num -= 2;
				stringBuilder.Append("█");
			}
			else if (num >= 1)
			{
				num--;
				stringBuilder.Append("▌");
			}
			else
			{
				stringBuilder.Append("─");
			}
		}
		return stringBuilder.ToString();
	}

	private bool TakeFuelItemFromUser(Humanoid user, int fuelItemIndex, bool takeAll)
	{
		Inventory inventory = user.GetInventory();
		if (inventory == null)
		{
			return false;
		}
		string fuelItemDisplayName = GetFuelItemDisplayName(fuelItemIndex);
		float fuelValue = fuelItems[fuelItemIndex].fuelValue;
		int num = inventory.CountItems(fuelItemDisplayName, -1, true);
		if (num <= 0)
		{
			((Character)user).Message((MessageType)2, Localization.instance.Localize("$piece_plateautem_noFuel", new string[1] { fuelItemDisplayName }), 0, (Sprite)null);
			return false;
		}
		float totalFuelStored = GetTotalFuelStored();
		int num2 = Mathf.FloorToInt(((float)configMaximumFuel.Value - totalFuelStored) / fuelValue);
		if (num2 <= 0)
		{
			((Character)user).Message((MessageType)2, Localization.instance.Localize("$msg_cantaddmore", new string[1] { fuelItemDisplayName }), 0, (Sprite)null);
			return false;
		}
		int num3 = ((!takeAll) ? 1 : Mathf.Min(num2, num));
		netView.InvokeRPC("AddFuelItem", new object[2] { fuelItemIndex, num3 });
		inventory.RemoveItem(fuelItemDisplayName, num3, -1, true);
		((Character)user).Message((MessageType)2, Localization.instance.Localize($"$msg_added {num3} {fuelItemDisplayName}"), 0, (Sprite)null);
		return true;
	}

	private bool TakeStoneFromUser(Humanoid user, bool takeAll)
	{
		Inventory inventory = user.GetInventory();
		if (inventory == null)
		{
			return false;
		}
		int num = inventory.CountItems("$item_stone", -1, true);
		if (num <= 0)
		{
			((Character)user).Message((MessageType)2, Localization.instance.Localize("$piece_plateautem_noFuel", new string[1] { "$item_stone" }), 0, (Sprite)null);
			return false;
		}
		int num2 = Mathf.FloorToInt((float)configMaximumStone.Value - currentStoneStored);
		if (num2 <= 0)
		{
			((Character)user).Message((MessageType)2, Localization.instance.Localize("$msg_cantaddmore", new string[1] { "$item_stone" }), 0, (Sprite)null);
			return false;
		}
		int num3 = ((!takeAll) ? 1 : Mathf.Min(num2, num));
		netView.InvokeRPC("AddStone", new object[1] { num3 });
		inventory.RemoveItem("$item_stone", num3, -1, true);
		((Character)user).Message((MessageType)2, Localization.instance.Localize(string.Format("$msg_added {0} {1}", num3, "$item_stone")), 0, (Sprite)null);
		return true;
	}

	private int GetFuelItemStorage(int index, int defaultAmount = 0)
	{
		if (!Object.op_Implicit((Object)(object)netView) || !netView.IsValid() || (Object)(object)Player.m_localPlayer == (Object)null)
		{
			return 0;
		}
		return netView.GetZDO().GetInt($"fuel_storage_{index}", defaultAmount);
	}

	private void SetFuelItemStorage(int index, int amount)
	{
		if (Object.op_Implicit((Object)(object)netView) && netView.IsValid() && !((Object)(object)Player.m_localPlayer == (Object)null))
		{
			netView.GetZDO().Set($"fuel_storage_{index}", amount);
		}
	}

	private float GetTotalFuelStored()
	{
		if (fuelItems == null)
		{
			return currentFuelStored;
		}
		float num = currentFuelStored;
		for (int i = 0; i < fuelItems.Count; i++)
		{
			num += (float)GetFuelItemStorage(i) * fuelItems[i].fuelValue;
		}
		return num;
	}

	private void ConsumeFuel(float amount)
	{
		float num = currentFuelStored;
		while (amount > 0f)
		{
			if (num >= amount)
			{
				num -= amount;
				break;
			}
			amount -= num;
			num = 0f;
			for (int i = 0; i < fuelItems.Count; i++)
			{
				FuelItem fuelItem = fuelItems[i];
				int fuelItemStorage = GetFuelItemStorage(i);
				if (fuelItemStorage > 0)
				{
					fuelItemStorage--;
					SetFuelItemStorage(i, fuelItemStorage);
					num += fuelItem.fuelValue;
					break;
				}
			}
		}
		currentFuelStored = num;
	}

	public static bool IsInsideNoBuildLocation(Vector3 point, float radius)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Location s_allLocation in Location.s_allLocations)
		{
			if (s_allLocation.m_noBuild && s_allLocation.IsInside(point, radius, false))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsInLoadedArea(Vector3 point, float radius, bool checkSelf = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = ZoneSystem.GetZone(point - new Vector3(radius, 0f, radius));
		Vector2i zone2 = ZoneSystem.GetZone(point + new Vector3(radius, 0f, radius));
		if (checkSelf)
		{
			Vector2i zone3 = ZoneSystem.GetZone(ZNet.instance.GetReferencePosition());
			bool flag = true;
			for (int i = zone.y; i <= zone2.y; i++)
			{
				for (int j = zone.x; j <= zone2.x; j++)
				{
					if (!ZNetScene.InActiveArea(new Vector2i(j, i), zone3))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return true;
			}
		}
		foreach (ZNetPeer connectedPeer in ZNet.instance.GetConnectedPeers())
		{
			Vector2i zone4 = ZoneSystem.GetZone(connectedPeer.GetRefPos());
			bool flag2 = true;
			for (int k = zone.y; k <= zone2.y; k++)
			{
				for (int l = zone.x; l <= zone2.x; l++)
				{
					if (!ZNetScene.InActiveArea(new Vector2i(l, k), zone4))
					{
						flag2 = false;
						break;
					}
				}
			}
			if (flag2)
			{
				return true;
			}
		}
		return false;
	}
}
