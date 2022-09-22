using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ExampleGallery
{
    public sealed partial class ShaderPlaygroundBlur : UserControl, IShaderPlaygroundStage
    {
        private CustomPipelineScenario pipelineScenario;
        private ShaderPlaygroundStageControl blurInput;

        public ShaderPlaygroundBlur(CustomPipelineScenario pipelineScenario)
        {
            this.InitializeComponent();
            this.pipelineScenario = pipelineScenario;
            blurInput = new ShaderPlaygroundStageControl(pipelineScenario);
            BlurInputHolder.Children.Add(blurInput);
        }

        public IGraphicsEffectSource GetGraphicsEffectSource()
        {
            IGraphicsEffectSource source = blurInput.GetGraphicsEffectSource();
            if (source == null)
            {
                blurInput.SetError();
                return null;
            }
            else
            {
                blurInput.ClearError();
            }

            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Source = source,
                BlurAmount = (float)BlurRadius.Value
            };
            return blurEffect;
        }

        private void BlurRadius_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            pipelineScenario.RecompileShader();
        }
    }
}
