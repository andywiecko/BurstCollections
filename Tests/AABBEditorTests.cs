using NUnit.Framework;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class AABBEditorTests
    {
        private static readonly TestCaseData[] AABBIntersectionData = new[]
        {
            new TestCaseData
            (
                new AABB(min: 0.0f, max: 1.0f),
                new AABB(min: 0.5f, max: 1.5f)
            )
            {
                TestName = "Test case 1",
                ExpectedResult = true
            },
            new TestCaseData
            (
                new AABB(min: 0.0f, max: 1.0f),
                new AABB(min: 2.0f, max: 3.0f)
            )
            {
                TestName = "Test case 2",
                ExpectedResult = false
            }
        };

        [Test, TestCaseSource(nameof(AABBIntersectionData))]
        public bool AABBIntersectionTest(AABB a, AABB b)
        {
            return a.Intersects(b);
        }

        private static readonly TestCaseData[] AABBUnionData = new[]
        {
            new TestCaseData(new AABB(0, 1), new AABB(1, 2)){ TestName = "Test case 1", ExpectedResult = new AABB(0, 2) },
            new TestCaseData(new AABB(1, 2), new AABB(0, 3)){ TestName = "Test case 2", ExpectedResult = new AABB(0, 3) }
        };

        [Test, TestCaseSource(nameof(AABBUnionData))]
        public AABB AABBUnionTest(AABB a, AABB b) => a.Union(b);

        private static readonly TestCaseData[] AABBContainsPointTestData = new[]
        {
            new TestCaseData(new AABB(0, 1), (float2)0.5f){ TestName = "Test case 1", ExpectedResult = true },
            new TestCaseData(new AABB(0, 1), (float2)2.5f){ TestName = "Test case 2", ExpectedResult = false },
            new TestCaseData(new AABB(0, 1), -(float2)2.5f){ TestName = "Test case 3", ExpectedResult = false },
            new TestCaseData(new AABB(0, 1), (float2)0){ TestName = "Test case 4", ExpectedResult = true },
        };

        [Test, TestCaseSource(nameof(AABBContainsPointTestData))]
        public bool AABBContainsPointTest(AABB aabb, float2 p) => aabb.Contains(p);
    }
}