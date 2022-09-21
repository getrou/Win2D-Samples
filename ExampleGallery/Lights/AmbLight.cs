using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Composition;

namespace ExampleGallery.Lights;

public class AmbLight : XamlLight
{
    private static readonly string Id = typeof(AmbLight).FullName;

    protected override void OnConnected(UIElement newElement)
    {
        Compositor compositor = CompositionTarget.GetCompositorForCurrentThread();

        // Create AmbientLight and set its properties
        AmbientLight ambientLight = compositor.CreateAmbientLight();
        ambientLight.Color = Colors.White;

        // Associate CompositionLight with XamlLight
        CompositionLight = ambientLight;

        // Add UIElement to the Light's Targets
        AmbLight.AddTargetElement(GetId(), newElement);
    }

    protected override void OnDisconnected(UIElement oldElement)
    {
        // Dispose Light when it is removed from the tree
        AmbLight.RemoveTargetElement(GetId(), oldElement);
        CompositionLight.Dispose();
    }

    protected override string GetId()
    {
        return Id;
    }
}