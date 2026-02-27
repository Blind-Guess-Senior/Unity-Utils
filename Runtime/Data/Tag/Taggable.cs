using System.Collections.Generic;
using Attributes;
using Core.Locator;
using Extensions;
using UnityEngine;

namespace Data.Tag
{
    /// <summary>
    /// A class that Indicates that an object can have game tags attached,
    /// and stores the game tags while providing related utility methods.
    /// </summary>
    public class Taggable : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A reference to tag registry to call utilities it provides.
        /// </summary>
        [Inject] private TagRegistry _tagRegistry;

        /// <summary>
        /// Store game tags that defined before runtime.
        /// Use List to make it serializable in inspector.
        /// </summary>
        [SerializeField] private List<GameTag> tags = new();

        /// <summary>
        /// Store game tags in runtime.
        /// Use HashSet to provide better performance.
        /// </summary>
        private HashSet<GameTag> _runtimeTags;

        /// <summary>
        /// Store disabled game tags in runtime.
        /// One tag should never be in both of these two sets at the same time.
        /// The remarks for all the following functions are based on this requirement.
        /// </summary>
        private HashSet<GameTag> _disabledTags;

        #endregion

        #region Unity Event Methods

        /// <summary>
        /// Awake method to initialize the taggable class.
        /// Will inject TagRegistry in runtime.
        /// </summary>
        private void Awake()
        {
            InitRuntimeTags();
            ServiceLocator.Inject(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create runtime tags by coping from pre-run tags.
        /// </summary>
        private void InitRuntimeTags()
        {
            _runtimeTags = new HashSet<GameTag>(tags);
            _disabledTags = new HashSet<GameTag>();
        }

        #endregion

        #region HasTag Methods

        /// <summary>
        /// Check if it has given tag by GameTag entry.
        /// </summary>
        /// <param name="tagToCompare">The game tag entry to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        /// <remarks>
        /// If tag is disabled, it won't be treated as "has".
        /// </remarks>
        public bool HasTag(GameTag tagToCompare)
        {
            if (!tagToCompare) return false;
            if (_runtimeTags != null)
            {
                return _runtimeTags.Contains(tagToCompare);
            }

            return tags.Contains(tagToCompare);
        }

        /// <summary>
        /// Check if it has given tag by full tag name.
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        /// <remarks>
        /// If tag is disabled, it won't be treated as "has".
        /// </remarks>
        public bool HasTag(string fullTagName)
            => !fullTagName.IsNullOrEmpty() &&
               HasTag(_tagRegistry.GetTag(fullTagName));

        /// <summary>
        /// Check if it has given tag by tag namespace and name.
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to compare.</param>
        /// <param name="tagName">The game tag's name to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        /// <remarks>
        /// If tag is disabled, it won't be treated as "has".
        /// </remarks>
        public bool HasTag(string tagNamespace, string tagName)
            => !(tagNamespace.IsNullOrEmpty() || tagName.IsNullOrEmpty()) &&
               HasTag(_tagRegistry.GetTag(tagNamespace, tagName));

        /// <summary>
        /// Check if it has given tag by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to compare.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        /// <remarks>
        /// If tag is disabled, it won't be treated as "has".
        /// </remarks>
        public bool HasTag(string tagName, int _)
            => !tagName.IsNullOrEmpty() &&
               HasTag(_tagRegistry.GetTag1(tagName));

        /// <summary>
        /// Check if it has given tag by tag hash.
        /// </summary>
        /// <param name="tagHash">The game tag's hash code to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        /// <remarks>
        /// If tag is disabled, it won't be treated as "has".
        /// </remarks>
        public bool HasTag(int tagHash)
            => tagHash != 0 &&
               HasTag(_tagRegistry.GetTag(tagHash));

        /// <summary>
        /// Check if it has given tag by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to compare.</param>
        /// <returns>True if it has given tag; otherwise, false.</returns>
        /// <remarks>
        /// If tag is disabled, it won't be treated as "has".
        /// </remarks>
        public bool HasTag1(string tagName)
            => !tagName.IsNullOrEmpty() &&
               HasTag(_tagRegistry.GetTag1(tagName));

        #endregion

        #region AddTag Methods

        /// <summary>
        /// Add tag to runtime tags by GameTag entry.
        /// </summary>
        /// <param name="tagToAdd">The game tag entry to add.</param>
        /// <returns>True if add successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool AddTag(GameTag tagToAdd)
        {
            if (!tagToAdd) return false;
            if (_runtimeTags == null || _disabledTags == null) return false;
            if (_disabledTags.Contains(tagToAdd)) return false;

            return _runtimeTags.Add(tagToAdd);
        }

        /// <summary>
        /// Add tag to runtime tags by full tag name.
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to add.</param>
        /// <returns>True if add successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool AddTag(string fullTagName)
            => !fullTagName.IsNullOrEmpty() &&
               AddTag(_tagRegistry.GetTag(fullTagName));

        /// <summary>
        /// Add tag to runtime tags by tag namespace and name.
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to add.</param>
        /// <param name="tagName">The game tag's name to add.</param>
        /// <returns>True if add successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool AddTag(string tagNamespace, string tagName)
            => !(tagNamespace.IsNullOrEmpty() || tagName.IsNullOrEmpty()) &&
               AddTag(_tagRegistry.GetTag(tagNamespace, tagName));

        /// <summary>
        /// Add tag to runtime tags by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to add.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if add successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool AddTag(string tagName, int _)
            => !tagName.IsNullOrEmpty() &&
               AddTag(_tagRegistry.GetTag1(tagName));

        /// <summary>
        /// Add tag to runtime tags by tag hash.
        /// </summary>
        /// <param name="tagHash">The game tag's hash to add.</param>
        /// <returns>True if add successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool AddTag(int tagHash)
            => tagHash != 0 &&
               AddTag(_tagRegistry.GetTag(tagHash));

        /// <summary>
        /// Add tag to runtime tags by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to add.</param>
        /// <returns>True if add successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool AddTag1(string tagName)
            => !tagName.IsNullOrEmpty() &&
               AddTag(_tagRegistry.GetTag1(tagName));

        /// <summary>
        /// Add tag to runtime tags by GameTag entry.
        /// </summary>
        /// <param name="tagToAdd">The game tag entry to add.</param>
        /// <returns>True if add successful or tag already exists; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag at least exist in one of two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryAddTag(GameTag tagToAdd)
        {
            if (!tagToAdd) return false;
            if (_runtimeTags == null || _disabledTags == null) return false;
            if (_disabledTags.Contains(tagToAdd)) return true;

            _runtimeTags.Add(tagToAdd); // If add failed and tag do not exist, it won't return false but throw.
            return true;
        }

        /// <summary>
        /// Add tag to runtime tags by full tag name.
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to add.</param>
        /// <returns>True if add successful or tag already exists; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag at least exist in one of two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryAddTag(string fullTagName)
            => !fullTagName.IsNullOrEmpty() &&
               TryAddTag(_tagRegistry.GetTag(fullTagName));

        /// <summary>
        /// Add tag to runtime tags by tag namespace and name.
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to add.</param>
        /// <param name="tagName">The game tag's name to add.</param>
        /// <returns>True if add successful or tag already exists; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag at least exist in one of two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryAddTag(string tagNamespace, string tagName)
            => !(tagNamespace.IsNullOrEmpty() || tagName.IsNullOrEmpty()) &&
               TryAddTag(_tagRegistry.GetTag(tagNamespace, tagName));

        /// <summary>
        /// Add tag to runtime tags by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to add.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if add successful or tag already exists; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag at least exist in one of two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryAddTag(string tagName, int _)
            => !tagName.IsNullOrEmpty() &&
               TryAddTag(_tagRegistry.GetTag1(tagName));

        /// <summary>
        /// Add tag to runtime tags by tag hash.
        /// </summary>
        /// <param name="tagHash">The game tag's hash to add.</param>
        /// <returns>True if add successful or tag already exists; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag at least exist in one of two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryAddTag(int tagHash)
            => tagHash != 0 &&
               TryAddTag(_tagRegistry.GetTag(tagHash));

        /// <summary>
        /// Add tag to runtime tags by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to add.</param>
        /// <returns>True if add successful or tag already exists; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag at least exist in one of two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryAddTag1(string tagName)
            => !tagName.IsNullOrEmpty() &&
               TryAddTag(_tagRegistry.GetTag1(tagName));

        #endregion

        #region RemoveTag Methods

        /// <summary>
        /// Remove tag from runtime tags by GameTag entry.
        /// </summary>
        /// <param name="tagToRemove">The game tag entry to remove.</param>
        /// <returns>True if remove successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool RemoveTag(GameTag tagToRemove)
        {
            if (!tagToRemove) return false;
            if (_runtimeTags == null || _disabledTags == null) return false;

            return _runtimeTags.Remove(tagToRemove) || _disabledTags.Remove(tagToRemove);
        }

        /// <summary>
        /// Remove tag from runtime tags by full tag name.
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to remove.</param>
        /// <returns>True if remove successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool RemoveTag(string fullTagName)
            => !fullTagName.IsNullOrEmpty() && RemoveTag(_tagRegistry.GetTag(fullTagName));

        /// <summary>
        /// Remove tag from runtime tags by tag namespace and name.
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to remove.</param>
        /// <param name="tagName">The game tag's name to remove.</param>
        /// <returns>True if remove successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool RemoveTag(string tagNamespace, string tagName)
            => !(tagNamespace.IsNullOrEmpty() || tagName.IsNullOrEmpty()) &&
               RemoveTag(_tagRegistry.GetTag(tagNamespace, tagName));

        /// <summary>
        /// Remove tag from runtime tags by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to remove.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if remove successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool RemoveTag(string tagName, int _)
            => !tagName.IsNullOrEmpty() &&
               RemoveTag(_tagRegistry.GetTag1(tagName));

        /// <summary>
        /// Remove tag from runtime tags by tag hash.
        /// </summary>
        /// <param name="tagHash">The game tag's hash to remove.</param>
        /// <returns>True if remove successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool RemoveTag(int tagHash)
            => tagHash != 0 &&
               RemoveTag(_tagRegistry.GetTag(tagHash));

        /// <summary>
        /// Remove tag to runtime tags by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to remove.</param>
        /// <returns>True if remove successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool RemoveTag1(string tagName)
            => !tagName.IsNullOrEmpty() &&
               RemoveTag(_tagRegistry.GetTag1(tagName));

        /// <summary>
        /// Remove tag from runtime tags by GameTag entry.
        /// </summary>
        /// <param name="tagToRemove">The game tag entry to remove.</param>
        /// <returns>True if remove successful or tag doesn't exist in set; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag does not exist in both two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryRemoveTag(GameTag tagToRemove)
        {
            if (!tagToRemove) return false;
            if (_runtimeTags == null || _disabledTags == null) return false;

            _runtimeTags.Remove(tagToRemove);
            _disabledTags.Remove(tagToRemove);
            return true;
        }

        /// <summary>
        /// Remove tag from runtime tags by full tag name.
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to remove.</param>
        /// <returns>True if remove successful or tag doesn't exist in set; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag does not exist in both two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryRemoveTag(string fullTagName)
            => !fullTagName.IsNullOrEmpty() &&
               TryRemoveTag(_tagRegistry.GetTag(fullTagName));

        /// <summary>
        /// Remove tag from runtime tags by tag namespace and name.
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to remove.</param>
        /// <param name="tagName">The game tag's name to remove.</param>
        /// <returns>True if remove successful or tag doesn't exist in set; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag does not exist in both two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryRemoveTag(string tagNamespace, string tagName)
            => !(tagNamespace.IsNullOrEmpty() || tagName.IsNullOrEmpty()) &&
               TryRemoveTag(_tagRegistry.GetTag(tagNamespace, tagName));

        /// <summary>
        /// Remove tag from runtime tags by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to remove.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if remove successful or tag doesn't exist in set; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag does not exist in both two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryRemoveTag(string tagName, int _)
            => !tagName.IsNullOrEmpty() &&
               TryRemoveTag(_tagRegistry.GetTag1(tagName));

        /// <summary>
        /// Remove tag from runtime tags by tag hash.
        /// </summary>
        /// <param name="tagHash">The game tag's hash to remove.</param>
        /// <returns>True if remove successful or tag doesn't exist in set; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag does not exist in both two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryRemoveTag(int tagHash)
            => tagHash != 0 &&
               TryRemoveTag(_tagRegistry.GetTag(tagHash));

        /// <summary>
        /// Remove tag to runtime tags by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to remove.</param>
        /// <returns>True if remove successful or tag doesn't exist in set; otherwise, false.</returns>
        /// <remarks>
        /// When return true, it ensures that input tag does not exist in both two sets.
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool TryRemoveTag1(string tagName)
            => !tagName.IsNullOrEmpty() &&
               TryRemoveTag(_tagRegistry.GetTag1(tagName));

        #endregion

        #region EnableTag Methods

        /// <summary>
        /// Enable tag in runtime by GameTag entry. It must already have that tag. 
        /// </summary>
        /// <param name="tagToEnable">The game tag entry to enable.</param>
        /// <returns>True if enable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool EnableTag(GameTag tagToEnable)
        {
            if (!tagToEnable) return false;
            if (_runtimeTags == null || _disabledTags == null) return false;

            // Short-circuit
            return _disabledTags.Remove(tagToEnable) && _runtimeTags.Add(tagToEnable);
        }

        /// <summary>
        /// Enable tag in runtime by full tag name. It must already have that tag. 
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to enable.</param>
        /// <returns>True if enable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool EnableTag(string fullTagName)
            => !fullTagName.IsNullOrEmpty() &&
               EnableTag(_tagRegistry.GetTag(fullTagName));


        /// <summary>
        /// Enable tag in runtime by tag namespace and name. It must already have that tag. 
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to enable.</param>
        /// <param name="tagName">The game tag's name to enable.</param>
        /// <returns>True if disable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool EnableTag(string tagNamespace, string tagName)
            => !(tagNamespace.IsNullOrEmpty() || tagName.IsNullOrEmpty()) &&
               EnableTag(_tagRegistry.GetTag(tagNamespace, tagName));


        /// <summary>
        /// Enable tag in runtime by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to enable.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if enable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool EnableTag(string tagName, int _)
            => !tagName.IsNullOrEmpty() &&
               EnableTag(_tagRegistry.GetTag1(tagName));


        /// <summary>
        /// Enable tag in runtime by tag hash.
        /// </summary>
        /// <param name="tagHash">The game tag's hash to enable.</param>
        /// <returns>True if enable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool EnableTag(int tagHash)
            => tagHash != 0 &&
               EnableTag(_tagRegistry.GetTag(tagHash));


        /// <summary>
        /// Enable tag in runtime by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to enable.</param>
        /// <returns>True if enable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool EnableTag1(string tagName)
            => !tagName.IsNullOrEmpty() &&
               EnableTag(_tagRegistry.GetTag1(tagName));


        /// <summary>
        /// Enable tag in runtime by GameTag entry. It must already have that tag (include disabled). 
        /// </summary>
        /// <param name="tagToEnable">The game tag entry to enable.</param>
        /// <returns>True if enable successful or tag already disabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in runtime tags and doesn't exist in disabled tags.
        /// </remarks>
        public bool TryEnableTag(GameTag tagToEnable)
        {
            if (!tagToEnable) return false;
            if (_runtimeTags == null || _disabledTags == null) return false;

            //  R     D 
            // Has | Has -> True
            // Has | No  -> True
            // No  | Has -> True
            // No  | No  -> False
            if (!_disabledTags.Remove(tagToEnable)) return _runtimeTags.Contains(tagToEnable);
            _runtimeTags.Add(tagToEnable);
            return true;
        }

        /// <summary>
        /// Enable tag in runtime by full tag name. It must already have that tag (include disabled). 
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to enable.</param>
        /// <returns>True if enable successful or tag already enabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in runtime tags and doesn't exist in disabled tags.
        /// </remarks>
        public bool TryEnableTag(string fullTagName)
            => !fullTagName.IsNullOrEmpty() &&
               TryEnableTag(_tagRegistry.GetTag(fullTagName));


        /// <summary>
        /// Enable tag in runtime by tag namespace and name. It must already have that tag (include disabled).
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to enable.</param>
        /// <param name="tagName">The game tag's name to enable.</param>
        /// <returns>True if enable successful or tag already enabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in runtime tags and doesn't exist in disabled tags.
        /// </remarks>
        public bool TryEnableTag(string tagNamespace, string tagName)
            => !(tagNamespace.IsNullOrEmpty() || tagName.IsNullOrEmpty()) &&
               TryEnableTag(_tagRegistry.GetTag(tagNamespace, tagName));


        /// <summary>
        /// Enable tag in runtime by tag name in "Default" namespace. It must already have that tag (include disabled).
        /// </summary>
        /// <param name="tagName">The game tag's name to enable.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if enable successful or tag already enabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in runtime tags and doesn't exist in disabled tags.
        /// </remarks>
        public bool TryEnableTag(string tagName, int _)
            => !tagName.IsNullOrEmpty() &&
               TryEnableTag(_tagRegistry.GetTag1(tagName));


        /// <summary>
        /// Enable tag in runtime by tag hash. It must already have that tag (include disabled).
        /// </summary>
        /// <param name="tagHash">The game tag's hash to enable.</param>
        /// <returns>True if enable successful or tag already enabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in runtime tags and doesn't exist in disabled tags.
        /// </remarks>
        public bool TryEnableTag(int tagHash)
            => tagHash != 0 &&
               TryEnableTag(_tagRegistry.GetTag(tagHash));


        /// <summary>
        /// Enable tag in runtime by tag name in "Default" namespace. It must already have that tag (include disabled).
        /// </summary>
        /// <param name="tagName">The game tag's name to enable.</param>
        /// <returns>True if enable successful or tag already enabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in runtime tags and doesn't exist in disabled tags.
        /// </remarks>
        public bool TryEnableTag1(string tagName)
            => !tagName.IsNullOrEmpty() &&
               TryEnableTag(_tagRegistry.GetTag1(tagName));

        #endregion

        #region DisableTag Methods

        /// <summary>
        /// Disable tag in runtime by GameTag entry. It must already have that tag. 
        /// </summary>
        /// <param name="tagToDisable">The game tag entry to disable.</param>
        /// <returns>True if disable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool DisableTag(GameTag tagToDisable)
        {
            if (!tagToDisable) return false;
            if (_runtimeTags == null || _disabledTags == null) return false;

            // Short-circuit
            return _runtimeTags.Remove(tagToDisable) && _disabledTags.Add(tagToDisable);
        }

        /// <summary>
        /// Disable tag in runtime by full tag name. It must already have that tag. 
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to disable.</param>
        /// <returns>True if disable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool DisableTag(string fullTagName)
            => !fullTagName.IsNullOrEmpty() &&
               DisableTag(_tagRegistry.GetTag(fullTagName));


        /// <summary>
        /// Disable tag in runtime by tag namespace and name. It must already have that tag. 
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to disable.</param>
        /// <param name="tagName">The game tag's name to disable.</param>
        /// <returns>True if disable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool DisableTag(string tagNamespace, string tagName)
            => !(tagNamespace.IsNullOrEmpty() || tagName.IsNullOrEmpty()) &&
               DisableTag(_tagRegistry.GetTag(tagNamespace, tagName));


        /// <summary>
        /// Disable tag in runtime by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to disable.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if disable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool DisableTag(string tagName, int _)
            => !tagName.IsNullOrEmpty() &&
               DisableTag(_tagRegistry.GetTag1(tagName));


        /// <summary>
        /// Disable tag in runtime by tag hash.
        /// </summary>
        /// <param name="tagHash">The game tag's hash to disable.</param>
        /// <returns>True if disable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool DisableTag(int tagHash)
            => tagHash != 0 &&
               DisableTag(_tagRegistry.GetTag(tagHash));


        /// <summary>
        /// Disable tag in runtime by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to disable.</param>
        /// <returns>True if disable successful; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// </remarks>
        public bool DisableTag1(string tagName)
            => !tagName.IsNullOrEmpty() &&
               DisableTag(_tagRegistry.GetTag1(tagName));


        /// <summary>
        /// Disable tag in runtime by GameTag entry. It must already have that tag (include disabled). 
        /// </summary>
        /// <param name="tagToDisable">The game tag entry to disable.</param>
        /// <returns>True if disable successful or tag already disabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in disabled tags and doesn't exist in runtime tags.
        /// </remarks>
        public bool TryDisableTag(GameTag tagToDisable)
        {
            if (!tagToDisable) return false;
            if (_runtimeTags == null || _disabledTags == null) return false;

            //  R     D 
            // Has | Has -> True
            // Has | No  -> True
            // No  | Has -> True
            // No  | No  -> False
            if (!_runtimeTags.Remove(tagToDisable)) return _disabledTags.Contains(tagToDisable);
            _disabledTags.Add(tagToDisable);
            return true;
        }

        /// <summary>
        /// Disable tag in runtime by full tag name. It must already have that tag (include disabled). 
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to disable.</param>
        /// <returns>True if disable successful or tag already disabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in disabled tags and doesn't exist in runtime tags.
        /// </remarks>
        public bool TryDisableTag(string fullTagName)
            => !fullTagName.IsNullOrEmpty() &&
               TryDisableTag(_tagRegistry.GetTag(fullTagName));


        /// <summary>
        /// Disable tag in runtime by tag namespace and name. It must already have that tag (include disabled).
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to disable.</param>
        /// <param name="tagName">The game tag's name to disable.</param>
        /// <returns>True if disable successful or tag already disabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in disabled tags and doesn't exist in runtime tags.
        /// </remarks>
        public bool TryDisableTag(string tagNamespace, string tagName)
            => !(tagNamespace.IsNullOrEmpty() || tagName.IsNullOrEmpty()) &&
               TryDisableTag(_tagRegistry.GetTag(tagNamespace, tagName));


        /// <summary>
        /// Disable tag in runtime by tag name in "Default" namespace. It must already have that tag (include disabled).
        /// </summary>
        /// <param name="tagName">The game tag's name to disable.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>True if disable successful or tag already disabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in disabled tags and doesn't exist in runtime tags.
        /// </remarks>
        public bool TryDisableTag(string tagName, int _)
            => !tagName.IsNullOrEmpty() &&
               TryDisableTag(_tagRegistry.GetTag1(tagName));


        /// <summary>
        /// Disable tag in runtime by tag hash. It must already have that tag (include disabled).
        /// </summary>
        /// <param name="tagHash">The game tag's hash to disable.</param>
        /// <returns>True if disable successful or tag already disabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in disabled tags and doesn't exist in runtime tags.
        /// </remarks>
        public bool TryDisableTag(int tagHash)
            => tagHash != 0 &&
               TryDisableTag(_tagRegistry.GetTag(tagHash));


        /// <summary>
        /// Disable tag in runtime by tag name in "Default" namespace. It must already have that tag (include disabled).
        /// </summary>
        /// <param name="tagName">The game tag's name to disable.</param>
        /// <returns>True if disable successful or tag already disabled; otherwise, false.</returns>
        /// <remarks>
        /// When return false, it performs no substantive operation.
        /// When return true, it ensures input tag exist in disabled tags and doesn't exist in runtime tags.
        /// </remarks>
        public bool TryDisableTag1(string tagName)
            => !tagName.IsNullOrEmpty() &&
               TryDisableTag(_tagRegistry.GetTag1(tagName));

        #endregion

        #region Indexer

        /// <summary>
        /// An indexer wrap of HasTag by GameTag entry.
        /// </summary>
        /// <param name="tagToGet">The game tag entry to compare.</param>
        /// <remarks>
        /// This indexer wrap return bool rather than GameTag.
        /// </remarks>
        public bool this[GameTag tagToGet]
            => HasTag(tagToGet);

        /// <summary>
        /// An indexer wrap of HasTag by full tag name.
        /// </summary>
        /// <param name="fullTagName">The game tag's full name to compare.</param>
        /// <remarks>
        /// This indexer wrap return bool rather than GameTag.
        /// </remarks>
        public bool this[string fullTagName]
            => HasTag(fullTagName);

        /// <summary>
        /// An indexer wrap of HasTag by tag namespace and name.
        /// </summary>
        /// <param name="tagNamespace">The game tag's namespace to compare.</param>
        /// <param name="tagName">The game tag's name to compare.</param>
        /// <remarks>
        /// This indexer wrap return bool rather than GameTag.
        /// </remarks>
        public bool this[string tagNamespace, string tagName]
            => HasTag(tagNamespace, tagName);

        /// <summary>
        /// An indexer wrap of HasTag by tag name in "Default" namespace.
        /// </summary>
        /// <param name="tagName">The game tag's name to compare.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <remarks>
        /// This indexer wrap return bool rather than GameTag.
        /// </remarks>
        public bool this[string tagName, int _]
            => HasTag1(tagName);

        /// <summary>
        /// An indexer wrap of HasTag by tag hash.
        /// </summary>
        /// <param name="tagHash">The game tag's hash code to compare.</param>
        /// <remarks>
        /// This indexer wrap return bool rather than GameTag.
        /// </remarks>
        public bool this[int tagHash]
            => HasTag(tagHash);

        #endregion

        // TODO: #region Operator Overloading
        //
        // // TODO: + - (for += -=) | & (return true/false) operator
        // public static Taggable operator +(Taggable taggable1, GameTag tag)
        // {
        //     return taggable1;
        // }
        //
        // #endregion
    }
}