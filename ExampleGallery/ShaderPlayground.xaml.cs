using ComputeSharp;
using ComputeSharp.D2D1;
using ComputeSharp.D2D1.Interop;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.Graphics.Effects;
using WinRT;

namespace ExampleGallery;

public sealed partial class ShaderPlayground : UserControl
{
    private ShaderPlaygroundStageControl root;
    private ICanvasImage imageEffect;
    private float time;

    public ShaderPlayground()
    {
        this.InitializeComponent();
        root = new ShaderPlaygroundStageControl(this);
        listView.Items.Add(root);

        // Redraw the canvas 60 times per second for when we do cool animated shaders
        DispatcherTimer timer = new DispatcherTimer();
        timer.Tick += Timer_Tick;
        timer.Interval = TimeSpan.FromMilliseconds(17);
        timer.Start();
    }

    private void Timer_Tick(object sender, object e)
    {
        canvasControl.Invalidate();
        time += 0.017f;
    }

    internal ICanvasResourceCreator GetResourceCreator()
    {
        return canvasControl.Device;
    }

    public void OnButtonClick(object sender, RoutedEventArgs e)
    {
        RecompileShader();
    }

    public void RecompileShader()
    {
        time = 0;
        IGraphicsEffectSource effect = root.GetGraphicsEffectSource();
        if (effect != null)
        {
            this.imageEffect = effect.As<ICanvasImage>();
        }
        canvasControl.Invalidate();
    }

    void canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (imageEffect != null)
        {
            args.DrawingSession.DrawImage(imageEffect);
            args.DrawingSession.DrawText("The background is a custom effect pipeline!", 100, 0, Colors.White);
            return;
        }

        else
        {
            // Get the shader bytecode
            byte[] bytecode = D2D1PixelShader.LoadBytecode<MyShader>().ToArray();

            // Create a Win2D pixel shader effect with the shader bytecode
            PixelShaderEffect effect = new(bytecode);

            // Set the shader properties in the constant buffer
            effect.Properties[nameof(MyShader.time)] = time;
            effect.Properties[nameof(MyShader.width)] = (int)sender.ActualWidth;
            effect.Properties[nameof(MyShader.height)] = (int)sender.ActualHeight;

            // Draw the pixel shader
            args.DrawingSession.DrawImage(effect);

            args.DrawingSession.DrawText("The background is a custom D2D1 pixel shader!", 100, 0, Colors.White);
        }
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

        // Trim the red out
        col = new float3(0, col.YZ);

        // Output to screen
        return new(col, 1f);
    }
}
