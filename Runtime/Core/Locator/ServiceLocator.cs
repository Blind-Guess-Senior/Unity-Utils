using System;
using System.Collections.Generic;
using System.Reflection;
using Attributes;
using UnityEngine;

namespace Core.Locator
{
    /// <summary>
    /// Service locator which provide global accessiable services. 
    /// </summary>
    /// <example>
    /// <code>
    /// // Tips: Only for example, do not confuse with example in EventManager.
    /// public AudioManager : MonoBehaviour
    /// {
    ///     private void Awake()
    ///     {
    ///         ServiceLocator.Register(this);
    ///         // or ServiceLocator.Register&lt;AudioManager&gt;(this);
    ///     }
    ///     public void PlaySound()
    ///     { ... }
    /// }
    ///  
    /// public class Player : MonoBehaviour
    /// {
    ///     public void Jump()
    ///     {
    ///         ServiceLocator.Get&lt;AudioManager&gt;().PlaySound();
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class ServiceLocator
    {
        #region Fields

        /// <summary>
        /// Store all services by type.
        /// </summary>
        private static Dictionary<Type, object> _services = new();

        #endregion

        #region Static Methods

        /// <summary>
        /// Register a service to this locator.
        /// </summary>
        /// <param name="service">The service want to be registered.</param>
        /// <typeparam name="T">The type of service.</typeparam>
        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Get service by type from this locator.
        /// </summary>
        /// <typeparam name="T">The type of service.</typeparam>
        /// <returns>The service of type T stored in this locator.</returns>
        public static T Get<T>()
        {
            return (T)_services[typeof(T)];
        }

        /// <summary>
        /// Inject field value by registered service.
        /// </summary>
        /// <param name="target">The object that want to get injection. Must have [Inject] attribute.</param>
        public static void Inject(object target)
        {
            var type = target.GetType();

            var fields = type.GetFields(BindingFlags.Instance |
                                        BindingFlags.Public |
                                        BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<InjectAttribute>() != null)
                {
                    var serviceType = field.FieldType;
                    if (_services.TryGetValue(serviceType, out var service))
                    {
                        field.SetValue(target, service);
                    }
                    else
                    {
                        Debug.LogError(
                            $"[ServiceLocator] Failed to inject {serviceType.Name} into {type.Name}. Service not found.");
                    }
                }
            }
        }

        #endregion
    }
}