using UnityEngine.UIElements;
using UnityEditor.UIElements;
using VRLabs.ToonyStandardRebuild.SSICustomControls;

namespace VRLabs.ToonyStandardRebuild
{
    public class ListSelectorItemUIElement : VisualElement
    {
        public class Template : IVisualElementTemplate<ListSelectorControl.ListSelectorItem>
        {
            public VisualElement CreateVisualElementForObject(ListSelectorControl.ListSelectorItem obj)
            {
                var element = new ListSelectorItemUIElement();
                element.AssignObject(obj);
                return element;
            }
        }

        public static Template ElementTemplate = new Template();

        private TextField _itemNameField;
        private IntegerField _elementValueField;
        private ChangedDataUIElement _onSelectValues;

        public ListSelectorItemUIElement()
        {
            _itemNameField = new TextField("Element name");
            _elementValueField = new IntegerField("Element value");
            _onSelectValues = new ChangedDataUIElement { FoldoutText = "On select" };

            Add(_itemNameField);
            Add(_elementValueField);
            Add(_onSelectValues);
        }

        public void AssignObject(ListSelectorControl.ListSelectorItem obj)
        {
            _itemNameField.SetValueWithoutNotify(obj.Name);
            _elementValueField.SetValueWithoutNotify(obj.Index);

            if (obj.PropsOnSelected == null) obj.PropsOnSelected = new UpdateData();
            _onSelectValues.AssignObject(obj.PropsOnSelected);

            _itemNameField.RegisterValueChangedCallback(e => obj.Name = e.newValue);
            _elementValueField.RegisterValueChangedCallback(e => obj.Index = e.newValue);
        }

    }
}