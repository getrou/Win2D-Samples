using ComputeSharp.D2D1.Interop;
using ExampleGallery.Brushes;
using ExampleGallery.Lights;
using ExampleGallery.PixelShaders;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Graphics.DirectX;
using Microsoft.UI.Composition;
using Microsoft.UI;
using System.Numerics;
using Windows.UI;
using Windows.Graphics;
using Microsoft.Graphics.Canvas.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ExampleGallery
{
    public sealed partial class TextScenario : UserControl
    {
        private Compositor _compositor;
        private CanvasDevice _canvasDevice;
        private CompositionGraphicsDevice _compositionGraphicsDevice;
        private CompositionDrawingSurface _colorDrawingSurface;
        private CanvasRenderTarget _heightMap;
        private GaussianBlurEffect _blurredHeightMap;
        private PixelShaderEffect _normalEffect;
        private CompositionDrawingSurface _normalDrawingSurface;
        private DistantLight _distantLight;

        public TextScenario()
        {
            this.InitializeComponent();
            this.Loaded += TextScenario_Loaded; ;
        }

        private void TextScenario_Loaded(object sender, RoutedEventArgs e)
        {
            CreateResources();
            RegenerateBackground();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RegenerateBackground();
        }

        private void BlurRadius_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            RegenerateBackground();
        }

        public class MaterialBrush2 : XamlCompositionBrushBase
        {
            private CompositionSurfaceBrush _colorMap;

            public MaterialBrush2(CompositionSurfaceBrush colorMap)
            {
                _colorMap = colorMap;
            }

            protected override void OnConnected()
            {
                // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
                CompositionBrush = _colorMap;
            }

            protected override void OnDisconnected()
            {
                if (CompositionBrush != null)
                {
                    CompositionBrush.Dispose();
                    CompositionBrush = null;
                }
            }
        }

        private void CreateResources()
        {
            var visual = ElementCompositionPreview.GetElementVisual(this);
            _compositor = visual.Compositor;
            _canvasDevice = CanvasDevice.GetSharedDevice();
            _compositionGraphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _canvasDevice);

            // Draw the color of the text
            _colorDrawingSurface = _compositionGraphicsDevice.CreateDrawingSurface2(new SizeInt32(1024, 1024), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

            // Draw the raw heightmap of the text to a CanvasBitmap
            _heightMap = new CanvasRenderTarget(_canvasDevice, 1024, 1024, 96);

            // Blur the height map
            _blurredHeightMap = new GaussianBlurEffect()
            {
                BlurAmount = 0,
                Source = _heightMap
            };

            // Get the shader bytecode for the Sobel shader
            //byte[] bytecode = D2D1PixelShader.LoadBytecode<SobelShader>().ToArray();
            byte[] bytecode = D2D1PixelShader.LoadBytecode<BumpMappingShader>().ToArray();

            // Create a Win2D pixel shader effect with the shader bytecode
            _normalEffect = new(bytecode)
            {
                Source1 = _blurredHeightMap,
                Source1Mapping = SamplerCoordinateMapping.Offset,
                MaxSamplerOffset = 1
            };

            _normalDrawingSurface = _compositionGraphicsDevice.CreateDrawingSurface2(new SizeInt32(1024, 1024), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

            var colorBrush = _compositor.CreateSurfaceBrush(_colorDrawingSurface);
            var normalBrush = _compositor.CreateSurfaceBrush(_normalDrawingSurface);

            colorBrush.Stretch = CompositionStretch.None;
            normalBrush.Stretch = CompositionStretch.None;
            //colorBrush.Stretch = CompositionStretch.Uniform;
            //normalBrush.Stretch = CompositionStretch.Uniform;

            DrawingGrid.Background = new MaterialBrush(colorBrush, normalBrush);

            DrawingGrid.Lights.Add(new HoverLight());

            Visual distantLightVisual = ElementCompositionPreview.GetElementVisual(DrawingGrid);
            _distantLight = _compositor.CreateDistantLight();
            _distantLight.Color = Colors.White;
            _distantLight.Intensity = 0.5f;
            _distantLight.CoordinateSpace = distantLightVisual;
            _distantLight.Targets.Add(distantLightVisual);

            ScalarKeyFrameAnimation lightRadianAnim = _compositor.CreateScalarKeyFrameAnimation();
            LinearEasingFunction easingFunction = _compositor.CreateLinearEasingFunction();
            _distantLight.Properties.InsertScalar("radians", 0);
            lightRadianAnim.InsertKeyFrame(0, 0, easingFunction);
            lightRadianAnim.InsertKeyFrame(1, MathF.PI * 2, easingFunction);
            lightRadianAnim.Duration = TimeSpan.FromSeconds(3);
            lightRadianAnim.IterationBehavior = AnimationIterationBehavior.Forever;
            _distantLight.Properties.StartAnimation("radians", lightRadianAnim);

            ExpressionAnimation lightAnim = _compositor.CreateExpressionAnimation();
            lightAnim.Expression = "Normalize(Vector3(Cos(props.radians), Sin(props.radians), -1))";
            lightAnim.SetReferenceParameter("props", _distantLight.Properties);
            _distantLight.StartAnimation("Direction", lightAnim);

            RegenerateBackground();
        }

        private void RegenerateBackground()
        {
            if (_compositor is null)
            {
                return;
            }

            // Draw the color of the text
            using (var ds = CanvasComposition.CreateDrawingSession(_colorDrawingSurface))
            {
                ds.Clear(Colors.Gray);
            }

            // Draw the raw heightmap of the text to a CanvasBitmap
            using (var ds = _heightMap.CreateDrawingSession())
            {
                // TODO - Why does this not result in a flat base with elevated text?
                ds.Clear(Color.FromArgb(0,0,0,0));
                DrawText(ds, textBox.Text, Colors.White);
            }

            // Update the blur radius on the height map
            _blurredHeightMap.BlurAmount = (float)BlurRadius.Value;
            
            // Draw the new normals to the drawing surface
            using (var ds = CanvasComposition.CreateDrawingSession(_normalDrawingSurface))
            {
                ds.Clear(Color.FromArgb(0, 0, 0, 0));
                ds.DrawImage(_normalEffect);
            }
        }

        private static void DrawText(CanvasDrawingSession ds, string text, Color color)
        {
            CanvasTextFormat format = new CanvasTextFormat();
            format.FontSize = 72;
            ds.DrawText(text, new Vector2(50, 400), color, format);
        }
    }
}
