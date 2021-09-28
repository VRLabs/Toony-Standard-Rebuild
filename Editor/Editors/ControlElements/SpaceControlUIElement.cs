using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class SpaceControlUIElement : VisualElement
    {
        private IntegerField _spaceField;
        public SpaceControlUIElement(List<object> Parameters)
        {
            _spaceField = new IntegerField("Space");

            if (Parameters.Count != 1)
            {
                Parameters.Clear();
                Parameters.Add(0);
            }
            else if (!(Parameters[0] is int))
            {
                Parameters[0] = 0;
            }

            //var controlsList = new ObjectInspectorList<Template, ControlUI>("Controls", ElementTemplate);

            _spaceField.SetValueWithoutNotify((int)Parameters[0]);
            _spaceField.RegisterValueChangedCallback(e => Parameters[0] = e.newValue);

            Add(_spaceField);
        }
    }
}