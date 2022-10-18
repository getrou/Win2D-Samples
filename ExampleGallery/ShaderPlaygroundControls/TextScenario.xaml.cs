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
using Windows.UI.Input.Inking;
using Microsoft.Graphics.Canvas.Brushes;

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
        private CanvasRenderTarget _shadowMap;
        private CanvasRenderTarget _normalMap;
        private GaussianBlurEffect _blurredHeightMap;
        private PixelShaderEffect _normalEffect;
        private PixelShaderEffect _shadowEffect;
        private CompositionDrawingSurface _normalDrawingSurface;
        private DistantLight _distantLight;
        
        // Drawing Properties connected to controls
        private int     _maxTextHeight;
        private float   _lightDirection;
        private float   _lightHeight;
        private float   _shadowDarkness;
        private float   _shadowStretch;
        private float   _inkHeight;

        // Inking Objects
        private InkManager _inkManager  = null;
        private bool _mouseDown         = false;

        public TextScenario()
        {
            this.InitializeComponent();
            this.Loaded += TextScenario_Loaded;
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

        private void LightDirection_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            RegenerateBackground();
        }

        private void LightHeight_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            RegenerateBackground();
        }

        private void ShadowDarkness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            RegenerateBackground();
        }

        private void ShadowStretch_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            RegenerateBackground();
        }

        private void MaxTextHeight_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            RegenerateBackground();
        }

        private void InkHeight_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _inkHeight = (float)InkHeight.Value;

            if (_inkManager != null)
            {
                _inkManager.SetInkHeight(_inkHeight);

                RegenerateBackground();
            }
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
            _colorDrawingSurface = _compositionGraphicsDevice.CreateDrawingSurface2(new SizeInt32(1024, 256), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);


            // Draw the raw heightmap of the text to a CanvasBitmap
            _heightMap = new CanvasRenderTarget(_canvasDevice, 1024, 256, 96);

            // Shadow accumulated in this buffer
            _shadowMap = new CanvasRenderTarget(_canvasDevice, 1024, 256, 96);

            // Normal will be calculated from heightmap and stored here
            _normalMap = new CanvasRenderTarget(_canvasDevice, 1024, 256, 96);

            // Blur the height map
            _blurredHeightMap = new GaussianBlurEffect()
            {
                BlurAmount = 0,
                Source = _heightMap
            };

            // Get the shader bytecode for the Sobel shader
            byte[] bytecode = D2D1PixelShader.LoadBytecode<SobelShader>().ToArray();

            // Create a Win2D pixel shader effect with the shader bytecode
            _normalEffect = new(bytecode)
            {
                Source1 = _blurredHeightMap,
                Source1Mapping = SamplerCoordinateMapping.Offset,
                MaxSamplerOffset = 1
            };

            byte[] shadowEffectByteCode = D2D1PixelShader.LoadBytecode<ShadowShader>().ToArray();

            _shadowEffect = new(shadowEffectByteCode)
            {
                Source1 = _blurredHeightMap,
                Source1Mapping = SamplerCoordinateMapping.Offset,
                MaxSamplerOffset = 1
            };

            _normalDrawingSurface = _compositionGraphicsDevice.CreateDrawingSurface2(new SizeInt32(1024, 256), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

            var colorBrush = _compositor.CreateSurfaceBrush(_colorDrawingSurface);

            var normalBrush = _compositor.CreateSurfaceBrush(_normalDrawingSurface);

            colorBrush.Stretch = CompositionStretch.None;
            normalBrush.Stretch = CompositionStretch.None;

            DrawingGrid.Background = new MaterialBrush(colorBrush, normalBrush);

            DrawingGrid.Lights.Add(new HoverLight());

            Visual distantLightVisual = ElementCompositionPreview.GetElementVisual(DrawingGrid);
            _distantLight = _compositor.CreateDistantLight();
            _distantLight.Color = Colors.White;
            _distantLight.Intensity = 0.5f;
            _distantLight.CoordinateSpace = distantLightVisual;
            _distantLight.Targets.Add(distantLightVisual);
            _distantLight.Direction = new Vector3(1.0f, 0.0f, 0.0f);

            // Drawing properties connected to UI controls
            _lightDirection = 0.0f;
            _lightHeight    = 0.1f;
            _shadowDarkness = 0.5f;
            _shadowStretch  = 1.0f;
            _inkHeight      = 255.0f;

            _inkManager = new InkManager(_canvasDevice);

            _inkManager.SetInkHeight(_inkHeight);

            RegenerateBackground();
        }

        private void RegenerateBackground()
        {
            if (_compositor is null)
            {
                return;
            }

            // Update the member variables from UI
            _blurredHeightMap.BlurAmount = (float)BlurRadius.Value;
            _maxTextHeight  = (int)MaxTextHeight.Value;
            _lightDirection = (float)LightAngle.Value;
            _lightHeight    = (float)LightHeight.Value;
            _shadowDarkness = (float)ShadowDarkness.Value;
            _shadowStretch  = (float)ShadowStretch.Value;

            // Draw the raw heightmap of the text to a CanvasBitmap
            using (var ds = _heightMap.CreateDrawingSession())
            {
                Color maxHeightColor = Color.FromArgb(255, (byte)_maxTextHeight, (byte)_maxTextHeight, (byte)_maxTextHeight);

                int subHeight = _maxTextHeight - 60;

                if (subHeight < 0)
                {
                    subHeight = 0;
                }

                ds.Clear(Color.FromArgb(255,127,127,127));
                DrawText(ds, textBox.Text, maxHeightColor, 72);

                Color subHeightColor = Color.FromArgb(255, (byte)subHeight, (byte)subHeight, (byte)subHeight);

                ds.DrawCircle(new Vector2(500, 40), 20.0f, maxHeightColor);
                ds.FillCircle(new Vector2(500, 80), 20.0f, maxHeightColor);

                ds.FillRectangle(new Rect(700, 40, 100, 50), subHeightColor);
                ds.FillRectangle(new Rect(800, 40, 100, 50), maxHeightColor);

                ds.FillRoundedRectangle(new Rect(500, 120, 200, 100), 15.0f, 15.0f, subHeightColor);
                ds.DrawRoundedRectangle(new Rect(500, 120, 200, 100), 15.0f, 15.0f, maxHeightColor);

                Matrix3x2 oldTransform = ds.Transform;

                ds.Transform = Matrix3x2.CreateTranslation(new Vector2(460, 50));

                DrawText(ds, "Height Button", maxHeightColor, 30);

                ds.Transform = oldTransform;

                if (_inkManager != null)
                {
                    _inkManager.Draw(ds);
                }
            }

            // Calculate Shadow Image from the heightmap
            {
                float lightDirX = (float)Math.Cos(_lightDirection);
                float lightDirY = (float)Math.Sin(_lightDirection);

                _distantLight.Direction = new Vector3(-lightDirX, -lightDirY, 0.0f);

                _shadowEffect.Properties["dirToLightNormalized"]    = new Vector2(lightDirX, lightDirY);
                _shadowEffect.Properties["lightHeightStep"]         = _lightHeight;
                _shadowEffect.Properties["shadowMult"]              = _shadowDarkness;
                _shadowEffect.Properties["shadowStretch"]           = _shadowStretch;

                using (var ds = _shadowMap.CreateDrawingSession())
                {
                    ds.Clear(Color.FromArgb(0, 0, 0, 0));
                    ds.DrawImage(_shadowEffect);
                }

            }

            // Calculate NormalMap from heightmap
            using (var ds = _normalMap.CreateDrawingSession())
            {
                ds.Clear(Color.FromArgb(0, 0, 0, 0));
                ds.DrawImage(_normalEffect);
            }

            // Move the normal results into the composition surface
            using (var ds = CanvasComposition.CreateDrawingSession(_normalDrawingSurface))
            {
                ds.DrawImage(_normalMap);
            }

            // Draw the color of the text and then the shadow on top of it
            using (var ds = CanvasComposition.CreateDrawingSession(_colorDrawingSurface))
            {
                ds.Clear(Color.FromArgb(255, 127, 127, 127));
                ds.DrawImage(_shadowMap);
            }

            // Be sure to invalidate the controls, because they have new content in them
            HeightMapView.Invalidate();
            ShadowView.Invalidate();
            NormalMapView.Invalidate();
        }

        private static void DrawText(CanvasDrawingSession ds, string text, Color color, int fontSize)
        {
            CanvasTextFormat format = new CanvasTextFormat();
            format.FontSize = fontSize;
            ds.DrawText(text, new Vector2(50, 100), color, format);
        }

        private void HeightMapView_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(_heightMap);
        }

        private void ShadowView_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            args.DrawingSession.Clear(Color.FromArgb(255, 255, 255, 255));
            args.DrawingSession.DrawImage(_shadowMap);
        }

        private void NormalMapView_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(_normalMap);
        }

        private void HeightMapView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _mouseDown = true;

            if (_inkManager != null)
            {
                _inkManager.BeginStroke();

                var pos = e.GetCurrentPoint(HeightMapView).Position;
                Vector2 point = new((float)pos.X, (float)pos.Y);
                _inkManager.AddPointToStroke(point);

                RegenerateBackground();
            }
        }

        private void HeightMapView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_mouseDown)
                return;

            if (_inkManager != null)
            {
                var pos = e.GetCurrentPoint(HeightMapView).Position;
                Vector2 point = new((float)pos.X, (float)pos.Y);
                _inkManager.AddPointToStroke(point);

                RegenerateBackground();
            }
        }

        private void HeightMapView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _mouseDown = false;
        }
    }


    public class InkManager
    {
        private struct Stroke
        {
            public Stroke() { }
            public List<Vector2> Points { get; } = new();
        };

        private List<Stroke> m_strokes = new();
        private ICanvasBrush m_inkBrush;
        private ICanvasResourceCreator _resourceCreator;

        public InkManager(ICanvasResourceCreator resourceCreator)
        {
            _resourceCreator = resourceCreator;
            m_inkBrush = new CanvasSolidColorBrush(resourceCreator, Colors.Black);
            BeginStroke(); // Ensure we always have an active stroke
        }

        public void SetInkHeight(float height)
        {
            if (height < 0)
                height = 0;

            if (height > 255)
                height = 255;

            byte byteHeight = (byte)height;
            m_inkBrush = new CanvasSolidColorBrush(_resourceCreator, Color.FromArgb(255, byteHeight, byteHeight, byteHeight));
        }

        public void BeginStroke() => m_strokes.Add(new Stroke());

        public void AddPointToStroke(Vector2 point) => CurrentStroke.Points.Add(point);

        public void Draw(CanvasDrawingSession session)
        {
            foreach (var stroke in m_strokes)
            {
                DrawStroke(session, stroke);
            }
        }

        private Stroke CurrentStroke => m_strokes[m_strokes.Count - 1];

        private void DrawStroke(CanvasDrawingSession session, Stroke stroke)
        {
            var points = stroke.Points;
            // Can't draw a line out of 1 point (or 0)
            if (points.Count < 2)
                return;

            for (int i = 1; i < points.Count; i++)
            {
                session.DrawLine(points[i - 1], points[i], m_inkBrush, 5.0f);
            }
        }

    }
}
