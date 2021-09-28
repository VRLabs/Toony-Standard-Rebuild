using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class ToggleListControlUIElement : VisualElement
    {
        private TextField _togglePropertyField;
        private FloatField _falseValueField;
        private FloatField _trueValueField;
        private ObjectInspectorList<ControlsUIElement.Template, ControlUI> _controlsList;
        public ToggleListControlUIElement(List<object> Parameters, List<ControlUI> controls)
        {
            _togglePropertyField = new TextField("Property name");
            _falseValueField = new FloatField("False value");
            _trueValueField = new FloatField("True value");
            _controlsList = new ObjectInspectorList<ControlsUIElement.Template, ControlUI>("Controls", ControlsUIElement.ElementTemplate);
            _controlsList.Items = controls;

            if (Parameters.Count != 3)
            {
                Parameters.Clear();
                Parameters.Add("");
                Parameters.Add(0f);
                Parameters.Add(1f);
            }
            else
            {
                if (!(Parameters[0] is string))
                    Parameters[0] = "";
                if (!(Parameters[1] is float))
                    Parameters[1] = 0f;
                if (!(Parameters[2] is float))
                    Parameters[2] = 1f;
            }

            _togglePropertyField.SetValueWithoutNotify((string)Parameters[0]);
            _falseValueField.SetValueWithoutNotify((float)Parameters[1]);
            _trueValueField.SetValueWithoutNotify((float)Parameters[2]);
            _togglePropertyField.RegisterValueChangedCallback(e => Parameters[0] = e.newValue);
            _falseValueField.RegisterValueChangedCallback(e => Parameters[1] = e.newValue);
            _trueValueField.RegisterValueChangedCallback(e => Parameters[2] = e.newValue);

            Add(_togglePropertyField);
            Add(_falseValueField);
            Add(_trueValueField);
            Add(_controlsList);
        }
    }
}