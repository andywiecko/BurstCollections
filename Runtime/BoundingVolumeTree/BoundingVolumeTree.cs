using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace andywiecko.BurstCollections
{
    /// <summary>
    /// Burst friendly implementation of the native bounding volume tree.
    /// </summary>
    public struct BoundingVolumeTree<T> : INativeDisposable where T : unmanaged, IBoundingVolume<T>
    {
        private const int None = -1;

        public BFSEnumerator BreadthFirstSearch => new BFSEnumerator(this);
        public bool IsCreated => Nodes.IsCreated;
        public bool IsEmpty => RootId.Value == None;
        public int LeavesCount { get; }

        #region BFS
        /// <summary>
        /// See wikipage about <see href="https://en.wikipedia.org/wiki/Breadth-first_search">Breadth-first search (BFS)</see>.
        /// </summary>
        public struct BFSEnumerator
        {
            private BoundingVolumeTree<T> owner;
            public (int, T) Current { get; private set; }

            public BFSEnumerator(BoundingVolumeTree<T> owner)
            {
                this.owner = owner;

                CheckIfTreeIsConstructed(owner.RootId);

                var root = owner.RootId.Value;
                Current = (root, owner.Volumes[root]);

                owner.tmpNodesQueue.Clear();
                owner.tmpNodesQueue.Enqueue(root);
            }

            public bool IsLeaf(int id) => owner.Nodes[id].IsLeaf;

            public void Traverse(int id)
            {
                var n = owner.Nodes[id];
                if (!n.IsLeaf)
                {
                    owner.tmpNodesQueue.Enqueue(n.LeftChildId);
                    owner.tmpNodesQueue.Enqueue(n.RightChildId);
                }
            }

            public bool MoveNext()
            {
                var isEmpty = owner.tmpNodesQueue.IsEmpty();
                if (isEmpty)
                {
                    return false;
                }
                else
                {
                    var id = owner.tmpNodesQueue.Dequeue();
                    Current = (id, owner.Volumes[id]);
                    return true;
                }
            }

            public BFSEnumerator GetEnumerator() => this;
        }
        #endregion

        #region Node
        public readonly struct Node
        {
            public bool IsRoot => ParentId == None;
            public bool IsLeaf => !LeftChildIsValid && !RightChildIsValid;
            public bool ParentIsValid => ParentId != None;
            public bool LeftChildIsValid => LeftChildId != None;
            public bool RightChildIsValid => RightChildId != None;

            public readonly int ParentId;
            public readonly int LeftChildId;
            public readonly int RightChildId;

            public Node(int parentId, int leftChild, int rightChild)
            {
                ParentId = parentId;
                LeftChildId = leftChild;
                RightChildId = rightChild;
            }

            public static implicit operator Node((int parent, int left, int right) n) => new Node(n.parent, n.left, n.right);
            public Node WithParent(int parentId) => new Node(parentId, LeftChildId, RightChildId);
            public Node WithLeftChild(int childId) => new Node(ParentId, childId, RightChildId);
            public Node WithRightChild(int childId) => new Node(ParentId, LeftChildId, childId);
            public void Deconstruct(out int parentId, out int leftChildId, out int rightChildId) => _ =
                (parentId = ParentId, leftChildId = LeftChildId, rightChildId = RightChildId);
        }
        #endregion

        #region Jobs
        [BurstCompile]
        private struct ConstructJob : IJob
        {
            private BoundingVolumeTree<T> tree;
            private NativeArray<T>.ReadOnly volumes;

            public ConstructJob(BoundingVolumeTree<T> tree, NativeArray<T>.ReadOnly volumes)
            {
                this.tree = tree;
                this.volumes = volumes;
            }

            public void Execute() => tree.Construct(volumes);
        }

        [BurstCompile]
        private struct UpdateLeafesVolumesJob : IJob
        {
            private BoundingVolumeTree<T> tree;
            private NativeArray<T>.ReadOnly volumes;

            public UpdateLeafesVolumesJob(BoundingVolumeTree<T> tree, NativeArray<T>.ReadOnly volumes)
            {
                this.tree = tree;
                this.volumes = volumes;
            }

            public void Execute() => tree.UpdateLeavesVolumes(volumes);
        }
        #endregion

        public NativeArray<Node> Nodes;
        public NativeArray<T> Volumes;
        public NativeReference<int> RootId;

        private NativeReference<int> currentInternalCount;
        private NativeQueue<int> tmpNodesQueue;
        private NativeStack<int> tmpNodesStack;

        public BoundingVolumeTree(int leavesCount, Allocator allocator)
        {
            LeavesCount = leavesCount;
            var length = 2 * leavesCount - 1;
            Nodes = new NativeArray<Node>(length, allocator);
            Volumes = new NativeArray<T>(length, allocator);
            RootId = new NativeReference<int>(-1, allocator);
            currentInternalCount = new NativeReference<int>(0, allocator);
            tmpNodesQueue = new NativeQueue<int>(allocator);
            tmpNodesStack = new NativeStack<int>(leavesCount - 1, allocator);
        }

        public JobHandle Dispose(JobHandle dependencies)
        {
            dependencies = Nodes.Dispose(dependencies);
            dependencies = Volumes.Dispose(dependencies);
            dependencies = RootId.Dispose(dependencies);
            dependencies = currentInternalCount.Dispose(dependencies);
            dependencies = tmpNodesQueue.Dispose(dependencies);
            dependencies = tmpNodesStack.Dispose(dependencies);

            return dependencies;
        }

        public void Dispose()
        {
            Nodes.Dispose();
            Volumes.Dispose();
            RootId.Dispose();
            currentInternalCount.Dispose();
            tmpNodesQueue.Dispose();
            tmpNodesStack.Dispose();
        }

        public void Clear() => RootId.Value = None;

        /// <summary>
        /// Incremental construction of the tree with the given <paramref name="volumes"/>.
        /// </summary>
        public void Construct(NativeArray<T>.ReadOnly volumes)
        {
            CheckLengths(volumes, LeavesCount);

            Clear();

            if (volumes.Length == 0)
            {
                return;
            }

            Nodes[0] = (parent: None, left: None, right: None);
            RootId.Value = 0;
            Volumes[0] = volumes[0];

            for (int objectId = 1; objectId < volumes.Length; objectId++)
            {
                var volume = volumes[objectId];
                var bestNodeId = FindBestNode(volume);
                InsertNode(objectId, volume, bestNodeId);
            }
        }

        /// <summary>
        /// Incremental (jobified) construction of the tree with the given <paramref name="volumes"/>.
        /// </summary>
        public JobHandle Construct(NativeArray<T>.ReadOnly volumes, JobHandle dependencies) =>
            new ConstructJob(this, volumes).Schedule(dependencies);

        /// <summary>
        /// Update leaves <paramref name="volumes"/> and all internal tree nodes.
        /// </summary>
        public void UpdateLeavesVolumes(NativeArray<T>.ReadOnly volumes)
        {
            CheckIfTreeIsConstructed(RootId);
            CheckLengths(volumes, LeavesCount);

            for (int i = 0; i < volumes.Length; i++)
            {
                Volumes[i] = volumes[i];
            }

            RecalculateVolumes();
        }

        /// <summary>
        /// Update leaves <paramref name="volumes"/> and all internal tree nodes (jobified).
        /// </summary>
        public JobHandle UpdateLeafesVolumes(NativeArray<T>.ReadOnly volumes, JobHandle dependencies) =>
            new UpdateLeafesVolumesJob(this, volumes).Schedule(dependencies);

        private void RecalculateVolumes()
        {
            var bfs = BreadthFirstSearch;
            foreach (var (id, _) in bfs)
            {
                if (!bfs.IsLeaf(id))
                {
                    tmpNodesStack.Push(id);
                }

                bfs.Traverse(id);
            }

            while (tmpNodesStack.TryPop(out var id))
            {
                var (_, leftChild, rightChild) = Nodes[id];
                Volumes[id] = Volumes[leftChild].Union(Volumes[rightChild]);
            }
        }

        private void InsertNode(int objectId, T volume, int targetId)
        {
            var target = Nodes[targetId];
            var targetParentId = target.ParentId;
            var targetVolume = Volumes[targetId];

            // Add internal node
            var tmpId = LeavesCount + currentInternalCount.Value;
            var tmp = new Node(parentId: targetParentId, leftChild: targetId, rightChild: objectId);
            Nodes[tmpId] = tmp;
            Volumes[tmpId] = volume.Union(targetVolume);
            currentInternalCount.Value++;

            // Update target
            Nodes[targetId] = target.WithParent(tmpId);

            // Change root if necessary
            if (targetParentId == None)
            {
                RootId.Value = tmpId;
            }
            else
            {
                var parent = Nodes[targetParentId];
                parent = parent.LeftChildId == targetId ? parent.WithLeftChild(tmpId) : parent.WithRightChild(tmpId);
                Nodes[targetParentId] = parent;
                var left = parent.LeftChildId;
                var right = parent.RightChildId;
                Volumes[targetParentId] = Volumes[left].Union(Volumes[right]);

                while (parent.ParentIsValid)
                {
                    var parentId = parent.ParentId;
                    parent = Nodes[parentId];
                    Volumes[parentId] = Volumes[parent.LeftChildId].Union(Volumes[parent.RightChildId]);
                }
            }

            // Add leaf
            Nodes[objectId] = new Node(parentId: tmpId, None, None);
            Volumes[objectId] = volume;
        }

        private int FindBestNode(T volume)
        {
            var bestSibling = None;
            var bestCost = float.MaxValue;

            tmpNodesQueue.Enqueue(RootId.Value);
            while (tmpNodesQueue.TryDequeue(out var otherId))
            {
                var directCost = GetDirectCost(volume, otherId);
                if (directCost >= bestCost)
                {
                    continue;
                }

                var inheritedCost = GetInheritedCost(volume, otherId);

                var cost = directCost + inheritedCost;
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestSibling = otherId;

                    var lowerBoundCost = GetLowerBoundCost(volume, otherId);
                    if (lowerBoundCost < bestCost)
                    {
                        EnqueueChildren(otherId);
                    }
                }
            }

            return bestSibling;
        }

        private void EnqueueChildren(int nodeId)
        {
            var node = Nodes[nodeId];
            if (!node.IsLeaf)
            {
                tmpNodesQueue.Enqueue(node.LeftChildId);
                tmpNodesQueue.Enqueue(node.RightChildId);
            }
        }

        private float GetDirectCost(T volume, int otherId)
        {
            var otherVolume = Volumes[otherId];
            return volume.Union(otherVolume).Volume;
        }

        private float GetLowerBoundCost(T volume, int otherId)
        {
            var otherVolume = Volumes[otherId];
            return volume.Volume + volume.Union(otherVolume).Volume - otherVolume.Volume + GetInheritedCost(volume, otherId);
        }

        private float GetInheritedCost(T volume, int otherId)
        {
            var cost = 0f;
            var other = Nodes[otherId];

            while (!other.IsRoot)
            {
                var parentId = other.ParentId;
                var parent = Nodes[parentId];
                var parentVolume = Volumes[parentId];
                cost += parentVolume.Union(volume).Volume - parentVolume.Volume;

                other = parent;
            }

            return cost;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckLengths(NativeArray<T>.ReadOnly volumes, int leavesCount)
        {
            if (volumes.Length != leavesCount)
            {
                throw new InvalidOperationException
                (
                    $"Tree leaves count ({leavesCount}) does not match " +
                    $"the provided volumes list length ({volumes.Length})"
                );
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckIfTreeIsConstructed(NativeReference<int> rootId)
        {
            if (rootId.Value == None)
            {
                throw new Exception
                (
                    $"{nameof(BoundingVolumeTree<T>)} has not been constructed! " +
                    $"One should construct tree before using it."
                );
            }
        }
    }
}