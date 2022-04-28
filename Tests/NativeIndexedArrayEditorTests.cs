using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativeIndexedArrayEditorTests
    {
        private struct Fake { }

        [Test]
        public void IndexOperatorTest()
        {
            var array = new NativeIndexedArray<Id<Fake>, int>(new[] { 0, 1, 2, 3, 4 }, Allocator.Persistent);
            array[(Id<Fake>)0] = 1;
            array[(Id<Fake>)4] = 42;

            Assert.That(array, Is.EqualTo(new[] { 1, 1, 2, 3, 42 }));

            array.Dispose();
        }

        [BurstCompile]
        private struct ReadOnlyJob : IJob
        {
            private NativeIndexedArray<Id<Fake>, int>.ReadOnly data;
            public ReadOnlyJob(NativeIndexedArray<Id<Fake>, int> data) => this.data = data.AsReadOnly();
            public void Execute() => _ = default(int);
        }

        [Test]
        public void ReadOnlyTest()
        {
            using var array = new NativeIndexedArray<Id<Fake>, int>(new[] { 0, 1, 2, 3, 4 }, Allocator.Persistent);
            new ReadOnlyJob(array).Schedule(default).Complete();
        }

        [Test]
        public void ElementAtTest()
        {
            using var array = new NativeIndexedArray<Id<Fake>, int>(new[] { 0, 1, 2, 3, 4 }, Allocator.Persistent);
            array.ElementAt((Id<Fake>)2) = 42;
            Assert.That(array, Is.EqualTo(new[] { 0, 1, 42, 3, 4 }));
        }
    }
}