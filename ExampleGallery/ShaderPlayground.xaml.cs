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
    private int _scenario = 0;
    private int _numScenarios = 2;

    public ShaderPlayground()
    {
        this.InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _scenario++;
        if (_scenario >= _numScenarios)
        {
            _scenario = _scenario % _numScenarios;
        }

        ScenarioHolder.Children.Clear();

        switch (_scenario)
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
