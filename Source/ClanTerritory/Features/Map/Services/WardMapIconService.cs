using System.Collections.Generic;
using System.IO;
using BepInEx;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.Territory.Zdo;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Integration.Guilds;
using ClanTerritory.Utils;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Features.Map.Services
{
    internal sealed class WardMapIconService
    {
        private const string PinNamePrefix = "ClanTerritory_Ward_";

        private const string AssetBundleFileName = "clanterritory_mapicons";
        private const string NeutralSpriteName = "clan_ward_neutral";
        private const string GuildSpriteName = "clan_ward_clan";

        private readonly TerritoryZdoService _zdoService;
        private readonly TerritoryRegistry _territoryRegistry;

        private static readonly System.Reflection.FieldInfo MinimapPinsField =
            AccessTools.Field(typeof(Minimap), "m_pins");

        private readonly Dictionary<string, Minimap.PinData> _pins =
            new Dictionary<string, Minimap.PinData>();

        private AssetBundle _assetBundle;
        private Sprite _neutralSprite;
        private Sprite _guildSprite;

        public WardMapIconService(
            TerritoryZdoService zdoService,
            TerritoryRegistry territoryRegistry)
        {
            _zdoService = zdoService;
            _territoryRegistry = territoryRegistry;
        }

        public void Initialize()
        {
            LoadIcons();

            ModLog.Info("[Map] Ward map icon service initialized.");
        }

        public void SyncAllFromZdo()
        {
            RemoveAll();

            if (Minimap.instance == null)
            {
                ModLog.Info("[Map] Minimap not ready. Ward pins sync skipped.");
                return;
            }

            List<WardModel> wards = _zdoService.GetAllWards();

            foreach (WardModel ward in wards)
                AddOrUpdate(ward);

            ModLog.Info("[Map] Ward map pins synced. Count: " + _pins.Count);
        }

        public void AddOrUpdate(WardModel ward)
        {
            if (ward == null)
                return;

            if (Minimap.instance == null)
                return;

            TrySyncWardGuildFromLocalPlayer(ward);

            string pinName = SelectPinName(ward);
            Sprite selectedSprite = null;

            Minimap.PinData existing;

            if (_pins.TryGetValue(ward.Id, out existing))
            {
                if (IsPinStillRegistered(existing))
                {
                    selectedSprite = SelectSprite(ward, existing.m_icon);
                    existing.m_pos = ward.Position;
                    existing.m_name = pinName;
                    existing.m_icon = selectedSprite;
                    UpdatePinIconElement(existing, selectedSprite);
                    return;
                }

                _pins.Remove(ward.Id);
            }

            Minimap.PinData pin =
                Minimap.instance.AddPin(
                    ward.Position,
                    Minimap.PinType.Icon0,
                    pinName,
                    false,
                    false,
                    0L);

            selectedSprite = SelectSprite(ward, pin.m_icon);
            pin.m_icon = selectedSprite;
            pin.m_doubleSize = true;
            UpdatePinIconElement(pin, selectedSprite);

            _pins[ward.Id] = pin;

            ModLog.Info("[Map] Ward map pin added: " + ward.Id + ", name: " + pinName);
        }

        public void Remove(string wardId)
        {
            if (string.IsNullOrEmpty(wardId))
                return;

            Minimap.PinData pin;

            if (!_pins.TryGetValue(wardId, out pin))
                return;

            if (Minimap.instance != null && IsPinStillRegistered(pin))
                Minimap.instance.RemovePin(pin);

            _pins.Remove(wardId);

            ModLog.Info("[Map] Ward map pin removed: " + wardId);
        }

        public void RemoveAll()
        {
            if (Minimap.instance != null)
            {
                foreach (Minimap.PinData pin in _pins.Values)
                {
                    if (IsPinStillRegistered(pin))
                        Minimap.instance.RemovePin(pin);
                }
            }

            _pins.Clear();
        }

        private string SelectPinName(WardModel ward)
        {
            string guildName;

            if (ward != null &&
                TerritoryGuildAccess.TryGetWardGuildName(
                    ward.Id,
                    out guildName))
            {
                return guildName;
            }

            return PinNamePrefix + ward.Id;
        }

        private Sprite SelectSprite(WardModel ward, Sprite fallback)
        {
            if (HasGuild(ward))
            {
                Sprite guildIcon;

                if (TerritoryGuildAccess.TryGetWardGuildIcon(
                        ward.Id,
                        out guildIcon) &&
                    guildIcon != null)
                {
                    return guildIcon;
                }

                if (_guildSprite != null)
                    return _guildSprite;
            }

            if (_neutralSprite != null)
                return _neutralSprite;

            return fallback;
        }

        private bool HasGuild(WardModel ward)
        {
            string guildId;

            return ward != null &&
                   TerritoryGuildAccess.TryGetWardGuildId(
                       ward.Id,
                       out guildId);
        }

        private static void UpdatePinIconElement(Minimap.PinData pin, Sprite sprite)
        {
            if (pin == null || sprite == null || pin.m_iconElement == null)
                return;

            pin.m_iconElement.sprite = sprite;
        }

        private static void TrySyncWardGuildFromLocalPlayer(WardModel ward)
        {
            if (ward == null || Player.m_localPlayer == null)
                return;

            PrivateArea privateArea =
                TerritoryGuildAccess.FindPrivateAreaByWardId(ward.Id);

            if (privateArea == null)
                return;

            TerritoryGuildAccess.SyncWardGuildFromPlayer(
                privateArea,
                Player.m_localPlayer,
                true);
        }

        private void LoadIcons()
        {
            string bundlePath =
                Path.Combine(
                    Paths.PluginPath,
                    "ClanTerritory",
                    AssetBundleFileName);

            if (!File.Exists(bundlePath))
            {
                ModLog.Warning(
                    "[Map] Ward icon asset bundle not found: " +
                    bundlePath +
                    ". Using default Valheim pin icon.");

                return;
            }

            _assetBundle = AssetBundle.LoadFromFile(bundlePath);

            if (_assetBundle == null)
            {
                ModLog.Warning(
                    "[Map] Failed to load ward icon asset bundle: " +
                    bundlePath);

                return;
            }

            _neutralSprite = _assetBundle.LoadAsset<Sprite>(NeutralSpriteName);
            _guildSprite = _assetBundle.LoadAsset<Sprite>(GuildSpriteName);

            ModLog.Info(
                "[Map] Ward map sprites loaded. Neutral: " +
                (_neutralSprite != null) +
                ", Guild: " +
                (_guildSprite != null));
        }

        private static bool IsPinStillRegistered(Minimap.PinData pin)
        {
            if (pin == null || Minimap.instance == null)
                return false;

            if (MinimapPinsField == null)
                return false;

            List<Minimap.PinData> minimapPins =
                MinimapPinsField.GetValue(Minimap.instance) as List<Minimap.PinData>;

            if (minimapPins == null)
                return false;

            return minimapPins.Contains(pin);
        }
    }
}
