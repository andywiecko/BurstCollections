using NUnit.Framework;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativeIndexedListEditorTests
    {
        private readonly struct Fake { }

        [Test]
        public void DefaultAddTest()
        {
            using var list = new NativeIndexedList<Id<Fake>, int>(initialCapacity: 10, Allocator.Persistent);

            list.Add(0);
            list.Add(2);
            list.Add(3);

            Assert.That(list.ToArray(), Is.EqualTo(new[] { 0, 2, 3 }));
        }

        [Test]
        public void IndexOperatorTest()
        {
            using var list = new NativeIndexedList<Id<Fake>, int>(initialCapacity: 10, Allocator.Persistent);

            list.Add(0);
            list.Add(1);
            list.Add(2);

            Assert.That(list[(Id<Fake>)0], Is.EqualTo(0));
            Assert.That(list[(Id<Fake>)1], Is.EqualTo(1));
            Assert.That(list[(Id<Fake>)2], Is.EqualTo(2));
        }

        [BurstCompile]
        private struct ParallelWriterTestJob : IJobParallelFor
        {
            public NativeIndexedList<Id<Fake>, int>.ParallelWriter List;

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(arrayLength: 128, innerloopBatchCount: 16, dependencies);
            }

            public void Execute(int index) => List.AddNoResize(index);
        }

        [Test]
        public void ParallelWriterTest()
        {
            using var list = new NativeIndexedList<Id<Fake>, int>(initialCapacity: 1024, Allocator.Persistent);
            new ParallelWriterTestJob() { List = list.AsParallelWriter() }.Schedule(default).Complete();
            Assert.That(list.ToArray(), Is.EquivalentTo(Enumerable.Range(0, 128)));
        }

        [BurstCompile]
        private struct ParallelReaderTestJob : IJobParallelFor
        {
            public NativeIndexedArray<Id<Fake>, int>.ReadOnly Read;
            public NativeIndexedList<Id<Fake>, int>.ParallelWriter Write;

            public JobHandle Schedule(JobHandle dependencies)
            {
                return this.Schedule(arrayLength: 32, innerloopBatchCount: 1, dependencies);
            }

            public void Execute(int index) => Write.AddNoResize(Read[(Id<Fake>)index]);
        }

        [Test]
        public void ParallelReaderTest()
        {
            using var read = new NativeIndexedList<Id<Fake>, int>(initialCapacity: 1024, Allocator.Persistent);
            using var write = new NativeIndexedList<Id<Fake>, int>(initialCapacity: 1024, Allocator.Persistent);

            foreach (var i in Enumerable.Range(0, 32)) read.Add(i);

            new ParallelReaderTestJob() { Read = read.AsParallelReader(), Write = write.AsParallelWriter() }.Schedule(default).Complete();

            Assert.That(write.ToArray(), Is.EquivalentTo(read.ToArray()));
        }

        [BurstCompile]
        private struct DeferredArrayTestJob1 : IJob
        {
            public NativeIndexedList<Id<Fake>, int> List;

            public void Execute()
            {
                for (int i = 0; i < 128; i++)
                {
                    List.Add(i);
                }
            }
        }

        [BurstCompile]
        private struct DeferredArrayTestJob2 : IJob
        {
            public NativeIndexedArray<Id<Fake>, int> DeferredArray;
            public NativeArray<int> Array;

            public void Execute()
            {
                var i = 0;
                foreach (var item in DeferredArray)
                {
                    Array[i] = item;
                    i++;
                }
            }
        }

        [Test]
        public void DeferredArrayTest()
        {
            using var list = new NativeIndexedList<Id<Fake>, int>(128, Allocator.Persistent);
            using var array = new NativeArray<int>(128, Allocator.Persistent);

            JobHandle dependencies = default;

            dependencies = new DeferredArrayTestJob1() { List = list }.Schedule(dependencies);
            dependencies = new DeferredArrayTestJob2() { DeferredArray = list.AsDeferredJobArray(), Array = array }.Schedule(dependencies);
            dependencies.Complete();

            Assert.That(array.ToArray(), Is.EqualTo(Enumerable.Range(0, 128)));
        }
    }
}
