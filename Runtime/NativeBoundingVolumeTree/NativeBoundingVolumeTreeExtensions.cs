using System;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.BurstCollections
{
    public static class NativeBoundingVolumeTreeExtensions
    {
        [Obsolete("Use " + nameof(NativeBoundingVolumeTree<AABB>) + " instead!")]
        public static void GetIntersectionsWithAABB(this ref BoundingVolumeTree<AABB> tree, AABB aabb, NativeList<int> result) => tree.tree.GetIntersectionsWithAABB(aabb, result);

        #region Tree vs. AABB intersections

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with given <paramref name="aabb"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> leaves which intersects with <paramref name="aabb"/>.</param>
        /// <remarks>
        /// It allocates temporary <see cref="NativeQueue{T}"/> internally.
        /// </remarks>
        public static void GetIntersectionsWithAABB(this NativeBoundingVolumeTree<AABB> tree, AABB aabb, NativeList<int> result)
        {
            using var queue = new NativeQueue<int>(Allocator.Temp);
            GetIntersectionsWithAABB(tree, aabb, result, queue);
        }

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with given <paramref name="aabb"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> leaves which intersects with <paramref name="aabb"/>.</param>
        /// <remarks>
        /// It allocates temporary <see cref="NativeQueue{T}"/> internally.
        /// </remarks>
        public static void GetIntersectionsWithAABB(this NativeBoundingVolumeTree<AABB>.ReadOnly tree, AABB aabb, NativeList<int> result)
        {
            using var queue = new NativeQueue<int>(Allocator.Temp);
            GetIntersectionsWithAABB(tree, aabb, result, queue);
        }

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with given <paramref name="aabb"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> leaves which intersects with <paramref name="aabb"/>.</param>
        public static void GetIntersectionsWithAABB(this NativeBoundingVolumeTree<AABB> tree, AABB aabb, NativeList<int> result, NativeQueue<int> queue) =>
            GetIntersectionsWithAABB(tree.AsReadOnly(), aabb, result, queue);

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with given <paramref name="aabb"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> leaves which intersects with <paramref name="aabb"/>.</param>
        public static void GetIntersectionsWithAABB(this NativeBoundingVolumeTree<AABB>.ReadOnly tree, AABB aabb, NativeList<int> result, NativeQueue<int> queue)
        {
            var bfs = tree.BreadthFirstSearch(queue);
            foreach (var (id, nodeAABB) in bfs)
            {
                if (aabb.Intersects(nodeAABB))
                {
                    if (bfs.IsLeaf(id))
                    {
                        result.Add(id);
                    }
                    bfs.Traverse(id);
                }
            }
        }
        #endregion
    }
}