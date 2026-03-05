#if __ARTIFACT_UNITY_UTILS__QUADTREE_ENABLED

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Installer;
using Core.Locator;
using UnityEngine;
using UnityEngine.Scripting;
using Utilities.DebugUtils;

namespace Utilities.DataStructure.QuadTree
{
    /// <summary>
    /// QuadTree data structure implementation for Collider2D's collision detection.
    /// <br/>
    /// It is noexcept in released runtime. Exception handling only take effect while in editor mode.
    /// </summary>
    /// <remarks>
    /// Toggle in editor's "Artifact Unity Utils/Data Structure/QuadTree/Features" to enable/disable advanced features.
    /// </remarks>
    /// <example>
    /// To create a new QuadTree, see <see cref="QuadTreeInstaller{TQuadTree}"/>.
    /// </example>
    public class QuadTree : IService
    {
        #region Fields

        /// <summary>
        /// Root node of this QuadTree.
        /// </summary>
        private readonly QuadTreeNode _root;

        /// <summary>
        /// All items stored in whole quad tree.
        /// </summary>
        private readonly Dictionary<int, Collider2D> _allItems = new();

        /// <summary>
        /// Store all nodes to which each item belongs.
        /// Used to improve the performance of remove.
        /// </summary>
        private readonly Dictionary<int, HashSet<QuadTreeNode>> _itemsNodeMap = new();

        #endregion

        #region Definitions

        /// <summary>
        /// Nodes in QuadTree.
        /// </summary>
        private class QuadTreeNode
        {
            #region Fields & Properties

            /// <summary>
            /// Center position of this node.
            /// </summary>
            public Vector2 Center;

            /// <summary>
            /// Boundaries in 4 directions of this node.
            /// They will be 10% larger than the actual size as tolerance
            /// to avoid entering and exiting too frequently and improve the accuracy of collision detection.
            /// </summary>
            public float Top;

            public float Bottom;
            public float Left;
            public float Right;

            /// <summary>
            /// Rect of current node.
            /// </summary>
            public Rect NodeRect;

            /// <summary>
            /// The depth level of this node.
            /// 0 for root.
            /// </summary>
            private int Level { get; }

            /// <summary>
            /// Tree reference.
            /// </summary>
            private readonly QuadTree _tree;

            /// <summary>
            /// Parent node of current node.
            /// </summary>
            private QuadTreeNode _parent;

            /// <summary>
            /// Four children nodes of current node.
            /// null; Right-Up; Right-Down; Left-Down; Left-Up; respectively.
            /// Same as cartesian coordinate system.
            /// </summary>
            private QuadTreeNode[] _subNodes;

            /// <summary>
            /// Store all object in this node. Key is InstanceID of object.
            /// </summary>
            private Dictionary<int, Collider2D> _items = new();

            /// <summary>
            /// Max object counts of one node.
            /// If exceed threshold, split will occur.
            /// </summary>
            public int SplitThreshold = 12;

            /// <summary>
            /// Max depth of QuadTree.
            /// If one node's level equals _maxDepth, split won't occur.
            /// </summary>
            public int MaxDepth = 8;

            #endregion

            #region Constructors

            /// <summary>
            /// Create node with given parent.
            /// Inherit split threshold, max depth from parent.
            /// </summary>
            /// <param name="parent">The parent node of newly created node.</param>
            /// <param name="tree">The reference to the tree itself.</param>
            public QuadTreeNode(QuadTreeNode parent, QuadTree tree)
            {
                _parent = parent;
                _tree = tree;

                Level = parent?.Level + 1 ?? 0;
                SplitThreshold = parent?.SplitThreshold ?? SplitThreshold;
                MaxDepth = parent?.MaxDepth ?? MaxDepth;
            }

