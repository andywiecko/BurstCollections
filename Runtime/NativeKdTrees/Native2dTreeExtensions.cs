using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.BurstCollections
{
    public static class Native2dTreeExtensions
    {
        private static void Traverse(Native2dTree.ReadOnly tree, int nodeId, AABB range, NativeArray<float2>.ReadOnly positions, NativeList<int> result, int dim)
        {
            var p = positions[nodeId];
            var n = tree.Nodes[nodeId];
            if (range.Contains(p))
            {
                result.Add(nodeId);
            }

            var leftId = n.LeftChildId;
            if (leftId != -1 && range.Min[dim] < p[dim])
            {
                Traverse(tree, leftId, range, positions, result, (dim + 1) % 2);
            }

            var rightId = n.RightChildId;
            if (rightId != -1 && range.Max[dim] > p[dim])
            {
                Traverse(tree, rightId, range, positions, result, (dim + 1) % 2);
            }
        }

        public static void RangeSearch(this Native2dTree tree, AABB range, NativeArray<float2> positions, NativeList<int> result) =>
            RangeSearch(tree.AsReadOnly(), range, positions.AsReadOnly(), result);

        public static void RangeSearch(this Native2dTree.ReadOnly tree, AABB range, NativeArray<float2>.ReadOnly positions, NativeList<int> result)
        {
            var nodeId = tree.RootId.Value;
            if (nodeId == -1)
            {
                return;
            }

            Traverse(tree, nodeId, range, positions, result, dim: 0);
        }

        private static (int nodeId, int dim) FindDeepestNodeInternal(this Native2dTree.ReadOnly tree, int nodeId, float2 point, NativeArray<float2>.ReadOnly positions, int dim)
        {
            var n = tree.Nodes[nodeId];
            if (n.IsLeaf)
            {
                return (nodeId, dim);
            }

            var p = positions[nodeId];
            var newNodeId = point[dim] < p[dim] ? n.LeftChildId : n.RightChildId;

            if (newNodeId == -1)
            {
                return (nodeId, dim);
            }

            return FindDeepestNodeInternal(tree, newNodeId, point, positions, (dim + 1) % 2);
        }

        public static (int nodeId, int dim) FindDeepestNode(this Native2dTree tree, float2 point, NativeArray<float2> positions) =>
            FindDeepestNode(tree.AsReadOnly(), point, positions.AsReadOnly());
        public static (int nodeId, int dim) FindDeepestNode(this Native2dTree.ReadOnly tree, float2 point, NativeArray<float2>.ReadOnly positions)
        {
            return FindDeepestNodeInternal(tree, tree.RootId.Value, point, positions, dim: 0);
        }
    }
}