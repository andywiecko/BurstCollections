using System;

namespace andywiecko.BurstCollections
{
    public struct IdEnumerator<Id> where Id : unmanaged, IIndexer
    {
        private int current;
        private readonly int start;
        private readonly int length;

        public IdEnumerator(int start, int length)
        {
            this.start = start;
            this.current = -1;
            this.length = length;
        }

        public Id Current => InternalUtility.AsId<Id>(current + start);
        public bool MoveNext() => ++current < length;
        public IdEnumerator<Id> GetEnumerator() => this;
    }

    public ref struct IdValueEnumerator<Id, T>
        where Id : unmanaged, IIndexer
        where T : unmanaged
    {
        private ReadOnlySpan<T> data;
        private int current;

        public IdValueEnumerator(ReadOnlySpan<T> data)
        {
            this.data = data;
            current = -1;
        }

        public (Id, T) Current => (InternalUtility.AsId<Id>(current), data[current]);
        public bool MoveNext() => ++current < data.Length;
        public IdValueEnumerator<Id, T> GetEnumerator() => this;
    }
}