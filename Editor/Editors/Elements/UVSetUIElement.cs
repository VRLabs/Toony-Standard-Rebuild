using UnityEngine.UIElements;

namespace VRLabs.ToonyStandardRebuild
{
    public class UVSetUIElement : VisualElement
    {
        public class Template : IVisualElementTemplate<UVSet>
        {
            public VisualElement CreateVisualElementForObject(UVSet obj)
            {
                var element = new UVSetUIElement();
                element.AssignObject(obj);
                return element;
            }
        }

        public static Template ElementTemplate = new Template();

        private TextField _uvSetIdField;
        private ObjectInspectorList<UVItem> _uvValues;

        public UVSetUIElement()
        {
            _uvSetIdField = new TextField("UV Set ID");
            _uvValues = new ObjectInspectorList<UVItem>("UV Items", UVItemUIElement.ElementTemplate);

            Add(_uvSetIdField);
            Add(_uvValues);
        }

        public void AssignObject(UVSet obj)
        {
            _uvSetIdField.SetValueWithoutNotify(obj.ID);
            _uvValues.Items = obj.Items;

            _uvSetIdField.RegisterValueChangedCallback(e => obj.ID = e.newValue);
        }
    }
}