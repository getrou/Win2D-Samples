using ComputeSharp.D2D1;
using ComputeSharp;

namespace ExampleGallery.Brushes;

/// <inheritdoc/>
partial class MaterialBrush2
{
    /// <summary>
    /// A pixel shader using a 3x3 sobel kernel to create a normal map from a height map.
    /// See: <see href="https://en.wikipedia.org/wiki/Sobel_operator"/>.
    /// </summary>
    [D2DInputCount(1)]
    [D2DInputComplex(0)]
    [D2DShaderProfile(D2D1ShaderProfile.PixelShader40)]
    [D2DRequiresScenePosition]
    private readonly partial struct SobelShader : ID2D1PixelShader
    {
        /// <inheritdoc/>
        public float4 Execute()
        {
            // Sample pixels in a 3x3 window around the current pixel
            float x00 = D2D.SampleInputAtOffset(0, new float2(-1, -1)).X;
            float x01 = D2D.SampleInputAtOffset(0, new float2(0, -1)).X;
            float x02 = D2D.SampleInputAtOffset(0, new float2(1, -1)).X;

            float x10 = D2D.SampleInputAtOffset(0, new float2(-1, 0)).X;
            float x12 = D2D.SampleInputAtOffset(0, new float2(1, 0)).X;

            float x20 = D2D.SampleInputAtOffset(0, new float2(-1, 1)).X;
            float x21 = D2D.SampleInputAtOffset(0, new float2(0, 1)).X;
            float x22 = D2D.SampleInputAtOffset(0, new float2(1, 1)).X;

            // Compute the approximate horizontal derivative:
            //
            // Gx = [ +1, 0, -1 ]
            //      [ +2, 0, -2 ]
            //      [ +1, 0, -1 ]
            float gx =
                (1 * x00) + (-1 * x02) +
                (2 * x10) + (-2 * x12) +
                (1 * x20) + (-1 * x22);

            // Compute the approximate vertical derivative:
            //
            // Gy = [ +1, +2, +1 ]
            //      [  0,  0,  0 ]
            //      [ -1, -2, -1 ]
            float gy =
                (1 * x00) + (2 * x01) + (1 * x02) +
                (-1 * x20) + (-2 * x21) + (-1 * x22);

            float dx = 0.0f;
            float dy = 0.0f;

            if (gx != 0.0f || gy != 0.0f)
            {
                // Compute direction of the vector
                float theta = Hlsl.Atan2(gy, gx);

                // Convert from polar to cartesian coordinates (in the [-1, 1] range)
                dx = Hlsl.Cos(theta);
                dy = Hlsl.Sin(theta);
            }

            // Normalize in the [0, 1] range
            float r = (dx + 1.0f) / 2.0f;
            float g = (dy + 1.0f) / 2.0f;

            return new(r, 1 - g, 1, 1);
        }
    }

