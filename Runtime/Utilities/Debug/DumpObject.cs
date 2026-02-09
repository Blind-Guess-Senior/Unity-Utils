using System;
using System.Collections.Generic;
using System.Reflection;
using Reflection;
using Unity.Plastic.Newtonsoft.Json;

namespace Utilities.Debug
{
    /// <summary>
    /// Utility class for dump an object's field into human-readable format.
    /// </summary>
    public static class DumpObject
    {
        #region Static Fields

        /// <summary>
        /// Types that do not need FieldToString().
        /// </summary>
        /// <remarks>
        /// The list of types here is not complete.
        /// </remarks>
        private static readonly List<Type> CanBeDefaultOutput = new List<Type>
            { typeof(int), typeof(string), typeof(float), typeof(double) };

        #endregion

        #region Static Methods

        /// <summary>
        /// Print all fields and their status of an object as itself class.
        /// </summary>
        /// <param name="obj">The object that wanna be dumped.</param>
        /// <example>
        /// <code>
        /// public class DumpTest
        /// {
        ///     public int DumpTestInt1 = 7;
        ///     private int DumpTestInt2Private = 8;
        /// 
        ///     private void Start()
        ///     {
        ///         DumpObject.Dump(this);
        ///     }
        /// }
        ///   
        /// ----- The Output will look like -----
        /// DumpTestInt1: 7
        /// DumpTestInt2Private: 8
        /// </code>
        /// </example>
        public static void Dump(object obj)
        {
            if (obj == null) return;
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Public |
                                        BindingFlags.NonPublic |
                                        BindingFlags.Instance |
                                        BindingFlags.DeclaredOnly);

            string dumped = "";
            foreach (var field in fields)
            {
                var value = field.GetValue(obj);
                if (CanBeDefaultOutput.Contains(value.GetType()))
                    dumped += field.Name + ": " + value + Environment.NewLine;
                else
                {
                    dumped += field.Name + ": " + FieldToString(value) + Environment.NewLine;
                }
            }

            UnityEngine.Debug.Log(dumped);
        }

        /// <summary>
        /// Print all fields and their status of an object (including base class stop at specified class). 
        /// </summary>
        /// <param name="obj">The object that wanna be dumped.</param>
        /// <typeparam name="T">The base type that dump will stop at.</typeparam>
        /// <example>
        /// <code>
        /// public class DumpTest1 : MonoBehaviour
        /// {
        ///     public int DumpTest1Int1 = 3;
        ///     private int DumpTest1Int2Private = 4;
        /// }
        ///  
        /// public class DumpTest2 : DumpTest1
        /// {
        ///     public int DumpTest2Int1 = 5;
        ///     private int DumpTest2Int2Private = 6;
        /// }
        ///  
        /// public class DumpTest : DumpTest2
        /// {
        ///     public int DumpTestInt1 = 7;
        ///     private int DumpTestInt2Private = 8;
        /// 
        ///     private void Start()
        ///     {
        ///         DumpObject.Dump&lt;DumpTest2&gt;(this);
        ///         DumpObject.Dump&lt;DumpTest1&gt;(this);
        ///     }
        /// }
        ///   
        /// ----- The Output will look like -----
        /// [DumpTest] DumpTestInt1: 7
        /// [DumpTest] DumpTestInt2Private: 8
        /// [DumpTest2] DumpTest2Int1: 5
        /// [DumpTest2] DumpTest2Int2Private: 6
        ///   
        /// ----- The second one below ----
        /// [DumpTest] DumpTestInt1: 7
        /// [DumpTest] DumpTestInt2Private: 8
        /// [DumpTest2] DumpTest2Int1: 5
        /// [DumpTest2] DumpTest2Int2Private: 6
        /// [DumpTest1] DumpTest1Int1: 3
        /// [DumpTest1] DumpTest1Int2Private: 4
        /// </code>
        /// </example>
        public static void DumpUpTo<T>(T obj)
        {
            if (obj == null) return;
            var type = obj.GetType();
            var baseType = typeof(T);

            if (!baseType.IsAssignableFrom(type))
            {
                UnityEngine.Debug.LogError($"Object of type {type} does not inherit from {baseType}");
                return;
            }

            string dumped = "";
            var currentType = type;
            while (currentType != null && baseType.IsAssignableFrom(currentType))
            {
                var fields = currentType.GetFields(BindingFlags.Public |
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.Instance |
                                                   BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    var value = field.GetValue(obj);
                    if (CanBeDefaultOutput.Contains(value.GetType()))
                        dumped += $"[{currentType.Name}] {field.Name}: {value}{Environment.NewLine}";
                    else
                    {
                        dumped += $"[{currentType.Name}] {field.Name}: {FieldToString(value)}{Environment.NewLine}";
                    }
                }

                currentType = currentType.BaseType;
            }

            UnityEngine.Debug.Log(dumped);
        }

