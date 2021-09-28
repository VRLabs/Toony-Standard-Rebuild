using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class ControlContainerUIElement : VisualElement
    {
        private ObjectInspectorList<ControlsUIElement.Template, ControlUI> _controlsList;
        public ControlContainerUIElement(List<ControlUI> controls)
        {
            _controlsList = new ObjectInspectorList<ControlsUIElement.Template, ControlUI>("Controls", ControlsUIElement.ElementTemplate);
            _controlsList.Items = controls;
            Add(_controlsList);
        }
    }
}