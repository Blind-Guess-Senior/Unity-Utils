using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Containers
{
    /// <summary>
    /// A custom dictionary class that can be serialized and show in inspector.
    /// </summary>
    /// <typeparam name="TKey">The type of the key of dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the value of dictionary.</typeparam>
    [Serializable]
    public class DictionarySerializable<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        #region Fields

        /// <summary>
        /// Another representation of dictionary to make it serializable.
        /// </summary>
        public List<Pair> pairList = new();

        /// <summary>
        /// Used for add pair in better and safer way in inspector.
        /// </summary>
        public Pair newPair = new();

        #endregion

        #region Definitions

        /// <summary>
        /// A representation of key-value pair
        /// </summary>
        [Serializable]
        public struct Pair
        {
            public TKey key;
            public TValue value;

            public Pair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }

        #endregion


        #region Methods

        /// <summary>
        /// Serialize pairList with dictionary's data.
        /// </summary>
        public void OnBeforeSerialize()
        {
            pairList.Clear();
            foreach (var p in this)
            {
                Pair pair = new Pair(p.Key, p.Value);
                pairList.Add(pair);
            }
        }

        /// <summary>
        /// Dump data from pairList to dictionary.
        /// </summary>
        public void OnAfterDeserialize()
        {
            Clear();
            foreach (var p in pairList)
            {
                if (!TryAdd(p.key, p.value))
                {
                    Debug.LogError($"Duplicate key: {p.key} and {p.value}");
                }
            }
        }

        /// <summary>
        /// Add <see cref="newPair"/> to dictionary. Would be called in inspector.
        /// </summary>
        /// <returns>Whether add process had success.</returns>
        public bool TryAddPair()
        {
            return TryAdd(newPair.key, newPair.value);
        }

        #endregion
    }
}