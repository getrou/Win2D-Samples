using System;
using ComputeSharp.D2D1.Interop;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Effects;
using Microsoft.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.DirectX;
using Windows.Graphics.Effects;

#nullable enable

namespace ExampleGallery.Brushes;

/// <summary>
/// A custom XAML composition brush displaying a texture with light effects over it.
/// </summary>
public sealed partial class MaterialBrush2 : XamlCompositionBrushBase
{
    /// <summary>
    /// The cached shader bytecode for <see cref="Sobel3x3Shader"/>.
    /// </summary>
    private static readonly byte[] Shader3x3Bytecode = D2D1PixelShader.LoadBytecode<Sobel3x3Shader>().ToArray();

    /// <summary>
    /// The cached shader bytecode for <see cref="Sobel5x5Shader"/>.
    /// </summary>
    private static readonly byte[] Shader5x5Bytecode = D2D1PixelShader.LoadBytecode<Sobel5x5Shader>().ToArray();

    /// <summary>
    /// The reusable <see cref="PixelShaderEffect"/> instance for the current brush using <see cref="Sobel3x3Shader"/>.
    /// </summary>
    private PixelShaderEffect? _pixelShader3x3Effect;

    /// <summary>
    /// The reusable <see cref="PixelShaderEffect"/> instance for the current brush using <see cref="Sobel5x5Shader"/>.
    /// </summary>
    private PixelShaderEffect? _pixelShader5x5Effect;

    /// <inheritdoc/>
    protected override async void OnConnected()
    {
        Compositor compositor = CompositionTarget.GetCompositorForCurrentThread();

        // Load the texture image and get a surface brush for it
        LoadedImageSurface textureImageSurface = LoadedImageSurface.StartLoadFromUri(TextureUri);
        CompositionSurfaceBrush textureSurfaceBrush = compositor.CreateSurfaceBrush(textureImageSurface);

        // Load the normal map
        CompositionBrush normalMapBrush = await GetNormalMapAsync();

        // Create the effect graph
        IGraphicsEffect graphicsEffect = new ArithmeticCompositeEffect
        {
            Name = "LightBlendEffect",
            Source1Amount = 1,
            Source2Amount = (float)LightBlendAmount,
            MultiplyAmount = 0,
            Source1 = new CompositionEffectSourceParameter("Texture"),
            Source2 = new SceneLightingEffect
            {
                Name = "LightEffect",
                AmbientAmount = (float)AmbientAmount,
                DiffuseAmount = (float)DiffuseAmount,
                SpecularAmount = (float)SpecularAmount,
                NormalMapSource = new GaussianBlurEffect
                {
                    Name = "BlurEffect",
                    BorderMode = EffectBorderMode.Hard,
                    Optimization = EffectOptimization.Balanced,
                    BlurAmount = (float)NormalMapBlurAmount,
                    Source = new CompositionEffectSourceParameter("NormalMap"),
                }
            },
        };

        // Create the effect factory and indicate the properties that can be animated
        CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect, new[]
        {
            "LightBlendEffect.Source2Amount",
            "LightEffect.AmbientAmount",
            "LightEffect.DiffuseAmount",
            "LightEffect.SpecularAmount",
            "BlurEffect.BlurAmount"
        });

        // Create the final brush from the defined graph
        CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

        // Set the input sources for the effect graph
        effectBrush.SetSourceParameter("Texture", textureSurfaceBrush);
        effectBrush.SetSourceParameter("NormalMap", normalMapBrush);

        // Connect the resulting brush
        CompositionBrush = effectBrush;
    }

    /// <inheritdoc/>
    protected override void OnDisconnected()
    {
        if (CompositionBrush != null)
        {
            CompositionBrush.Dispose();
            CompositionBrush = null;
        }
    }

    /// <summary>
    /// Loads a <see cref="CompositionSurfaceBrush"/> with the normal map of the current texture.
    /// </summary>
    /// <returns>A <see cref="CompositionSurfaceBrush"/> with the normal map of the current texture.</returns>
    private async Task<CompositionBrush> GetNormalMapAsync()
    {
        // Load the Win2D bitmap for the input image
        CanvasDevice canvasDevice = CanvasDevice.GetSharedDevice();
        CanvasBitmap canvasBitmap = await CanvasBitmap.LoadAsync(canvasDevice, TextureUri, 96);

        // Create a drawing surface with the same size as the image
        Compositor compositor = CompositionTarget.GetCompositorForCurrentThread();
        CompositionGraphicsDevice compositionGraphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(compositor, canvasDevice);
        CompositionDrawingSurface drawingSurface = compositionGraphicsDevice.CreateDrawingSurface(
            sizePixels: new Size(canvasBitmap.Bounds.Width, canvasBitmap.Bounds.Height),
            pixelFormat: DirectXPixelFormat.B8G8R8A8UIntNormalized,
            alphaMode: DirectXAlphaMode.Ignore);

        PixelShaderEffect effect;

        if (EdgeDetectionQuality == EdgeDetectionQuality.Normal)
        {
            // Create a Win2D pixel shader effect with the shader bytecode
            _pixelShader3x3Effect ??= new PixelShaderEffect(Shader3x3Bytecode)
            {
                Source1Mapping = SamplerCoordinateMapping.Offset,
                MaxSamplerOffset = 1
            };

            // Set the shader source
            _pixelShader3x3Effect.Source1 = canvasBitmap;

            effect = _pixelShader3x3Effect;
        }
        else
        {
            _pixelShader5x5Effect ??= new PixelShaderEffect(Shader5x5Bytecode)
            {
                Source1Mapping = SamplerCoordinateMapping.Offset,
                MaxSamplerOffset = 2
            };

            _pixelShader5x5Effect.Source1 = canvasBitmap;

            effect = _pixelShader5x5Effect;
        }

        // Draw the pixel shader producing the normal map
        using (CanvasDrawingSession drawingSession = CanvasComposition.CreateDrawingSession(drawingSurface))
        {
            drawingSession.DrawImage(effect);
        }

        // Create a composition surface brush and assign the resulting drawing surface to it
        CompositionSurfaceBrush normalBrush = compositor.CreateSurfaceBrush();

        normalBrush.Surface = drawingSurface;

        return normalBrush;
    }
}