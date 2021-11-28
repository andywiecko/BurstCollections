using NUnit.Framework;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class MBCEditorTests
    {
        private static readonly TestCaseData[] MBCIntersectionData = new[]
        {
            new TestCaseData
            (
                new MBC(position: math.float2(0, 0), radius: 1),
                new MBC(position: math.float2(2, 0), radius: 1)
            )
            {
                TestName = "Test case 1",
                ExpectedResult = true
            },
            new TestCaseData
            (
                new MBC(position: 0, radius: 1),
                new MBC(position: 2.1f, radius: 1)
            )
            {
                TestName = "Test case 2",
                ExpectedResult = false
            },
            new TestCaseData
            (
                new MBC(position: 0, radius: 1),
                new MBC(position: 1f, radius: 2)
            )
            {
                TestName = "Test case 3",
                ExpectedResult = true
            },
            new TestCaseData
            (
                new MBC(position: 0, radius: 1),
                new MBC(position: 0, radius: 2)
            )
            {
                TestName = "Test case 4",
                ExpectedResult = true
            }
        };

        [Test, TestCaseSource(nameof(MBCIntersectionData))]
        public bool MBCIntersectionTest(MBC a, MBC b)
        {
            return a.Intersects(b);
        }

        private static readonly TestCaseData[] MBCUnionData = new[]
        {
            new TestCaseData(new MBC(position: 0, radius: 1), new MBC(position: 0, radius: 2))
            {
                TestName = "Test case 1",
                ExpectedResult = new MBC(position: 0, radius: 2)
            },
            new TestCaseData(new MBC(math.float2(0, 0), radius: 1), new MBC(math.float2(1, 0), radius: 1))
            {
                TestName = "Test case 2",
                ExpectedResult = new MBC(math.float2(0.5f, 0), radius: 1.5f)
            },
        };

        [Test, TestCaseSource(nameof(MBCUnionData))]
        public MBC MBCUnionTest(MBC a, MBC b) => a.Union(b);
    }
}