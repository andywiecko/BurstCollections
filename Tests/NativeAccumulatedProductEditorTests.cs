using NUnit.Framework;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativeAccumulatedProductEditorTests
    {
        private static readonly TestCaseData[] accumulatedSumTestData = new[]
        {
            // Int
            new TestCaseData(1, 1024 * 1024, new IntSum()) {TestName = "Int", ExpectedResult = 1024 * 1024},
            new TestCaseData((int2)1, 1024 * 1024, new Int2Sum()) {TestName = "Int2", ExpectedResult = (int2)(1024 * 1024)},
            new TestCaseData((int3)1, 1024 * 1024, new Int3Sum()) {TestName = "Int3", ExpectedResult = (int3)(1024 * 1024)},
            new TestCaseData((int4)1, 1024 * 1024, new Int4Sum()) {TestName = "Int4", ExpectedResult = (int4)(1024 * 1024)},

            // Float
            new TestCaseData(1f, 1024 * 1024, new FloatSum()) {TestName = "Float", ExpectedResult = 1024 * 1024f},
            new TestCaseData((float2)1, 1024 * 1024, new Float2Sum()) {TestName = "Float2", ExpectedResult = (float2)(1024 * 1024)},
            new TestCaseData((float3)1, 1024 * 1024, new Float3Sum()) {TestName = "Float3", ExpectedResult = (float3)(1024 * 1024)},
            new TestCaseData((float4)1, 1024 * 1024, new Float4Sum()) {TestName = "Float4", ExpectedResult = (float4)(1024 * 1024)}
        };

        [Test, TestCaseSource(nameof(accumulatedSumTestData))]
        public T AccumulatedSumTest<T, Op>(T element, int size, Op _)
            where T : unmanaged
            where Op : unmanaged, IAbelianOperator<T>
        {
            var vals = Enumerable.Repeat(element, size).ToArray();

            using var array = new NativeArray<T>(vals, Allocator.Persistent);
            using var tmp = new NativeAccumulatedProduct<T, Op>(Allocator.Persistent);

            JobHandle dependencies = default;

            dependencies = tmp.AccumulateProducts(array.AsReadOnly(), innerloopBatchCount: 64, dependencies);
            dependencies = tmp.Combine(dependencies);

            dependencies.Complete();

            return tmp.Value;
        }

        private static readonly TestCaseData[] accumulatedMinMaxTestData = new[]
        {
            // Int
            new TestCaseData(1, 1024 * 1024, new IntMin()) {TestName = "IntMin", ExpectedResult = 0},
            new TestCaseData((int2)1, 1024 * 1024, new Int2Min()) {TestName = "Int2Min", ExpectedResult = (int2)0},
            new TestCaseData((int3)1, 1024 * 1024, new Int3Min()) {TestName = "Int3Min", ExpectedResult = (int3)0},
            new TestCaseData((int4)1, 1024 * 1024, new Int4Min()) {TestName = "Int4Min", ExpectedResult = (int4)0},
            new TestCaseData(-1, 1024 * 1024, new IntMax()) {TestName = "IntMax", ExpectedResult = 0},
            new TestCaseData(-(int2)1, 1024 * 1024, new Int2Max()) {TestName = "Int2Max", ExpectedResult = (int2)0},
            new TestCaseData(-(int3)1, 1024 * 1024, new Int3Max()) {TestName = "Int3Max", ExpectedResult = (int3)0},
            new TestCaseData(-(int4)1, 1024 * 1024, new Int4Max()) {TestName = "Int4Max", ExpectedResult = (int4)0},

            // Float
            new TestCaseData(1f, 1024 * 1024, new FloatMin()) {TestName = "FloatMin", ExpectedResult = 0f},
            new TestCaseData((float2)1, 1024 * 1024, new Float2Min()) {TestName = "Float2Min", ExpectedResult = (float2)0},
            new TestCaseData((float3)1, 1024 * 1024, new Float3Min()) {TestName = "Float3Min", ExpectedResult = (float3)0},
            new TestCaseData((float4)1, 1024 * 1024, new Float4Min()) {TestName = "Float4Min", ExpectedResult = (float4)0},
            new TestCaseData(-1f, 1024 * 1024, new FloatMax()) {TestName = "FloatMax", ExpectedResult = 0f},
            new TestCaseData(-(float2)1, 1024 * 1024, new Float2Max()) {TestName = "Float2Max", ExpectedResult = (float2)0},
            new TestCaseData(-(float3)1, 1024 * 1024, new Float3Max()) {TestName = "Float3Max", ExpectedResult = (float3)0},
            new TestCaseData(-(float4)1, 1024 * 1024, new Float4Max()) {TestName = "Float4Max", ExpectedResult = (float4)0}
        };

        [Test, TestCaseSource(nameof(accumulatedMinMaxTestData))]
        public T AccumulatedMinMaxTest<T, Op>(T element, int size, Op _)
            where T : unmanaged
            where Op : unmanaged, IAbelianOperator<T>
        {
            var vals = Enumerable.Repeat(element, size).ToArray();
            vals[0] = default;

            using var array = new NativeArray<T>(vals, Allocator.Persistent);
            using var tmp = new NativeAccumulatedProduct<T, Op>(Allocator.Persistent);

            JobHandle dependencies = default;

            dependencies = tmp.AccumulateProducts(array.AsReadOnly(), innerloopBatchCount: 64, dependencies);
            dependencies = tmp.Combine(dependencies);

            dependencies.Complete();

            return tmp.Value;
        }
    }
}