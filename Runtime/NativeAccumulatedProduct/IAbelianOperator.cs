namespace andywiecko.BurstCollections
{
    public interface IAbelianOperator<TSelf> where TSelf : unmanaged
    {
        TSelf Product(TSelf a, TSelf b);
        TSelf NeturalElement { get; }
    }
}
