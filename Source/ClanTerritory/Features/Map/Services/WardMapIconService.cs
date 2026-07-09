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

        private readonly Dictionary<string, Minimap.PinData> _pins =
            new Dictionary<string, Minimap.PinData>();

        private readonly Dictionary<string, Sprite> _tintedSpritesByColor =
            new Dictionary<string, Sprite>();

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
                Sprite guildMapSprite;

                if (TerritoryGuildAccess.IsLocalPlayerInWardGuild(ward.Id) &&
                    TerritoryGuildAccess.TryGetGuildMapSprite(out guildMapSprite) &&
                    guildMapSprite != null)
                {
                    return guildMapSprite;
                }

                Sprite tintedSprite = TryCreateGuildTintedSprite(ward, fallback);

                if (tintedSprite != null)
                    return tintedSprite;

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

        private Sprite TryCreateGuildTintedSprite(WardModel ward, Sprite fallback)
        {
            if (ward == null || fallback == null)
                return null;

            Color guildColor;

            if (!TerritoryGuildAccess.TryGetWardGuildColor(
                    ward.Id,
                    out guildColor))
            {
                return null;
            }

            string key = ColorUtility.ToHtmlStringRGBA(guildColor);

            Sprite cached;

            if (_tintedSpritesByColor.TryGetValue(key, out cached) &&
                cached != null)
            {
                return cached;
            }

            Sprite source = _guildSprite != null
                ? _guildSprite
                : fallback;

            if (source == null || source.texture == null)
                return null;

            try
            {
                Texture2D sourceTexture = source.texture;
                Rect rect = source.rect;
                int width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
                int height = Mathf.Max(1, Mathf.RoundToInt(rect.height));

                Color[] pixels = sourceTexture.GetPixels(
                    Mathf.RoundToInt(rect.x),
                    Mathf.RoundToInt(rect.y),
                    width,
                    height);

                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i].a <= 0f)
                        continue;

                    if (pixels[i].r > 0.45f &&
                        pixels[i].g < 0.35f &&
                        pixels[i].b < 0.35f)
                    {
                        pixels[i].r = guildColor.r;
                        pixels[i].g = guildColor.g;
                        pixels[i].b = guildColor.b;
                    }
                }

                Texture2D texture = new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false);

                texture.SetPixels(pixels);
                texture.Apply();

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, width, height),
                    new Vector2(0.5f, 0.5f),
                    source.pixelsPerUnit);

                _tintedSpritesByColor[key] = sprite;
                return sprite;
            }
            catch
            {
                return null;
            }
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
