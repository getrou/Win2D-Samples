using System;
using ExampleGallery.Brushes;
using ExampleGallery.Lights;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ExampleGallery;

public sealed partial class BrickScenario : UserControl
{
    public BrickScenario()
    {
        this.InitializeComponent();
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        Grid grid = (Grid)sender;

        grid.Lights.Add(new HoverLight());
        grid.Lights.Add(new AmbLight());
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        FileOpenPicker picker = new();
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        InitializeWithWindow.Initialize(picker, App.m_mainWindowHandle);

        var file = await picker.PickSingleFileAsync();

        var copy = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, file.Name, NameCollisionOption.ReplaceExisting);

        BackgroundBrush.TextureUri = new Uri($"ms-appdata:///temp/{copy.Name}");
    }

    private void LightBlendAmount_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        BackgroundBrush.LightBlendAmount = e.NewValue;
    }

    private void AmbientAmount_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        BackgroundBrush.AmbientAmount = e.NewValue;
    }

    private void DiffuseAmount_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        BackgroundBrush.DiffuseAmount = e.NewValue;
    }

    private void SpecularAmount_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        BackgroundBrush.SpecularAmount = e.NewValue;
    }

    private void NormalMapBlurAmount_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        BackgroundBrush.NormalMapBlurAmount = e.NewValue;
    }

    private void ComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        ((ComboBox)sender).ItemsSource = new[]
        {
            EdgeDetectionQuality.Normal,
            EdgeDetectionQuality.High
        };

        ((ComboBox)sender).SelectedIndex = 0;
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EdgeDetectionQuality quality = ((EdgeDetectionQuality[])((ComboBox)sender).ItemsSource)[((ComboBox)sender).SelectedIndex];

        BackgroundBrush.EdgeDetectionQuality = quality;
    }
}
