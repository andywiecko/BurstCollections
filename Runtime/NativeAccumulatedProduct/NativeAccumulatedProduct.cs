using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace andywiecko.BurstCollections
{
    /// <summary>
    /// Wrapper which allows for parallel abelian operations calculations between threads, 
    /// i.e. calculating the sum, min, max, etc. for the given array in parallel.
    /// </summary>
    /// <typeparam name="T">Type of operator elements.</typeparam>
    /// <typeparam name="Op">Type of abelian operator.</typeparam>
    public struct NativeAccumulatedProduct<T, Op> : INativeDisposable
        where T : unmanaged
        where Op : unmanaged, IAbelianOperator<T>
    {
        /// <summary>
        /// The accumulated result of the products.
        /// </summary>
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

            result = new NativeReference<T>(op.NeturalElement, allocator);
            tmp = new NativeArray<T>(maxThreadCount, allocator);
            Reset();
        }

        /// <summary>
        /// Accumulate the result of the products for all elements in the <paramref name="data"/>.
        /// </summary>
        public JobHandle AccumulateProducts(NativeArray<T>.ReadOnly data, int innerloopBatchCount, JobHandle dependencies)
        {
            return new AccumulateProductsJob(this, data).Schedule(data.Length, innerloopBatchCount, dependencies);
        }

        /// <summary>
        /// Accumulate <paramref name="element"/> product with the accumulated result.
        /// </summary>
        public void AccumulateProduct(T element)
        {
            tmp[threadIndex] = op.Product(tmp[threadIndex], element);
        }

        /// <summary>
        /// Reset threads temporary data to neutral elements.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = op.NeturalElement;
            }
        }

        /// <summary>
        /// Reset threads temporary data to neutral elements (jobified).
        /// </summary>
        public JobHandle Reset(JobHandle dependencies) => new ResetJob(this).Schedule(dependencies);

        /// <summary>
        /// Combine all threads temporary data to the final result.
        /// </summary>
        public JobHandle Combine(JobHandle dependencies = default) => new CombineJob(this).Schedule(dependencies);

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
            private NativeAccumulatedProduct<T, Op> product;

            public CombineJob(NativeAccumulatedProduct<T, Op> product)
            {
                this.product = product;
            }

            public void Execute()
            {
                var op = product.op;
                var result = op.NeturalElement;
                for (int i = 0; i < product.tmp.Length; i++)
                {
                    result = op.Product(result, product.tmp[i]);
                }
                product.result.Value = result;
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

            public void Execute() => product.Reset();
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

            public void Execute(int index) => product.AccumulateProduct(data[index]);
        }

        #endregion
    }
}