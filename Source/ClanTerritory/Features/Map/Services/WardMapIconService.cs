using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClanTerritory.Domain.Entities;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.Territory.Zdo;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Utils;
using UnityEngine;

namespace ClanTerritory.Features.Map.Services
{
    internal sealed class WardMapIconService
    {
        private const string PinNamePrefix = "ClanTerritory_Ward_";
        private const string NeutralIconResourceSuffix =
            "Resources.Icons.Map.clan_ward_neutral.png";
        private const string ClanIconResourceSuffix =
            "Resources.Icons.Map.clan_ward_clan.png";

        private readonly TerritoryZdoService _zdoService;
        private readonly TerritoryRegistry _territoryRegistry;
        private readonly Dictionary<string, Minimap.PinData> _pins =
            new Dictionary<string, Minimap.PinData>();

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
            _neutralSprite = LoadSprite(NeutralIconResourceSuffix);
            _clanSprite = LoadSprite(ClanIconResourceSuffix);

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
                existing.m_pos = ward.Position;
                existing.m_icon = SelectSprite(ward);
                return;
            }

            Minimap.PinData pin =
                Minimap.instance.AddPin(
                    ward.Position,
                    Minimap.PinType.Icon0,
                    PinNamePrefix + ward.Id,
                    false,
                    false,
                    0L);

            pin.m_icon = SelectSprite(ward);
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

            if (Minimap.instance != null)
                Minimap.instance.RemovePin(pin);

            _pins.Remove(wardId);

            ModLog.Info("[Map] Ward map pin removed: " + wardId);
        }

        public void RemoveAll()
        {
            if (Minimap.instance != null)
            {
                foreach (Minimap.PinData pin in _pins.Values)
                    Minimap.instance.RemovePin(pin);
            }

            _pins.Clear();
        }

        private Sprite SelectSprite(WardModel ward)
        {
            if (HasClan(ward) && _clanSprite != null)
                return _clanSprite;

            return _neutralSprite;
        }

        private bool HasClan(WardModel ward)
        {
            if (ward == null || _territoryRegistry == null)
                return false;

            foreach (Territory territory in _territoryRegistry.GetAll())
            {
                if (territory.WardId.ToString() == ward.Id)
                    return false;
            }

            return false;
        }

        private static Sprite LoadSprite(string resourceSuffix)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string resourceName =
                assembly
                    .GetManifestResourceNames()
                    .FirstOrDefault(name => name.EndsWith(resourceSuffix));

            if (string.IsNullOrEmpty(resourceName))
            {
                ModLog.Warning(
                    "[Map] Ward icon resource not found: " +
                    resourceSuffix);

                return null;
            }

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                ImageConversion.LoadImage(texture, data);

                return Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
        }
    }
}