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

        #region QuadTree Enable

        /// <summary>
        /// Menu path of QuadTree config.
        /// </summary>
        private const string QuadTreePath = "Data Structures/QuadTree/QuadTree Enabled";

        /// <summary>
        /// Compile flags of QuadTree.
        /// </summary>
        private const string DefineSymbolQuadTree = "__ARTIFACT_UNITY_UTILS__QUADTREE_ENABLED";


        /// <summary>
        /// Toggle QuadTree's enable or disable by editing scripting define symbols.
        /// </summary>
        [MenuItem(MenuPath + QuadTreePath, false, -1000)]
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

        #region QuadTree Features

        #region Destroy Auto Detect Feature

        /// <summary>
        /// Menu path of QuadTree's destroy auto-detect feature.
        /// </summary>
        private const string QuadTreeDestroyAutoDetectPath = "Data Structures/QuadTree/Features/Destroy Auto-Detect";

        /// <summary>
        /// Compile flags of QuadTree's destroy auto-detect feature.
        /// <br/>
        /// It will automatically remove dead items (whose gameObject had been destroyed, while SetActive and disable will not).
        /// when running GetIntersected() method.
        /// <br/>
        /// It will consume additional performance.
        /// <br/>
        /// Only use it when you often destroy game objects and hard to write dispose method for them.
        /// </summary>
        private const string DefineSymbolQuadTreeDestroyAutoDetect =
            "__ARTIFACT_UNITY_UTILS__QUADTREE_DESTROYAUTODETECT";

        /// <summary>
        /// Toggle QuadTree's destroy auto-detect feature by editing scripting define symbols.
        /// </summary>
        [MenuItem(MenuPath + QuadTreeDestroyAutoDetectPath, false, -900)]
        private static void ToggleQuadTreeDestroyAutoDetectFeature()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            if (defines.Contains(DefineSymbolQuadTreeDestroyAutoDetect))
            {
                // If enabled, then disable.
                defines = defines.Replace(DefineSymbolQuadTreeDestroyAutoDetect, "").Replace(";;", ";").Trim(';');
            }
            else
            {
                // If disabled, then enable.
                if (!string.IsNullOrEmpty(defines))
                {
                    defines += ";";
                }

                defines += DefineSymbolQuadTreeDestroyAutoDetect;
            }

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        }

        /// <summary>
        /// Display a check mark next to the menu item to indicate whether it is currently enabled.
        /// </summary>
        /// <returns>Always true.</returns>
        [MenuItem(MenuPath + QuadTreeDestroyAutoDetectPath, true)]
        private static bool ValidateToggleQuadTreeDestroyAutoDetectFeature()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            Menu.SetChecked(MenuPath + QuadTreeDestroyAutoDetectPath,
                defines.Contains(DefineSymbolQuadTreeDestroyAutoDetect));

            return true;
        }

        #endregion

        #region Not-In-Tree Item Query

        /// <summary>
        /// Menu path of QuadTree's not-in-tree item query feature.
        /// </summary>
        private const string QuadTreeNotInTreeItemQueryPath =
            "Data Structures/QuadTree/Features/Not-In-Tree Item Query";

        /// <summary>
        /// Compile flags of QuadTree's not-in-tree item query feature.
        /// <br/>
        /// It will allow GetIntersected() for not-in-tree item.
        /// <br/>
        /// It will consume additional performance.
        /// </summary>
        private const string DefineSymbolQuadTreeNotInTreeItemQuery =
            "__ARTIFACT_UNITY_UTILS__QUADTREE_NOTINTREEITEMQUERY";

        /// <summary>
        /// Toggle QuadTree's not-in-tree item query feature by editing scripting define symbols.
        /// </summary>
        [MenuItem(MenuPath + QuadTreeNotInTreeItemQueryPath, false, -900)]
        private static void ToggleQuadTreeNotInTreeItemQueryFeature()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            if (defines.Contains(DefineSymbolQuadTreeNotInTreeItemQuery))
            {
                // If enabled, then disable.
                defines = defines.Replace(DefineSymbolQuadTreeNotInTreeItemQuery, "").Replace(";;", ";").Trim(';');
            }
            else
            {
                // If disabled, then enable.
                if (!string.IsNullOrEmpty(defines))
                {
                    defines += ";";
                }

                defines += DefineSymbolQuadTreeNotInTreeItemQuery;
            }

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        }

        /// <summary>
        /// Display a check mark next to the menu item to indicate whether it is currently enabled.
        /// </summary>
        /// <returns>Always true.</returns>
        [MenuItem(MenuPath + QuadTreeNotInTreeItemQueryPath, true)]
        private static bool ValidateToggleQuadTreeNotInTreeItemQueryFeature()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            Menu.SetChecked(MenuPath + QuadTreeNotInTreeItemQueryPath,
                defines.Contains(DefineSymbolQuadTreeNotInTreeItemQuery));

            return true;
        }

        #endregion

        #endregion

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

        /// <summary>
        /// Used to find this file.
        /// </summary>
        public static void CompileFlagInterfaceNavigator()
        {
        }

        #endregion
    }
}