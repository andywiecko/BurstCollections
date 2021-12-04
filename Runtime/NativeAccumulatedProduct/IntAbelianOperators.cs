using Unity.Mathematics;

namespace andywiecko.BurstCollections
{
    #region Int
    public readonly struct IntSum : IAbelianOperator<int>
    {
        public int NeturalElement => 0;
        public int Product(int a, int b) => a + b;
    }

    public readonly struct IntMin : IAbelianOperator<int>
    {
        public int NeturalElement => int.MaxValue;
        public int Product(int a, int b) => math.min(a, b);
    }

    public readonly struct IntMax : IAbelianOperator<int>
    {
        public int NeturalElement => int.MinValue;
        public int Product(int a, int b) => math.max(a, b);
    }
    #endregion

    #region Int2
    public readonly struct Int2Sum : IAbelianOperator<int2>
    {
        public int2 NeturalElement => 0;
        public int2 Product(int2 a, int2 b) => a + b;
    }

    public readonly struct Int2Min : IAbelianOperator<int2>
    {
        public int2 NeturalElement => int.MaxValue;
        public int2 Product(int2 a, int2 b) => math.min(a, b);
    }

    public readonly struct Int2Max : IAbelianOperator<int2>
    {
        public int2 NeturalElement => int.MinValue;
        public int2 Product(int2 a, int2 b) => math.max(a, b);
    }
    #endregion

    #region Int3
    public readonly struct Int3Sum : IAbelianOperator<int3>
    {
        public int3 NeturalElement => 0;
        public int3 Product(int3 a, int3 b) => a + b;
    }

    public readonly struct Int3Min : IAbelianOperator<int3>
    {
        public int3 NeturalElement => int.MaxValue;
        public int3 Product(int3 a, int3 b) => math.min(a, b);
    }

    public readonly struct Int3Max : IAbelianOperator<int3>
    {
        public int3 NeturalElement => int.MinValue;
        public int3 Product(int3 a, int3 b) => math.max(a, b);
    }
    #endregion

    #region Int4
    public readonly struct Int4Sum : IAbelianOperator<int4>
    {
        public int4 NeturalElement => 0;
        public int4 Product(int4 a, int4 b) => a + b;
    }

    public readonly struct Int4Min : IAbelianOperator<int4>
    {
        public int4 NeturalElement => int.MaxValue;
        public int4 Product(int4 a, int4 b) => math.min(a, b);
    }

    public readonly struct Int4Max : IAbelianOperator<int4>
    {
        public int4 NeturalElement => int.MinValue;
        public int4 Product(int4 a, int4 b) => math.max(a, b);
    }
    #endregion
}
