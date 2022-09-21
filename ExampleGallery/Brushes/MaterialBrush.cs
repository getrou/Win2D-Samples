using System;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Effects;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

namespace ExampleGallery.Brushes;

public class MaterialBrush : XamlCompositionBrushBase
{
    private LoadedImageSurface _surface;

    private CompositionSurfaceBrush _normalMap;

    public MaterialBrush(CompositionSurfaceBrush brush)
    {
        _normalMap = brush;
    }

    protected override void OnConnected()
    {
        Compositor compositor = CompositionTarget.GetCompositorForCurrentThread();

        // Load NormalMap onto an ICompositionSurface using LoadedImageSurface
        _surface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/bricks.jpg"));

        // Load Surface onto SurfaceBrush
        CompositionSurfaceBrush texture = compositor.CreateSurfaceBrush(_surface);
        texture.Stretch = CompositionStretch.None;

        _normalMap.Stretch = CompositionStretch.None;

        // Define Effect graph
        const float glassLightAmount = 0.5f;
        Color tintColor = Color.FromArgb(255, 128, 128, 128);

        var graphicsEffect = new Microsoft.Graphics.Canvas.Effects.ArithmeticCompositeEffect()
        {
            Name = "LightComposite",
            Source1Amount = 1,
            Source2Amount = glassLightAmount,
            MultiplyAmount = 0,
            Source1 = new CompositionEffectSourceParameter("Texture"),
            Source2 = new SceneLightingEffect()
            {
                AmbientAmount = 0.15f,
                DiffuseAmount = 1,
                SpecularAmount = 0.1f,
                NormalMapSource = new CompositionEffectSourceParameter("NormalMap")
            },
        };

        // Create EffectFactory and EffectBrush
        CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect);
        CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

        // Set Sources to Effect
        effectBrush.SetSourceParameter("NormalMap", _normalMap);
        effectBrush.SetSourceParameter("Texture", texture);

        // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
        CompositionBrush = effectBrush;
    }

    protected override void OnDisconnected()
    {
        // Dispose Surface and CompositionBrushes if XamlCompBrushBase is removed from tree
        if (_surface != null)
        {
            _surface.Dispose();
            _surface = null;
        }
        if (CompositionBrush != null)
        {
            CompositionBrush.Dispose();
            CompositionBrush = null;
        }
    }
}