using System;
using ComputeSharp.D2D1.Interop;
using ExampleGallery.PixelShaders;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Effects;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.UI;
using Microsoft.Graphics.DirectX;

namespace ExampleGallery.Brushes;

public class MaterialBrush : XamlCompositionBrushBase
{
    private CompositionSurfaceBrush _colorMap;
    private CompositionSurfaceBrush _normalMap;

    public MaterialBrush(CompositionSurfaceBrush colorMap, CompositionSurfaceBrush normalMap)
    {
        _colorMap = colorMap;
        _normalMap = normalMap;
    }

    protected override void OnConnected()
    {
        Compositor compositor = CompositionTarget.GetCompositorForCurrentThread();

        // Define Effect graph
        const float glassLightAmount = 0.5f;
        Color tintColor = Color.FromArgb(255, 128, 128, 128);

        var graphicsEffect = new Microsoft.Graphics.Canvas.Effects.ArithmeticCompositeEffect()
        {
            Name = "LightComposite",
            Source1Amount = 1,
            Source2Amount = glassLightAmount,
            MultiplyAmount = 0,
            Source1 = new CompositionEffectSourceParameter("ColorMap"),
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
        effectBrush.SetSourceParameter("ColorMap", _colorMap);
        effectBrush.SetSourceParameter("NormalMap", _normalMap);

        // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
        CompositionBrush = effectBrush;
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