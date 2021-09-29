using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class SpaceControlUIElement : VisualElement
    {
        public SpaceControlUIElement(List<object> parameters)
        {
            var spaceField = new IntegerField("Space");

            if (parameters.Count != 1)
            {
                parameters.Clear();
                parameters.Add(0);
            }
            else if (!(parameters[0] is int))
            {
                parameters[0] = 0;
            }

            //var controlsList = new ObjectInspectorList<Template, ControlUI>("Controls", ElementTemplate);

            spaceField.SetValueWithoutNotify((int)parameters[0]);
            spaceField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);

            Add(spaceField);
        }
    }
}