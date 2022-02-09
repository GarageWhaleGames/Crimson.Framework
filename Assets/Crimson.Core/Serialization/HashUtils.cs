using System;
using System.Text;
using UnityEngine;

namespace Crimson.Core.Serialization
{
    public static class HashUtils
    {
#if UNITY_EDITOR

        public static int GetAssetHash(GameObject prefab)
        {
            var components = prefab.GetComponents<Component>();
            int result = 0;

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    continue;
                }
                result += components[i].GetHashCode();
                result += i;
            }

            return result;
        }

        public static ushort GetAssetHashUShort(GameObject prefab)
        {
            if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out var guid, out long _))
            {
                ushort result = 0;
                var bytes = Encoding.UTF8.GetBytes(guid);
                for (var i = 1; i < bytes.Length; i++)
                {
                    result ^= BitConverter.ToUInt16(new[] { bytes[i - 1], bytes[i] }, 0);
                }

                return result;
            }

            throw new InvalidOperationException($"Cannot get guid for {prefab.name}");
        }

#endif
    }
}