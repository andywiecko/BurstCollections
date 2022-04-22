using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativeBoundingVolumeTreeEditorTests
    {
        private NativeBoundingVolumeTree<AABB> tree;
        private NativeArray<AABB> volumes;

        [TearDown]
        public void TearDown()
        {
            if (tree.IsCreated)
            {
                tree.Dispose();
            }

            if (volumes.IsCreated)
            {
                volumes.Dispose();
            }
        }

        [Test]
        public void NodesCountTest()
        {
            var leavesCount = 4;
            tree = new NativeBoundingVolumeTree<AABB>(leavesCount, Allocator.Persistent);
            Assert.That(tree.Nodes.Length, Is.EqualTo(2 * leavesCount - 1));
        }

        [Test]
        public void ClearTest()
        {
            tree = new NativeBoundingVolumeTree<AABB>(1, Allocator.Persistent);
            tree.RootId.Value = 42;
            tree.Clear();
            Assert.That(tree.RootId.Value, Is.EqualTo(-1));
        }

        [Test]
        public void BreadthFirstSearchTest()
        {
            var visitedNodes = new List<int>();
            tree = new NativeBoundingVolumeTree<AABB>(leavesCount: 4, Allocator.Persistent);
            tree.RootId.Value = 0;
            //       0
            //     /   \
            //    1     2
            //   / \   / \
            //  3   4 5   6
            tree.Nodes.CopyFrom(new NativeBoundingVolumeTree<AABB>.Node[]
            {
                (parent:-1, left: 1, right: 2),
                (parent: 0, left: 3, right: 4),
                (parent: 0, left: 5, right: 6),
                (parent: 1, left:-1, right:-1),
                (parent: 1, left:-1, right:-1),
                (parent: 2, left:-1, right:-1),
                (parent: 2, left:-1, right:-1),
            });

            var bfs = tree.BreadthFirstSearch();
            foreach (var (id, _) in bfs)
            {
                visitedNodes.Add(id);
                bfs.Traverse(id);
            }

            Assert.That(visitedNodes, Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5, 6 }));
        }

        [Test]
        public void ConstructTreeNodesTest()
        {
            tree = new NativeBoundingVolumeTree<AABB>(leavesCount: 4, Allocator.Persistent);
            //  __________________
            // |                  |
            // |    X         X   |
            // |  X         X     |
            // |__________________|
            var leaves = new[]
            {
                new AABB(math.float2(0, 0), math.float2(1, 1)),
                new AABB(math.float2(1, 1), math.float2(2, 2)),
                new AABB(math.float2(5, 0), math.float2(6, 1)),
                new AABB(math.float2(6, 1), math.float2(7, 2)),
            };
            volumes = new NativeArray<AABB>(leaves, Allocator.Persistent);

            tree.Construct(volumes.AsReadOnly());

            var expectedNodes = new NativeBoundingVolumeTree<AABB>.Node[]
            {
                (parent: 4, left:-1, right:-1),
                (parent: 4, left:-1, right:-1),
                (parent: 6, left:-1, right:-1),
                (parent: 6, left:-1, right:-1),
                (parent: 5, left: 0, right: 1),
                (parent:-1, left: 4, right: 6),
                (parent: 5, left: 2, right: 3),
            };
            Assert.That(tree.Nodes, Is.EqualTo(expectedNodes));
        }

        [Test]
        public void ConstructTreeVolumesTest()
        {
            tree = new NativeBoundingVolumeTree<AABB>(leavesCount: 4, Allocator.Persistent);
            //  __________________
            // |                  |
            // |    X         X   |
            // |  X         X     |
            // |__________________|
            var leaves = new[]
            {
                new AABB(math.float2(0, 0), math.float2(1, 1)),
                new AABB(math.float2(1, 1), math.float2(2, 2)),
                new AABB(math.float2(5, 0), math.float2(6, 1)),
                new AABB(math.float2(6, 1), math.float2(7, 2)),
            };
            volumes = new NativeArray<AABB>(leaves, Allocator.Persistent);

            tree.Construct(volumes.AsReadOnly());

            var expectedVolumes = new[]
            {
                leaves[0],
                leaves[1],
                leaves[2],
                leaves[3],
                leaves[0].Union(leaves[1]),
                leaves[0].Union(leaves[1]).Union(leaves[2]).Union(leaves[3]),
                leaves[2].Union(leaves[3])
            };
            Assert.That(tree.Volumes, Is.EqualTo(expectedVolumes));
        }

        [Test]
        public void UpdateVolumesTest()
        {
            tree = new NativeBoundingVolumeTree<AABB>(leavesCount: 4, Allocator.Persistent);
            tree.RootId.Value = 6;
            //        6
            //      /   \
            //     5     3
            //   /   \
            //  0     4
            //       / \
            //      1   2
            tree.Nodes.CopyFrom(new NativeBoundingVolumeTree<AABB>.Node[]
            {
                (parent: 5, left:-1, right:-1),
                (parent: 4, left:-1, right:-1),
                (parent: 4, left:-1, right:-1),
                (parent: 6, left:-1, right:-1),
                (parent: 5, left: 1, right: 2),
                (parent: 6, left: 0, right: 4),
                (parent:-1, left: 5, right: 3),
            });
            volumes = new NativeArray<AABB>(4, Allocator.Persistent);
            var leaves = new[]
            {
                new AABB(math.float2(0, 0), math.float2(1, 1)),
                new AABB(math.float2(2, 2), math.float2(3, 3)),
                new AABB(math.float2(0, 3), math.float2(1, 4)),
                new AABB(math.float2(6, 2), math.float2(7, 3)),
            };
            volumes.CopyFrom(leaves);

            tree.UpdateLeavesVolumes(volumes.AsReadOnly());

            var expectedVolumes = new[]
            {
                leaves[0],
                leaves[1],
                leaves[2],
                leaves[3],
                leaves[1].Union(leaves[2]),
                leaves[0].Union(leaves[1]).Union(leaves[2]),
                leaves[0].Union(leaves[1]).Union(leaves[2]).Union(leaves[3])
            };
            Assert.That(tree.Volumes, Is.EqualTo(expectedVolumes));
        }

        [Test]
        public void ThrowWhenNotConstructedTest()
        {
            tree = new NativeBoundingVolumeTree<AABB>(4, Allocator.Persistent);
            volumes = new NativeArray<AABB>(4, Allocator.Persistent);
            Assert.Throws<Exception>(() => tree.UpdateLeavesVolumes(volumes.AsReadOnly()));
        }

        [Test]
        public void ThrowWhenLengthsAreWrong()
        {
            tree = new NativeBoundingVolumeTree<AABB>(4, Allocator.Persistent);
            volumes = new NativeArray<AABB>(3, Allocator.Persistent);
            Assert.Throws<InvalidOperationException>(() => tree.Construct(volumes.AsReadOnly()));
        }
    }
}
