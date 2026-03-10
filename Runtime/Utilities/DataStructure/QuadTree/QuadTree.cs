#if __ARTIFACT_UNITY_UTILS__QUADTREE_ENABLED
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Artifact.UnityUtils.Utilities.DebugUtils;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace Artifact.UnityUtils.Utilities.DataStructure.QuadTree
{
    /// <summary>
    /// QuadTree data structure implementation.
    /// </summary>
    /// <remarks>
    /// Toggle in editor's "Artifact Unity Utils/Data Structure/QuadTree/Features" to enable/disable advanced features.
    /// </remarks>
    /// <example>
    /// <code>
    /// var leftTop = new Vector2(-5, 5);
    /// var rightBottom = new Vector2(5, -5);
    /// var quadTree = new QuadTree&lt;Collider2D&gt;(
    ///            leftTop, rightBottom,
    ///            QuadTreeUtils.GetCollider2DRect,
    ///            QuadTreeUtils.CollisionResultOf);
    /// </code>
    /// </example>
    public class QuadTree<TItem>
    {
        #region Fields

        /// <summary>
        /// Function that receive a TItem typed object and return Rect object which represent the item's AABB.
        /// </summary>
        protected readonly Func<TItem, Rect> _rectFunc;

        /// <summary>
        /// Function that receive two TItem typed objects and return whether these two are collided.
        /// </summary>
        protected readonly Func<TItem, TItem, bool> _touchingFunc;

        /// <summary>
        /// Root node of this QuadTree.
        /// </summary>
        private readonly QuadTreeNode _root;

        /// <summary>
        /// All items stored in whole quad tree.
        /// </summary>
        protected readonly Dictionary<int, TItem> _allItems = new();

        /// <summary>
        /// Store all nodes to which each item belongs.
        /// Used to improve the performance of remove.
        /// </summary>
        protected readonly Dictionary<int, HashSet<QuadTreeNode>> _itemsNodeMap = new();

        #endregion

        #region Definitions

        /// <summary>
        /// Nodes in QuadTree.
        /// </summary>
        protected class QuadTreeNode
        {
            #region Fields & Properties

            /// <summary>
            /// Center position of this node.
            /// </summary>
            public Vector2 Center;

            /// <summary>
            /// Boundaries in 4 directions of this node.
            /// Which is stored in Rect coordinate system.
            /// <br/>
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
            private readonly QuadTree<TItem> _tree;

            /// <summary>
            /// Parent node of current node.
            /// </summary>
            private readonly QuadTreeNode _parent;

            /// <summary>
            /// Four children nodes of current node.
            /// null; Right-Up; Right-Down; Left-Down; Left-Up; respectively.
            /// Same as cartesian coordinate system.
            /// </summary>
            private QuadTreeNode[] _subNodes;

            /// <summary>
            /// Store all object in this node. Key is InstanceID of object.
            /// </summary>
            private Dictionary<int, TItem> _items = new();

            /// <summary>
            /// Function that receive a TItem typed object and return Rect object which represent the item's AABB.
            /// Inherit from _tree.
            /// </summary>
            private readonly Func<TItem, Rect> _rectFunc;

            /// <summary>
            /// Function that receive two TItem typed objects and return whether these two are collided.
            /// Inherit from _tre.
            /// </summary>
            private readonly Func<TItem, TItem, bool> _touchingFunc;

            /// <summary>
            /// Max object counts of one node.
            /// If exceed threshold, split will occur.
            /// </summary>
            public int SplitThreshold;

            /// <summary>
            /// Max depth of QuadTree.
            /// If one node's level equals _maxDepth, split won't occur.
            /// </summary>
            public int MaxDepth;

            #endregion

            #region Constructors

            /// <summary>
            /// Create root node.
            /// </summary>
            public QuadTreeNode(QuadTreeNode parent, QuadTree<TItem> tree)
            {
                _parent = parent;
                _tree = tree;

                Level = 0;

                _rectFunc = tree._rectFunc;
                _touchingFunc = tree._touchingFunc;
            }

            /// <summary>
            /// Create node with given parent.
            /// Inherit split threshold, max depth from parent.
            /// </summary>
            /// <param name="parent">The parent node of newly created node.</param>
            public QuadTreeNode(QuadTreeNode parent)
            {
                _parent = parent;
                _tree = parent._tree;

                Level = parent.Level + 1;
                SplitThreshold = parent.SplitThreshold;
                MaxDepth = parent.MaxDepth;

                _rectFunc = parent._rectFunc;
                _touchingFunc = parent._touchingFunc;
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

                var xTolerance = Math.Abs((left + right) / 10);
                var yTolerance = Math.Abs((top + bottom) / 10);
                NodeRect = new Rect(left - xTolerance, top - yTolerance,
                    right - left + xTolerance * 2, bottom - top + yTolerance * 2);
            }

            /// <summary>
            /// Try adding an item to current node.
            /// If exceed threshold, split.
            /// </summary>
            /// <param name="item">The item to add.</param>
            private void Add(KeyValuePair<int, TItem> item)
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
            public void Add(TItem item)
                => Add(new KeyValuePair<int, TItem>(item.GetHashCode(), item));

            /// <summary>
            /// Try removing an item from current node.
            /// </summary>
            /// <param name="itemID">The item's id to remove.</param>
            public void Remove(int itemID)
            {
                if (_items.Remove(itemID))
                {
#if UNITY_EDITOR
                    try
                    {
#endif
                        _tree._itemsNodeMap[itemID].Remove(this);
#if UNITY_EDITOR
                    }
                    catch (KeyNotFoundException)
                    {
                        ArtifactDebug.PackageLog(
                            "[QuadTree] Could not found item(with id): " + itemID + " when remove;" +
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
            public void Remove(TItem item)
                => Remove(item.GetHashCode());

            /// <summary>
            /// Try deleting an item from QuadTree.
            /// </summary>
            /// <param name="itemID">The item to delete.</param>
            [Conditional("__ARTIFACT_UNITY_UTILS__QUADTREE_NOTINTREEITEMQUERY")]
            private void Delete(int itemID)
            {
                Remove(itemID);

                if (_tree._itemsNodeMap[itemID].Count == 0)
                {
                    _tree._itemsNodeMap.Remove(itemID);
                }
            }

            /// <summary>
            /// Try to dismiss an item if it is not in range of current node.
            /// </summary>
            /// <param name="item">The item to evaluate.</param>
            public void TryDismiss(TItem item)
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
            private bool Intersects(TItem item)
                => NodeRect.Overlaps(_rectFunc(item));

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

                for (var i = 1; i < 5; ++i)
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
                    // Add items to subnodes.
                    for (var i = 1; i < 5; i++)
                    {
                        _subNodes[i].Add(item);
                    }

                    // Remove self from item's node map.
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
            public void GetIntersected(TItem item, Func<TItem, bool> predicate, HashSet<TItem> results)
            {
                var itemID = item.GetHashCode();
                foreach (var pair in _items
                             .Where(pair => pair.Key != itemID && _touchingFunc(item, pair.Value))
                             .ToArray())
                {
#if __ARTIFACT_UNITY_UTILS__QUADTREE_DESTROYAUTODETECT
                    if (pair.Value == null)
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
        /// Initialize the quadtree with two anchor points (left-top and right-bottom) of boundary and required functions.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint. In Unity world pos.</param>
        /// <param name="rightBottom">The anchor point of right-bottom endpoint. In Unity world pos.</param>
        /// <param name="rectFunc">The function that receive a TItem typed object and return Rect object which represent the item's AABB.
        /// <br/>
        /// See <see cref="QuadTreeUtils.GetCollider2DRect"/>'s comment to get hints of making rect or use GetRect utils.</param>
        /// <param name="touchingFunc">The function that receive two TItem typed objects and return whether these two are collided.</param>
        /// <param name="splitThreshold">The new split threshold of tree.</param>
        /// <param name="maxDepth">The new max depth of tree.</param>
        public QuadTree(Vector2 leftTop, Vector2 rightBottom,
            Func<TItem, Rect> rectFunc,
            Func<TItem, TItem, bool> touchingFunc,
            int splitThreshold = 12,
            int maxDepth = 8)
        {
#if UNITY_EDITOR
            if (leftTop.Equals(rightBottom))
            {
                ArtifactDebug.PackageLog("[QuadTree] Tree without region size.", DebugLogLevel.Warning);
            }

            if (rectFunc == null)
            {
                ArtifactDebug.PackageLog("[QuadTree] rectFunc is null.", DebugLogLevel.Error);
                throw new ArgumentNullException(nameof(rectFunc));
            }

            if (touchingFunc == null)
            {
                ArtifactDebug.PackageLog("[QuadTree] touchingFunc is null.", DebugLogLevel.Error);
                throw new ArgumentNullException(nameof(touchingFunc));
            }
#endif

            _rectFunc = rectFunc;
            _touchingFunc = touchingFunc;

            _root = new QuadTreeNode(null, this)
            {
                // IMPORTANT: the coordinate system of rect and Unity world is different:
                // Rect                         Unity
                // minx,miny    maxx,miny      minx,maxy    maxx,maxy
                // 
                // minx,maxy    maxx,maxy      minx,miny    maxx,miny
                // And Rect's y-axis is mirror to 0 compare with Unity's.
                Left = leftTop.x,
                Top = -leftTop.y,
                Right = rightBottom.x,
                Bottom = -rightBottom.y,
                Center = new Vector2((leftTop.x + rightBottom.x) / 2,
                    (-leftTop.y - rightBottom.y) / 2),
                NodeRect = new Rect(leftTop.x, -leftTop.y,
                    rightBottom.x - leftTop.x, leftTop.y - rightBottom.y),
                SplitThreshold = splitThreshold,
                MaxDepth = maxDepth
            };
        }

        /// <summary>
        /// Initialize the quadtree with an anchor point (left-top) and width &amp; height of boundary and required functions.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint. In Unity world pos.</param>
        /// <param name="width">The horizontal length of boundary.</param>
        /// <param name="height">The vertical length of boundary.</param>
        /// <param name="rectFunc">The function that receive a TItem typed object and return Rect object which represent the item's AABB. </param>
        /// <param name="touchingFunc">Function that receive two TItem typed objects and return whether these two are collided.</param>
        /// <param name="splitThreshold">The new split threshold of tree.</param>
        /// <param name="maxDepth">The new max depth of tree.</param>
        public QuadTree(Vector2 leftTop, float width, float height,
            Func<TItem, Rect> rectFunc,
            Func<TItem, TItem, bool> touchingFunc,
            int splitThreshold = 12,
            int maxDepth = 8)
            : this(leftTop, new Vector2(leftTop.x + width, leftTop.y - height),
                rectFunc, touchingFunc, splitThreshold, maxDepth)
        {
        }

        /// <summary>
        /// Initialize the quadtree with an anchor point (left-top) and width &amp; height of boundary and required functions.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint. In Unity world pos.</param>
        /// <param name="width">The horizontal length of boundary.</param>
        /// <param name="height">The vertical length of boundary.</param>
        /// <param name="rectFunc">The function that receive a TItem typed object and return Rect object which represent the item's AABB. </param>
        /// <param name="touchingFunc">Function that receive two TItem typed objects and return whether these two are collided.</param>
        /// <param name="splitThreshold">The new split threshold of tree.</param>
        /// <param name="maxDepth">The new max depth of tree.</param>
        public QuadTree(Vector2 leftTop, int width, int height,
            Func<TItem, Rect> rectFunc,
            Func<TItem, TItem, bool> touchingFunc,
            int splitThreshold = 12,
            int maxDepth = 8)
            : this(leftTop, (float)width, (float)height,
                rectFunc, touchingFunc, splitThreshold, maxDepth)
        {
        }

        /// <summary>
        /// Initialize the quadtree with two anchor points (left-top and right-bottom)'s Transform of boundary and required functions.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint.</param>
        /// <param name="rightBottom">The anchor point of right-bottom endpoint.</param>
        /// <param name="rectFunc">The function that receive a TItem typed object and return Rect object which represent the item's AABB. </param>
        /// <param name="touchingFunc">Function that receive two TItem typed objects and return whether these two are collided.</param>
        /// <param name="splitThreshold">The new split threshold of tree.</param>
        /// <param name="maxDepth">The new max depth of tree.</param>
        public QuadTree(Transform leftTop, Transform rightBottom,
            Func<TItem, Rect> rectFunc,
            Func<TItem, TItem, bool> touchingFunc,
            int splitThreshold = 12,
            int maxDepth = 8)
            : this((Vector2)leftTop.position, (Vector2)rightBottom.position,
                rectFunc, touchingFunc, splitThreshold, maxDepth)
        {
        }

        /// <summary>
        /// Initialize the quadtree with an anchor point (left-top)'s Transform and width &amp; height of boundary and required functions.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint. In Unity world pos.</param>
        /// <param name="width">The horizontal length of boundary.</param>
        /// <param name="height">The vertical length of boundary.</param>
        /// <param name="rectFunc">The function that receive a TItem typed object and return Rect object which represent the item's AABB. </param>
        /// <param name="touchingFunc">Function that receive two TItem typed objects and return whether these two are collided.</param>
        /// <param name="splitThreshold">The new split threshold of tree.</param>
        /// <param name="maxDepth">The new max depth of tree.</param>
        public QuadTree(Transform leftTop, float width, float height,
            Func<TItem, Rect> rectFunc,
            Func<TItem, TItem, bool> touchingFunc,
            int splitThreshold = 12,
            int maxDepth = 8)
            : this((Vector2)leftTop.position, width, height,
                rectFunc, touchingFunc, splitThreshold, maxDepth)
        {
        }

        /// <summary>
        /// Initialize the quadtree with an anchor point (left-top)'s Transform and width &amp; height of boundary and required functions.
        /// </summary>
        /// <param name="leftTop">The anchor point of left-top endpoint. In Unity world pos.</param>
        /// <param name="width">The horizontal length of boundary.</param>
        /// <param name="height">The vertical length of boundary.</param>
        /// <param name="rectFunc">The function that receive a TItem typed object and return Rect object which represent the item's AABB. </param>
        /// <param name="touchingFunc">Function that receive two TItem typed objects and return whether these two are collided.</param>
        /// <param name="splitThreshold">The new split threshold of tree.</param>
        /// <param name="maxDepth">The new max depth of tree.</param>
        public QuadTree(Transform leftTop, int width, int height,
            Func<TItem, Rect> rectFunc,
            Func<TItem, TItem, bool> touchingFunc,
            int splitThreshold = 12,
            int maxDepth = 8)
            : this((Vector2)leftTop.position, (float)width, (float)height,
                rectFunc, touchingFunc, splitThreshold, maxDepth)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add an item to QuadTree.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public virtual void Add(TItem item)
        {
            _allItems.TryAdd(item.GetHashCode(), item);
            _root.Add(item);
        }

        /// <summary>
        /// Remove an item from QuadTree.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public virtual void Remove(TItem item)
        {
            var itemID = item.GetHashCode();
            if (_allItems.Remove(itemID))
            {
#if UNITY_EDITOR
                try
                {
#endif
                    foreach (var node in _itemsNodeMap[itemID].ToArray())
                    {
                        node.Remove(itemID);
                    }

                    _itemsNodeMap.Remove(itemID);
#if UNITY_EDITOR
                }
                catch (KeyNotFoundException)
                {
                    ArtifactDebug.PackageLog(
                        "[QuadTree] Could not found item: " + item + " when remove;" +
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
        public virtual void Update(TItem item)
        {
#if UNITY_EDITOR
            try
            {
#endif
                foreach (var node in _itemsNodeMap[item.GetHashCode()].ToArray())
                {
                    node.TryDismiss(item);
                }
#if UNITY_EDITOR
            }
            catch (KeyNotFoundException)
            {
                ArtifactDebug.PackageLog(
                    "[QuadTree] Could not found item: " + item + " when update;" +
                    "Try check if object is added correctly or Init is done correctly with right rectangle range.",
                    DebugLogLevel.Fatal);
                throw;
            }
#endif

            Add(item);
        }

        /// <summary>
        /// Get all items that intersected with given item.
        /// </summary>
        /// <param name="item">The item to check intersecting.</param>
        /// <param name="predicate">The judgement function for intersection detection.</param>
        /// <returns>A list of all intersected items. If no item found, it will return empty list.</returns>
        /// <remarks>
        /// Feature: Destroy auto-detect: When item is null, this method will automatically remove it.
        /// If item is part of gameObject, then only when its gameObject is destroyed
        /// <br/>
        /// Feature: Not-in-tree item query: Enable intersection detection for not-in-tree items.
        /// <br/>
        /// See Editor/SettingToggler/CompileFlagInterface for more information.
        /// You can write down CompileFlagInterface.CompileFlagInterfaceNavigator and use IDE to goto this file.
        /// </remarks>
        public virtual List<TItem> GetIntersected(TItem item, Func<TItem, bool> predicate = null)
        {
            predicate ??= _ => true;

            var result = new HashSet<TItem>();

            var itemID = item.GetHashCode();
            var nodes = new HashSet<QuadTreeNode>();
            if (_allItems.ContainsKey(itemID))
            {
                if (!_itemsNodeMap.TryGetValue(itemID, out nodes))
                {
                    return new List<TItem>();
                }
            }
#if __ARTIFACT_UNITY_UTILS__QUADTREE_NOTINTREEITEMQUERY
            else
            {
                var itemRect = _rectFunc(item);
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
        public virtual bool Contains(TItem item)
        {
            return _allItems.ContainsKey(item.GetHashCode());
        }

        /// <summary>
        /// Clear QuadTree into empty state. Won't reset boundaries.
        /// </summary>
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
        /// Getting all nodes that intersected with given item.
        /// </summary>
        /// <param name="itemRect">The rect of item to check intersecting.</param>
        /// <param name="results">The results of intersection. Will be modified.</param>
        [Conditional("__ARTIFACT_UNITY_UTILS__QUADTREE_NOTINTREEITEMQUERY")]
        private void CollectIntersectingNodes(Rect itemRect, HashSet<QuadTreeNode> results)
            => _root.CollectIntersectingNodes(itemRect, results);

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
        public static QuadTree<TItem> operator +(QuadTree<TItem> @this, TItem item)
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
        public static QuadTree<TItem> operator -(QuadTree<TItem> @this, TItem item)
        {
            @this.Remove(item);
            return @this;
        }

        #endregion
    }
}
#endif