        /// <summary>
        /// Print all fields and their status of an object (including base class stop at specified class).
        /// This is the implementation for support of generic base type.
        /// </summary>
        /// <param name="baseType">The base type that dump will stop at.</param>
        /// <param name="obj">The object that wanna be dumped.</param>
        /// <example>
        /// <code>
        /// public abstract class EventBus&lt;TEventBus&gt; : IEventBus where TEventBus : EventBus&lt;TEventBus&gt;
        /// {
        ///     protected readonly Dictionary&lt;...&gt; handlers = new();
        ///     protected readonly object _lock = new();
        ///     protected int nextHandlerId = 0;
        /// } 
        ///   
        /// public class DefaultEventBus : EventBus&lt;DefaultEventBus&gt;
        /// {
        /// } 
        ///   
        /// public class DumpTest3 : DefaultEventBus
        /// {
        ///     public int DumpTest3Int1 = 7;
        ///     private int DumpTest3Int2Private = 8;
        /// } 
        ///    
        /// var bus = new DumpTest3();
        /// DumpObject.DumpUpTo(typeof(EventBus&lt;&gt;), bus);
        ///    
        /// ----- The Output will look like -----
        /// [DumpTest3] DumpTest3Int1: 7
        /// [DumpTest3] DumpTest3Int2Private: 8
        /// [EventBus`1] handlers: {...may be something in it...}
        /// [EventBus`1] _lock: {}
        /// [EventBus`1] nextHandlerId: 0
        /// </code>
        /// </example>
        public static void DumpUpTo(Type baseType, object obj)
        {
            if (obj == null) return;
            var type = obj.GetType();

            if (!TypeUtils.IsAssignable(baseType, type))
            {
                UnityEngine.Debug.LogError($"Object of type {type} does not inherit from {baseType}");
                return;
            }

            string dumped = "";
            var currentType = type;
            while (currentType != null && TypeUtils.IsAssignable(baseType, currentType))
            {
                var fields = currentType.GetFields(BindingFlags.Public |
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.Instance |
                                                   BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    var value = field.GetValue(obj);
                    if (CanBeDefaultOutput.Contains(value.GetType()))
                        dumped += $"[{currentType.Name}] {field.Name}: {value}{Environment.NewLine}";
                    else
                    {
                        dumped += $"[{currentType.Name}] {field.Name}: {FieldToString(value)}{Environment.NewLine}";
                    }
                }

                currentType = currentType.BaseType;
            }

            UnityEngine.Debug.Log(dumped);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Dump an field into human-readable string.
        /// TODO: May need more check. JsonConvert works well now.
        /// </summary>
        /// <param name="obj">The field that wanna be dumped.</param>
        /// <returns>A string representation of input field.</returns>
        private static string FieldToString(object obj)
        {
            string dumped = "";

            // var type = obj.GetType();
            dumped = JsonConvert.SerializeObject(obj, Formatting.None);

            return dumped;
        }

        #endregion
    }
}