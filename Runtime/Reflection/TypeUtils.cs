using System;
using System.Collections.Generic;
using System.Reflection;

namespace Reflection
{
    /// <summary>
    /// Utility class for retrieving types from predefined assemblies.
    /// </summary>
    public static class TypeUtils
    {
        /// <summary>
        /// Gets a list of types that are subclasses of the specified generic base type.
        /// </summary>
        /// <param name="genericBaseType">The generic base type to match.</param>
        /// <param name="includeInterfaces">Whether to include interfaces in the search.</param>
        /// <exception cref="System.Reflection.ReflectionTypeLoadException">Thrown when there is an error loading types from an assembly.</exception>
        /// <returns>A list of types that are subclasses of the specified generic base type.</returns>
        /// <example>
        /// Example usage:
        /// <code>
        /// List&lt;Type&gt; derivedTypes = TypeUtils.GetTypes(typeof(MyGenericBase&lt;&gt;));
        /// foreach (Type t in derivedTypes)
        /// {
        ///     Debug.Log($"Found: {t.FullName}");
        /// }
        /// </code>
        /// </example>
        public static List<Type> GetTypes(Type genericBaseType, bool includeInterfaces = false)
        {
            List<Type> types = new();

            // Loop through all loaded assemblies
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Get types defined in the assembly
                    foreach (Type type in assembly.GetTypes())
                    {
                        // Check if the type is a subclass of the specified generic type
                        bool hasGenericBaseType = HasGenericBaseOrInterface(type, genericBaseType, includeInterfaces);
                        if (type.IsClass && !type.IsAbstract && hasGenericBaseType)
                        {
                            types.Add(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException e)
                {
                    throw new ReflectionTypeLoadException(
                        e.Types,
                        e.LoaderExceptions,
                        $"Failed to load types from assembly '{assembly.FullName}': {e.Message}"
                    );
                }
            }

            return types;
        }

        /// <summary>
        /// Checks if a type has the specified generic base type or implements the specified generic interface.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="genericBaseType">The generic base type to match.</param>
        /// <param name="includeInterfaces">Whether to include interfaces in the check.</param>
        /// <returns>True if the type has the specified generic base type; otherwise, false.</returns>
        private static bool HasGenericBaseOrInterface(Type type, Type genericBaseType, bool includeInterfaces = false)
        {
            Type baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericBaseType)
                    return true;

                baseType = baseType.BaseType;
            }

            if (!includeInterfaces) return false;
            foreach (Type iface in type.GetInterfaces())
            {
                if (iface == genericBaseType)
                    return true;

                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == genericBaseType)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a type is derived from base type.
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="type"></param>
        /// <param name="includeInterfaces"></param>
        /// <returns>True if the type is the derive of base type; otherwise, false.</returns>
        public static bool IsAssignable(Type baseType, Type type, bool includeInterfaces = false)
        {
            if (type == null || baseType == null)
                return false;

            if (baseType.IsAssignableFrom(type))
                return true;

            if (baseType.IsGenericTypeDefinition)
            {
                var currentType = type;
                while (currentType != null && currentType != typeof(object))
                {
                    if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == baseType)
                    {
                        return true;
                    }

                    currentType = currentType.BaseType;
                }
            }

            if (!includeInterfaces)
                return false;

            foreach (Type iface in type.GetInterfaces())
            {
                if (iface == baseType)
                    return true;

                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == baseType)
                    return true;
            }

            return false;
        }
    }
}