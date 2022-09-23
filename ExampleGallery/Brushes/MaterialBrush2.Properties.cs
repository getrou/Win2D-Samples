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
        typeof(float),
        typeof(MaterialBrush2),
        new PropertyMetadata(0.5f, OnLightBlendAmountPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="AmbientAmount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty AmbientAmountProperty = DependencyProperty.Register(
        nameof(AmbientAmount),
        typeof(float),
        typeof(MaterialBrush2),
        new PropertyMetadata(0.15f, OnAmbientAmountPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="DiffuseAmount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DiffuseAmountProperty = DependencyProperty.Register(
        nameof(DiffuseAmount),
        typeof(float),
        typeof(MaterialBrush2),
        new PropertyMetadata(1f, OnDiffuseAmountPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="SpecularAmount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SpecularAmountProperty = DependencyProperty.Register(
        nameof(SpecularAmount),
        typeof(float),
        typeof(MaterialBrush2),
        new PropertyMetadata(0.1f, OnSpecularAmountPropertyChanged));

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
    public float LightBlendAmount
    {
        get => (float)GetValue(LightBlendAmountProperty);
        set => SetValue(LightBlendAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the ambient amount for the light effect over the texture (default is <c>0.15</c>).
    /// </summary>
    public float AmbientAmount
    {
        get => (float)GetValue(AmbientAmountProperty);
        set => SetValue(AmbientAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the specular amount for the light effect over the texture (default is <c>1</c>).
    /// </summary>
    public float DiffuseAmount
    {
        get => (float)GetValue(DiffuseAmountProperty);
        set => SetValue(DiffuseAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the specular amount for the light effect over the texture (default is <c>0.1</c>).
    /// </summary>
    public float SpecularAmount
    {
        get => (float)GetValue(SpecularAmountProperty);
        set => SetValue(SpecularAmountProperty, value);
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
            brush.Properties.InsertScalar("LightBlendEffect.Source2Amount", (float)e.NewValue);
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
            brush.Properties.InsertScalar("LightEffect.AmbientAmount", (float)e.NewValue);
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
            brush.Properties.InsertScalar("LightEffect.DiffuseAmount", (float)e.NewValue);
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
            brush.Properties.InsertScalar("LightEffect.SpecularAmount", (float)e.NewValue);
        }
    }
}