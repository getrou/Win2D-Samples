using ComputeSharp.D2D1.Interop;
using ExampleGallery.Brushes;
using ExampleGallery.Lights;
using ExampleGallery.PixelShaders;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Diagnostics;
using Microsoft.Graphics.DirectX;
using Microsoft.UI.Composition;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ExampleGallery
{
    public sealed partial class BrickScenario : UserControl
    {
        private CanvasBitmap _heightmap;

        public BrickScenario()
        {
            this.InitializeComponent();
            this.Loaded += CreateResources;
        }

        private async void CreateResources(object sender, RoutedEventArgs e)
        {
            var visual = ElementCompositionPreview.GetElementVisual(this);
            var compositor = visual.Compositor;
            var canvasDevice = CanvasDevice.GetSharedDevice();
            var compositionGraphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(compositor, canvasDevice);

            var drawingSurface = compositionGraphicsDevice.CreateDrawingSurface(new Size(1080, 1080), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

            using (var ds = CanvasComposition.CreateDrawingSession(drawingSurface))
            {
                Debug.Assert(_heightmap is null);
                _heightmap = await CanvasBitmap.LoadAsync(canvasDevice, new Uri("ms-appx:///Assets/bricks.jpg"), 96);

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
                ds.DrawImage(effect);

                var brush = compositor.CreateSurfaceBrush();
                brush.Surface = drawingSurface;

                RootGrid.Background = new MaterialBrush(brush);

                RootGrid.Lights.Add(new HoverLight());
                RootGrid.Lights.Add(new AmbLight());
            }
        }
    }
}
