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
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ExampleGallery
{
    public sealed partial class ShaderPlaygroundImage : UserControl, IShaderPlaygroundStage
    {
        private CanvasBitmap bitmap;
        private CustomPipelineScenario pipelineScenario;

        public ShaderPlaygroundImage(CustomPipelineScenario pipelineScenario)
        {
            this.InitializeComponent();
            this.pipelineScenario = pipelineScenario;
        }

        public async void OnButtonClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.mainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");
            StorageFile file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                FileButton.Content = "Selected File:" + file.Name;
                bitmap = await CanvasBitmap.LoadAsync(pipelineScenario.GetResourceCreator(), file.Path);
            }
            else
            {
                FileButton.Content = "Select Image";
                bitmap = null;
            }
            pipelineScenario.RecompileShader();
        }

        public IGraphicsEffectSource GetGraphicsEffectSource()
        {
            return bitmap;
        }
    }
}
