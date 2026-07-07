using System.Collections.Generic;
using System.IO;
using BepInEx;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.Territory.Zdo;
using ClanTerritory.Features.WardDetection.Models;
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
        private const string ClanSpriteName = "clan_ward_clan";

        private readonly TerritoryZdoService _zdoService;
        private readonly TerritoryRegistry _territoryRegistry;

        private readonly Dictionary<string, Minimap.PinData> _pins =
            new Dictionary<string, Minimap.PinData>();

        private AssetBundle _assetBundle;
        private Sprite _neutralSprite;
        private Sprite _clanSprite;

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

            Minimap.PinData existing;

            if (_pins.TryGetValue(ward.Id, out existing))
            {
                if (IsPinStillRegistered(existing))
                {
                    existing.m_pos = ward.Position;
                    existing.m_icon = SelectSprite(ward, existing.m_icon);
                    return;
                }

                _pins.Remove(ward.Id);
            }

            Minimap.PinData pin =
                Minimap.instance.AddPin(
                    ward.Position,
                    Minimap.PinType.Icon0,
                    PinNamePrefix + ward.Id,
                    false,
                    false,
                    0L);

            pin.m_icon = SelectSprite(ward, pin.m_icon);
            pin.m_doubleSize = true;

            _pins[ward.Id] = pin;

            ModLog.Info("[Map] Ward map pin added: " + ward.Id);
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

        private Sprite SelectSprite(WardModel ward, Sprite fallback)
        {
            if (HasClan(ward) && _clanSprite != null)
                return _clanSprite;

            if (_neutralSprite != null)
                return _neutralSprite;

            return fallback;
        }

        private bool HasClan(WardModel ward)
        {
            return false;
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
            _clanSprite = _assetBundle.LoadAsset<Sprite>(ClanSpriteName);

            ModLog.Info(
                "[Map] Ward map sprites loaded. Neutral: " +
                (_neutralSprite != null) +
                ", Clan: " +
                (_clanSprite != null));
        }

        private static bool IsPinStillRegistered(Minimap.PinData pin)
        {
            if (pin == null || Minimap.instance == null)
                return false;

            List<Minimap.PinData> minimapPins =
                AccessTools
                    .Field(typeof(Minimap), "m_pins")
                    .GetValue(Minimap.instance) as List<Minimap.PinData>;

            if (minimapPins == null)
                return false;

            return minimapPins.Contains(pin);
        }
    }
}