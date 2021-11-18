using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace VRLabs.ToonyStandardRebuild
{
    public class UVItemUIElement : VisualElement
    {
        public class Template : IVisualElementTemplate<UVItem>
        {
            public VisualElement CreateVisualElementForObject(UVItem obj)
            {
                var element = new UVItemUIElement();
                element.AssignObject(obj);
                return element;
            }
        }

        public static Template ElementTemplate = new Template();

        private TextField _uvSetIdField;
        private TextField _uvSetNameField;

        public UVItemUIElement()
        {
            _uvSetIdField = new TextField("UV Item ID");
            _uvSetNameField = new TextField("UV Item Name");

            Add(_uvSetIdField);
            Add(_uvSetNameField);
        }

        public void AssignObject(UVItem obj)
        {
            _uvSetIdField.SetValueWithoutNotify(obj.ID);
            _uvSetNameField.SetValueWithoutNotify(obj.Name);

            _uvSetIdField.RegisterValueChangedCallback(e => obj.ID = e.newValue);
            _uvSetNameField.RegisterValueChangedCallback(e => obj.Name = e.newValue);
        }
    }
}