            /// <summary>
            /// Overloading ctor for non-null parent.
            /// </summary>
            /// <param name="parent">The parent node of newly created node.</param>
            public QuadTreeNode(QuadTreeNode parent)
            {
                _parent = parent;
                _tree = parent._tree;

                Level = parent.Level + 1;
                SplitThreshold = parent.SplitThreshold;
                MaxDepth = parent.MaxDepth;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Set pos for current node.
            /// <br/>
            /// They will be 10% larger than the actual size as tolerance
            /// to avoid entering and exiting too frequently and improve the accuracy of collision detection.
            /// </summary>
            /// <param name="left">The left pos of current node.</param>
            /// <param name="top">The top pos of current node.</param>
            /// <param name="right">The right pos of current node.</param>
            /// <param name="bottom">The bottom pos of current node.</param>
            /// <param name="center">The center vector of current node.</param>
            public void SetPos(float left, float top, float right, float bottom, Vector2 center)
            {
                Center = center;
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;

                var xTolerance = (left + right) / 10;
                var yTolerance = (top + bottom) / 10;
                // IMPORTANT: the coordinate system of rect and Unity world is different:
                // Rect                         Unity
                // minx,miny    maxx,miny      minx,maxy    maxx,maxy
                // 
                // minx,maxy    maxx,maxy      minx,miny    maxx,miny
                // And Rect's y-axis is mirror to 0 compare with Unity's.
                NodeRect = new Rect(left - xTolerance, -top - yTolerance,
                    right - left + xTolerance * 2, top - bottom + yTolerance * 2);
            }

            /// <summary>
            /// Try adding an item to current node.
            /// If exceed threshold, split.
            /// </summary>
            /// <param name="item">The item to add.</param>
            private void Add(KeyValuePair<int, Collider2D> item)
            {
                if (!Intersects(item.Value))
                {
                    return;
                }

                if (_subNodes != null)
                {
                    for (var i = 1; i < 5; ++i)
                    {
                        _subNodes[i].Add(item);
                    }
                }
                else
                {
                    if (_items.ContainsKey(item.Key))
                    {
                        return;
                    }

                    _items.Add(item.Key, item.Value);

                    // Add current node into item node map.
                    if (!_tree._itemsNodeMap.TryGetValue(item.Key, out var nodes))
                    {
                        nodes = new HashSet<QuadTreeNode>();
                        _tree._itemsNodeMap[item.Key] = nodes;
                    }

                    nodes.Add(this);

                    if (_items.Count > SplitThreshold && Level < MaxDepth)
                    {
                        Split();
                    }
                }
            }

            /// <summary>
            /// Try adding an item to current node.
            /// If exceed threshold, split.
            /// <br/>
            /// Overloading method for raw item input.
            /// </summary>
            /// <param name="item">The item to add.</param>
            public void Add(Collider2D item)
                => Add(new KeyValuePair<int, Collider2D>(item.GetInstanceID(), item));

            /// <summary>
            /// Try removing an item from current node.
            /// </summary>
            /// <param name="itemInstanceID">The item's instance id to remove.</param>
            public void Remove(int itemInstanceID)
            {
                if (_items.Remove(itemInstanceID))
                {
#if UNITY_EDITOR
                    try
                    {
#endif
                        _tree._itemsNodeMap[itemInstanceID].Remove(this);
#if UNITY_EDITOR
                    }
                    catch (KeyNotFoundException)
                    {
                        ArtifactDebug.PackageLog(
                            "[QuadTree] Could not found item(with id): " + itemInstanceID + " when remove;" +
                            "; Try check if object is added correctly or Init is done correctly with right rectangle range.",
                            DebugLogLevel.Fatal);
                        throw;
                    }
#endif
                }
            }

            /// <summary>
            /// Try removing an item from current node.
            /// </summary>
            /// <param name="item">The item to remove.</param>
            public void Remove(Collider2D item)
                => Remove(item.GetInstanceID());

            /// <summary>
            /// Try deleting an item from QuadTree.
            /// </summary>
            /// <param name="itemInstanceID">The item to delete.</param>
            [Conditional("__ARTIFACT_UNITY_UTILS__QUADTREE_NOTINTREEITEMQUERY")]
            private void Delete(int itemInstanceID)
            {
                Remove(itemInstanceID);

                if (_tree._itemsNodeMap[itemInstanceID].Count == 0)
                {
                    _tree._itemsNodeMap.Remove(itemInstanceID);
                }
            }

            /// <summary>
            /// Try to dismiss an item if it is not in range of current node.
            /// </summary>
            /// <param name="item">The item to evaluate.</param>
            public void TryDismiss(Collider2D item)
            {
                if (!Intersects(item))
                {
                    Remove(item);
                }
            }

            /// <summary>
            /// Check if given item intersects with current node.
            /// </summary>
            /// <param name="item">The item to check.</param>
            /// <returns>True if item intersects with current node; otherwise, false.</returns>
            private bool Intersects(Collider2D item)
                => NodeRect.Overlaps(GetItemRect(item));

            /// <summary>
            /// Getting all nodes that intersected with given item.
            /// </summary>
            /// <param name="itemRect">The rect of item to check intersecting.</param>
            /// <param name="results">The results of intersection. Will be modified.</param>
            [Conditional("__ARTIFACT_UNITY_UTILS__QUADTREE_NOTINTREEITEMQUERY")]
            public void CollectIntersectingNodes(Rect itemRect, HashSet<QuadTreeNode> results)
            {
                if (!NodeRect.Overlaps(itemRect))
                    return;

                if (_subNodes != null)
                {
                    for (var i = 1; i < 5; i++)
                    {
                        _subNodes[i].CollectIntersectingNodes(itemRect, results);
                    }
                }
                else
                {
                    results.Add(this);
                }
            }

            /// <summary>
            /// Split current node into 4 nodes.
            /// </summary>
            private void Split()
            {
                _subNodes = new QuadTreeNode[5];

                for (var i = 1; i < 5; i++)
                {
                    _subNodes[i] = new QuadTreeNode(this);

                    float newTop = 0f, newBottom = 0f, newLeft = 0f, newRight = 0f;
                    switch (i)
                    {
                        case 1:
                            newTop = Top;
                            newBottom = Center.y;
                            newLeft = Center.x;
                            newRight = Right;
                            break;
                        case 2:
                            newTop = Center.y;
                            newBottom = Bottom;
                            newLeft = Center.x;
                            newRight = Right;
                            break;
                        case 3:
                            newTop = Center.y;
                            newBottom = Bottom;
                            newLeft = Left;
                            newRight = Center.x;
                            break;
                        case 4:
                            newTop = Top;
                            newBottom = Center.y;
                            newLeft = Left;
                            newRight = Center.x;
                            break;
                    }

                    var newCenter = new Vector2((newLeft + newRight) / 2, (newTop + newBottom) / 2);

                    _subNodes[i].SetPos(newLeft, newTop, newRight, newBottom, newCenter);
                }

                foreach (var item in _items)
                {
                    for (var i = 1; i < 5; i++)
                    {
                        _subNodes[i].Add(item);
                    }

                    _tree._itemsNodeMap[item.Key].Remove(this);
                }

                _items.Clear();
            }

            /// <summary>
            /// Get all items that intersected with given item. Used for item that not in QuadTree.
            /// </summary>
            /// <param name="item">The item to check intersecting.</param>
            /// <param name="predicate">The judgement function for intersection detection.</param>
            /// <param name="results">The results of intersection. Will be modified.</param>
            public void GetIntersected(Collider2D item, Func<Collider2D, bool> predicate, HashSet<Collider2D> results)
            {
                var instanceID = item.GetInstanceID();
                foreach (var pair in _items
                             .Where(pair => pair.Key != instanceID && item.IsTouching(pair.Value))
                             .ToArray())
                {
#if __ARTIFACT_UNITY_UTILS__QUADTREE_DESTROYAUTODETECT
                    if (!pair.Value)
                    {
                        Delete(pair.Key);
                        continue;
                    }
#endif
                    if (predicate(pair.Value))
                    {
                        results.Add(pair.Value);
                    }
                }
            }

            /// <summary>
            /// Clear current node and its children nodes.
            /// </summary>
            public void Clear()
            {
                if (_subNodes != null)
                {
                    for (var i = 1; i < 5; ++i)
                    {
                        _subNodes[i].Clear();
                    }

                    _subNodes = null;
                }

                _items.Clear();
            }

            #endregion
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Init QuadTree and provide zero-param constructor for installer.
        /// </summary>
        public QuadTree()
        {
            _root = new QuadTreeNode(null, this);
        }

        #endregion

        #region Methods

        #region Init Methods

        /// <summary>
        /// Initialize the quadtree with two anchor points (left-top and right-bottom) of boundary.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint. In Unity world pos.</param>
        /// <param name="rightBottom">The anchor point of right-bottom endpoint. In Unity world pos.</param>
        public virtual void Init(Vector2 leftTop, Vector2 rightBottom)
        {
            _root.Center = (leftTop + rightBottom) / 2;
            _root.Left = leftTop.x;
            _root.Top = leftTop.y;
            _root.Right = rightBottom.x;
            _root.Bottom = rightBottom.y;

            // IMPORTANT: the coordinate system of rect and Unity world is different:
            // Rect                         Unity
            // minx,miny    maxx,miny      minx,maxy    maxx,maxy
            // 
            // minx,maxy    maxx,maxy      minx,miny    maxx,miny
            // And Rect's y-axis is mirror to 0 compare with Unity's.
            _root.NodeRect = new Rect(leftTop.x, -leftTop.y,
                rightBottom.x - leftTop.x, leftTop.y - rightBottom.y);
        }

        /// <summary>
        /// Initialize the quadtree with an anchor point and width &amp; height of boundary.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint. In Unity world pos.</param>
        /// <param name="width">The horizontal length of boundary.</param>
        /// <param name="height">The vertical length of boundary.</param>
        public virtual void Init(Vector2 leftTop, float width, float height)
            => Init(leftTop, new Vector2(leftTop.x + width, leftTop.y - height));

        /// <summary>
        /// Initialize the quadtree with an anchor point and width &amp; height of boundary.
        /// <br/>
        /// Overload method for %int% type params.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint. In Unity world pos.</param>
        /// <param name="width">The horizontal length of boundary.</param>
        /// <param name="height">The vertical length of boundary.</param>
        public virtual void Init(Vector2 leftTop, int width, int height)
            => Init(leftTop, (float)width, (float)height);

        /// <summary>
        /// Initialize the quadtree with two anchor points (left-top and right-bottom)'s Transform of boundary.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint.</param>
        /// <param name="rightBottom">The anchor point of right-bottom endpoint.</param>
        public virtual void Init(Transform leftTop, Transform rightBottom)
            => Init((Vector2)leftTop.position, (Vector2)rightBottom.position);

        /// <summary>
        /// Initialize the quadtree with an anchor point's Transform and width &amp; height of boundary.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint.</param>
        /// <param name="width">The horizontal length of boundary.</param>
        /// <param name="height">The vertical length of boundary.</param>
        public virtual void Init(Transform leftTop, float width, float height)
            => Init((Vector2)leftTop.position, width, height);

        /// <summary>
        /// Initialize the quadtree with an anchor point's Transform and width &amp; height of boundary.
        /// <br/>
        /// Overload method for %int% type params.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint.</param>
        /// <param name="width">The horizontal length of boundary.</param>
        /// <param name="height">The vertical length of boundary.</param>
        public virtual void Init(Transform leftTop, int width, int height)
            => Init(leftTop, (float)width, (float)height);

        #endregion

        #region Param Methods

        /// <summary>
        /// Set QuadTree's default param. Can only run once.
        /// </summary>
        /// <param name="splitThreshold">The new split threshold of tree.</param>
        /// <param name="maxDepth">The new max depth of tree.</param>
        /// <remarks>
        /// Rerun it in runtime is undefined behaviour. QuadTree do not process runtime param switching.
        /// </remarks>
        public virtual void SetParam(int splitThreshold, int maxDepth)
        {
            if (_root.Center == Vector2.zero && _root.Top == 0)
            {
                ArtifactDebug.PackageLog("[QuadTree] Set param before Init, do nothing.", DebugLogLevel.Warning);
                return;
            }

            _root.SplitThreshold = splitThreshold;
            _root.MaxDepth = maxDepth;
        }

        #endregion

        /// <summary>
        /// Add an item to QuadTree.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public virtual void Add(Collider2D item)
        {
            _allItems.TryAdd(item.GetInstanceID(), item);
            _root.Add(item);
        }

        /// <summary>
        /// Remove an item from QuadTree.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public virtual void Remove(Collider2D item)
        {
            var instanceID = item.GetInstanceID();

            if (_allItems.Remove(instanceID))
            {
#if UNITY_EDITOR
                try
                {
#endif
                    foreach (var node in _itemsNodeMap[instanceID].ToArray())
                    {
                        node.Remove(instanceID);
                    }

                    _itemsNodeMap.Remove(instanceID);
#if UNITY_EDITOR
                }
                catch (KeyNotFoundException)
                {
                    ArtifactDebug.PackageLog(
                        "[QuadTree] Could not found item: " + item.name + " when remove;" +
                        "; Try check if object is added correctly or Init is done correctly with right rectangle range.",
                        DebugLogLevel.Fatal);
                    throw;
                }
#endif
            }
        }

        /// <summary>
        /// Update an item's position in QuadTree.
        /// </summary>
        /// <param name="item">The item to update.</param>
        public virtual void Update(Collider2D item)
        {
#if UNITY_EDITOR
            try
            {
#endif
                foreach (var node in _itemsNodeMap[item.GetInstanceID()].ToArray())
                {
                    node.TryDismiss(item);
                }
#if UNITY_EDITOR
            }
            catch (KeyNotFoundException)
            {
                ArtifactDebug.PackageLog(
                    "[QuadTree] Could not found item: " + item.name + " when update;" +
                    "Try check if object is added correctly or Init is done correctly with right rectangle range.",
                    DebugLogLevel.Fatal);
                throw;
            }
#endif

            Add(item);
        }

        /// <summary>
        /// Check if QuadTree is empty.
        /// </summary>
        /// <returns>True if QuadTree is empty; otherwise, false.</returns>
        public virtual bool IsEmpty()
        {
            return _allItems.Count == 0;
        }

        /// <summary>
        /// Check if item is in QuadTree.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if item found in QuadTree; otherwise, false.</returns>
        public virtual bool Contains(Collider2D item)
        {
            return _allItems.ContainsKey(item.GetInstanceID());
        }

        /// <summary>
        /// Get all items that intersected with given item.
        /// </summary>
        /// <param name="item">The item to check intersecting.</param>
        /// <param name="predicate">The judgement function for intersection detection.</param>
        /// <returns>A list of all intersected items. If no item found, it will return empty list.</returns>
        /// <remarks>
        /// Feature: Destroy auto-detect: When item's gameObject is destroyed, this method will automatically remove it.
        /// <br/>
        /// Feature: Not-in-tree item query: Enable intersection detection for not-in-tree items.
        /// <br/>
        /// See Editor/SettingToggler/CompileFlagInterface for more information.
        /// You can write down CompileFlagInterface.CompileFlagInterfaceNavigator and use IDE to goto this file.
        /// </remarks>
        public virtual List<Collider2D> GetIntersected(Collider2D item, Func<Collider2D, bool> predicate = null)
        {
            predicate ??= _ => true;

            var result = new HashSet<Collider2D>();
            var instanceID = item.GetInstanceID();
            var nodes = new HashSet<QuadTreeNode>();
            if (_allItems.ContainsKey(instanceID))
            {
                if (!_itemsNodeMap.TryGetValue(instanceID, out nodes))
                {
                    return new List<Collider2D>();
                }
            }
#if __ARTIFACT_UNITY_UTILS__QUADTREE_NOTINTREEITEMQUERY
            else
            {
                var itemRect = GetItemRect(item);
                CollectIntersectingNodes(itemRect, nodes);
            }
#endif

            foreach (var node in nodes)
            {
                node.GetIntersected(item, predicate, result);
            }

            return result.ToList();
        }

        /// <summary>
        /// Getting all nodes that intersected with given item.
        /// </summary>
        /// <param name="itemRect">The rect of item to check intersecting.</param>
        /// <param name="results">The results of intersection. Will be modified.</param>
        [Conditional("__ARTIFACT_UNITY_UTILS__QUADTREE_NOTINTREEITEMQUERY")]
        private void CollectIntersectingNodes(Rect itemRect, HashSet<QuadTreeNode> results)
            => _root.CollectIntersectingNodes(itemRect, results);

        /// <summary>
        /// Clear QuadTree into empty state. Won't reset boundaries.
        /// </summary>
        /// <remarks>
        /// Fully clear: see <see cref="Reset"/>.
        /// </remarks>
        public virtual void Clear()
        {
            _allItems.Clear();

            foreach (var nodes in _itemsNodeMap.ToArray())
            {
                foreach (var node in nodes.Value)
                {
                    node.Clear();
                }
            }

            _itemsNodeMap.Clear();

            _root.Clear();
        }

        /// <summary>
        /// Reset QuadTree into blank state.
        /// <br/>
        /// You should do <see cref="SetParam"/> after Reset if you want to reuse this QuadTree.
        /// </summary>
        public virtual void Reset()
        {
            Clear();

            _root.Center = Vector2.zero;
            _root.Top = 0;
            _root.Bottom = 0;
            _root.Left = 0;
            _root.Right = 0;
            _root.NodeRect = new Rect(0, 0, 0, 0);

            _root.SplitThreshold = 12;
            _root.MaxDepth = 8;
        }

        #endregion

        #region Operator Overloading

        /// <summary>
        /// Operator overloading of += for simplified QuadTree adding object operation.
        /// </summary>
        /// <param name="this">The QuadTree object want to be operated.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>The input QuadTree object.</returns>
        /// <remarks>
        /// NEVER USE +! Only += is allowed!
        /// </remarks>
        public static QuadTree operator +(QuadTree @this, Collider2D item)
        {
            @this.Add(item);
            return @this;
        }

        /// <summary>
        /// Operator overloading of -= for simplified QuadTree removing object operation.
        /// </summary>
        /// <param name="this">The QuadTree object want to be operated.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns>The input QuadTree object.</returns>
        /// <remarks>
        /// NEVER USE -! Only -= is allowed!
        /// </remarks>
        public static QuadTree operator -(QuadTree @this, Collider2D item)
        {
            @this.Remove(item);
            return @this;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method for getting item's rect from Collider2D entry.
        /// </summary>
        /// <param name="item">The item to getting rect.</param>
        /// <returns>The rect of given item.</returns>
        public static Rect GetItemRect(Collider2D item)
        {
            var itemBounds = item.bounds;
            // IMPORTANT: the coordinate system of rect and Unity world is different:
            // Rect                         Unity
            // minx,miny    maxx,miny      minx,maxy    maxx,maxy
            // 
            // minx,maxy    maxx,maxy      minx,miny    maxx,miny
            // And Rect's y-axis is mirror to 0 compare with Unity's.
            var itemRect = new Rect(itemBounds.min.x, -itemBounds.max.y,
                itemBounds.size.x, itemBounds.size.y);

            return itemRect;
        }

        #endregion
    }

    /// <summary>
    /// Abstract installer for QuadTree that make registry available in service locator.
    /// </summary>
    /// <typeparam name="TQuadTree">The type of quad tree.</typeparam>
    /// <example>
    /// <code>
    /// public class EnemyQuadTree : QuadTree
    /// {
    ///     ...
    /// }
    ///  
    /// public class EnemyQuadTreeInstaller : QuadTreeInstaller&lt;EnemyQuadTree&gt;
    /// {
    ///     // Just left empty.
    ///     // It will enable EnemyQuadTree to ServiceLocator. 
    /// }
    /// </code>
    /// </example>
    public abstract class QuadTreeInstaller<TQuadTree> : GenericInstaller<TQuadTree>
        where TQuadTree : QuadTree, new()
    {
        /// <summary>
        /// Install QuadTree as a service in locator.
        /// <br/>
        /// It would create an instance of QuadTree and then register it to locator.
        /// </summary>
        public override void InstallService()
        {
            ServiceLocator.Register(new TQuadTree());
            ArtifactDebug.PackageLog(
                $"[Data Structure - QuadTree] QuadTree {nameof(TQuadTree)} has been enabled."
#if __ARTIFACT_UNITY_UTILS__QUADTREE_DESTROYAUTODETECT
                + " With destroy auto-detect feature."
#endif
#if __ARTIFACT_UNITY_UTILS__QUADTREE_NOTINTREEITEMQUERY
                + " With not-in-tree item query feature."
#endif
                , DebugLogLevel.WorksWell);
        }
    }

    [Preserve]
    public class DefaultQuadTreeInstaller : QuadTreeInstaller<QuadTree>
    {
    }
}

#endif