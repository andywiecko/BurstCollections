namespace andywiecko.BurstCollections
{
    public readonly struct AABBUnion : IAbelianOperator<AABB>
    {
        public AABB NeturalElement => new AABB(min: float.MaxValue, max: float.MinValue);
        public AABB Product(AABB a, AABB b) => a.Union(b);
    }
}