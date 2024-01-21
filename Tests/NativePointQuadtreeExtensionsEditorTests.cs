using NUnit.Framework;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativePointQuadtreeExtensionsEditorTests
    {
        private static readonly TestCaseData[] rangeSearchTestData =
        {
            new TestCaseData(new AABB(0, 10), new float2[]{ }, new AABB(0, 10))
            {
                ExpectedResult = new int[]{ },
                TestName = "Test case 1 (empty tree)",
            },

            new TestCaseData(new AABB(0, 10), new float2[]{ 1, 2, 3, 4, 5 }, new AABB(0, 10))
            {
                ExpectedResult = new int[]{ 0, 1, 2, 3, 4 },
                TestName = "Test case 2 (grab all)",
            },

            new TestCaseData(new AABB(0, 10), new float2[]{ 1, 2, 3, 4, 5 }, new AABB(1.5f, 3.5f))
            {
                ExpectedResult = new int[]{ 1, 2 },
                TestName = "Test case 3 (grab selected)",
            }
        };

        [Test, TestCaseSource(nameof(rangeSearchTestData))]
        public int[] RangeSearchTest(AABB bounds, float2[] points, AABB range)
        {
            using var result = new NativeList<int>(64, Allocator.Persistent);
            using var tree = new NativePointQuadtree(bounds, treeCapacity: 64, nodeCapacity: 1, Allocator.Persistent);
            tree.Build(points);
            tree.AsReadOnly().RangeSearch(range, points, result);
            return result.AsArray().OrderBy(i => i).ToArray();
        }
    }
}