using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativePointQuadtreeEditorTests
    {
        private static readonly TestCaseData[] buildTestData =
        {
            new TestCaseData((
                points: new[]{ float2.zero },
                bounds: new AABB(0, 10),
                nodeCapacity: 1,
                nodes: new NativePointQuadtree.Node[]{ new (parentId: -1, new(0, 10), childId: -1, count: 1) },
                elementsIds: new[]{ 0 }
            )){ TestName = "Test case 1" },

            new TestCaseData((
                points: new[]{ math.float2(1, 1), math.float2(6, 6) },
                bounds: new AABB(0, 10),
                nodeCapacity: 1,
                nodes: new NativePointQuadtree.Node[]
                {
                    new (parentId: -1, new(0, 10), childId: 1, count: 0),
                    new (parentId: 0, new(math.float2(0, 5), math.float2(5, 10)), childId: -1, count: 0),
                    new (parentId: 0, new(math.float2(5, 5), math.float2(10, 10)), childId: -1, count: 1),
                    new (parentId: 0, new(math.float2(0, 0), math.float2(5, 5)), childId: -1, count: 1),
                    new (parentId: 0, new(math.float2(5, 0), math.float2(10, 5)), childId: -1, count: 0),
                },
                elementsIds: new[]{ -1, -1, 1, 0, -1 }
            )){ TestName = "Test case 2" },

            new TestCaseData((
                points: new float2[]{ },
                bounds: new AABB(0, 10),
                nodeCapacity: 1,
                nodes: new NativePointQuadtree.Node[]
                {
                    new (parentId: -1, new(0, 10), childId: -1, count: 0),
                },
                elementsIds: new[]{ -1 }
            )){ TestName = "Test case 3 (empty tree)" },

            new TestCaseData((
                points: new float2[]{ },
                bounds: new AABB(0, 10),
                nodeCapacity: 4,
                nodes: new NativePointQuadtree.Node[]
                {
                    new (parentId: -1, new(0, 10), childId: -1, count: 0),
                },
                elementsIds: new[]{ -1, -1, -1, -1 }
            )){ TestName = "Test case 4 (empty tree, extra capacity)" },

            new TestCaseData((
                points: new float2[]{ math.float2(1, 1), math.float2(5, 5) },
                bounds: new AABB(0, 10),
                nodeCapacity: 1,
                nodes: new NativePointQuadtree.Node[]
                {
                    new (parentId: -1, new(0, 10), childId: 1, count: 0),
                    new (parentId: 0, new(math.float2(0, 5), math.float2(5, 10)), childId: -1, count: 1),
                    new (parentId: 0, new(math.float2(5, 5), math.float2(10, 10)), childId: -1, count: 0),
                    new (parentId: 0, new(math.float2(0, 0), math.float2(5, 5)), childId: -1, count: 1),
                    new (parentId: 0, new(math.float2(5, 0), math.float2(10, 5)), childId: -1, count: 0),
                },
                elementsIds: new[]{ -1, 1, -1, 0, -1 }
            )){ TestName = "Test case 5" },

            new TestCaseData((
                points: new float2[]{ math.float2(1, 1), math.float2(5, 5) },
                bounds: new AABB(0, 10),
                nodeCapacity: 3,
                nodes: new NativePointQuadtree.Node[]
                {
                    new (parentId: -1, new(0, 10), childId: -1, count: 2),
                },
                elementsIds: new[]{ 0, 1, -1 }
            )){ TestName = "Test case 6 (extra capacity)" },

            new TestCaseData((
                points: new float2[]{ 1, 3, 6 },
                bounds: new AABB(0, 10),
                nodeCapacity: 1,
                nodes: new NativePointQuadtree.Node[]
                {
                    new (parentId: -1, new(0, 10), childId: 1, count: 0),
                    new (parentId: 0, new(math.float2(0, 5), math.float2(5, 10)), childId: -1, count: 0),
                    new (parentId: 0, new(5, 10), childId: -1, count: 1),
                    new (parentId: 0, new(0, 5), childId: 5, count: 0),
                    new (parentId: 0, new(math.float2(5, 0), math.float2(10,5)), childId: -1, count: 0),
                    new (parentId: 3, new(math.float2(0, 2.5f), math.float2(2.5f, 5)), childId: -1, count: 0),
                    new (parentId: 3, new(2.5f, 5), childId: -1, count: 1),
                    new (parentId: 3, new(0, 2.5f), childId: -1, count: 1),
                    new (parentId: 3, new(math.float2(2.5f,0), math.float2(5, 2.5f)), childId: -1, count: 0),
                },
                elementsIds: new[]{ -1, -1, 2, -1, -1, -1, 1, 0, -1 }
            )){ TestName = "Test case 7 (three level tree)" }
        };

        [Test, TestCaseSource(nameof(buildTestData))]
        public void BuildTest((float2[] points, AABB bounds, int nodeCapacity, NativePointQuadtree.Node[] nodes, int[] elementsIds) input)
        {
            using var tree = new NativePointQuadtree(input.bounds, treeCapacity: 1024, input.nodeCapacity, Allocator.Persistent);
            tree.Build(input.points);
            Assert.That(tree.Nodes.ToArray(), Is.EqualTo(input.nodes));
            Assert.That(tree.ElementsIds.ToArray(), Is.EqualTo(input.elementsIds));
        }

        [Test]
        public void ThrowOnDuplicatesTest()
        {
            float2[] points = { 0, 1, 2, 3.14f, 3.14f, 5 };
            using var tree = new NativePointQuadtree(bounds: new(0, 10), 64, 1, Allocator.Persistent);
            Assert.Throws<ArgumentException>(() => tree.Build(points));
        }

        [Test]
        public void ThrowOnOutOfRangeTest()
        {
            float2[] points = { 0, 1, 2, 3.14f, 5 };
            using var tree = new NativePointQuadtree(bounds: new(0, 1), 64, 1, Allocator.Persistent);
            Assert.Throws<ArgumentException>(() => tree.Build(points));
        }
    }
}