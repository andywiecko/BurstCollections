using NUnit.Framework;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class AbelianOperatorsEditorTests
    {
        private static readonly TestCaseData[] neutralElementOperationTestData = new[]
        {
            // Int
            new TestCaseData(12, new IntSum()) {TestName = "IntSum", ExpectedResult = 12},
            new TestCaseData(math.int2(12, 12), new Int2Sum()) {TestName = "Int2Sum", ExpectedResult = math.int2(12, 12)},
            new TestCaseData(math.int3(12, 12, 12), new Int3Sum()) {TestName = "Int3Sum", ExpectedResult = math.int3(12, 12, 12)},
            new TestCaseData(math.int4(12, 12, 12, 12), new Int4Sum()) {TestName = "Int4Sum", ExpectedResult = math.int4(12, 12, 12, 12)},
            
            new TestCaseData(12, new IntMin()) {TestName = "IntMin", ExpectedResult = 12},
            new TestCaseData(math.int2(12, 12), new Int2Min()) {TestName = "Int2Min", ExpectedResult = math.int2(12, 12)},
            new TestCaseData(math.int3(12, 12, 12), new Int3Min()) {TestName = "Int3Min", ExpectedResult = math.int3(12, 12, 12)},
            new TestCaseData(math.int4(12, 12, 12, 12), new Int4Min()) {TestName = "Int4Min", ExpectedResult = math.int4(12, 12, 12, 12)},

            new TestCaseData(12, new IntMax()) {TestName = "IntMax", ExpectedResult = 12},
            new TestCaseData(math.int2(12, 12), new Int2Max()) {TestName = "Int2Max", ExpectedResult = math.int2(12, 12)},
            new TestCaseData(math.int3(12, 12, 12), new Int3Max()) {TestName = "Int3Max", ExpectedResult = math.int3(12, 12, 12)},
            new TestCaseData(math.int4(12, 12, 12, 12), new Int4Max()) {TestName = "Int4Max", ExpectedResult = math.int4(12, 12, 12, 12)},

            // Float Sum
            new TestCaseData(12f, new FloatSum()) {TestName = "FloatSum", ExpectedResult = 12f},
            new TestCaseData(math.float2(12, 12), new Float2Sum()) {TestName = "Float2Sum", ExpectedResult = math.float2(12, 12)},
            new TestCaseData(math.float3(12, 12, 12), new Float3Sum()) {TestName = "Float3Sum", ExpectedResult = math.float3(12, 12, 12)},
            new TestCaseData(math.float4(12, 12, 12, 12), new Float4Sum()) {TestName = "Float4Sum", ExpectedResult = math.float4(12, 12, 12, 12)},

            new TestCaseData(12f, new FloatMin()) {TestName = "FloatMin", ExpectedResult = 12f},
            new TestCaseData(math.float2(12, 12), new Float2Min()) {TestName = "Float2Min", ExpectedResult = math.float2(12, 12)},
            new TestCaseData(math.float3(12, 12, 12), new Float3Min()) {TestName = "Float3Min", ExpectedResult = math.float3(12, 12, 12)},
            new TestCaseData(math.float4(12, 12, 12, 12), new Float4Min()) {TestName = "Float4Min", ExpectedResult = math.float4(12, 12, 12, 12)},

            new TestCaseData(12f, new FloatMax()) {TestName = "FloatMax", ExpectedResult = 12f},
            new TestCaseData(math.float2(12, 12), new Float2Max()) {TestName = "Float2Max", ExpectedResult = math.float2(12, 12)},
            new TestCaseData(math.float3(12, 12, 12), new Float3Max()) {TestName = "Float3Max", ExpectedResult = math.float3(12, 12, 12)},
            new TestCaseData(math.float4(12, 12, 12, 12), new Float4Max()) {TestName = "Float4Max", ExpectedResult = math.float4(12, 12, 12, 12)},

            // AABB
            new TestCaseData(new AABB(1, 2), new AABBUnion()) {TestName = "AABB Union", ExpectedResult = new AABB(1, 2)}
        };

        [Test, TestCaseSource(nameof(neutralElementOperationTestData))]
        public T NeutralElementOperationTest<T, Op>(T value, Op op)
            where T : unmanaged
            where Op : IAbelianOperator<T>
        {
            return op.Product(op.NeturalElement, value);
        }
    }
}