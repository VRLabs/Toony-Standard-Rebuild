using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;

namespace VRLabs.ToonyStandardRebuild
{
    public class HelpBoxControlUIElement : VisualElement
    {
        public HelpBoxControlUIElement(List<object> parameters)
        {
            var boxTypeField = new EnumField("Box type");
            boxTypeField.Init(MessageType.Info);
            var wideboxField = new Toggle("Is wide box");

            if (parameters.Count != 2)
            {
                parameters.Clear();
                parameters.Add(MessageType.Info);
                parameters.Add(true);
            }
            else
            {
                if (!(parameters[0] is MessageType))
                    parameters[0] = MessageType.Info;
                if (!(parameters[1] is bool))
                    parameters[1] = true;
            }

            boxTypeField.SetValueWithoutNotify((MessageType)parameters[0]);
            wideboxField.SetValueWithoutNotify((bool)parameters[1]);
            boxTypeField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            wideboxField.RegisterValueChangedCallback(e => parameters[1] = e.newValue);

            Add(boxTypeField);
            Add(wideboxField);
            
            Add(new Label("The box text is dependent by the localization"));
        }
    }
}