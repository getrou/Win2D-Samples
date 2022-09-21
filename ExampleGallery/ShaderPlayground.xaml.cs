using ComputeSharp.D2D1.Interop;
using ExampleGallery.PixelShaders;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics.Effects;
using WinRT;

namespace ExampleGallery;

public sealed partial class ShaderPlayground : UserControl
{
    private ShaderPlaygroundStageControl root;
    private ICanvasImage imageEffect;
    private float time;
    private CanvasBitmap _heightmap;

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

    private async void canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (imageEffect is not null)
        {
            args.DrawingSession.DrawImage(imageEffect);
            args.DrawingSession.DrawText("The background is a custom effect pipeline!", 100, 0, Colors.White);

            return;
        }

        if (_heightmap is null)
        {
            _heightmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/heightmap.png"));

            sender.Invalidate();

            return;
        }

        // Get the shader bytecode
        byte[] bytecode = D2D1PixelShader.LoadBytecode<SobelShader>().ToArray();

        // Create a Win2D pixel shader effect with the shader bytecode
        PixelShaderEffect effect = new(bytecode)
        {
            Source1 = _heightmap,
            Source1Mapping = SamplerCoordinateMapping.Offset,
            MaxSamplerOffset = 1
        };

        // Draw the pixel shader
        args.DrawingSession.DrawImage(effect);

        args.DrawingSession.DrawText("The background is a custom D2D1 pixel shader!", 100, 0, Colors.White);
    }
}
