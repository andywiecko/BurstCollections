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
        public static void GetIntersectionsWithAABB<T>(this NativeBoundingVolumeTree<T> tree, T aabb, NativeList<int> result)
            where T : unmanaged, IBoundingVolume<T> => GetIntersectionsWithAABB(tree.AsReadOnly(), aabb, result);

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with given <paramref name="aabb"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> leaves which intersects with <paramref name="aabb"/>.</param>
        /// <remarks>
        /// It allocates temporary <see cref="NativeQueue{T}"/> internally.
        /// </remarks>
        public static void GetIntersectionsWithAABB<T>(this NativeBoundingVolumeTree<T>.ReadOnly tree, T aabb, NativeList<int> result)
            where T : unmanaged, IBoundingVolume<T>
        {
            using var queue = new NativeQueue<int>(Allocator.Temp);
            GetIntersectionsWithAABB(tree, aabb, result, queue);
        }

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with given <paramref name="aabb"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> leaves which intersects with <paramref name="aabb"/>.</param>
        public static void GetIntersectionsWithAABB<T>(this NativeBoundingVolumeTree<T> tree, T aabb, NativeList<int> result, NativeQueue<int> queue)
            where T : unmanaged, IBoundingVolume<T> => GetIntersectionsWithAABB(tree.AsReadOnly(), aabb, result, queue);

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with given <paramref name="aabb"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> leaves which intersects with <paramref name="aabb"/>.</param>
        public static void GetIntersectionsWithAABB<T>(this NativeBoundingVolumeTree<T>.ReadOnly tree, T aabb, NativeList<int> result, NativeQueue<int> queue)
            where T : unmanaged, IBoundingVolume<T>
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

        #region Tree vs. Tree intersections
        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with <paramref name="otherTree"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> and <paramref name="otherTree"/> leaves which intersects.</param>
        /// <remarks>
        /// It allocates temporary <see cref="NativeQueue{T}"/> internally.
        /// </remarks>
        public static void GetIntersectionsWithTree<T>(this NativeBoundingVolumeTree<T> tree, NativeBoundingVolumeTree<T> otherTree, NativeList<int2> result)
            where T : unmanaged, IBoundingVolume<T> => GetIntersectionsWithTree(tree.AsReadOnly(), otherTree.AsReadOnly(), result);

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with <paramref name="otherTree"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> and <paramref name="otherTree"/> leaves which intersects.</param>
        /// <remarks>
        /// It allocates temporary <see cref="NativeQueue{T}"/> internally.
        /// </remarks>
        public static void GetIntersectionsWithTree<T>(this NativeBoundingVolumeTree<T>.ReadOnly tree, NativeBoundingVolumeTree<T>.ReadOnly otherTree, NativeList<int2> result)
            where T : unmanaged, IBoundingVolume<T>
        {
            using var queue = new NativeQueue<int2>(Allocator.Temp);
            GetIntersectionsWithTree(tree, otherTree, result, queue);
        }

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with <paramref name="otherTree"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> and <paramref name="otherTree"/> leaves which intersects.</param>
        public static void GetIntersectionsWithTree<T>(this NativeBoundingVolumeTree<T> tree, NativeBoundingVolumeTree<T> otherTree, NativeList<int2> result, NativeQueue<int2> queue)
            where T : unmanaged, IBoundingVolume<T> => GetIntersectionsWithTree(tree.AsReadOnly(), otherTree.AsReadOnly(), result, queue);

        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with <paramref name="otherTree"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> and <paramref name="otherTree"/> leaves which intersects.</param>
        public static void GetIntersectionsWithTree<T>(this NativeBoundingVolumeTree<T>.ReadOnly tree, NativeBoundingVolumeTree<T>.ReadOnly otherTree, NativeList<int2> result, NativeQueue<int2> queue)
            where T : unmanaged, IBoundingVolume<T>
        {
            if (tree.IsEmpty || otherTree.IsEmpty)
            {
                return;
            }

            queue.Clear();
            queue.Enqueue(new(tree.RootId.Value, otherTree.RootId.Value));

            while (queue.TryDequeue(out int2 ids))
            {
                var idA = ids.x;
                var idB = ids.y;
                var a = tree.Volumes[idA];
                var b = otherTree.Volumes[idB];

                if (a.Intersects(b))
                {
                    var na = tree.Nodes[idA];
                    var nb = otherTree.Nodes[idB];
                    switch (na.IsLeaf, nb.IsLeaf)
                    {
                        case (true, true):
                            result.Add(ids);
                            continue;

                        case (false, false):
                            queue.Enqueue(new(na.LeftChildId, nb.LeftChildId));
                            queue.Enqueue(new(na.LeftChildId, nb.RightChildId));
                            queue.Enqueue(new(na.RightChildId, nb.LeftChildId));
                            queue.Enqueue(new(na.RightChildId, nb.RightChildId));
                            continue;

                        case (true, false):
                            queue.Enqueue(new(idA, nb.LeftChildId));
                            queue.Enqueue(new(idA, nb.RightChildId));
                            continue;

                        case (false, true):
                            queue.Enqueue(new(na.LeftChildId, idB));
                            queue.Enqueue(new(na.RightChildId, idB));
                            continue;
                    }
                }
            }
        }
        #endregion
    }
}