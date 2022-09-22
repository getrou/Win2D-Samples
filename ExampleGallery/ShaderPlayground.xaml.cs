using ComputeSharp.D2D1.Interop;
using ExampleGallery.Brushes;
using ExampleGallery.Lights;
using ExampleGallery.PixelShaders;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.DirectX;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Effects;
using WinRT;

namespace ExampleGallery;

public sealed partial class ShaderPlayground : UserControl
{
    public ShaderPlayground()
    {
        this.InitializeComponent();
    }

    int scenario = 0;
    int numScenarios = 2;

    private void ScenarioHolder_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Tab)
        {
            scenario++;
            if (scenario > numScenarios)
            {
                scenario = scenario % numScenarios;
            }

            ScenarioHolder.Children.Clear();

            switch (scenario)
            {
                case 0:
                    ScenarioHolder.Children.Add(new TextScenario());
                    break;
                case 1:
                    ScenarioHolder.Children.Add(new BrickScenario());
                    break;
            }
        }
    }

    //private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    var addedItems = e.AddedItems;
    //    Debug.Assert(addedItems.Count == 1);
    //    Debug.Assert(addedItems[0].GetType() == typeof(string));

    //    string effectType = (string)addedItems[0];
    //    UserControl control = null;
    //    switch (effectType)
    //    {
    //        case ("Brick"):
    //            control = new BrickScenario();
    //            break;
    //        case ("Text"):
    //            control = new TextScenario();
    //            break;
    //        default:
    //            Debug.Fail("Unrecognized Effect");
    //            break;
    //    }

    //    ScenarioHolder.Children.Clear();
    //    ScenarioHolder.Children.Add(control);
    //}
}
