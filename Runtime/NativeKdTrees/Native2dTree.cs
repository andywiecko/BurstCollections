using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.BurstCollections
{
    public struct Native2dTree : INativeDisposable
    {
        #region Structs
        private readonly struct Float2XComparer : IComparer<Float2WithId>
        {
            public int Compare(Float2WithId a, Float2WithId b) => a.Value.x.CompareTo(b.Value.x);
        }

        private readonly struct Float2YComparer : IComparer<Float2WithId>
        {
            public int Compare(Float2WithId a, Float2WithId b) => a.Value.y.CompareTo(b.Value.y);
        }

        private readonly struct NodeId
        {
            public static readonly NodeId Invalid = (-1, -1);
            public bool IsValid => Med != Invalid;
            public readonly int Med;
            public readonly int Value;
            public NodeId(int med, int nodeId) => (Med, Value) = (med, nodeId);
            public static implicit operator NodeId((int med, int nodeId) t) => new(t.med, t.nodeId);
            public static implicit operator int(NodeId i) => i.Value;
            public void Deconstruct(out int med, out int nodeId) => (med, nodeId) = (Med, Value);
        }

        private readonly struct Float2WithId
        {
            public readonly int Id;
            public readonly float2 Value;
            public Float2WithId(int id, float2 value) => (Id, Value) = (id, value);
            public static implicit operator Float2WithId((int id, float2 value) t) => new(t.id, t.value);
        }

        public readonly struct Node
        {
            public bool IsLeaf => LeftChildId == -1 && RightChildId == -1;
            public readonly int ParentId;
            public readonly int LeftChildId;
            public readonly int RightChildId;

            public Node(int parentId) : this(parentId, NodeId.Invalid, NodeId.Invalid) { }

            public Node(int parentId, int leftChild, int rightChild)
            {
                ParentId = parentId;
                LeftChildId = leftChild;
                RightChildId = rightChild;
            }

            public Node WithChildren(int leftId, int rightId) => new(ParentId, leftId, rightId);
        }
        #endregion

        public NativeArray<Node> Nodes;
        public NativeReference<int> RootId;

        public Native2dTree(int pointsCount, Allocator allocator)
        {
            Nodes = new NativeArray<Node>(pointsCount, allocator);
            RootId = new NativeReference<int>(NodeId.Invalid, allocator);
        }

        public JobHandle Dispose(JobHandle dependencies)
        {
            dependencies = Nodes.Dispose(dependencies);
            dependencies = RootId.Dispose(dependencies);
            return dependencies;
        }

        public void Dispose()
        {
            Nodes.Dispose();
            RootId.Dispose();
        }

        public void Clear() => RootId.Value = NodeId.Invalid;
        public ReadOnly AsReadOnly() => new(this);
        public void Construct(NativeArray<float2> positions) => Construct(positions.AsReadOnly());
        public void Construct(NativeArray<float2>.ReadOnly positions)
        {
            if (positions.Length == 0)
            {
                return;
            }

            var data = new NativeArray<Float2WithId>(positions.Length, Allocator.Temp);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (i, positions[i]);
            }

            var nodeId = AddNode<Float2XComparer>(NodeId.Invalid, data);
            RootId.Value = nodeId;
            TrySliceParent<Float2YComparer, Float2XComparer>(nodeId, data);

            data.Dispose();
        }

        private void TrySliceParent<T1, T2>(NodeId nodeId, NativeArray<Float2WithId> data)
            where T1 : IComparer<Float2WithId>
            where T2 : IComparer<Float2WithId>
        {
            var (leftNodeId, leftData) = TryAddLeftNode<T1>(nodeId, data);
            var (rightNodeId, rightData) = TryAddRightNode<T1>(nodeId, data);
            AttachChildren(nodeId, leftNodeId, rightNodeId);

            if (leftNodeId.IsValid)
            {
                TrySliceParent<T2, T1>(leftNodeId, leftData);
            }

            if (rightNodeId.IsValid)
            {
                TrySliceParent<T2, T1>(rightNodeId, rightData);
            }
        }

        private (NodeId, NativeArray<Float2WithId>) TryAddLeftNode<T>(NodeId nodeId, NativeArray<Float2WithId> data)
            where T : IComparer<Float2WithId>
        {
            var med = nodeId.Med;
            if (med > 0)
            {
                data = data.GetSubArray(0, med);
                return (AddNode<T>(nodeId, data), data);
            }
            else
            {
                return (NodeId.Invalid, data);
            }
        }

        private (NodeId, NativeArray<Float2WithId>) TryAddRightNode<T>(NodeId nodeId, NativeArray<Float2WithId> data)
            where T : IComparer<Float2WithId>
        {
            var med = nodeId.Med;
            if (nodeId.IsValid && data.Length - med - 1 > 0)
            {
                data = data.GetSubArray(med + 1, data.Length - med - 1);
                return (AddNode<T>(nodeId, data), data);
            }
            else
            {
                return (NodeId.Invalid, data);
            }
        }

        private void AttachChildren(NodeId parentId, NodeId leftId, NodeId rightId)
        {
            var n = Nodes[parentId];
            Nodes[parentId] = n.WithChildren(leftId, rightId);
        }

        private NodeId AddNode<T>(NodeId parentId, NativeArray<Float2WithId> positions) where T : IComparer<Float2WithId>
        {
            positions.Sort(default(T));
            var med = positions.Length / 2;
            var nodeId = positions[med].Id;
            Nodes[nodeId] = new(parentId);
            return (med, nodeId);
        }

        #region ReadOnly
        public struct ReadOnly
        {
            public NativeArray<Node>.ReadOnly Nodes;
            public NativeReference<int>.ReadOnly RootId;
            public ReadOnly(Native2dTree tree) => (Nodes, RootId) = (tree.Nodes.AsReadOnly(), tree.RootId.AsReadOnly());
        }
        #endregion
    }
}