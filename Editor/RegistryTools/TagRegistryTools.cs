using Data.Tag;
using UnityEditor;

namespace Editor.RegistryTools
{
    /// <summary>
    /// Useful editor tools for tag registry.
    /// </summary>
    public static class TagRegistryTools
    {
        /// <summary>
        /// Update all game tags into tag registry.
        /// </summary>
        [MenuItem("Jobs/Registry/Tag/Update Tag Registry")]
        public static void RegisterTag()
            => GenericRegistryTools.UpdateRegistryLibrary<TagLibrary, GameTag>();
    }
}