    /// <summary>
    /// A pixel shader using a 5x5 sobel kernel to create a normal map from a height map.
    /// See: <see href="https://www.mathship.com/730/Sobel5x5_lisbon.pdf"/>.
    /// </summary>
    [D2DInputCount(1)]
    [D2DInputComplex(0)]
    [D2DShaderProfile(D2D1ShaderProfile.PixelShader40)]
    [D2DRequiresScenePosition]
    private readonly partial struct Sobel5x5Shader : ID2D1PixelShader
    {
        /// <inheritdoc/>
        public float4 Execute()
        {
            // Sample pixels in a 5x5 window around the current pixel
            float x00 = D2D.SampleInputAtOffset(0, new float2(-2, -2)).X;
            float x01 = D2D.SampleInputAtOffset(0, new float2(-1, -2)).X;
            float x02 = D2D.SampleInputAtOffset(0, new float2(0, -2)).X;
            float x03 = D2D.SampleInputAtOffset(0, new float2(1, -2)).X;
            float x04 = D2D.SampleInputAtOffset(0, new float2(2, -2)).X;

            float x10 = D2D.SampleInputAtOffset(0, new float2(-2, -1)).X;
            float x11 = D2D.SampleInputAtOffset(0, new float2(-1, -1)).X;
            float x12 = D2D.SampleInputAtOffset(0, new float2(0, -1)).X;
            float x13 = D2D.SampleInputAtOffset(0, new float2(1, -1)).X;
            float x14 = D2D.SampleInputAtOffset(0, new float2(2, -1)).X;

            float x20 = D2D.SampleInputAtOffset(0, new float2(-2, 0)).X;
            float x21 = D2D.SampleInputAtOffset(0, new float2(-1, 0)).X;
            float x23 = D2D.SampleInputAtOffset(0, new float2(1, 0)).X;
            float x24 = D2D.SampleInputAtOffset(0, new float2(2, 0)).X;

            float x30 = D2D.SampleInputAtOffset(0, new float2(-2, 1)).X;
            float x31 = D2D.SampleInputAtOffset(0, new float2(-1, 1)).X;
            float x32 = D2D.SampleInputAtOffset(0, new float2(0, 1)).X;
            float x33 = D2D.SampleInputAtOffset(0, new float2(1, 1)).X;
            float x34 = D2D.SampleInputAtOffset(0, new float2(2, 1)).X;

            float x40 = D2D.SampleInputAtOffset(0, new float2(-2, 2)).X;
            float x41 = D2D.SampleInputAtOffset(0, new float2(-1, 2)).X;
            float x42 = D2D.SampleInputAtOffset(0, new float2(0, 2)).X;
            float x43 = D2D.SampleInputAtOffset(0, new float2(1, 2)).X;
            float x44 = D2D.SampleInputAtOffset(0, new float2(2, 2)).X;

            // Compute the approximate horizontal derivative:
            //
            // Gx = [  -5,  -4, 0,  4,  5 ]
            //      [  -8, -10, 0, 10,  8 ]
            //      [ -10, -20, 0, 20, 10 ]
            //      [  -8, -10, 0, 10,  8 ]
            //      [  -5,  -4, 0,  4,  5 ]
            float gx =
                (-5 * x00) + (-4 * x01) + (4 * x03) + (5 * x04) +
                (-8 * x10) + (-10 * x11) + (10 * x13) + (8 * x14) +
                (-10 * x20) + (-20 * x21) + (20 * x23) + (10 * x24) +
                (-8 * x30) + (-10 * x31) + (10 * x33) + (8 * x34) +
                (-5 * x40) + (-4 * x41) + (4 * x43) + (5 * x44);

            // Compute the approximate vertical derivative:
            //
            // Gx = [ -5,  -8, -10,  -8, -5 ]
            //      [ -4, -10, -20, -10, -4 ]
            //      [  0,   0,   0,   0,  0 ]
            //      [  4,  10,  20,  10,  4 ]
            //      [  5,   8,  10,   8,  5 ]
            float gy =
                (-5 * x00) + (-4 * x01) + (10 * x02) + (-8 * x03) + (-5 * x04) +
                (-4 * x10) + (-10 * x11) + (-20 * x12) + (-10 * x13) + (-4 * x14) +
                (4 * x30) + (10 * x31) + (20 * x32) + (10 * x33) + (4 * x34) +
                (5 * x40) + (8 * x41) + (10 * x42) + (8 * x43) + (5 * x44);

            float dx = 0.0f;
            float dy = 0.0f;

            if (gx != 0.0f || gy != 0.0f)
            {
                // Compute direction of the vector
                float theta = Hlsl.Atan2(gy, gx);

                // Convert from polar to cartesian coordinates (in the [-1, 1] range)
                dx = Hlsl.Cos(theta);
                dy = Hlsl.Sin(theta);
            }

            // Normalize in the [0, 1] range
            float r = (dx + 1.0f) / 2.0f;
            float g = (dy + 1.0f) / 2.0f;

            return new(r, 1 - g, 1, 1);
        }
    }
}