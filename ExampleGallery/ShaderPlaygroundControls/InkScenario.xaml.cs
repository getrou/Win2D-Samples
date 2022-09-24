using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.DirectX;
using Microsoft.UI;
using Microsoft.UI.Composition;
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
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ExampleGallery
{
    public sealed partial class InkScenario : UserControl
    {
        private Compositor _compositor;
        private CanvasDevice _canvasDevice;
        private CompositionGraphicsDevice _compositionGraphicsDevice;
        private CanvasRenderTarget _heightMap;

        private CompositionDrawingSurface _inkingDrawingSurface;
        private InkManager _inkManager = null;
        private bool _mouseDown = false;

        public InkScenario()
        {
            this.InitializeComponent();
            this.Loaded += InkScenario_Loaded;
            Microsoft.UI.Xaml.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            if (_mouseDown)
                DrawCanvas();
        }

        private void InkScenario_Loaded(object sender, RoutedEventArgs e)
        {
            CreateCompositionResources();
        }

        private void CreateCompositionResources()
        {
            var visual = ElementCompositionPreview.GetElementVisual(this);
            _compositor = visual.Compositor;
            _canvasDevice = CanvasDevice.GetSharedDevice();
            _compositionGraphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _canvasDevice);

            // Draw the color of the text
            _inkingDrawingSurface = _compositionGraphicsDevice.CreateDrawingSurface2(new SizeInt32(1024, 1024), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

            //_heightMap = new CanvasRenderTarget(_canvasDevice, 1024, 1024, 96);

            _inkManager = new InkManager(_canvasDevice);

            // TODO: temporary - want to use 'DrawingGrid.Background = MaterialBrush(...);' instead

            var inkingVisual = _compositor.CreateSpriteVisual();
            var surfaceBrush = _compositor.CreateSurfaceBrush(_inkingDrawingSurface);
            surfaceBrush.Stretch = CompositionStretch.None;
            inkingVisual.Brush = surfaceBrush;
            inkingVisual.Size = new Vector2(1024, 1024);
            ElementCompositionPreview.SetElementChildVisual(DrawingGrid, inkingVisual);

            // TODO: end temporary

            ClearCanvas();
        }

        private void ClearCanvas()
        {
            using (var ds = CanvasComposition.CreateDrawingSession(_inkingDrawingSurface))
            {
                ds.Clear(Colors.White);
            }
        }

        private void DrawCanvas()
        {
            if (_inkManager == null)
                return;

            using (var ds = CanvasComposition.CreateDrawingSession(_inkingDrawingSurface))
            {
                _inkManager.Draw(ds);
            }
        }

        private void DrawingGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _mouseDown = true;
            if (_inkManager != null)
            {
                _inkManager.BeginStroke();

                Debug.WriteLine("Begin Stroke!");

                var pos = e.GetCurrentPoint(DrawingGrid).Position;
                Vector2 point = new((float)pos.X, (float)pos.Y);
                _inkManager.AddPointToStroke(point);
            }
        }

        private void DrawingGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_mouseDown)
                return;

            if (_inkManager != null)
            {
                var pos = e.GetCurrentPoint(DrawingGrid).Position;
                Vector2 point = new((float)pos.X, (float)pos.Y);
                _inkManager.AddPointToStroke(point);
            }
        }

        private void DrawingGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _mouseDown = false;

            // Force a draw now
            DrawCanvas();
        }

        private void GenerateHeightMap()
        {
            if (_inkManager == null)
                return;

            
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

        public InkManager(ICanvasResourceCreator resourceCreator)
        {
            m_inkBrush = new CanvasSolidColorBrush(resourceCreator, Colors.Black);
            BeginStroke(); // Ensure we always have an active stroke
        }

        public void BeginStroke() => m_strokes.Add(new Stroke());

        public void AddPointToStroke(Vector2 point) => CurrentStroke.Points.Add(point);

        public void Draw(CanvasDrawingSession session)
        {
            session.Clear(Colors.White);

            foreach (var stroke in m_strokes)
            {
                DrawStroke(session, stroke);
            }

            //// Can't draw a line out of 1 point (or 0)
            //if (m_currentStroke.Count < 2)
            //    return;

            //for (int i = 1; i < m_currentStroke.Count; i++)
            //{
            //    session.DrawLine(m_currentStroke[i - 1], m_currentStroke[i], m_inkBrush, 5.0f);
            //    Debug.WriteLine("Drew line!");
            //}

            //// Now that we've drawn these points, can remove them. But keep the last point around so the next point connects
            //m_currentStroke.RemoveRange(0, m_currentStroke.Count - 1);
            //Debug.Assert(m_currentStroke.Count == 1);
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
