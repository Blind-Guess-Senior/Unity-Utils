using System.Collections.Generic;
using System.Reflection;
using Data.Registry;
using UnityEditor;
using UnityEngine;

namespace Editor.RegistryTools
{
    /// <summary>
    /// Useful editor tools for registry.
    /// </summary>
    public static class GenericRegistryTools
    {
        /// <summary>
        /// Update all entries which belongs to given library by search for all existed entry SO assets.
        /// </summary>
        /// <typeparam name="TLib">The type of library want to be updated.</typeparam>
        /// <typeparam name="TEntry">The type of entry signed for given library.</typeparam>
        public static void UpdateRegistryLibrary<TLib, TEntry>()
            where TLib : GenericLibrary<TEntry>
            where TEntry : ScriptableObject
        {
            string[] libraryGuids = AssetDatabase.FindAssets($"t:{typeof(TLib).Name}");
            if (libraryGuids.Length == 0)
            {
                Debug.LogError($"{typeof(TLib).Name}.asset not found! Please create one.");
                return;
            }

            string libraryPath = AssetDatabase.GUIDToAssetPath(libraryGuids[0]);
            var library = AssetDatabase.LoadAssetAtPath<TLib>(libraryPath);

            string[] allEntries = AssetDatabase.FindAssets($"t:{typeof(TEntry).Name}");
            List<TEntry> entries = new();

            foreach (string guid in allEntries)
            {
                string entryPath = AssetDatabase.GUIDToAssetPath(guid);
                var entry = AssetDatabase.LoadAssetAtPath<TEntry>(entryPath);
                if (entry)
                {
                    entries.Add(entry);
                }
            }

            library.GetType()
                .GetField($"{nameof(library.Entries).ToLower()}", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(library, entries);

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"<color=green>[Registry Library - {typeof(TLib).Name}] has been successfully updated with {entries.Count} entries.</color>");
        }
    }
}