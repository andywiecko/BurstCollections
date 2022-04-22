using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace andywiecko.BurstCollections
{
    [Obsolete("Use " + nameof(NativeBoundingVolumeTree<AABB>) + " instead!")]
    public struct BoundingVolumeTree<T> : INativeDisposable where T : unmanaged, IBoundingVolume<T>
    {
        private const int None = -1;

        #region BFS
        public struct BFSEnumerator
        {
            private NativeBoundingVolumeTree<T> owner;
            public (int, T) Current { get; private set; }

            public BFSEnumerator(NativeBoundingVolumeTree<T> owner)
            {
                this.owner = owner;

                NativeBoundingVolumeTree<T>.CheckIfTreeIsConstructed(owner.RootId.Value);

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

        public BFSEnumerator BreadthFirstSearch => new BFSEnumerator(tree);
        public bool IsCreated => tree.IsCreated;
        public bool IsEmpty => tree.IsEmpty;
        public int LeavesCount => tree.LeavesCount;
        public NativeArray<Node> Nodes => tree.Nodes.Reinterpret<Node>();
        public NativeArray<T> Volumes => tree.Volumes;
        public NativeReference<int> RootId => tree.RootId;
        internal NativeBoundingVolumeTree<T> tree;
        public BoundingVolumeTree(int leavesCount, Allocator allocator) => tree = new NativeBoundingVolumeTree<T>(leavesCount, allocator);
        public JobHandle Dispose(JobHandle dependencies) => tree.Dispose(dependencies);
        public void Dispose() => tree.Dispose();
        public void Clear() => tree.Clear();
        public void Construct(NativeArray<T>.ReadOnly volumes) => tree.Construct(volumes);
        public JobHandle Construct(NativeArray<T>.ReadOnly volumes, JobHandle dependencies) => tree.Construct(volumes, dependencies);
        public void UpdateLeavesVolumes(NativeArray<T>.ReadOnly volumes) => tree.UpdateLeavesVolumes(volumes);
        public JobHandle UpdateLeafesVolumes(NativeArray<T>.ReadOnly volumes, JobHandle dependencies) => tree.UpdateLeavesVolumes(volumes, dependencies);
    }

    /// <summary>
    /// Burst friendly implementation of the native bounding volume tree.
    /// </summary>
    public struct NativeBoundingVolumeTree<T> : INativeDisposable where T : unmanaged, IBoundingVolume<T>
    {
        private const int None = -1;

        public bool IsCreated => Nodes.IsCreated;
        public bool IsEmpty => RootId.Value == None;
        public int LeavesCount { get; }

        #region BFS
        /// <summary>
        /// See wikipage about <see href="https://en.wikipedia.org/wiki/Breadth-first_search">Breadth-first search (BFS)</see>.
        /// </summary>
        public struct BFSEnumerator
        {
            public (int, T) Current { get; private set; }

            private NativeArray<Node>.ReadOnly nodes;
            private NativeArray<T>.ReadOnly volumes;
            private NativeReference<int>.ReadOnly rootId;
            private NativeQueue<int> queue;

            public BFSEnumerator(ReadOnly owner, NativeQueue<int> queue)
            {
                this.nodes = owner.Nodes;
                this.volumes = owner.Volumes;
                this.rootId = owner.RootId;
                this.queue = queue;

                CheckIfTreeIsConstructed(rootId.Value);

                var root = rootId.Value;
                Current = (root, volumes[root]);

                queue.Clear();
                queue.Enqueue(root);
            }

            public bool IsLeaf(int id) => nodes[id].IsLeaf;

            public void Traverse(int id)
            {
                var n = nodes[id];
                if (!n.IsLeaf)
                {
                    queue.Enqueue(n.LeftChildId);
                    queue.Enqueue(n.RightChildId);
                }
            }

            public bool MoveNext()
            {
                var isEmpty = queue.IsEmpty();
                if (isEmpty)
                {
                    return false;
                }
                else
                {
                    var id = queue.Dequeue();
                    Current = (id, volumes[id]);
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
            private NativeBoundingVolumeTree<T> tree;
            private NativeArray<T>.ReadOnly volumes;

            public ConstructJob(NativeBoundingVolumeTree<T> tree, NativeArray<T>.ReadOnly volumes)
            {
                this.tree = tree;
                this.volumes = volumes;
            }

            public void Execute() => tree.Construct(volumes);
        }

        [BurstCompile]
        private struct UpdateLeafesVolumesJob : IJob
        {
            private NativeBoundingVolumeTree<T> tree;
            private NativeArray<T>.ReadOnly volumes;

            public UpdateLeafesVolumesJob(NativeBoundingVolumeTree<T> tree, NativeArray<T>.ReadOnly volumes)
            {
                this.tree = tree;
                this.volumes = volumes;
            }

            public void Execute() => tree.UpdateLeavesVolumes(volumes);
        }
        #endregion

        #region ReadOnly
        public struct ReadOnly
        {
            public bool IsEmpty => RootId.Value == None;
            public int LeavesCount { get; }

            public NativeArray<Node>.ReadOnly Nodes;
            public NativeArray<T>.ReadOnly Volumes;
            public NativeReference<int>.ReadOnly RootId;

            internal ReadOnly(NativeBoundingVolumeTree<T> owner)
            {
                Nodes = owner.Nodes.AsReadOnly();
                Volumes = owner.Volumes.AsReadOnly();
                RootId = owner.RootId.AsReadOnly();
                LeavesCount = owner.LeavesCount;
            }

            public BFSEnumerator BreadthFirstSearch(NativeQueue<int> queue) => new BFSEnumerator(this, queue);
        }
        #endregion

        public NativeArray<Node> Nodes;
        public NativeArray<T> Volumes;
        public NativeReference<int> RootId;

        internal NativeReference<int> currentInternalCount;
        internal NativeQueue<int> tmpNodesQueue;
        internal NativeStack<int> tmpNodesStack;

        public NativeBoundingVolumeTree(int leavesCount, Allocator allocator)
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

        public BFSEnumerator BreadthFirstSearch() => new BFSEnumerator(AsReadOnly(), tmpNodesQueue);
        public ReadOnly AsReadOnly() => new ReadOnly(this);
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
            CheckIfTreeIsConstructed(RootId.Value);
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
        public JobHandle UpdateLeavesVolumes(NativeArray<T>.ReadOnly volumes, JobHandle dependencies) =>
            new UpdateLeafesVolumesJob(this, volumes).Schedule(dependencies);

        private void RecalculateVolumes()
        {
            var bfs = BreadthFirstSearch();
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
        internal static void CheckIfTreeIsConstructed(int rootId)
        {
            if (rootId == None)
            {
                throw new Exception
                (
                    $"{nameof(NativeBoundingVolumeTree<T>)} has not been constructed! " +
                    $"One should construct tree before using it."
                );
            }
        }
    }
}