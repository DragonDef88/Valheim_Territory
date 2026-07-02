using BepInEx;
using System.IO;
using UnityEngine;

namespace ClanTerritory.Features.Persistence.Serialization
{
    internal sealed class JsonSerializerService
    {
        public string Serialize<T>(T data) where T : class
        {
            return JsonUtility.ToJson(data, true);
        }

        public T Deserialize<T>(string json) where T : class, new()
        {
            if (string.IsNullOrEmpty(json))
                return new T();

            return JsonUtility.FromJson<T>(json);
        }
    }
}