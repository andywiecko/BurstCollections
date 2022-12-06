using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace andywiecko.BurstCollections
{
    public struct NativeStackedLists<T> :
        INativeDisposable,
        IEnumerable<IEnumerable<T>>
            where T : unmanaged
    {
        private struct List
        {
            public int Start { get; set; }
            public int Length { get; set; }
            public List(int start, int length) => (Start, Length) = (start, length);
            public static List operator ++(List @this) => new(@this.Start, @this.Length + 1);
        }

        public struct Enumerator
        {
            private NativeStackedLists<T> owner;
            private int i;
            public Enumerator(NativeStackedLists<T> owner) => (this.owner, i) = (owner, -1);
            public NativeArray<T> Current => owner[i];
            public bool MoveNext() => ++i < owner.stack.Length;
            public Enumerator GetEnumerator() => this;
        }

        public int Length => stack.Length;
        public NativeArray<T> this[int i] => elements.AsArray().GetSubArray(stack[i].Start, stack[i].Length);

        private NativeList<List> stack;
        private NativeList<T> elements;

        public NativeStackedLists(int elementsInitialCapacity, int stackInitialCapacity, Allocator allocator)
        {
            elements = new(elementsInitialCapacity, allocator);
            stack = new(stackInitialCapacity, allocator);
        }
        public NativeStackedLists(int elementsInitialCapacity, Allocator allocator) : this(elementsInitialCapacity, stackInitialCapacity: 1, allocator) { }
        public NativeStackedLists(Allocator allocator) : this(elementsInitialCapacity: 1, stackInitialCapacity: 1, allocator) { }

        public void Push() => stack.Add(new() { Start = elements.Length });

        public void Add(T element)
        {
            elements.Add(element);
            ++stack[^1];
        }

        public void Clear()
        {
            stack.Clear();
            elements.Clear();
        }

        public JobHandle Dispose(JobHandle dependencies)
        {
            dependencies = stack.Dispose(dependencies);
            dependencies = elements.Dispose(dependencies);
            return dependencies;
        }

        public void Dispose()
        {
            stack.Dispose();
            elements.Dispose();
        }

        public Enumerator GetEnumerator() => new(this);

        IEnumerator<IEnumerable<T>> IEnumerable<IEnumerable<T>>.GetEnumerator()
        {
            foreach (var list in this)
            {
                yield return list;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<IEnumerable<T>>).GetEnumerator();
    }
}