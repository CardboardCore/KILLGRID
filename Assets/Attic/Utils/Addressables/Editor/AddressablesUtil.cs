// using System.Collections.Generic;
// using UnityEditor.AddressableAssets;
// using UnityEditor.AddressableAssets.Settings;
// using UnityEngine;
//
// namespace WiredDreams.Utils.Addressables.Editor
// {
//     public static class AddressablesUtility
//     {
//         public static List<string> GetAddressableGroupEntries(string groupName)
//         {
//             List<string> entries = new List<string>();
//             AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
//
//             if (settings == null)
//             {
//                 Debug.LogError("AddressableAssetSettings not found.");
//
//                 return entries;
//             }
//
//             AddressableAssetGroup group = settings.FindGroup(groupName);
//
//             if (group == null)
//             {
//                 Debug.LogError($"Addressable group '{groupName}' not found.");
//
//                 return entries;
//             }
//
//             foreach (AddressableAssetEntry entry in group.entries)
//             {
//                 entries.Add(entry.address);
//             }
//
//             return entries;
//         }
//     }
// }
