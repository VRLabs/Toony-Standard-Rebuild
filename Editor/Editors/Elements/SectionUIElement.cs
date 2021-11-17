using UnityEngine.UIElements;
using UnityEditor.UIElements;

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
        private TextField _enableNameField;
        private Toggle _isPermanentToggle;
        private FloatField _enableValueField;
        private FloatField _disableValueField;
        private ObjectInspectorList<ControlUI> _controlsList;
        private UpdateDataUIElement _onDisableValues;

        public SectionUIElement()
        {
            _sectionNameField = new TextField("Section Name");
            _isPermanentToggle = new Toggle("Is permanent section");
            _enableNameField = new TextField("Property name");
            _enableValueField = new FloatField("Enable value");
            _disableValueField = new FloatField("Disable value");
            _controlsList = new ObjectInspectorList<ControlUI>("Controls", ControlsUIElement.ElementTemplate);

            _onDisableValues = new UpdateDataUIElement { FoldoutText = "On disable" };

            Add(_sectionNameField);
            Add(_isPermanentToggle);
            Add(_enableNameField);
            Add(_enableValueField);
            Add(_disableValueField);
            Add(_controlsList);
            Add(_onDisableValues);
        }

        public void AssignObject(SectionUI obj)
        {
            _sectionNameField.SetValueWithoutNotify(obj.SectionName);
            _isPermanentToggle.SetValueWithoutNotify(obj.IsPermanent);
            _enableNameField.SetValueWithoutNotify(obj.ActivatePropertyName);
            _enableValueField.SetValueWithoutNotify(obj.EnableValue);
            _disableValueField.SetValueWithoutNotify(obj.DisableValue);
            _controlsList.Items = obj.Controls;

            if (obj.OnSectionDisableData == null) obj.OnSectionDisableData = new UpdateData();
            _onDisableValues.AssignObject(obj.OnSectionDisableData);

            _sectionNameField.RegisterValueChangedCallback(e => obj.SectionName = e.newValue);
            _enableNameField.RegisterValueChangedCallback(e => obj.ActivatePropertyName = e.newValue);
            _enableValueField.RegisterValueChangedCallback(e => obj.EnableValue = e.newValue);
            _disableValueField.RegisterValueChangedCallback(e => obj.DisableValue = e.newValue);
            
            _isPermanentToggle.RegisterValueChangedCallback(e =>
            {
                obj.IsPermanent = e.newValue;

                ApplyDisplay(obj);
            });
            
            ApplyDisplay(obj);
        }

        private void ApplyDisplay(SectionUI obj)
        {
            _enableNameField.style.display = obj.IsPermanent ? DisplayStyle.None : DisplayStyle.Flex;
            _enableValueField.style.display = obj.IsPermanent ? DisplayStyle.None : DisplayStyle.Flex;
            _disableValueField.style.display = obj.IsPermanent ? DisplayStyle.None : DisplayStyle.Flex;
            _onDisableValues.style.display = obj.IsPermanent ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}