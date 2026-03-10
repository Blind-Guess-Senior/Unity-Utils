#if __ARTIFACT_UNITY_UTILS__QUADTREE_ENABLED
using UnityEngine;

// ReSharper disable CheckNamespace

namespace Artifact.UnityUtils.Utilities.DataStructure.QuadTree
{
    /// <summary>
    /// Utils class for QuadTree. Especially for TItem = Collider2D situation.
    /// </summary>
    public static class QuadTreeUtils
    {
        #region Rect Utils

        /// <summary>
        /// Get rect by Unity world coordinate.
        /// </summary>
        /// <param name="minX">The left x pos.</param>
        /// <param name="maxX">The right x pos.</param>
        /// <param name="minY">The bottom y pos.</param>
        /// <param name="maxY">The top y pos.</param>
        /// <returns>The rect of given range.</returns>
        public static Rect GetRect(float minX, float minY, float maxX, float maxY)
            => new Rect(minX, -maxY, maxX - minX, maxY - minY);

        public static Rect GetRect(int minX, int minY, int maxX, int maxY)
            => GetRect((float)minX, (float)minY, (float)maxX, (float)maxY);

        public static Rect GetRect(double minX, double minY, double maxX, double maxY)
            => GetRect((float)minX, (float)minY, (float)maxX, (float)maxY);

        /// <summary>
        /// Get rect by Unity world coordinate.
        /// </summary>
        /// <param name="leftBottom">The Vector2 position in left-bottom.</param>
        /// <param name="rightTop">The Vector2 position in right-top.</param>
        /// <returns>The rect of given range.</returns>
        public static Rect GetRect(Vector2 leftBottom, Vector2 rightTop)
            => GetRect(leftBottom.x, leftBottom.y, rightTop.x, rightTop.y);

        public static Rect GetRect(Vector2 leftBottom, float maxX, float maxY)
            => GetRect(leftBottom.x, leftBottom.y, maxX, maxY);

        public static Rect GetRect(Vector2 leftBottom, int maxX, int maxY)
            => GetRect(leftBottom.x, leftBottom.y, maxX, maxY);

        public static Rect GetRect(Vector2 leftBottom, double maxX, double maxY)
            => GetRect(leftBottom.x, leftBottom.y, maxX, maxY);

        #endregion

        #region Collider2D Utils

        /// <summary>
        /// Helper method for getting Collider2D item's rect.
        /// </summary>
        /// <param name="item">The Collider2D item to getting rect.</param>
        /// <returns>The rect of given Collider2D item.</returns>
        public static Rect GetCollider2DRect(Collider2D item)
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

        /// <summary>
        /// Helper method for getting collision result of two Collider2D items.
        /// </summary>
        /// <param name="coa">The Collider2D item to check.</param>
        /// <param name="cob">The Collider2D item to check.</param>
        /// <returns>True if they collide; otherwise, false.</returns>
        public static bool CollisionResultOf(Collider2D coa, Collider2D cob)
            => coa.IsTouching(cob);

        #endregion

        #region GameObject Utils

        /// <summary>
        /// Helper method for getting GameObject item's Collider2D component's rect.
        /// </summary>
        /// <param name="item">The GameObject item to getting rect.</param>
        /// <returns>The rect of given GameObject item.</returns>
        public static Rect GetGameObjectRect(GameObject item)
        {
            var co = item.GetComponent<Collider2D>();

            var itemBounds = co.bounds;
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

        /// <summary>
        /// Helper method for getting collision result of two GameObject items by checking their Collider2D components.
        /// </summary>
        /// <param name="coa">The GameObject item to check.</param>
        /// <param name="cob">The GameObject item to check.</param>
        /// <returns>True if they collide; otherwise, false.</returns>
        public static bool CollisionResultOf(GameObject coa, GameObject cob)
            => coa.GetComponent<Collider2D>().IsTouching(cob.GetComponent<Collider2D>());

        #endregion
    }
}
#endif