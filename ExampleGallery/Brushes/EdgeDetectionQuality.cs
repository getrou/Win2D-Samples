namespace ExampleGallery.Brushes;

/// <summary>
/// Indicates the quality to use to calculate edges in <see cref="MaterialBrush2"/>.
/// </summary>
public enum EdgeDetectionQuality
{
    /// <summary>
    /// The normal quality (will use a 3x3 sobel filter).
    /// </summary>
    Normal,

    /// <summary>
    /// High quality (will use a 5x5 sobel filter).
    /// </summary>
    High
}