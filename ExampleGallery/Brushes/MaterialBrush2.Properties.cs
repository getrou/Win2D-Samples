using System;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

#nullable enable

namespace ExampleGallery.Brushes;

/// <inheritdoc/>
partial class MaterialBrush2
{
    /// <summary>
    /// Identifies the <see cref="TextureUri"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TextureUriProperty = DependencyProperty.Register(
        nameof(TextureUri),
        typeof(Uri),
        typeof(MaterialBrush2),
        new PropertyMetadata(null, OnTextureUriPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="LightBlendAmount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty LightBlendAmountProperty = DependencyProperty.Register(
        nameof(LightBlendAmount),
        typeof(double),
        typeof(MaterialBrush2),
        new PropertyMetadata(0.5, OnLightBlendAmountPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="AmbientAmount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty AmbientAmountProperty = DependencyProperty.Register(
        nameof(AmbientAmount),
        typeof(double),
        typeof(MaterialBrush2),
        new PropertyMetadata(0.15, OnAmbientAmountPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="DiffuseAmount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DiffuseAmountProperty = DependencyProperty.Register(
        nameof(DiffuseAmount),
        typeof(double),
        typeof(MaterialBrush2),
        new PropertyMetadata(1, OnDiffuseAmountPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="SpecularAmount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SpecularAmountProperty = DependencyProperty.Register(
        nameof(SpecularAmount),
        typeof(double),
        typeof(MaterialBrush2),
        new PropertyMetadata(0.1, OnSpecularAmountPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="NormalMapBlurAmount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NormalMapBlurAmountProperty = DependencyProperty.Register(
        nameof(NormalMapBlurAmount),
        typeof(double),
        typeof(MaterialBrush2),
        new PropertyMetadata(0.0, OnNormalMapBlurAmountPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="EdgeDetectionQuality"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty EdgeDetectionQualityProperty = DependencyProperty.Register(
        nameof(EdgeDetectionQuality),
        typeof(EdgeDetectionQuality),
        typeof(MaterialBrush2),
        new PropertyMetadata(EdgeDetectionQuality.Normal, OnEdgeDetectionQualityPropertyChanged));

    /// <summary>
    /// Gets or sets the <see cref="Uri"/> for the texture to use
    /// </summary>
    public Uri? TextureUri
    {
        get => (Uri)GetValue(TextureUriProperty);
        set => SetValue(TextureUriProperty, value);
    }

    /// <summary>
    /// Gets or sets the amount to use to blend the light effect over the texture (default is <c>0.5</c>).
    /// </summary>
    public double LightBlendAmount
    {
        get => (double)GetValue(LightBlendAmountProperty);
        set => SetValue(LightBlendAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the ambient amount for the light effect over the texture (default is <c>0.15</c>).
    /// </summary>
    public double AmbientAmount
    {
        get => (double)GetValue(AmbientAmountProperty);
        set => SetValue(AmbientAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the specular amount for the light effect over the texture (default is <c>1</c>).
    /// </summary>
    public double DiffuseAmount
    {
        get => (double)GetValue(DiffuseAmountProperty);
        set => SetValue(DiffuseAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the specular amount for the light effect over the texture (default is <c>0.1</c>).
    /// </summary>
    public double SpecularAmount
    {
        get => (double)GetValue(SpecularAmountProperty);
        set => SetValue(SpecularAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the blur amount for the normal map texture (default is <c>0</c>).
    /// </summary>
    public double NormalMapBlurAmount
    {
        get => (double)GetValue(NormalMapBlurAmountProperty);
        set => SetValue(NormalMapBlurAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the edge detection quality to use to calculate the normal maps (default is <see cref="EdgeDetectionQuality.Normal"/>).
    /// </summary>
    public EdgeDetectionQuality EdgeDetectionQuality
    {
        get => (EdgeDetectionQuality)GetValue(EdgeDetectionQualityProperty);
        set => SetValue(EdgeDetectionQualityProperty, value);
    }

    /// <summary>
    /// Updates the UI when <see cref="TextureUri"/> changes
    /// </summary>
    /// <param name="d">The current <see cref="MaterialBrush2"/> instance</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="TextureUriProperty"/></param>
    private static void OnTextureUriPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MaterialBrush2 @this = (MaterialBrush2)d;

        if (@this.CompositionBrush != null)
        {
            @this.OnDisconnected();
            @this.OnConnected();
        }
    }

    /// <summary>
    /// Updates the UI when <see cref="LightBlendAmount"/> changes
    /// </summary>
    /// <param name="d">The current <see cref="MaterialBrush2"/> instance</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="LightBlendAmountProperty"/></param>
    private static void OnLightBlendAmountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MaterialBrush2 @this = (MaterialBrush2)d;

        if (@this.CompositionBrush is CompositionBrush brush)
        {
            brush.Properties.InsertScalar("LightBlendEffect.Source2Amount", (float)(double)e.NewValue);
        }
    }

    /// <summary>
    /// Updates the UI when <see cref="AmbientAmount"/> changes
    /// </summary>
    /// <param name="d">The current <see cref="MaterialBrush2"/> instance</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="AmbientAmountProperty"/></param>
    private static void OnAmbientAmountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MaterialBrush2 @this = (MaterialBrush2)d;

        if (@this.CompositionBrush is CompositionBrush brush)
        {
            brush.Properties.InsertScalar("LightEffect.AmbientAmount", (float)(double)e.NewValue);
        }
    }    

    /// <summary>
    /// Updates the UI when <see cref="DiffuseAmount"/> changes
    /// </summary>
    /// <param name="d">The current <see cref="MaterialBrush2"/> instance</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="DiffuseAmountProperty"/></param>
    private static void OnDiffuseAmountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MaterialBrush2 @this = (MaterialBrush2)d;

        if (@this.CompositionBrush is CompositionBrush brush)
        {
            brush.Properties.InsertScalar("LightEffect.DiffuseAmount", (float)(double)e.NewValue);
        }
    }

    /// <summary>
    /// Updates the UI when <see cref="SpecularAmount"/> changes
    /// </summary>
    /// <param name="d">The current <see cref="MaterialBrush2"/> instance</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="SpecularAmountProperty"/></param>
    private static void OnSpecularAmountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MaterialBrush2 @this = (MaterialBrush2)d;

        if (@this.CompositionBrush is CompositionBrush brush)
        {
            brush.Properties.InsertScalar("LightEffect.SpecularAmount", (float)(double)e.NewValue);
        }
    }

    /// <summary>
    /// Updates the UI when <see cref="NormalMapBlurAmount"/> changes
    /// </summary>
    /// <param name="d">The current <see cref="MaterialBrush2"/> instance</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="NormalMapBlurAmountProperty"/></param>
    private static void OnNormalMapBlurAmountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MaterialBrush2 @this = (MaterialBrush2)d;

        if (@this.CompositionBrush is CompositionBrush brush)
        {
            brush.Properties.InsertScalar("BlurEffect.BlurAmount", (float)(double)e.NewValue);
        }
    }

    /// <summary>
    /// Updates the UI when <see cref="EdgeDetectionQuality"/> changes
    /// </summary>
    /// <param name="d">The current <see cref="MaterialBrush2"/> instance</param>
    /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance for <see cref="EdgeDetectionQualityProperty"/></param>
    private static void OnEdgeDetectionQualityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MaterialBrush2 @this = (MaterialBrush2)d;

        if (@this.CompositionBrush != null)
        {
            @this.OnDisconnected();
            @this.OnConnected();
        }
    }
}