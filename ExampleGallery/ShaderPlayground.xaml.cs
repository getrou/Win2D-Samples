using ComputeSharp;
using ComputeSharp.D2D1;
using ComputeSharp.D2D1.Interop;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;

namespace ExampleGallery;

public sealed partial class ShaderPlayground : UserControl
{
    public ShaderPlayground()
    {
        this.InitializeComponent();
    }

    void canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        // Get the shader bytecode
        byte[] bytecode = D2D1PixelShader.LoadBytecode<MyShader>().ToArray();

        // Create a Win2D pixel shader effect with the shader bytecode
        PixelShaderEffect effect = new(bytecode);

        // Set the shader properties in the constant buffer
        effect.Properties[nameof(MyShader.time)] = 0.0f;
        effect.Properties[nameof(MyShader.width)] = (int)sender.ActualWidth;
        effect.Properties[nameof(MyShader.height)] = (int)sender.ActualHeight;

        // Draw the pixel shader
        args.DrawingSession.DrawImage(effect);

        args.DrawingSession.DrawText("The background is a custom D2D1 pixel shader!", 100, 100, Colors.White);
    }
}

[D2DInputCount(0)]
[D2DShaderProfile(D2D1ShaderProfile.PixelShader40)]
[D2DRequiresScenePosition]
[AutoConstructor]
public readonly partial struct MyShader : ID2D1PixelShader
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

        // Output to screen
        return new(col, 1f);
    }
}
