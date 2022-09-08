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
    /// Wrapper for <see cref="NativeArray{T}"/> which supports indexing via <see cref="Id{T}"/> instead of <see langword="int"/>.
    /// </summary>
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(NativeIndexedArrayDebugView<,>))]
    public struct NativeIndexedArray<Id, T> : INativeDisposable, IEnumerable<T>, IEnumerable, IEquatable<NativeIndexedArray<Id, T>>
        where Id : unmanaged, IIndexer
        where T : unmanaged
    {
        public T this[Id index] { get => array[index.Value]; set => array[index.Value] = value; }
        public bool IsCreated => array.IsCreated;
        public int Length => array.Length;

        internal NativeArray<T> array;

        public NativeIndexedArray(T[] array, Allocator allocator) : this(array.Length, allocator)
        {
            this.array.CopyFrom(array);
        }

        public NativeIndexedArray(int length, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            array = new NativeArray<T>(length, allocator, options);
        }

        public void Dispose() => array.Dispose();
        public JobHandle Dispose(JobHandle dependencies) => array.Dispose(dependencies);

        public NativeArray<T>.Enumerator GetEnumerator() => array.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(NativeIndexedArray<Id, T> other) => array.Equals(other.GetInnerArray());

        public NativeArray<T> GetInnerArray() => array;
        public ReadOnly AsReadOnly() => new(this);
        public ReadOnlySpan<T> AsReadOnlySpan() => array.AsReadOnlySpan();
        unsafe public Span<T> AsSpan() => array.AsSpan();
        public T[] ToArray() => array.ToArray();
        public NativeIndexedArray<Id, U> Reinterpret<U>() where U : unmanaged => new() { array = array.Reinterpret<U>() };
        public NativeIndexedArray<NewId, U> Reinterpret<NewId, U>() where NewId : unmanaged, IIndexer where U : unmanaged =>
            new() { array = array.Reinterpret<U>() };
        public NativeIndexedArray<NewId, T> ReinterpretId<NewId>() where NewId : unmanaged, IIndexer => new() { array = array };
        unsafe public ref T ElementAt(Id id) => ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), id.Value);

        public IdEnumerator<Id> Ids => AsReadOnly().Ids;
        public IdValueEnumerator<Id, T> IdsValues => AsReadOnly().IdsValues;

        public static implicit operator Span<T>(NativeIndexedArray<Id, T> array) => array.AsSpan();
        public static implicit operator ReadOnlySpan<T>(NativeIndexedArray<Id, T> array) => array.AsReadOnlySpan();

        #region ReadOnly
        [DebuggerDisplay("Length = {Length}")]
        [DebuggerTypeProxy(typeof(NativeIndexedArrayReadOnlyDebugView<,>))]
        public struct ReadOnly
        {
            public T this[Id index] => array[index.Value];
            public int Length => array.Length;

            internal NativeArray<T>.ReadOnly array;
            internal ReadOnly(NativeIndexedArray<Id, T> owner) => array = owner.GetInnerArray().AsReadOnly();

            public void CopyTo(T[] array) => this.array.CopyTo(array);
            public void CopyTo(NativeArray<T> array) => this.array.CopyTo(array);
            public NativeIndexedArray<Id, U>.ReadOnly Reinterpret<U>() where U : unmanaged => new() { array = array.Reinterpret<U>() };
            public T[] ToArray() => array.ToArray();
            public NativeArray<T>.ReadOnly.Enumerator GetEnumerator() => array.GetEnumerator();
            unsafe public ReadOnlySpan<T> AsReadOnlySpan() => array.AsReadOnlySpan();
            public IdEnumerator<Id> Ids => new(start: 0, Length);
            public IdValueEnumerator<Id, T> IdsValues => new(AsReadOnlySpan());
            public static implicit operator ReadOnlySpan<T>(ReadOnly array) => array.AsReadOnlySpan();
        }
        #endregion
    }
}