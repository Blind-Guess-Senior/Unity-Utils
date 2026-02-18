using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utilities
{
    public interface ISingleton
    {
    }

    /// <summary>
    /// Abstract base class for creating singleton instances.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class. T must not inherit MonoBehaviour.</typeparam>
    public abstract class Singleton<T> : ISingleton where T : new()
    {
        #region Fields

        protected static readonly Lazy<T> _lazy = new Lazy<T>(() => new T());

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton instance. It would be created when accessed first time.
        /// </summary>
        /// <returns>The singleton instance of type T.</returns>
        public static T Instance => _lazy.Value;

        #endregion
    }

    /// <summary>
    /// Abstract base class for creating singleton instances.
    /// Which would be init whenever any static member first being accessed.
    /// Every static members would be init whenever any static member first being accessed.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class. T must not inherit MonoBehaviour.</typeparam>
    public abstract class UnlazySingleton<T> : ISingleton where T : new()
    {
        #region Fields

        protected static T Instance { get; } = new();

        #endregion
    }

    /// <summary>
    /// Abstract base class for creating singleton instances.
    /// It will be auto initialized when AfterAssembliesLoaded.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class. T must not inherit MonoBehaviour.</typeparam>
    public abstract class AutoInitSingleton<T> : Singleton<T> where T : new()
    {
        /// <summary>
        /// Access the Instance to trigger the initialization of the singleton. 
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void AutoInit()
        {
            _ = Instance;
        }
    }

    /// <summary>
    /// Abstract base class for creating singleton instances.
    /// All static members will be auto initialized when AfterAssembliesLoaded.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class. T must not inherit MonoBehaviour.</typeparam>
    public abstract class AutoInitUnlazySingleton<T> : UnlazySingleton<T> where T : new()
    {
        /// <summary>
        /// Access the Instance to trigger the initialization of the singleton. 
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void AutoInit()
        {
            _ = Instance;
        }
    }

    /// <summary>
    /// A class that ensures there is only one in the game. But it cannot be accessed globally.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class.</typeparam>
    public abstract class LocalSingleton<T>
    {
        #region Fields

        /// <summary>
        /// Flag var that shows if there exist a LocalSingleton.
        /// </summary>
        protected static bool _instantiated = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// Initializes the singleton instance and ensures that only one instance can be created.
        /// </summary>
        protected LocalSingleton()
        {
            Assert.IsTrue(!_instantiated);
            _instantiated = true;
            Init();
        }

        /// <summary>
        /// Destructor.
        /// Clears the flag var _instantiated when the instance is destroyed.
        /// </summary>
        ~LocalSingleton()
        {
            Assert.IsTrue(_instantiated);
            _instantiated = false;
            Dispose();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Behavior that should be called when constructed.
        /// </summary>
        protected virtual void Init()
        {
        }

        /// <summary>
        /// Behavior that should be called when destructed.
        /// </summary>
        protected virtual void Dispose()
        {
        }

        #endregion
    }
}