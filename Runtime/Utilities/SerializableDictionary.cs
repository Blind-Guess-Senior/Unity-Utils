using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// A custom dictionary class that can be serialized and show in inspector.
    /// </summary>
    /// <typeparam name="TKey">The type of the key of dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the value of dictionary.</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public List<Pair> pairList = new();

        /// <summary>
        /// Used for add pair in better and safer way in inspector.
        /// </summary>
        public Pair newPair = new();

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

        public void OnBeforeSerialize()
        {
            pairList.Clear();
            foreach (var p in this)
            {
                Pair pair = new Pair(p.Key, p.Value);
                pairList.Add(pair);
            }
        }

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

        public bool TryAddPair()
        {
            return TryAdd(newPair.key, newPair.value);
        }
    }
}