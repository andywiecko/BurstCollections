using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.BurstCollections
{
    public static class NativePointQuadtreeExtensions
    {
        public static void RangeSearch(this NativePointQuadtree.ReadOnly tree, AABB range, NativeArray<float2>.ReadOnly positions, NativeList<int> result)
        {
            using var queue = new NativeQueue<int>(Allocator.Temp);
            queue.Enqueue(0);
            while (queue.TryDequeue(out var nodeId))
            {
                var node = tree.Nodes[nodeId];
                if (!node.AABB.Intersects(range))
                {
                    continue;
                }

                if (node.IsLeaf)
                {
                    for (int i = 0; i < node.Count; i++)
                    {
                        var eId = tree.GetElementId(i, nodeId);
                        var p = positions[eId];
                        if (range.Contains(p))
                        {
                            result.Add(eId);
                        }
                    }
                }
                else
                {
                    queue.Enqueue(node[0]);
                    queue.Enqueue(node[1]);
                    queue.Enqueue(node[2]);
                    queue.Enqueue(node[3]);
                }
            }
        }
    }
}