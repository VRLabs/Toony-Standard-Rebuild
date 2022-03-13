using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class EnableInstancingControlUIElement : VisualElement
    {
        public EnableInstancingControlUIElement(List<object> parameters)
        {
            if (parameters.Count != 0)
            {
                parameters.Clear();
            }
        }
    }
}