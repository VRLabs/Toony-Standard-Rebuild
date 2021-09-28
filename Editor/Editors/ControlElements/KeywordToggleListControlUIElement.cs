using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class KeywordToggleListControlUIElement : VisualElement
    {
        private TextField _toggleKeywordField;
        private ObjectInspectorList<ControlsUIElement.Template, ControlUI> _controlsList;
        public KeywordToggleListControlUIElement(List<object> Parameters, List<ControlUI> controls)
        {
            _toggleKeywordField = new TextField("Property name");
            _controlsList = new ObjectInspectorList<ControlsUIElement.Template, ControlUI>("Controls", ControlsUIElement.ElementTemplate);
            _controlsList.Items = controls;

            if (Parameters.Count != 1)
            {
                Parameters.Clear();
                Parameters.Add("");
            }
            else
            {
                if (!(Parameters[0] is string))
                    Parameters[0] = "";
            }

            _toggleKeywordField.SetValueWithoutNotify((string)Parameters[0]);
            _toggleKeywordField.RegisterValueChangedCallback(e => Parameters[0] = e.newValue);

            Add(_toggleKeywordField);
            Add(_controlsList);
        }
    }
}