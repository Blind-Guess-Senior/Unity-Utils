using System;
using Extensions;
using Unity.Collections;
using UnityEngine;

namespace Data.Tag
{
    /// <summary>
    /// Game tag SO which can be created as a static asset.
    /// </summary>
    [CreateAssetMenu(fileName = "GameTag", menuName = "Tag System/Tag")]
    public class GameTag : ScriptableObject, IEquatable<GameTag>, ISerializationCallbackReceiver
    {
        #region Fields

        /// <summary>
        /// The group that game tag belongs to. 
        /// </summary>
        /// <remarks>
        /// Should not be empty.
        /// </remarks>
        [SerializeField] private string tagNamespace = "Default";

        /// <summary>
        /// The visible name of that game tag.
        /// </summary>
        /// <remarks>
        /// Should not be empty.
        /// </remarks>
        [SerializeField] private string tagName;

        /// <summary>
        /// Full name is a combine of tag group and tag name. It is unique.
        /// </summary>
        [SerializeField] private string _fullName;

        /// <summary>
        /// Hash code of that game tag, generated from full name. It is unique.
        /// </summary>
        [SerializeField] [ReadOnly] private int _hash;

        public string FullName => _fullName;
        public int Hash => _hash;

        #endregion

        #region IEquatable Implementation and Override Methods

        public bool Equals(GameTag other) => other && _hash == other._hash;
        public override bool Equals(object obj) => Equals(obj as GameTag);
        public override int GetHashCode() => _hash;
        public override string ToString() => _fullName;

        #endregion

        #region ISerializationCallbackReceiver Implementation

        /// <summary>
        /// Actions to do before serialize to make GameTag serializable.
        /// </summary>
        public void OnBeforeSerialize() => Bake();

        public void OnAfterDeserialize()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bake game tag to make it match the modify in inspector.
        /// </summary>
        private void Bake()
        {
            var newName = $"{tagNamespace}@{tagName}";
            if (_fullName != newName)
            {
                _fullName = newName;
                _hash = _fullName.GetStableHash();
                Debug.Log(_hash);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        #endregion

        #region Unity Event Methods

#if UNITY_EDITOR
        /// <summary>
        /// Bake when validate in editor environment.
        /// </summary>
        private void OnValidate() => Bake();
#endif

        #endregion
    }
}