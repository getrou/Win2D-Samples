using ComputeSharp;
using ComputeSharp.D2D1;

namespace ExampleGallery.PixelShaders;

/// <summary>
/// A shader showing an animated colored background.
/// </summary>
[D2DInputCount(0)]
[D2DShaderProfile(D2D1ShaderProfile.PixelShader40)]
[D2DRequiresScenePosition]
[AutoConstructor]
public readonly partial struct AnimatedColorsShader : ID2D1PixelShader
{
    public readonly float time;
    public readonly int width;
    public readonly int height;

    /// <inheritdoc/>
    public float4 Execute()
    {
        // Normalized screen space UV coordinates from 0.0 to 1.0
        float2 uv = D2D.GetScenePosition().XY / new float2(width, height);

        // Time varying pixel color
        float3 col = 0.5f + 0.5f * Hlsl.Cos(time + new float3(uv, uv.X) + new float3(0, 2, 4));

        // Trim the red out
        col = new float3(0, col.YZ);

        // Output to screen
        return new(col, 1f);
    }
}