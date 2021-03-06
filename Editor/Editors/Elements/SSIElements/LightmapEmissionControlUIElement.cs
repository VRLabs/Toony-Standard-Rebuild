using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class LightmapEmissionControlUIElement : VisualElement
    {
        public LightmapEmissionControlUIElement(List<object> parameters)
        {
            if (parameters.Count != 0)
            {
                parameters.Clear();
            }
        }
    }
}