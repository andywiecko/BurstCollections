using Unity.Mathematics;

namespace andywiecko.BurstCollections
{
    #region Float
    public readonly struct FloatSum : IAbelianOperator<float>
    {
        public float NeturalElement => 0;
        public float Product(float a, float b) => a + b;
    }

    public readonly struct FloatMin : IAbelianOperator<float>
    {
        public float NeturalElement => float.MaxValue;
        public float Product(float a, float b) => math.min(a, b);
    }

    public readonly struct FloatMax : IAbelianOperator<float>
    {
        public float NeturalElement => float.MinValue;
        public float Product(float a, float b) => math.max(a, b);
    }
    #endregion

    #region Float2
    public readonly struct Float2Sum : IAbelianOperator<float2>
    {
        public float2 NeturalElement => 0;
        public float2 Product(float2 a, float2 b) => a + b;
    }

    public readonly struct Float2Min : IAbelianOperator<float2>
    {
        public float2 NeturalElement => float.MaxValue;
        public float2 Product(float2 a, float2 b) => math.min(a, b);
    }

    public readonly struct Float2Max : IAbelianOperator<float2>
    {
        public float2 NeturalElement => float.MinValue;
        public float2 Product(float2 a, float2 b) => math.max(a, b);
    }
    #endregion

    #region Float3
    public readonly struct Float3Sum : IAbelianOperator<float3>
    {
        public float3 NeturalElement => 0;
        public float3 Product(float3 a, float3 b) => a + b;
    }

    public readonly struct Float3Min : IAbelianOperator<float3>
    {
        public float3 NeturalElement => float.MaxValue;
        public float3 Product(float3 a, float3 b) => math.min(a, b);
    }

    public readonly struct Float3Max : IAbelianOperator<float3>
    {
        public float3 NeturalElement => float.MinValue;
        public float3 Product(float3 a, float3 b) => math.max(a, b);
    }
    #endregion

    #region Float4
    public readonly struct Float4Sum : IAbelianOperator<float4>
    {
        public float4 NeturalElement => 0;
        public float4 Product(float4 a, float4 b) => a + b;
    }

    public readonly struct Float4Min : IAbelianOperator<float4>
    {
        public float4 NeturalElement => float.MaxValue;
        public float4 Product(float4 a, float4 b) => math.min(a, b);
    }

    public readonly struct Float4Max : IAbelianOperator<float4>
    {
        public float4 NeturalElement => float.MinValue;
        public float4 Product(float4 a, float4 b) => math.max(a, b);
    }
    #endregion
}