using System;
using Artifact.Utils.Core.Installer;
using Artifact.Utils.Reflection;
using Artifact.Utils.Utilities.DebugUtils;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace Artifact.Utils.Core
{
    /// <summary>
    /// Bootstrapper of game.
    /// <br/>
    /// Do things like install all services.
    /// </summary>
    public static class GameBootstrapper
    {
        /// <summary>
        /// Find all installer by reflection and then invoke their install methods.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Install()
        {
            var types = TypeUtils.GetTypes(typeof(GenericInstaller<>));
            foreach (var type in types)
            {
                if (type.IsAbstract) continue;

                var installMethod = type.GetMethod("InstallService");
                if (installMethod != null)
                {
                    try
                    {
                        // Create an instance of the installer
                        var instance = Activator.CreateInstance(type);
                        installMethod.Invoke(instance, null);
                    }
                    catch (Exception ex)
                    {
                        ArtifactDebug.PackageLog(
                            $"[GameBootstrapper] Failed to install {type.Name}: {ex.Message}", DebugLogLevel.Error);
                    }
                }
                else
                {
                    ArtifactDebug.PackageLog(
                        $"[GameBootstrapper] {type.Name} can not install: No InstallMethod found.",
                        DebugLogLevel.Fatal);
                }
            }
        }
    }
}