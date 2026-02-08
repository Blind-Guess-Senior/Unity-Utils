using UnityEngine;
using UnityEngine.Assertions;

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
        /// Gets the singleton instance. If the instance is undefined, it searches for it.
        /// The result of search is not guaranteed. The existence of returned singleton is not guaranteed.
        /// </summary>
        /// <returns>The singleton instance of type T.</returns>
        /// <remarks>
        /// 1) It is not recommended to let it search.
        /// </remarks>
        /// <remarks>
        /// 2) Use <see cref="EnsuredSingleton{T}"/> to ensure singleton's existence. 
        /// </remarks>
        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindAnyObjectByType<T>();
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
        /// Please override <see cref="Init"/> to append awake behaviour.
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

            if (_instance == this)
            {
                Init();
            }
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


    /// <summary>
    /// A singleton that ensure the existence of access of Instance.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class. T must inherit MonoBehaviour.</typeparam>
    public abstract class EnsuredSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// Gets the singleton instance. If the instance is undefined, it searches for it or create one on the scene.
        /// The result of search is not guaranteed. The existence of returned singleton is guaranteed.
        /// </summary>
        /// <returns>The singleton instance of type T.</returns>
        /// <remarks>
        /// It is not recommended to let it search or create.
        /// </remarks>
        public new static T Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindAnyObjectByType<T>();
                    if (!_instance)
                    {
                        var go = new GameObject("(Auto-Gen Singleton) " + typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        #endregion
    }


    /// <summary>
    /// A persistent singleton that is not destroyed on scene load. And it ensures the existence of access of Instance.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class. T must inherit MonoBehaviour.</typeparam>
    public abstract class EnsuredPersistentSingleton<T> : EnsuredSingleton<T> where T : MonoBehaviour
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


    /// <summary>
    /// A class that ensures there is only one in the game. But it cannot be accessed globally.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class. T must inherit MonoBehaviour.</typeparam>
    public abstract class LocalSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Flag var that shows if there exist a LocalSingleton.
        /// </summary>
        protected static bool _instantiated = false;

        #endregion

        #region Methods

        /// <summary>
        /// Awake method to initialize and set flag var to true.
        /// </summary>
        /// <remarks>
        /// Do not override it in normal case.
        /// See <see cref="Init"/> to append awake behaviour.
        /// </remarks>
        protected virtual void Awake()
        {
            Assert.IsFalse(_instantiated);
            _instantiated = true;
            Init();
        }

        /// <summary>
        /// Clear flag var _instantiated when destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            Assert.IsTrue(_instantiated);
            _instantiated = false;
        }

        /// <summary>
        /// Behavior that should be called when Awake. Override it to append behaviour is safe and recommended.
        /// </summary>
        protected virtual void Init()
        {
        }

        #endregion
    }
}