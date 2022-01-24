using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;

namespace andywiecko.BurstCollections
{
    /// <summary>
    /// An unmanaged, fixed capacity native stack.
    /// </summary>
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(NativeStackDebugView<>))]
    public struct NativeStack<T> : INativeDisposable where T : unmanaged
    {
        public bool IsCreated => buffer.IsCreated;
        public bool IsEmpty => buffer.IsEmpty;
        public int Length => buffer.Length;

        internal NativeList<T> buffer;

        public NativeStack(int capacity, Allocator allocator)
        {
            buffer = new NativeList<T>(capacity, allocator);
        }

        public void Clear() => buffer.Clear();
        public void Push(T item) => buffer.Add(item);
        public T Pop()
        {
            if (!TryPop(out var item))
            {
                ThrowEmpty();
            }

            return item;
        }
        public bool TryPop(out T item)
        {
            if (IsEmpty)
            {
                item = default;
                return false;
            }

            var id = buffer.Length - 1;
            item = buffer[id];
            buffer.RemoveAtSwapBack(id);
            return true;
        }
        public JobHandle Dispose(JobHandle dependencies) => buffer.Dispose(dependencies);
        public void Dispose() => buffer.Dispose();

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowEmpty()
        {
            throw new InvalidOperationException("Trying to pop from an empty stack.");
        }
    }
}