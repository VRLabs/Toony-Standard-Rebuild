using UnityEngine.UIElements;
using System.Collections.Generic;
using VRLabs.ToonyStandardRebuild.SSICustomControls;

namespace VRLabs.ToonyStandardRebuild
{
    public class ListSelectorControlUIElement : VisualElement
    {
        public ListSelectorControlUIElement(List<object> parameters)
        {
            var parameterNameField = new TextField("Parameter name");
            var elementsList = new ObjectInspectorList<ListSelectorControl.ListSelectorItem>("Elements", ListSelectorItemUIElement.ElementTemplate);

            if (parameters.Count != 2)
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add(new List<ListSelectorControl.ListSelectorItem>());
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is List<ListSelectorControl.ListSelectorItem>))
                    parameters[1] = new List<ListSelectorControl.ListSelectorItem>();
            }

            parameterNameField.SetValueWithoutNotify((string)parameters[0]);
            elementsList.Items = (List<ListSelectorControl.ListSelectorItem>)parameters[1];
            parameterNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);

            Add(parameterNameField);
            Add(elementsList);
        }
    }
}