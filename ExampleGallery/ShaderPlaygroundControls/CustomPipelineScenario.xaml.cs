using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
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
    public sealed partial class CustomPipelineScenario : UserControl
    {
        private ShaderPlaygroundStageControl root;
        private ICanvasImage _imageEffect;
        private float time;

        public CustomPipelineScenario()
        {
            this.InitializeComponent();
            this.Loaded += CustomPipelineScenario_Loaded;
        }

        private void CustomPipelineScenario_Loaded(object sender, RoutedEventArgs e)
        {
            root = new ShaderPlaygroundStageControl(this);
            listView.Items.Add(root);

            // Redraw the canvas 60 times per second for when we do cool animated shaders
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(17);
            //timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            canvasControl.Invalidate();
            time += 0.017f;
        }

        internal ICanvasResourceCreator GetResourceCreator()
        {
            return canvasControl.Device;
        }

        public void OnButtonClick(object sender, RoutedEventArgs e)
        {
            RecompileShader();
        }

        public void RecompileShader()
        {
            time = 0;
            IGraphicsEffectSource effect = root.GetGraphicsEffectSource();
            if (effect != null)
            {
                _imageEffect = effect.As<ICanvasImage>();
            }
            canvasControl.Invalidate();
        }

        private void CanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (_imageEffect is not null)
            {
                args.DrawingSession.DrawImage(_imageEffect);
            }
        }
    }
}
