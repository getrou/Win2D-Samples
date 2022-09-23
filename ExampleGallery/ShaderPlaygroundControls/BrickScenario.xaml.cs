using ExampleGallery.Lights;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
}
