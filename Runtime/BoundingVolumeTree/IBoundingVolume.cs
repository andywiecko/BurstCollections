namespace andywiecko.BurstCollections
{
    public interface IBoundingVolume<TSelf> where TSelf : unmanaged
    {
        float Volume { get; }
        TSelf Union(TSelf other);
        bool Intersects(TSelf other);
    }
}
