using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class KeywordToggleControlUIElement : VisualElement
    {
        public KeywordToggleControlUIElement(List<object> parameters)
        {
            var toggleKeywordField = new TextField("Property name");
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
        }
    }
}