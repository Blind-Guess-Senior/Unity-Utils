using Artifact.UnityUtils.Data.Tag;
using UnityEditor;

// ReSharper disable CheckNamespace

namespace Artifact.UnityUtils.Editor.RegistryTools
{
    /// <summary>
    /// Useful editor tools for tag registry.
    /// </summary>
    public static class TagRegistryTools
    {
        /// <summary>
        /// Update all game tags into tag registry.
        /// </summary>
        [MenuItem("Artifact Unity Utils/Registry/Tag/Update Tag Registry")]
        public static void RegisterTag()
            => GenericRegistryTools.UpdateRegistryLibrary<TagLibrary, GameTag>();
    }
}