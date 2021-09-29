using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class KeywordToggleListControlUIElement : VisualElement
    {
        public KeywordToggleListControlUIElement(List<object> parameters, List<ControlUI> controls)
        {
            var toggleKeywordField = new TextField("Property name");
            var controlsList = new ObjectInspectorList<ControlUI>("Controls", ControlsUIElement.ElementTemplate);
            controlsList.Items = controls;

            if (parameters.Count != 1)
            {
                parameters.Clear();
                parameters.Add("");
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
            }

            toggleKeywordField.SetValueWithoutNotify((string)parameters[0]);
            toggleKeywordField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);

            Add(toggleKeywordField);
            Add(controlsList);
        }
    }
}