using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.BurstCollections
{
    public struct NativePointQuadtree : INativeDisposable
    {
        private const int Invalid = -1;

        public int RootId { get; private set; }
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
            RootId = Invalid;
        }

        #region Node
        public struct Node
        {
            public readonly int ParentId;
            public readonly AABB AABB;
            public int Count;

            public readonly int this[int childId] => childId switch
            {
                0 => nw,
                1 => ne,
                2 => sw,
                3 => se,
                _ => throw new ArgumentOutOfRangeException()
            };

            private readonly int nw;
            private readonly int ne => nw + 1;
            private readonly int sw => nw + 2;
            private readonly int se => nw + 3;

            public bool IsLeaf => nw == Invalid;

            public static Node Root(AABB aabb) => new(Invalid, aabb, Invalid, count: 0);
            public static Node Leaf(int parentId, AABB aabb) => new(parentId, aabb, Invalid, count: 0);
            public Node(int parentId, AABB aabb, int childId, int count)
            {
                ParentId = parentId;
                AABB = aabb;
                Count = count;
                nw = childId;
            }

            public bool Contains(float2 p) => AABB.Contains(p);

            public (Node nw, Node ne, Node sw, Node se) Subdivide(int nodeId, int nodesCount)
            {
                this = new(ParentId, AABB, nodesCount, Count);

                var size = 0.5f * AABB.Size;
                return (
                    nw: Leaf(nodeId, new(AABB.Min + math.float2(0, size.y), AABB.Max - math.float2(size.x, 0))),
                    ne: Leaf(nodeId, new(AABB.Min + size, AABB.Max)),
                    sw: Leaf(nodeId, new(AABB.Min, AABB.Max - size)),
                    se: Leaf(nodeId, new(AABB.Min + math.float2(size.x, 0), AABB.Max - math.float2(0, size.y)))
                );
            }
            public override string ToString() => $"{{Count: {Count}, ParentId: {ParentId}, AABB: {AABB}, childId: {nw}}}";
        }
        #endregion

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

        public ReadOnly AsDeferredReadOnly() => new(this, deferred: true);
        public ReadOnly AsReadOnly() => new(this, deferred: false);

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
            CheckForDuplicates(points);

            Nodes.Clear();
            ElementsIds.Clear();
            AddRoot();
            foreach (var p in points)
            {
                CheckPointInBounds(p, Bounds);
                var succeed = TryAdd(p, points, RootId);
                CheckIfPointIsInserted(succeed, p);
            }
        }

        private void AddRoot()
        {
            RootId = 0;
            Nodes.Add(Node.Root(Bounds));

            ElementsIds.Length = NodeCapacity;
            for (int i = 0; i < NodeCapacity; i++)
            {
                ElementsIds[i] = Invalid;
            }

            elementsCount = 0;
        }

        private bool TryAdd(float2 p, ReadOnlySpan<float2> points, int nodeId)
        {
            var node = Nodes[nodeId];
            if (!node.Contains(p))
            {
                return false;
            }

            if (node.Count < NodeCapacity && node.IsLeaf)
            {
                PushElement(nodeId);
                return true;
            }

            if (node.IsLeaf)
            {
                node = SubdivideNode(nodeId, points);
            }

            if (TryAdd(p, points, nodeId: node[0]) ||
                TryAdd(p, points, nodeId: node[1]) ||
                TryAdd(p, points, nodeId: node[2]) ||
                TryAdd(p, points, nodeId: node[3]))
            {
                return true;
            }

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
                    SubdivideNodeFailThrowException();
                }
            }

            node.Count = 0;
            return node;
        }

        private void PushElement(int nodeId)
        {
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

        private readonly struct Comparer : IComparer<float2>
        {
            public int Compare(float2 x, float2 y) => x.x != y.x ? x.x.CompareTo(y.x) : x.y.CompareTo(y.y);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckIfPointIsInserted(bool succeed, float2 point)
        {
            if (!succeed)
            {
                throw new Exception($"Insertion of point {point} fails!");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckForDuplicates(ReadOnlySpan<float2> points)
        {
            using var copy = new NativeList<float2>(points.Length, Allocator.Temp);
            foreach (var p in points)
            {
                copy.Add(p);
            }
            copy.Sort(new Comparer());

            for (int i = 0; i < copy.Length - 1; i++)
            {
                if (math.all(copy[i] == copy[i + 1]))
                {
                    throw new ArgumentException("Provided points data contains duplicated entries, this is not supported!");
                }
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void SubdivideNodeFailThrowException()
        {
            throw new Exception("Tree node subdividion fails.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckPointInBounds(float2 p, AABB bounds)
        {
            if (!bounds.Contains(p))
            {
                throw new ArgumentException($"Position {p} is out of bounds {bounds}!");
            }
        }

        #region ReadOnly
        public struct ReadOnly
        {
            // Use NativeArray.ReadOnly instead when Unity fix the bug.
            [ReadOnly]
            public NativeArray<Node> Nodes;
            [ReadOnly]
            public NativeArray<int> ElementsIds;

            public readonly int RootId { get; }
            public readonly int NodeCapacity;

            internal ReadOnly(NativePointQuadtree tree, bool deferred)
            {
                Nodes = deferred ? tree.Nodes.AsDeferredJobArray() : tree.Nodes.AsArray();
                ElementsIds = deferred ? tree.ElementsIds.AsDeferredJobArray() : tree.ElementsIds.AsArray();
                NodeCapacity = tree.NodeCapacity;
                RootId = tree.RootId;
            }
            public int GetElementId(int i, int nodeId) => ElementsIds[nodeId * NodeCapacity + i];
        }
        #endregion

        #region Jobs
        [BurstCompile]
        private struct BuildTreeJob : IJob
        {
            private NativePointQuadtree tree;
            private NativeArray<float2> points;
            public BuildTreeJob(NativePointQuadtree tree, NativeArray<float2> points) => (this.tree, this.points) = (tree, points);
            public void Execute() => tree.Build(points);
        }
        #endregion
    }
}