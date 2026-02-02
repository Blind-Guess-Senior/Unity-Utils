using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Abstract base class for creating singleton MonoBehaviour instances.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class. Must inherit MonoBehaviour.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Fields

        protected static T _instance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton instance. If the instance is undefined, it searches for it or create one on the scene.
        /// The result of search is not guaranteed.
        /// </summary>
        /// <returns>The singleton instance of type T.</returns>
        /// <remarks>
        /// It is not recommended to let it search or create.
        /// </remarks>
        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindAnyObjectByType<T>();
                    if (!_instance)
                    {
                        var go = new GameObject(typeof(T).Name + " (Auto-Gen Singleton)");
                        _instance = go.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Awake method to initialize the singleton instance.
        /// </summary>
        /// <remarks>
        /// Do not override it.
        /// See <see cref="Init"/> to append awake behaviour.
        /// </remarks>
        protected virtual void Awake()
        {
            if (!_instance)
            {
                _instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
            
            Init();
        }

        /// <summary>
        /// Behavior that should be called when Awake. Override it to append behaviour is safe and recommended.
        /// </summary>
        protected virtual void Init()
        {
        }

        #endregion
    }


    /// <summary>
    /// A persistent singleton that is not destroyed on scene load.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class. T must inherit MonoBehaviour.</typeparam>
    /// <example>
    /// <code>
    /// // Example usage of PersistentSingleton
    /// public class AudioManager : PersistentSingleton&lt;AudioManager&gt;
    /// {
    ///     public void PlaySound(string clipName)
    ///     {
    ///         Debug.Log("Playing sound: " + clipName);
    ///     }
    /// }
    ///
    /// public class GameController : MonoBehaviour
    /// {
    ///     void Start()
    ///     {
    ///         AudioManager.Instance.PlaySound("GameStart");
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        #region Methods

        /// <summary>
        /// Awake method to call DontDestroyOnLoad on the object.
        /// </summary>
        /// <remarks>
        /// Init() is executed before DontDestroyOnLoad.
        /// </remarks>
        protected override void Awake()
        {
            base.Awake();
            if (_instance == this)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        #endregion
    }
}