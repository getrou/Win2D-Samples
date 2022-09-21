using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Effects;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ExampleGallery
{
    public interface IShaderPlaygroundStage
    {
        public IGraphicsEffectSource GetGraphicsEffectSource();
    }

    public sealed partial class ShaderPlaygroundStageControl : UserControl, IShaderPlaygroundStage
    {
        private ShaderPlayground shaderPlayground;

        public ShaderPlaygroundStageControl(ShaderPlayground shaderPlayground)
        {
            this.InitializeComponent();
            this.shaderPlayground = shaderPlayground;
        }

        public ICanvasResourceCreator CanvasResourceCreator { get; set; }

        public IGraphicsEffectSource GetGraphicsEffectSource()
        {
            Debug.Assert(Parameters.Children.Count < 2);
            if (Parameters.Children.Count == 0)
            {
                return null;
            }
            UIElement effect = Parameters.Children[0];

            // Forward call onto content UIElement that has the actual effect
            IShaderPlaygroundStage effectStage = effect.As<IShaderPlaygroundStage>();
            return effectStage.GetGraphicsEffectSource();
        }

        internal void SetError()
        {
            BackgroundColor.Color = Colors.Red;
            BackgroundColor.Opacity = 1.0f;
        }

        internal void ClearError()
        {
            BackgroundColor.Color = Colors.Azure;
            BackgroundColor.Opacity = 0.2f;
        }

        // Create the xaml control with the effect knobs
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var addedItems = e.AddedItems;
            Debug.Assert(addedItems.Count == 1);
            Debug.Assert(addedItems[0].GetType() == typeof(string));

            string effectType = (string)addedItems[0];
            UserControl control = null;
            switch(effectType)
            {
                case ("Blur"):
                    control = new ShaderPlaygroundBlur(shaderPlayground);
                    break;
                case ("Image"):
                    control = new ShaderPlaygroundImage(shaderPlayground);
                    break;
                default:
                    Debug.Fail("Unrecognized Effect");
                    break;
            }

            Parameters.Children.Clear();
            Parameters.Children.Add(control);

            shaderPlayground.RecompileShader();
        }
    }
}
