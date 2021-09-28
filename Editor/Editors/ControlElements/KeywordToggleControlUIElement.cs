using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class KeywordToggleControlUIElement : VisualElement
    {
        private TextField _toggleKeywordField;
        public KeywordToggleControlUIElement(List<object> Parameters)
        {
            _toggleKeywordField = new TextField("Property name");
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
        }
    }
}