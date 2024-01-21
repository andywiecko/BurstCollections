namespace andywiecko.BurstCollections
{
    internal sealed class NativeStackDebugView<T> where T : unmanaged
    {
        public T[] Items => stack.buffer.AsArray().ToArray();
        private NativeStack<T> stack;
        public NativeStackDebugView(NativeStack<T> stack) => this.stack = stack;
    }
}
