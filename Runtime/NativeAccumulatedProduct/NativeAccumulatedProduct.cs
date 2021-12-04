using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace andywiecko.BurstCollections
{
    public struct NativeAccumulatedProduct<T, Op> : INativeDisposable
        where T : unmanaged
        where Op : unmanaged, IAbelianOperator<T>
    {
        public T Value => result.Value;

        [NativeSetThreadIndex] private int threadIndex;

        private readonly int maxThreadCount;
        private readonly Op op;

        [NativeDisableParallelForRestriction]
        private NativeArray<T> tmp;
        [NativeDisableContainerSafetyRestriction]
        private NativeReference<T> result;

        public NativeAccumulatedProduct(Allocator allocator)
        {
            threadIndex = default;
            op = default;

            maxThreadCount = JobsUtility.JobWorkerMaximumCount + 2;
            //maxThreadCount = JobsUtility.MaxJobThreadCount;

            result = new NativeReference<T>(op.NeturalElement, allocator);
            tmp = new NativeArray<T>(maxThreadCount, allocator);
            Reset();
        }

        public JobHandle AccumulateProducts(NativeArray<T>.ReadOnly data, int innerloopBatchCount, JobHandle dependencies)
        {
            return new AccumulateProductsJob(this, data).Schedule(data.Length, innerloopBatchCount, dependencies);
        }

        public void AccumulateProduct(T element)
        {
            tmp[threadIndex] = op.Product(tmp[threadIndex], element);
        }

        public void Reset()
        {
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = op.NeturalElement;
            }
        }

        public JobHandle Reset(JobHandle dependencies) => new ResetJob(this).Schedule(dependencies);

        public JobHandle Combine(JobHandle dependencies) => new CombineJob(this).Schedule(dependencies);

        public JobHandle Dispose(JobHandle dependencies)
        {
            dependencies = tmp.Dispose(dependencies);
            dependencies = result.Dispose(dependencies);

            return dependencies;
        }

        public void Dispose()
        {
            tmp.Dispose();
            result.Dispose();
        }

        #region Jobs
        [BurstCompile]
        private struct CombineJob : IJob
        {
            private NativeAccumulatedProduct<T, Op> output;

            public CombineJob(NativeAccumulatedProduct<T, Op> output)
            {
                this.output = output;
            }

            public void Execute()
            {
                var op = output.op;
                var result = op.NeturalElement;
                for (int i = 0; i < output.tmp.Length; i++)
                {
                    result = op.Product(result, output.tmp[i]);
                }
                output.result.Value = result;
            }
        }

        [BurstCompile]
        private struct ResetJob : IJob
        {
            private NativeAccumulatedProduct<T, Op> product;

            public ResetJob(NativeAccumulatedProduct<T, Op> product)
            {
                this.product = product;
            }

            public void Execute()
            {
                product.Reset();
            }
        }

        [BurstCompile]
        private struct AccumulateProductsJob : IJobParallelFor
        {
            private NativeAccumulatedProduct<T, Op> product;
            private NativeArray<T>.ReadOnly data;

            public AccumulateProductsJob(NativeAccumulatedProduct<T, Op> product, NativeArray<T>.ReadOnly data)
            {
                this.product = product;
                this.data = data;
            }

            public void Execute(int index)
            {
                product.AccumulateProduct(data[index]);
            }
        }

        #endregion
    }
}