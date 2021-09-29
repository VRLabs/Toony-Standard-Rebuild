using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using System.Reflection;

namespace VRLabs.ToonyStandardRebuild
{
    public class SectionUIElement : VisualElement
    {
        public class Template : IVisualElementTemplate<SectionUI>
        {
            public VisualElement CreateVisualElementForObject(SectionUI obj)
            {
                var element = new SectionUIElement();
                element.AssignObject(obj);
                return element;
            }
        }

        public static Template ElementTemplate = new Template();

        private TextField _sectionNameField;
        private TextField _activateNameField;
        private FloatField _activateValueField;
        private ObjectInspectorList<ControlUI> _controlsList;

        public SectionUIElement()
        {
            _sectionNameField = new TextField("Section Name");
            _activateNameField = new TextField("Property name");
            _activateValueField = new FloatField("Activate value");
            _controlsList = new ObjectInspectorList<ControlUI>("Controls", ControlsUIElement.ElementTemplate);
            Add(_sectionNameField);
            Add(_activateNameField);
            Add(_activateValueField);
            Add(_controlsList);
        }

        public void AssignObject(SectionUI obj)
        {
            _sectionNameField.SetValueWithoutNotify(obj.SectionName);
            _activateNameField.SetValueWithoutNotify(obj.ActivatePropertyName);
            _activateValueField.SetValueWithoutNotify(obj.ActivateValue);
            _controlsList.Items = obj.Controls;

            _sectionNameField.RegisterValueChangedCallback(e => obj.SectionName = e.newValue);
            _activateNameField.RegisterValueChangedCallback(e => obj.ActivatePropertyName = e.newValue);
            _activateValueField.RegisterValueChangedCallback(e => obj.ActivateValue = e.newValue);
        }
    }
}