using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace JinxMod.Modules
{
    // Loads vanilla RoR2 assets without relying on SeekersPatcher.
    // 1) tries the old LegacyResourcesAPI path (still valid for many assets),
    // 2) otherwise looks the asset up in the game's Addressables catalog by file name,
    //    so we don't depend on hard-coded addresses that move between game patches.
    internal static class GameAssets
    {
        private static Dictionary<string, List<string>> keysByFileName;

        internal static T Load<T>(string legacyPath, string fileName) where T : UnityEngine.Object
        {
            T asset = null;

            if (!string.IsNullOrEmpty(legacyPath))
            {
                try { asset = RoR2.LegacyResourcesAPI.Load<T>(legacyPath); }
                catch (Exception) { asset = null; }
                if (asset) return asset;
            }

            foreach (string key in FindKeys(fileName))
            {
                try { asset = Addressables.LoadAssetAsync<T>(key).WaitForCompletion(); }
                catch (Exception) { asset = null; }
                if (asset) return asset;
            }

            Log.Warning("[GameAssets] Could not load vanilla asset '" + fileName + "' (legacy path '" + legacyPath + "').");
            return null;
        }

        private static List<string> FindKeys(string fileName)
        {
            if (keysByFileName == null) BuildIndex();

            List<string> keys;
            if (fileName != null && keysByFileName.TryGetValue(fileName.ToLowerInvariant(), out keys)) return keys;
            return new List<string>();
        }

        private static void BuildIndex()
        {
            keysByFileName = new Dictionary<string, List<string>>();
            try
            {
                Addressables.InitializeAsync().WaitForCompletion();
                foreach (var locator in Addressables.ResourceLocators)
                {
                    foreach (object rawKey in locator.Keys)
                    {
                        string key = rawKey as string;
                        if (key == null || !key.StartsWith("RoR2/")) continue;

                        string name = key.Substring(key.LastIndexOf('/') + 1).ToLowerInvariant();
                        List<string> list;
                        if (!keysByFileName.TryGetValue(name, out list))
                        {
                            list = new List<string>();
                            keysByFileName[name] = list;
                        }
                        if (list.Contains(key)) continue;

                        // prefer base-game assets over DLC copies with the same file name
                        if (key.StartsWith("RoR2/Base/")) list.Insert(0, key);
                        else list.Add(key);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[GameAssets] Failed to index Addressables catalog: " + e);
            }
        }
    }
}
