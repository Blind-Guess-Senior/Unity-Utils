using System;
using System.Reflection;
using Artifact.UnityUtils.Core.Installer;
using Artifact.UnityUtils.Core.Locator;
using Artifact.UnityUtils.Data.Registry;
using Artifact.UnityUtils.Extensions;
using Artifact.UnityUtils.Utilities.DebugUtils;
using UnityEditor;
using UnityEngine;
using StringExtensions = Artifact.UnityUtils.Extensions.StringExtensions;

// ReSharper disable CheckNamespace

namespace Artifact.UnityUtils.Data.Tag
{
    /// <summary>
    /// Tag library SO which can be created as a static asset.
    /// Stores all exist game tags.
    /// </summary>
    [CreateAssetMenu(fileName = "TagLibrary", menuName = "Artifact Unity Utils/Tag System/TagLibrary")]
    public class TagLibrary : GenericLibrary<GameTag>
    {
    }

    /// <summary>
    /// Registry utility class for tag library. 
    /// Provide query methods for all exist game tags.
    /// </summary>
    public class TagRegistry : GenericRegistry<TagLibrary, GameTag, int>
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// Using tag's hash code as key.
        /// </summary>
        /// <param name="library">The tag library this registry works for.</param>
        protected TagRegistry(TagLibrary library) : base(library, TagHash)
        {
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Get game tag's hash code.
        /// </summary>
        /// <param name="gameTag">The game tag.</param>
        /// <returns>The hash code of given game tag.</returns>
        private static int TagHash(GameTag gameTag) => gameTag.Hash;

        #endregion

        #region Methods

        /// <summary>
        /// A wrap of entry getting function. Get tag by tag's full name.
        /// </summary>
        /// <param name="fullTagName">The tag name in complete representation.</param>
        /// <returns>The GameTag entry for given tag name.</returns>
        public GameTag GetTag(string fullTagName)
            => GetEntry(fullTagName.GetStableHash());

        /// <summary>
        /// A wrap of entry getting function. Get tag by tag's namespace and name.
        /// </summary>
        /// <param name="tagNamespace">The namespace of tag.</param>
        /// <param name="tagName">The name of tag.</param>
        /// <returns>The GameTag entry for given tag name.</returns>
        public GameTag GetTag(string tagNamespace, string tagName)
            => GetEntry(StringExtensions.GetStableHash(tagNamespace, "@", tagName));

        /// <summary>
        /// A wrap of entry getting function. Get tag by tag's name with "Default" namespace.
        /// </summary>
        /// <param name="tagName">The name of tag.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>The GameTag entry for given tag name.</returns>
        public GameTag GetTag(string tagName, int _)
            => GetEntry(StringExtensions.GetStableHash("Default@", tagName));

        /// <summary>
        /// A wrap of entry getting function. Get tag by tag's hash code.
        /// </summary>
        /// <param name="tagHash">The hash code of a tag.</param>
        /// <returns>The GameTag entry for given tag hash.</returns>
        public GameTag GetTag(int tagHash)
            => GetEntry(tagHash);

        /// <summary>
        /// A wrap of entry getting function. Get tag by tag's name with "Default" namespace.
        /// </summary>
        /// <param name="tagName">The name of tag.</param>
        /// <returns>The GameTag entry for given tag name.</returns>
        public GameTag GetTag1(string tagName)
            => GetEntry(StringExtensions.GetStableHash("Default@", tagName));

        #endregion

        #region Indexer

        /// <summary>
        /// An indexer wrap of entry getting function. Get tag by tag's fullname
        /// </summary>
        /// <param name="fullTagName">The tag name in complete representation.</param>
        public GameTag this[string fullTagName]
            => GetEntry(fullTagName.GetStableHash());

        /// <summary>
        /// An indexer wrap of entry getting function. Get tag by tag's namespace and name.
        /// </summary>
        /// <param name="tagNamespace">The namespace of tag.</param>
        /// <param name="tagName">The name of tag.</param>
        /// <returns>The GameTag entry for given tag name.</returns>
        public GameTag this[string tagNamespace, string tagName]
            => GetEntry(StringExtensions.GetStableHash(tagNamespace, "@", tagName));

        /// <summary>
        /// An indexer wrap of entry getting function. Get tag by tag's name with "Default" namespace.
        /// </summary>
        /// <param name="tagName">The name of tag.</param>
        /// <param name="_">Use for distinguish from fullname overloading.</param>
        /// <returns>The GameTag entry for given tag name.</returns>
        public GameTag this[string tagName, int _]
            => GetEntry(StringExtensions.GetStableHash("Default@", tagName));

        /// <summary>
        /// An indexer wrap of entry getting function. Get tag by tag's hash code.
        /// </summary>
        /// <param name="tagHash">The hash code of a tag.</param>
        /// <returns>The GameTag entry for given tag hash.</returns>
        public GameTag this[int tagHash]
            => GetEntry(tagHash);

        #endregion
    }

    /// <summary>
    /// Installer for TagRegistry that make registry available for taggable injection.
    /// </summary>
    public class TagRegistryInstaller : GenericInstaller<TagRegistry>
    {
        /// <summary>
        /// Install tag registry as a service in locator.
        /// <br/>
        /// It would create an instance of TagRegistry and then register it to locator.
        /// </summary>
        public override void InstallService()
        {
            string[] libraryGuids = AssetDatabase.FindAssets($"t:{nameof(TagLibrary)}");
            if (libraryGuids.Length == 0)
            {
                ArtifactDebug.PackageLog(
                    $"[Tag Library] {nameof(TagLibrary)}.asset not found! Please create one.", DebugLogLevel.Error);
                return;
            }

            string libraryPath = AssetDatabase.GUIDToAssetPath(libraryGuids[0]);
            var library = AssetDatabase.LoadAssetAtPath<TagLibrary>(libraryPath);

            var registry = (TagRegistry)Activator.CreateInstance(
                typeof(TagRegistry),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { library },
                null
            );

            ServiceLocator.Register(registry);
        }
    }
}