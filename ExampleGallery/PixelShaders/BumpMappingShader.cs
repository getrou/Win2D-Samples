using ComputeSharp;
using ComputeSharp.D2D1;

namespace ExampleGallery.PixelShaders;

/// <summary>
/// A pixel shader implementing bump mapping for a height map.
/// </summary>
[D2DInputCount(1)]
[D2DInputComplex(0)]
[D2DShaderProfile(D2D1ShaderProfile.PixelShader40)]
[D2DRequiresScenePosition]
public readonly partial struct BumpMappingShader : ID2D1PixelShader
{
    /// <inheritdoc/>
    public float4 Execute()
    {
        // Sample pixels on the edges around the current one
        float left = D2D.SampleInputAtOffset(0, new float2(-1, 0)).X;
        float top = D2D.SampleInputAtOffset(0, new float2(0, -1)).X;
        float right = D2D.SampleInputAtOffset(0, new float2(1, 0)).X;
        float bottom = D2D.SampleInputAtOffset(0, new float2(0, 1)).X;

        // Compute the normalized delta on each axis
        float3 horizontal = Hlsl.Normalize(new float3(1, 0, right - left));
        float3 vertical = Hlsl.Normalize(new float3(0, 1, bottom - top));

        // Calculate the cross product to get the approximate normal vector
        float3 normal = Hlsl.Cross(horizontal, vertical);

        return new(normal, 1);
    }
}