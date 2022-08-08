using Unity.Mathematics;

namespace andywiecko.BurstCollections
{
    /// <summary>
    /// <b>M</b>inimum <b>B</b>ounding <b>C</b>ircle.
    /// </summary>
    /// <remarks>
    /// See <see href="https://en.wikipedia.org/wiki/Minimum_bounding_circle">wikipage</see>.
    /// </remarks>
    public readonly struct MBC : IBoundingVolume<MBC>
    {
        public float Volume => Radius * Radius * math.PI;
        public readonly float2 Position;
        public readonly float Radius;

        public MBC(float2 position, float radius) => (Position, Radius) = (position, radius);

        public bool Intersects(MBC other)
        {
            var r = Radius + other.Radius;
            return math.distancesq(Position, other.Position) <= r * r;
        }

        public MBC Union(MBC other)
        {
            var d = math.distance(Position, other.Position);
            if (d < math.abs(Radius - other.Radius))
            {
                return Radius > other.Radius ? this : other;
            }

            var theta = 0.5f + (Radius - other.Radius) / (2 * d);
            var p = (1 - theta) * other.Position + theta * Position;
            var r = 0.5f * (Radius + other.Radius + d);
            return new MBC(p, r);
        }
    }
}
