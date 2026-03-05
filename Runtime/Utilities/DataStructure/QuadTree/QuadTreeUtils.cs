using UnityEngine;

namespace Utilities.DataStructure.QuadTree
{
    /// <summary>
    /// Utils class for QuadTree. Especially for TItem = Collider2D situation.
    /// </summary>
    public static class QuadTreeUtils
    {
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
    }
}