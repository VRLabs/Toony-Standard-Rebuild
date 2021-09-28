using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class LabelControlUIElement : VisualElement
    {
        public LabelControlUIElement(List<object> Parameters)
        {
            Parameters.Clear();
            Add(new Label("The label text is dependent by the localization"));
        }
    }
}