namespace andywiecko.BurstCollections
{
    internal sealed class NativeIndexedArrayDebugView<Id, T>
        where Id : unmanaged, IIndexer
        where T : unmanaged
    {
        public T[] Items => array.ToArray();
        private NativeIndexedArray<Id, T> array;
        public NativeIndexedArrayDebugView(NativeIndexedArray<Id, T> array) => this.array = array;
    }

    internal sealed class NativeIndexedArrayReadOnlyDebugView<Id, T>
        where Id : unmanaged, IIndexer
        where T : unmanaged
    {
        public T[] Items => array.ToArray();
        private NativeIndexedArray<Id, T>.ReadOnly array;
        public NativeIndexedArrayReadOnlyDebugView(NativeIndexedArray<Id, T>.ReadOnly array) => this.array = array;
    }

    internal sealed class NativeIndexedListDebugView<Id, T>
        where Id : unmanaged, IIndexer
        where T : unmanaged
    {
        public T[] Items => list.ToArray();
        private NativeIndexedList<Id, T> list;
        public NativeIndexedListDebugView(NativeIndexedList<Id, T> list) => this.list = list;
    }
}