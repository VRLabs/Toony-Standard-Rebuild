using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class ControlContainerUIElement : VisualElement
    {
        public ControlContainerUIElement(List<ControlUI> controls)
        {
            var controlsList = new ObjectInspectorList<ControlUI>("Controls", ControlsUIElement.ElementTemplate);
            controlsList.Items = controls;
            Add(controlsList);
        }
    }
}