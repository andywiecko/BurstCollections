using NUnit.Framework;
using Unity.Collections;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class RefEditorTests
    {
        [Test]
        public void RefValueTest()
        {
            using var array = new NativeArray<int>(64, Allocator.Persistent);
            var arrayRef = new Ref<NativeArray<int>>(array);
            Assert.That(arrayRef.Value, Is.EqualTo(array));
        }

        [Test]
        public void DisposeTest()
        {
            var array = new NativeArray<int>(64, Allocator.Persistent);
            var arrayRef = new Ref<NativeArray<int>>(array);

            arrayRef.Dispose();

            Assert.That(arrayRef.Value.IsCreated, Is.EqualTo(false));
        }

        [Test]
        public void ImplicitCastTest()
        {
            using var array = new NativeArray<int>(64, Allocator.Persistent);
            Ref<NativeArray<int>> arrayRef = array;
            Assert.That(arrayRef.Value, Is.EqualTo(array));
        }
    }
}