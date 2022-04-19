using Unity.Mathematics;

namespace andywiecko.BurstCollections
{
    /// <summary>
    /// <b>A</b>xis-<b>A</b>ligned <b>B</b>ounding <b>B</b>ox.
    /// </summary>
    /// <remarks>
    /// See <see href="https://en.wikipedia.org/wiki/Axis-aligned_bounding_box">wikipage.</see>
    /// </remarks>
    public readonly struct AABB : IBoundingVolume<AABB>
    {
        public readonly float Volume { get; }
        public readonly float2 Min;
        public readonly float2 Max;

        public AABB(float2 min, float2 max)
        {
            Min = min;
            Max = max;
            Volume = (Max - Min).x * (Max - Min).y;
        }
        public bool Contains(float2 point) => math.all(point >= Min & point <= Max);
        public bool Intersects(AABB other) => math.all(Max >= other.Min) && math.all(Min <= other.Max);
        public AABB Union(AABB other) => new AABB(math.min(Min, other.Min), math.max(Max, other.Max));
        public void Deconstruct(out float2 min, out float2 max) => _ = (min = Min, max = Max);
    }
}
