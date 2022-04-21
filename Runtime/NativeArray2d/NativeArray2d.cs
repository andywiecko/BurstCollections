using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace andywiecko.BurstCollections
{
    public struct NativeArray2d<T> : INativeDisposable, IEnumerable<T>, IEnumerable, IEquatable<NativeArray2d<T>>
        where T : unmanaged
    {
        public T this[int index] { get => array[index]; set => array[index] = value; }
        public T this[int row, int col]
        {
            get { CheckIndex(row, col, RowsCount, ColsCount); return this[RowColToIndex(row, col)]; }
            set { CheckIndex(row, col, RowsCount, ColsCount); this[RowColToIndex(row, col)] = value; }
        }
        public T this[int2 rowcol] { get => this[rowcol.x, rowcol.y]; set => this[rowcol.x, rowcol.y] = value; }
        public bool IsCreated => array.IsCreated;
        public readonly int Length => array.Length;
        public readonly int RowsCount { get; }
        public readonly int ColsCount { get; }

        internal NativeArray<T> array;

        public NativeArray2d(T[,] array, Allocator allocator) : this(array.GetLength(0), array.GetLength(1), allocator)
        {
            var tmp = array.Cast<T>().ToArray();
            this.array.CopyFrom(tmp);
        }

        public NativeArray2d(int rowsCount, int colsCount, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            RowsCount = rowsCount;
            ColsCount = colsCount;
            var length = colsCount * rowsCount;
            array = new NativeArray<T>(length, allocator, options);
        }

        public static int RowColToIndex(int i, int j, int colsCount) => i * colsCount + j;
        private int RowColToIndex(int i, int j) => RowColToIndex(i, j, ColsCount);
        public static int2 IndexToRowCol(int index, int colsCount) => new(index / colsCount, index % colsCount);
        private int2 IndexToRowCol(int index) => IndexToRowCol(index, ColsCount);

        public T[,] ToArray()
        {
            var tmp = new T[RowsCount, ColsCount];
            var index = 0;
            foreach (var el in array)
            {
                var ij = IndexToRowCol(index);
                tmp[ij.x, ij.y] = el;
                index++;
            }

            return tmp;
        }

        public JobHandle Dispose(JobHandle dependencies) => array.Dispose(dependencies);
        public void Dispose() => array.Dispose();

        public NativeArray<T>.Enumerator GetEnumerator() => array.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool Equals(NativeArray2d<T> other) => array.Equals(other.GetInnerArray());

        public NativeArray<T> GetInnerArray() => array;
        public NativeArray<T> GetRow(int rowId) => array.GetSubArray(start: rowId * ColsCount, length: ColsCount);

        public ReadOnly AsReadOnly() => new(this);
        public ReadOnlySpan<T> AsReadOnlySpan() => AsReadOnly().AsReadOnlySpan();
        unsafe public Span<T> AsSpan() => new(array.GetUnsafePtr(), Length);

        public static implicit operator Span<T>(NativeArray2d<T> array) => array.AsSpan();
        public static implicit operator ReadOnlySpan<T>(NativeArray2d<T> array) => array.AsReadOnlySpan();

        #region ReadOnly
        public struct ReadOnly
        {
            public T this[int index] => array[index];
            public T this[int row, int col]
            {
                get { CheckIndex(row, col, RowsCount, ColsCount); return this[RowColToIndex(row, col, ColsCount)]; }
            }
            public T this[int2 rowcol] => this[rowcol.x, rowcol.y];
            public int Length => array.Length;
            public readonly int RowsCount { get; }
            public readonly int ColsCount { get; }
            internal NativeArray<T>.ReadOnly array;
            internal ReadOnly(NativeArray2d<T> owner)
            {
                array = owner.GetInnerArray().AsReadOnly();
                RowsCount = owner.RowsCount;
                ColsCount = owner.ColsCount;
            }
            public NativeArray<T>.ReadOnly.Enumerator GetEnumerator() => array.GetEnumerator();
            unsafe public ReadOnlySpan<T> AsReadOnlySpan() => new(array.GetUnsafeReadOnlyPtr(), Length);
            public static implicit operator ReadOnlySpan<T>(ReadOnly array) => array.AsReadOnlySpan();
        }
        #endregion

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckIndex(int row, int col, int rowsCount, int colsCount)
        {
            if (row < 0 || row >= rowsCount || col < 0 || col >= colsCount)
            {
                throw new IndexOutOfRangeException($"{nameof(NativeArray2d<T>)}[{rowsCount}, {colsCount}]: index ({row}, {col}) is out of range!");
            }
        }
    }
}