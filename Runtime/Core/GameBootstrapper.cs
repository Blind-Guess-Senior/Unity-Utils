using Core.Installer;
using Reflection;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Bootstrapper of game.
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
                var installMethod = type.GetMethod("InstallService");
                if (installMethod != null)
                {
                    try
                    {
                        // Create an instance of the installer
                        var instance = System.Activator.CreateInstance(type);
                        installMethod.Invoke(instance, null);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to install {type.Name}: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogError(type.Name + " can not install: No InstallMethod found.");
                }
            }
        }
    }
}
