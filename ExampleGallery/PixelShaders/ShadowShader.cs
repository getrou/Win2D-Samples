using ComputeSharp;
using ComputeSharp.D2D1;

namespace ExampleGallery.PixelShaders;

/// <summary>
/// A pixel shader using a sobel kernel to create a normal map from a height map.
/// See: <see href="https://en.wikipedia.org/wiki/Sobel_operator"/>.
/// </summary>
[D2DInputCount(1)]
[D2DInputComplex(0)]
[D2DShaderProfile(D2D1ShaderProfile.PixelShader40)]
[D2DRequiresScenePosition]
public readonly partial struct ShadowShader : ID2D1PixelShader
{
    public readonly float2 dirToLightNormalized;
    public readonly float lightHeightStep;
    public readonly float shadowMult;
    public readonly float shadowStretch;

    /// <inheritdoc/>
    public float4 Execute()
    {
        // Sample the heightmap along the direction of the light ray.
        float centerHeight      = D2D.SampleInputAtOffset(0, new float2(0, 0)).X;
        float oneStepHeight     = D2D.SampleInputAtOffset(0, shadowStretch * dirToLightNormalized).X;
        float twoStepHeight     = D2D.SampleInputAtOffset(0, 2.0f * shadowStretch * dirToLightNormalized).X;
        float threeStepHeight   = D2D.SampleInputAtOffset(0, 3.0f * shadowStretch * dirToLightNormalized).X;
        float fourStepHeight    = D2D.SampleInputAtOffset(0, 4.0f * shadowStretch * dirToLightNormalized).X;
        float fifthStepHeight   = D2D.SampleInputAtOffset(0, 5.0f * shadowStretch * dirToLightNormalized).X;

        float mult = shadowMult;
        float a = 0.0f;

        if (oneStepHeight > centerHeight + shadowStretch * lightHeightStep)
        {
            a = (oneStepHeight - (centerHeight + shadowStretch * lightHeightStep))*mult;
        }
        else if (twoStepHeight > centerHeight + shadowStretch * lightHeightStep * 2.0f)
        {
            a = (twoStepHeight - (centerHeight + shadowStretch * lightHeightStep * 2.0f)) * mult;
        }
        else if (threeStepHeight > centerHeight + shadowStretch * lightHeightStep * 3.0f)
        {
            a = (threeStepHeight - (centerHeight + shadowStretch * lightHeightStep * 3.0f)) * mult;
        }
        else if (fourStepHeight > centerHeight + shadowStretch * lightHeightStep * 4.0f)
        {
            a = (fourStepHeight - (centerHeight + shadowStretch * lightHeightStep * 4.0f)) * mult;
        }
        else if (fifthStepHeight > centerHeight + shadowStretch * lightHeightStep * 5.0f)
        {
            a = (fifthStepHeight - (centerHeight + shadowStretch * lightHeightStep * 5.0f)) * mult;
        }

        a = Hlsl.Clamp(a, 0.0f, 1.0f);

        return new(0.0f, 0.0f, 0.0f, a);
    }
}