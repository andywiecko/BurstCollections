using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class BoundingVolumeTreeExtensionsEditorTests
    {
        [Test]
        public void TreeAABBGetIntersectionsWithAABBTest()
        {
            var tree = new BoundingVolumeTree<AABB>(leavesCount: 4, Allocator.Persistent);
            //       6
            //     /   \
            //    4     5
            //   / \   / \
            //  0   1 2   3
            tree.RootId.Value = 6;
            tree.Nodes.CopyFrom(new BoundingVolumeTree<AABB>.Node[]
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
            var result1 = result.ToArray();
            result.Clear();

            aabb = new AABB(0.5f, 1.5f);
            tree.GetIntersectionsWithAABB(aabb, result);
            var result2 = result.ToArray();
            result.Clear();

            aabb = new AABB(math.float2(5.5f, 0.5f), math.float2(5.5f, 0.5f));
            tree.GetIntersectionsWithAABB(aabb, result);
            var result3 = result.ToArray();
            result.Clear();

            Assert.That(result1, Is.Empty);
            Assert.That(result2, Is.EqualTo(new[] { 0, 1 }));
            Assert.That(result3, Is.EqualTo(new[] { 2 }));

            tree.Dispose();
            result.Dispose();
        }
    }
}
