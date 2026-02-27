using UnityEngine;

namespace Data.Tag
{
    /// <summary>
    /// Provide utility functions for tag operation.
    /// </summary>
    public static class TagUtils
    {
        #region HasTag Static Util Methods

        /// <summary>
        /// Check if given GameObject has given tag by GameTag entry.
        /// </summary>
        /// <param name="gameObject">The game object wanna to check.</param>
        /// <param name="tagToCompare">The game tag entry to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        public static bool HasTag(GameObject gameObject, GameTag tagToCompare)
            => gameObject?.GetComponent<Taggable>()?.HasTag(tagToCompare) ?? false;

        /// <summary>
        /// Check if given GameObject has given tag by full tag name.
        /// </summary>
        /// <param name="gameObject">The game object wanna to check.</param>
        /// <param name="fullTagName">The game tag's full name to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        public static bool HasTag(GameObject gameObject, string fullTagName)
            => gameObject?.GetComponent<Taggable>()?.HasTag(fullTagName) ?? false;

        /// <summary>
        /// Check if given GameObject has given tag by GameTag entry.
        /// </summary>
        /// <param name="gameObject">The game object wanna to check.</param>
        /// <param name="tagNamespace">The game tag's namespace to compare.</param>
        /// <param name="tagName">The game tag's name to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        public static bool HasTag(GameObject gameObject, string tagNamespace, string tagName)
            => gameObject?.GetComponent<Taggable>()?.HasTag(tagNamespace, tagName) ?? false;
        
        /// <summary>
        /// Check if given GameObject has given tag by tag name in "Default" namespace.
        /// </summary>
        /// <param name="gameObject">The game object wanna to check.</param>
        /// <param name="tagName">The game tag's name to compare.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        public static bool HasTag(GameObject gameObject, string tagName, int _)
            => gameObject?.GetComponent<Taggable>()?.HasTag(tagName, _) ?? false;

        /// <summary>
        /// Check if given GameObject has given tag by tag hash.
        /// </summary>
        /// <param name="gameObject">The game object wanna to check.</param>
        /// <param name="tagHash">The game tag's hash code to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        public static bool HasTag(GameObject gameObject, int tagHash)
            => gameObject?.GetComponent<Taggable>()?.HasTag(tagHash) ?? false;

        /// <summary>
        /// Check if given GameObject has given tag by tag name in "Default" namespace.
        /// </summary>
        /// <param name="gameObject">The game object wanna to check.</param>
        /// <param name="tagName">The game tag's name to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        public static bool HasTag1(GameObject gameObject, string tagName)
            => gameObject?.GetComponent<Taggable>()?.HasTag1(tagName) ?? false;

        #endregion
    }
}