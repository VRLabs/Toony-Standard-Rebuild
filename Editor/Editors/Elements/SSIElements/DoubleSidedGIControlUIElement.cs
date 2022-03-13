using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class DoubleSidedGIControlUIElement : VisualElement
    {
        public DoubleSidedGIControlUIElement(List<object> parameters)
        {
            if (parameters.Count != 0)
            {
                parameters.Clear();
            }
        }
    }
}