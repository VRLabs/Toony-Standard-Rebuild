using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class LightmapEmissionControlUIElement : VisualElement
    {
        public LightmapEmissionControlUIElement(List<object> Parameters)
        {
            if (Parameters.Count != 0)
            {
                Parameters.Clear();
            }
        }
    }
}