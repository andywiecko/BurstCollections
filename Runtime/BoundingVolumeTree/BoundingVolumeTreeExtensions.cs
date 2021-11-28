using Unity.Collections;

namespace andywiecko.BurstCollections
{
    public static class BoundingVolumeTreeExtensions
    {
        /// <summary>
        /// Collects all intersections of the <paramref name="tree"/> with given <paramref name="aabb"/>
        /// and stores them in the <paramref name="result"/>.
        /// </summary>
        /// <param name="result">Indicies of the <paramref name="tree"/> leaves which intersects with <paramref name="aabb"/>.</param>
        public static void GetIntersectionsWithAABB(this ref BoundingVolumeTree<AABB> tree, AABB aabb, NativeList<int> result)
        {
            var bfs = tree.BreadthFirstSearch;
            foreach(var (id, nodeAABB) in bfs)
            {
                if(aabb.Intersects(nodeAABB))
                {
                    if(bfs.IsLeaf(id))
                    {
                        result.Add(id);
                    }
                    bfs.Traverse(id);
                }
            }
        }
    }
}