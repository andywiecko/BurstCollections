using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.BurstCollections.Editor.Tests
{
    public class NativeArray2dEditorTests
    {
        [Test]
        public void RowsColsCountTest()
        {
            var managed = new int[4, 3];
            using var array = new NativeArray2d<int>(managed, Allocator.Persistent);

            Assert.That(array, Has.Length.EqualTo(12));
            Assert.That(array.RowsCount, Is.EqualTo(4));
            Assert.That(array.ColsCount, Is.EqualTo(3));
        }

        [Test]
        public void ManagedArrayConstructorTest()
        {
            int[,] managed =
            {
                {0, 1, 2 },
                {3, 4, 5 },
                {6, 7, 8 },
                {9, 10, 11 }
            };
            using var array = new NativeArray2d<int>(managed, Allocator.Persistent);

            Assert.That(array.GetInnerArray(), Is.EqualTo(managed));
        }

        [Test]
        public void ToArrayTest()
        {
            int[,] managed =
            {
                {0, 1, 2 },
                {3, 4, 5 },
                {6, 7, 8 },
                {9, 10, 11 }
            };
            using var array = new NativeArray2d<int>(managed, Allocator.Persistent);

            Assert.That(array.ToArray(), Is.EqualTo(managed));
        }

        [Test]
        public void GetRowTest()
        {
            int[,] managed =
            {
                {0, 1, 2 },
                {3, 4, 5 },
                {6, 7, 8 },
                {9, 10, 11 }
            };
            using var array = new NativeArray2d<int>(managed, Allocator.Persistent);

            Assert.That(array.GetRow(2), Is.EqualTo(new[] { 6, 7, 8 }));
        }

        [BurstCompile]
        private struct TestLengthJob : IJobParallelFor
        {
            private NativeArray2d<int> array;
            public TestLengthJob(NativeArray2d<int> array) => this.array = array;
            public void Execute(int i) => array[i] = i;
        }

        [Test]
        public void ParallelForLengthTest()
        {
            using var array = new NativeArray2d<int>(64, 64, Allocator.Persistent);
            new TestLengthJob(array).Schedule(array.Length, innerloopBatchCount: 64, default).Complete();
            Assert.That(array, Is.EqualTo(Enumerable.Range(0, 64 * 64)));
        }

        [BurstCompile]
        private struct TestRowsJob : IJobParallelFor
        {
            private NativeArray2d<int> array;
            public TestRowsJob(NativeArray2d<int> array) => this.array = array;
            public void Execute(int rowId)
            {
                var row = array.GetRow(rowId);
                for (int i = 0; i < row.Length; i++)
                {
                    row[i] = rowId;
                }
            }
        }

        [Test]
        public void ParallelForRowsTest()
        {
            using var array = new NativeArray2d<int>(64, 64, Allocator.Persistent);
            new TestRowsJob(array).Schedule(array.RowsCount, innerloopBatchCount: 1, default).Complete();
            Assert.That(array, Is.EqualTo(Enumerable.Range(0, 64).SelectMany(i => Enumerable.Repeat(i, 64))));
        }

        [BurstCompile]
        private struct TestReadOnlyJob : IJobParallelFor
        {
            private NativeArray2d<int>.ReadOnly array1;
            private NativeArray2d<int> array2;
            public TestReadOnlyJob(NativeArray2d<int> array1, NativeArray2d<int> array2)
            {
                this.array1 = array1.AsReadOnly();
                this.array2 = array2;
            }
            public void Execute(int i) => array2[i] = array1[0];
        }

        [Test]
        public void ReadOnlyTest()
        {
            using var array1 = new NativeArray2d<int>(new[,] { { 42 } }, Allocator.Persistent);
            using var array2 = new NativeArray2d<int>(64, 64, Allocator.Persistent);
            new TestReadOnlyJob(array1, array2).Schedule(64 * 64, innerloopBatchCount: 64, default).Complete();
            Assert.That(array2, Is.EqualTo(Enumerable.Repeat(element: 42, count: 64 * 64)));
        }

        [Test]
        public void ThrowIndexOutOfRange()
        {
            var array = new NativeArray2d<int>(32, 16, Allocator.Persistent);
            Assert.Throws<IndexOutOfRangeException>(() => array[32, 4] = 1);
            array.Dispose();
        }

        [Test]
        public void ThrowIndexOutOfRangeReadOnly()
        {
            var array = new NativeArray2d<int>(32, 16, Allocator.Persistent);
            Assert.Throws<IndexOutOfRangeException>(() => _ = array.AsReadOnly()[32, 4]);
            array.Dispose();
        }

        [Test]
        public void AccessOperatorsTest()
        {
            int[,] managed =
            {
                {0, 1, 2 },
                {3, 4, 5 },
                {6, 7, 8 },
                {9, 10, 11 }
            };

            var array = new NativeArray2d<int>(managed, Allocator.Persistent);

            Assert.That(array[11], Is.EqualTo(11));
            Assert.That(array[0, 0] = 100, Is.EqualTo(100));
            Assert.That(array[2, 1], Is.EqualTo(7));
            Assert.That(array[math.int2(1, 2)], Is.EqualTo(5));

            array.Dispose();
        }

        [Test]
        public void EnumeratorTest()
        {
            int[,] managed =
            {
                {0, 1, 2 },
                {3, 4, 5 },
                {6, 7, 8 },
                {9, 10, 11 }
            };

            using var array = new NativeArray2d<int>(managed, Allocator.Persistent);

            Assert.That(array as IEnumerable, Is.EqualTo(managed));
            Assert.That(array as IEnumerable<int>, Is.EqualTo(managed));
        }
    }
}