using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class Native2dTreeEditorTests
    {
        private const int None = -1;

        [Test]
        public void TreeConstructionWikipediaTestCase()
        {
            // Test case from wikipage: https://en.wikipedia.org/wiki/K-d_tree
            //
            //   ^
            //   :                  .           .                
            // 7 -                  #           .                
            //   :                  .           .                
            // 6 -                  .           .------ # ------ 
            //   :                  .           .   .            
            // 5 -                  .           .   .            
            //   :                  .           .   .            
            // 4 ---------------------- # ------.   .            
            //   :          .                   .   .            
            // 3 -          #                   .   .            
            //   :          .                   .   .            
            // 2 -          .                   #   .            
            //   :          .                   .   .            
            // 1 -          .                   .   #            
            //   :          .                   .   .            
            // 0 -          .                   .   .            
            //   :          .                   .   .             
            // -----.---.---.---.---.---.---.---.---.---.---.---->
            //   :  0   1   2   3   4   5   6   7   8   9   10
            //   :
            using var tree = new Native2dTree(6, Allocator.Persistent);
            using var positions = new NativeArray<float2>(new float2[]
            {
                new(7, 2), new(5, 4), new(9, 6), new(2, 3), new(4, 7), new(8, 1)
            }, Allocator.Persistent);

            tree.Construct(positions);

            var nodes = tree.Nodes.ToArray();
            var expected = new Native2dTree.Node[]
            {
                new(None, 1, 2),
                new(0, 3, 4), new(0, 5, None),
                new(1, None, None), new(1, None, None), new(2, None, None)
            };
            Assert.That(nodes, Is.EqualTo(expected));
        }

        private static readonly TestCaseData[] rangeSearchTestData = new[]
        {
            new TestCaseData( new AABB(
                min: new(-1, -1),
                max: new(0, 0)))
            { TestName = "Test Case 1", ExpectedResult = new int[]{ } },
            new TestCaseData( new AABB(
                min: new(0, 0),
                max: new(10, 10)))
            { TestName = "Test Case 2", ExpectedResult = new int[]{ 0, 1, 2, 3, 4, 5 } },
            new TestCaseData( new AABB(
                min: new(2, 3),
                max: new(2, 3)))
            { TestName = "Test Case 3", ExpectedResult = new int[]{ 3 } },
            new TestCaseData( new AABB(
                min: new(2, 2),
                max: new(7, 4)))
            { TestName = "Test Case 4", ExpectedResult = new int[]{ 0, 1, 3 } },
        };

        [Test, TestCaseSource(nameof(rangeSearchTestData))]
        public int[] RangeSearchTest(AABB range)
        {
            using var result = new NativeList<int>(64, Allocator.Persistent);
            using var tree = new Native2dTree(6, Allocator.Persistent);
            using var positions = new NativeArray<float2>(new float2[]
            {
                // Same configuration as in the previous test.
                new(7, 2), new(5, 4), new(9, 6), new(2, 3), new(4, 7), new(8, 1)
            }, Allocator.Persistent);

            tree.Construct(positions);
            tree.RangeSearch(range, positions, result);

            result.Sort();
            return result.ToArray();
        }

        private static readonly TestCaseData[] findDeepestNodeTestData = new[]
        {
            new TestCaseData(math.float2(10.0f, 8)) { TestName = "Test Case 1", ExpectedResult = (2, 1)},
            new TestCaseData(math.float2(7.50f, 0)) { TestName = "Test Case 2", ExpectedResult = (5, 0)},
            new TestCaseData(math.float2(0.00f, 0)) { TestName = "Test Case 3", ExpectedResult = (3, 0)},
        };

        [Test, TestCaseSource(nameof(findDeepestNodeTestData))]
        public (int, int) FindDeepestNodeTest(float2 point)
        {
            using var result = new NativeList<int>(64, Allocator.Persistent);
            using var tree = new Native2dTree(6, Allocator.Persistent);
            using var positions = new NativeArray<float2>(new float2[]
            {
                // Same configuration as in the previous test.
                new(7, 2), new(5, 4), new(9, 6), new(2, 3), new(4, 7), new(8, 1)
            }, Allocator.Persistent);

            tree.Construct(positions);

            return tree.FindDeepestNode(point, positions);
        }
    }
}