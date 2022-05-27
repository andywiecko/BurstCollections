using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.BurstCollections
{
    public struct NativePointQuadtree : INativeDisposable
    {
        private static readonly int Invalid = -1;

        public NativeList<Node> Nodes;
        public NativeList<int> ElementsIds;
        public readonly AABB Bounds;
        public readonly int NodeCapacity;

        private int elementsCount;

        public NativePointQuadtree(AABB bounds, int treeCapacity, int nodeCapacity, Allocator allocator)
        {
            Bounds = bounds;
            NodeCapacity = nodeCapacity;
            Nodes = new NativeList<Node>(treeCapacity, allocator);
            ElementsIds = new NativeList<int>(treeCapacity * nodeCapacity, allocator);
            elementsCount = 0;
        }

        public struct Node
        {
            public readonly int ParentId;
            public readonly AABB AABB;
            public int Count;

            // TODO: add foreach iterator
            public readonly int this[int childId] => childId switch
            {
                0 => nw,
                1 => ne,
                2 => sw,
                3 => se,
                _ => throw new ArgumentOutOfRangeException()
            };

            private readonly int nw;

            public static Node Root(AABB aabb) => new(Invalid, aabb, Invalid, count: 0);
            public static Node Leaf(int parentId, AABB aabb) => new(parentId, aabb, Invalid, count: 0);
            private Node(int parentId, AABB aabb, int nwId, int count)
            {
                ParentId = parentId;
                AABB = aabb;
                Count = count;
                nw = nwId;
            }

            private readonly int ne => nw + 1;
            private readonly int sw => nw + 2;
            private readonly int se => nw + 3;

            public bool IsLeaf => nw == Invalid;
            public bool Contains(float2 p) => AABB.Contains(p);

            public (Node nw, Node ne, Node sw, Node se) Subdivide(int nodeId, int nodesCount)
            {
                this = new(ParentId, AABB, nodesCount, Count);

                var size = AABB.Size / 2;
                return (
                    nw: Leaf(nodeId, new(AABB.Min + math.float2(0, size.y), AABB.Max - math.float2(size.x, 0))),
                    ne: Leaf(nodeId, new(AABB.Min + size, AABB.Max)),
                    sw: Leaf(nodeId, new(AABB.Min, AABB.Max - size)),
                    se: Leaf(nodeId, new(AABB.Min + math.float2(size.x, 0), AABB.Max - math.float2(0, size.y)))
                );
            }
        }

        public ReadOnly AsReadOnly() => new(this);

        public JobHandle Build(NativeArray<float2> points, JobHandle dependencies)
        {
            return new BuildTreeJob(this, points).Schedule(dependencies);
        }

        unsafe public void Build(NativeArray<float2> points)
        {
            Build(new ReadOnlySpan<float2>(points.GetUnsafeReadOnlyPtr(), points.Length));
        }

        public void Build(ReadOnlySpan<float2> points)
        {
            Nodes.Clear();
            ElementsIds.Clear();

            AddRoot();
            foreach (var p in points)
            {
                Add(p, points);
            }
        }

        private void AddRoot()
        {
            Nodes.Add(Node.Root(Bounds));
            ElementsIds.Length = NodeCapacity;
            for (int i = 0; i < NodeCapacity; i++)
            {
                ElementsIds[i] = Invalid;
            }

            elementsCount = 0;
        }

        private bool Add(float2 p, ReadOnlySpan<float2> points, int nodeId = 0)
        {
            var node = Nodes[nodeId];
            if (!node.Contains(p))
            {
                return false;
            }

            if (node.Count < NodeCapacity)
            {
                PushElement(nodeId);
                return true;
            }
            else
            {
                if (node.IsLeaf)
                {
                    node = SubdivideNode(nodeId, points);
                }

                if (Add(p, points, node[0]) || Add(p, points, node[1]) || Add(p, points, node[2]) || Add(p, points, node[3]))
                {
                    return true;
                }
            }

            // TODO: add checks
            return false;
        }

        private Node SubdivideNode(int nodeId, ReadOnlySpan<float2> points)
        {
            ref var node = ref Nodes.ElementAt(nodeId);
            var (nw, ne, sw, se) = node.Subdivide(nodeId, Nodes.Length);

            Nodes.Add(nw);
            Nodes.Add(ne);
            Nodes.Add(sw);
            Nodes.Add(se);

            var i = ElementsIds.Length;
            ElementsIds.Length = i + 4 * NodeCapacity;
            for (; i < ElementsIds.Length; i++)
            {
                ElementsIds[i] = Invalid;
            }

            for (i = 0; i < NodeCapacity; i++)
            {
                var j = i + nodeId * NodeCapacity;
                var elId = ElementsIds[j];
                ElementsIds[j] = Invalid;
                var p = points[elId];
                if (nw.Contains(p))
                {
                    MoveElement(node[0], elId);
                }
                else if (ne.Contains(p))
                {
                    MoveElement(node[1], elId);
                }
                else if (sw.Contains(p))
                {
                    MoveElement(node[2], elId);
                }
                else if (se.Contains(p))
                {
                    MoveElement(node[3], elId);
                }
                else
                {
                    // TODO throw
                }
            }

            return node;
        }

        public void Remove(int elementId)
        {

        }

        public JobHandle Dispose(JobHandle dependencies)
        {
            dependencies = Nodes.Dispose(dependencies);
            dependencies = ElementsIds.Dispose(dependencies);
            return dependencies;
        }

        public void Dispose()
        {
            Nodes.Dispose();
            ElementsIds.Dispose();
        }

        private void PushElement(int nodeId)
        {
            //MoveElement(nodeId, elementsCount);
            //elementsCount++;

            var node = Nodes[nodeId];
            ElementsIds[nodeId * NodeCapacity + node.Count] = elementsCount;
            elementsCount++;
            node.Count++;
            Nodes[nodeId] = node;

        }

        private void MoveElement(int nodeId, int elementId)
        {
            var node = Nodes[nodeId];
            ElementsIds[nodeId * NodeCapacity + node.Count] = elementId;
            Nodes.ElementAt(nodeId).Count++;
        }

        public struct ReadOnly
        {
            // Use NativeArray.ReadOnly instead when Unity fix the bug.
            [ReadOnly]
            public NativeArray<Node> Nodes;
            [ReadOnly]
            public NativeArray<int> ElementsIds;

            public readonly int NodeCapacity;

            internal ReadOnly(NativePointQuadtree tree)
            {
                Nodes = tree.Nodes.AsDeferredJobArray();
                ElementsIds = tree.ElementsIds.AsDeferredJobArray();
                NodeCapacity = tree.NodeCapacity;
            }
            public int GetElementId(int i, int nodeId) => ElementsIds[nodeId * NodeCapacity + i];
        }

        [BurstCompile]
        private struct BuildTreeJob : IJob
        {
            private NativePointQuadtree tree;
            private NativeArray<float2> points;
            public BuildTreeJob(NativePointQuadtree tree, NativeArray<float2> points) => (this.tree, this.points) = (tree, points);
            public void Execute() => tree.Build(points);
        }
    }
}