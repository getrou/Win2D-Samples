using System;
using ComputeSharp.D2D1.Interop;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Effects;
using Microsoft.UI.Xaml;
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
    /// The cached shader bytecode for <see cref="SobelShader"/>.
    /// </summary>
    private static readonly byte[] ShaderBytecode = D2D1PixelShader.LoadBytecode<SobelShader>().ToArray();

    /// <summary>
    /// The reusable <see cref="PixelShaderEffect"/> instance for the current brush.
    /// </summary>
    private PixelShaderEffect? _pixelShaderEffect;

    /// <summary>
    /// Gets or sets the <see cref="Uri"/> for the texture to use
    /// </summary>
    public Uri? TextureUri
    {
        get => (Uri)GetValue(TextureUriProperty);
        set => SetValue(TextureUriProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="TextureUri"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TextureUriProperty = DependencyProperty.Register(
        nameof(TextureUri),
        typeof(Uri),
        typeof(MaterialBrush2),
        new PropertyMetadata(null, OnTextureUriPropertyChanged));

    /// <summary>
    /// Updates the UI when <see cref="TextureUri"/> changes
    /// </summary>
    /// <param name="d">The current <see cref="AcrylicBrush"/> instance</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TextureUriProperty"/></param>
    private static void OnTextureUriPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MaterialBrush2 brush &&
            brush.CompositionBrush != null)
        {
            brush.OnDisconnected();
            brush.OnConnected();
        }
    }

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
        IGraphicsEffect graphicsEffect = new ArithmeticCompositeEffect()
        {
            Source1Amount = 1,
            Source2Amount = 0.5f,
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

        // Create the effect factory and final brush from the defined graph
        CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect);
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

        // Create a Win2D pixel shader effect with the shader bytecode
        _pixelShaderEffect ??= new(ShaderBytecode)
        {
            Source1 = canvasBitmap,
            Source1Mapping = SamplerCoordinateMapping.Offset,
            MaxSamplerOffset = 1
        };

        // Draw the pixel shader producing the normal map
        using (CanvasDrawingSession drawingSession = CanvasComposition.CreateDrawingSession(drawingSurface))
        {
            drawingSession.DrawImage(_pixelShaderEffect);
        }

        // Create a composition surface brush and assign the resulting drawing surface to it
        CompositionSurfaceBrush normalBrush = compositor.CreateSurfaceBrush();

        normalBrush.Surface = drawingSurface;

        return normalBrush;
    }
}