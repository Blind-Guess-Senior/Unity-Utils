using Core.Locator;

namespace Core.Installer
{
    /// <summary>
    /// Generic installer for ScriptableObject. Which will install itself into ServiceLocator.
    /// </summary>
    /// <typeparam name="T">The type of that ScriptableObject. Must implement <see cref="IService"/>.</typeparam>
    public abstract class GenericInstaller<T>
        where T : IService
    {
        /// <summary>
        /// Virtual function for install service into locator.
        /// Override it to do things like load resource and register in locator.
        /// </summary>
        public virtual void InstallService()
        {
        }
    }
}