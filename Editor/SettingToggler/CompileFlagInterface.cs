using UnityEditor;
using UnityEditor.Build;

namespace Editor.SettingToggler
{
    /// <summary>
    /// Editor tools for simplify compile flag management.
    /// </summary>
    public static class CompileFlagInterface
    {
        #region Fields

        /// <summary>
        /// Parent menu path for all item of Artifact Unity Utils.
        /// </summary>
        private const string MenuPath = "Artifact Unity Utils/";

        #endregion

        #region QuadTree

        /// <summary>
        /// Menu path of QuadTree config.
        /// </summary>
        private const string QuadTreePath = "Data Structures/QuadTree";

        /// <summary>
        /// Compile flags of QuadTree.
        /// </summary>
        private const string DefineSymbolQuadTree = "__ARTIFACT_UNITY_UTILS__ENABLE_QUADTREE";

        /// <summary>
        /// Toggle QuadTree's enable or disable by editing scripting define symbols.
        /// </summary>
        [MenuItem(MenuPath + QuadTreePath)]
        private static void ToggleQuadTree()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            if (defines.Contains(DefineSymbolQuadTree))
            {
                // If enabled, then disable.
                defines = defines.Replace(DefineSymbolQuadTree, "").Replace(";;", ";").Trim(';');
            }
            else
            {
                // If disabled, then enable.
                if (!string.IsNullOrEmpty(defines))
                {
                    defines += ";";
                }

                defines += DefineSymbolQuadTree;
            }

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        }

        /// <summary>
        /// Display a check mark next to the menu item to indicate whether it is currently enabled.
        /// </summary>
        /// <returns>Always true.</returns>
        [MenuItem(MenuPath + QuadTreePath, true)]
        private static bool ValidateToggleQuadTree()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            Menu.SetChecked(MenuPath + QuadTreePath, defines.Contains(DefineSymbolQuadTree));

            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get current named build target.
        /// </summary>
        /// <returns>The named build target of current platform.</returns>
        private static NamedBuildTarget GetNamedBuildTarget()
        {
            // Get right named build target.
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

            return namedBuildTarget;
        }

        /// <summary>
        /// Get project scripting define symbols of given named build target.
        /// </summary>
        /// <param name="namedBuildTarget">The named build target want to get scripting define symbols.</param>
        /// <returns>The define symbols of given named build target.</returns>
        private static string GetScriptingDefineSymbols(NamedBuildTarget namedBuildTarget)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);

            return defines;
        }

        #endregion
    }
}