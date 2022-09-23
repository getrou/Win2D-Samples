using System;
using ExampleGallery.Lights;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
}
