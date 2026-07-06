using Artifact.Utils.Core.Locator;

// ReSharper disable CheckNamespace

namespace Artifact.Utils.Core.Installer
{
    /// <summary>
    /// <para>
    /// Generic installer for <see cref="IService"/>. Which will install itself into <c>ServiceLocator</c>.
    /// </para>
    /// <para>
    /// Usage: derive from this class and override <see cref="InstallService"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of that <see cref="IService"/>.</typeparam>
    /// <seealso cref="GameBootstrapper"/>
    public abstract class GenericInstaller<T>
        where T : IService
    {
        /// <summary>
        /// Virtual function for install service into locator.
        /// <br/>
        /// Override it to do things like load resource and register in locator.
        /// </summary>
        public abstract void InstallService();
    }
}