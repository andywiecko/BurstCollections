using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace andywiecko.BurstCollections
{
    /// <summary>
    /// Wrapper for <see cref="NativeList{T}"/> which supports indexing via <see cref="Id{T}"/> instead of <see langword="int"/>.
    /// </summary>
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(NativeIndexedListDebugView<,>))]
    public struct NativeIndexedList<Id, T> : INativeDisposable, IEnumerable<T>
        where T : unmanaged
        where Id : unmanaged, IIndexer
    {
        private NativeList<T> list;

        public T this[Id index] { get => list[index.Value]; set => list[index.Value] = value; }
        public int Capacity { get => list.Capacity; set => list.Capacity = value; }
        public bool IsEmpty => list.IsEmpty;
        public int Length { get => list.Length; set => list.Length = value; }

        public NativeIndexedList(AllocatorManager.AllocatorHandle allocator) : this(1, allocator) { }
        public NativeIndexedList(int initialCapacity, AllocatorManager.AllocatorHandle allocator) =>
            list = new NativeList<T>(initialCapacity, allocator);

        public void Clear() => list.Clear();
        public ref T ElementAt(int index) => ref list.ElementAt(index);
        public JobHandle Dispose(JobHandle dependency) => list.Dispose(dependency);
        public void Dispose() => list.Dispose();
        public void AddNoResize(T value) => list.AddNoResize(value);
        public unsafe void AddRangeNoResize(void* ptr, int count) => list.AddRangeNoResize(ptr, count);
        public void AddRangeNoResize(NativeList<T> list) => this.list.AddRangeNoResize(list);
        public void Add(in T value) => list.Add(value);
        public void AddRange(NativeArray<T> array) => list.AddRange(array);
        public unsafe void AddRange(void* ptr, int count) => list.AddRange(ptr, count);
        public void InsertRangeWithBeginEnd(int begin, int end) => list.InsertRangeWithBeginEnd(begin, end);
        public void RemoveAtSwapBack(int index) => list.RemoveAtSwapBack(index);
        public void RemoveRangeSwapBack(int index, int count) => list.RemoveRangeSwapBack(index, count);
        public void RemoveAt(int index) => list.RemoveAt(index);
        public void RemoveRange(int index, int count) => list.RemoveRange(index, count);
        public NativeIndexedArray<Id, T> AsArray() => new() { array = list.AsArray() };
        public NativeIndexedArray<Id, T> AsDeferredJobArray() => new() { array = list.AsDeferredJobArray() };
        public T[] ToArray() => list.ToArray();
        public NativeIndexedArray<Id, T> ToArray(AllocatorManager.AllocatorHandle allocator) => new() { array = list.ToArray(allocator) };
        public NativeArray<T>.Enumerator GetEnumerator() => list.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => (list as IEnumerable<T>).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (list as IEnumerable).GetEnumerator();

        public void CopyFrom(NativeArray<T> array) => list.CopyFrom(array);
        public void Resize(int length, NativeArrayOptions options) => list.Resize(length, options);
        public void ResizeUninitialized(int length) => list.ResizeUninitialized(length);
        public NativeIndexedArray<Id, T>.ReadOnly AsParallelReader() => new() { array = list.AsParallelReader() };
        public ParallelWriter AsParallelWriter() => new(this);
        unsafe public ReadOnlySpan<T> AsReadOnlySpan() => new(list.GetUnsafeReadOnlyPtr(), Length);
        unsafe public Span<T> AsSpan() => new(list.GetUnsafePtr(), Length);
        public IdEnumerator<Id> Ids => new(start: 0, Length);
        public IdValueEnumerator<Id, T> IdsValues => new(AsReadOnlySpan());
        public static implicit operator Span<T>(NativeIndexedList<Id, T> array) => array.AsSpan();
        public static implicit operator ReadOnlySpan<T>(NativeIndexedList<Id, T> array) => array.AsReadOnlySpan();

        public struct ParallelWriter
        {
            private NativeList<T>.ParallelWriter list;

            public ParallelWriter(NativeIndexedList<Id, T> nativeIndexedList) =>
                list = nativeIndexedList.list.AsParallelWriter();

            public void AddNoResize(T value) => list.AddNoResize(value);
            public unsafe void AddRangeNoResize(void* ptr, int count) => list.AddRangeNoResize(ptr, count);
            public void AddRangeNoResize(UnsafeList<T> list) => this.list.AddRangeNoResize(list);
            public void AddRangeNoResize(NativeList<T> list) => list.AddRangeNoResize(list);
        }
    }
}