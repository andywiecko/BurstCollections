using NUnit.Framework;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativeBoundingVolumeTreeExtensionsEditorTests
    {
        [Test]
        public void TreeAABBGetIntersectionsWithAABBTest()
        {
            var tree = new NativeBoundingVolumeTree<AABB>(leavesCount: 4, Allocator.Persistent);
            //       6
            //     /   \
            //    4     5
            //   / \   / \
            //  0   1 2   3
            tree.RootId.Value = 6;
            tree.Nodes.CopyFrom(new NativeBoundingVolumeTree<AABB>.Node[]
            {
                (parent: 4, left:-1, right:-1),
                (parent: 4, left:-1, right:-1),
                (parent: 5, left:-1, right:-1),
                (parent: 5, left:-1, right:-1),
                (parent: 6, left: 0, right: 1),
                (parent: 6, left: 2, right: 3),
                (parent:-1, left: 4, right: 5),
            });
            //  __________________
            // |                  |
            // |    1         3   |
            // |  0         2     |
            // |__________________|
            var managedVolumes = new[]
            {
                new AABB(math.float2(0, 0), math.float2(1, 1)),
                new AABB(math.float2(1, 1), math.float2(2, 2)),
                new AABB(math.float2(5, 0), math.float2(6, 1)),
                new AABB(math.float2(6, 1), math.float2(7, 2)),
            };
            using var volumes = new NativeArray<AABB>(managedVolumes, Allocator.Persistent);

            tree.UpdateLeavesVolumes(volumes.AsReadOnly());

            AABB aabb;
            var result = new NativeList<int>(initialCapacity: 8, Allocator.Persistent);

            aabb = new AABB(-2, -1);
            tree.GetIntersectionsWithAABB(aabb, result);
            var result1 = result.AsArray().ToArray();
            result.Clear();

            aabb = new AABB(0.5f, 1.5f);
            tree.GetIntersectionsWithAABB(aabb, result);
            var result2 = result.AsArray().ToArray();
            result.Clear();

            aabb = new AABB(math.float2(5.5f, 0.5f), math.float2(5.5f, 0.5f));
            tree.GetIntersectionsWithAABB(aabb, result);
            var result3 = result.AsArray().ToArray();
            result.Clear();

            Assert.That(result1, Is.Empty);
            Assert.That(result2, Is.EqualTo(new[] { 0, 1 }));
            Assert.That(result3, Is.EqualTo(new[] { 2 }));

            tree.Dispose();
            result.Dispose();
        }

        private static readonly AABB UnitAABB = new AABB(0, 1);

        private static readonly TestCaseData[] intersectionsWithOtherTreeTestData = new[]
        {
            new TestCaseData(
                new[]{ UnitAABB, UnitAABB, UnitAABB, UnitAABB },
                new[]{ UnitAABB.Translate(3), UnitAABB.Translate(3) }
            ){ TestName = "Test case 1 (empty result)", ExpectedResult = new int2[]{ } },

            new TestCaseData(
                new[]
                {
                    new AABB(math.float2(0, 0), math.float2(1, 1)),
                    new AABB(math.float2(1, 0), math.float2(2, 1)),
                    new AABB(math.float2(1, 1), math.float2(2, 2)),
                    new AABB(math.float2(0, 1), math.float2(1, 2)),
                },
                new[]
                {
                    new AABB(math.float2(0, 0), math.float2(1, 1)).Translate(1.5f),
                    new AABB(math.float2(1, 0), math.float2(2, 1)).Translate(1.5f),
                    new AABB(math.float2(1, 1), math.float2(2, 2)).Translate(1.5f),
                    new AABB(math.float2(0, 1), math.float2(1, 2)).Translate(1.5f),
                }
            ){ TestName = "Test case 2 (single intersection)", ExpectedResult = new int2[]{ math.int2(2, 0) } },

            new TestCaseData(
                new[]
                {
                    UnitAABB.Translate(0),
                    UnitAABB.Translate(2),
                    UnitAABB.Translate(4),
                    UnitAABB.Translate(6),
                },
                new[]
                {
                    UnitAABB.Translate(0),
                    UnitAABB.Translate(2),
                    UnitAABB.Translate(4),
                    UnitAABB.Translate(6),
                }
            ){ TestName = "Test case 3 (\"self\" intersection)", ExpectedResult = new[]{ math.int2(0, 0), math.int2(1, 1), math.int2(2, 2), math.int2(3, 3) } },

            new TestCaseData(
                new[]{ UnitAABB, UnitAABB, UnitAABB },
                new[]{ UnitAABB, UnitAABB, UnitAABB }
            ){ TestName = "Test case 4 (all with all)", ExpectedResult = new[]
            {
                math.int2(0, 0), math.int2(0, 1), math.int2(0, 2),
                math.int2(1, 0), math.int2(1, 1), math.int2(1, 2),
                math.int2(2, 0), math.int2(2, 1), math.int2(2, 2),
            } },
        };

        [Test, TestCaseSource(nameof(intersectionsWithOtherTreeTestData))]
        public int2[] IntersectionsWithOtherTreeTest(AABB[] managedVolumes1, AABB[] managedVolumes2)
        {
            using var volumes1 = new NativeArray<AABB>(managedVolumes1, Allocator.Persistent);
            using var volumes2 = new NativeArray<AABB>(managedVolumes2, Allocator.Persistent);
            using var tree1 = new NativeBoundingVolumeTree<AABB>(managedVolumes1.Length, Allocator.Persistent);
            using var tree2 = new NativeBoundingVolumeTree<AABB>(managedVolumes2.Length, Allocator.Persistent);
            using var result = new NativeList<int2>(64, Allocator.Persistent);

            tree1.Construct(volumes1.AsReadOnly());
            tree2.Construct(volumes2.AsReadOnly());

            tree1.GetIntersectionsWithTree(tree2, result);

            return result.AsArray().OrderBy(i => i.x).ThenBy(i => i.y).ToArray();
        }
    }
}
