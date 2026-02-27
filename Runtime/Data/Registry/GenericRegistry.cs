using System;
using System.Collections.Generic;
using Core.Locator;
using UnityEngine;

namespace Data.Registry
{
    /// <summary>
    /// A library that store objects in specified type, and provide readonly query methods. 
    /// </summary>
    /// <typeparam name="TEntry">The type of entry in this library.</typeparam>
    public abstract class GenericLibrary<TEntry> : ScriptableObject
        where TEntry : ScriptableObject
    {
        #region Fields

        /// <summary>
        /// All elements/entries in this library.
        /// </summary>
        [SerializeField] protected List<TEntry> entries = new();

        /// <summary>
        /// Public access port for all elements/entries.
        /// </summary>
        public IReadOnlyList<TEntry> Entries => entries.AsReadOnly();

        #endregion
    }

    /// <summary>
    /// Utility class for provide index & query methods for given library.
    /// </summary>
    /// <typeparam name="TLibrary">The type of library want to be indexed with given entry type.</typeparam>
    /// <typeparam name="TEntry">The type of entry in given library.</typeparam>
    /// <typeparam name="TKey">The type of the key which registry used to index entries.</typeparam>
    public abstract class GenericRegistry<TLibrary, TEntry, TKey> : IService
        where TLibrary : GenericLibrary<TEntry>
        where TEntry : ScriptableObject
        where TKey : struct
    {
        #region Fields

        /// <summary>
        /// Runtime cached registry that provide index & query methods by Dictionary type.
        /// </summary>
        private readonly Dictionary<TKey, TEntry> _cacheMap = new();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// It will traverse given library, and use given key-calculate function to generate a key for each entry.
        /// Then build cache-map of given library. 
        /// </summary>
        /// <param name="library">The exist library that want to generate a registry utility.</param>
        /// <param name="keySetter">The function that map entry into a unique key. It receives 1 param typed entry type,
        /// and return key typed key type.</param>
        /// <remarks>
        /// <see cref="keySetter"/> must return unique result for each different input.
        /// </remarks>
        protected GenericRegistry(TLibrary library, Func<TEntry, TKey> keySetter)
        {
            foreach (var entry in library.Entries)
            {
                var key = keySetter(entry);
                if (!_cacheMap.TryAdd(key, entry))
                {
                    Debug.LogWarning($"[GenericRegistry] Key collision with Key {key}, Entry {entry} missing.");
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get an entry for the given key.
        /// </summary>
        /// <param name="key">The key to get the value.</param>
        /// <returns>The value typed entry type for given key if existed.</returns>
        /// <exception cref="ArgumentException">Thrown when there is no entry with given key.</exception>
        protected TEntry GetEntry(TKey key) =>
            _cacheMap.TryGetValue(key, out var entry)
                ? entry
                : throw new ArgumentException(
                    $"[GenericRegistry - Library {typeof(TLibrary).Name}] Entry not found for the given key {key}");

        #endregion
    }
}