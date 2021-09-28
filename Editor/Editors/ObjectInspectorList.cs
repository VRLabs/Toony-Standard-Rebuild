using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRLabs.ToonyStandardRebuild
{
    public interface IVisualElementTemplate<T>
    {
        VisualElement CreateVisualElementForObject(T obj);
    }
    // Shamelessly taken from here: https://forum.unity.com/threads/custom-bindableelement.989693/    
    public class ObjectInspectorList<TElement, TValue> : VisualElement where TElement : IVisualElementTemplate<TValue> where TValue : new()
    {
        Foldout _listContainer;
        Button _addButton;
        private IVisualElementTemplate<TValue> _template;
        private List<TValue> _items;
        public List<TValue> Items
        {
            get => _items;
            set
            {
                _items = value;
                UpdateList();
            }
        }

        private bool _showElementsButtons;


        public ObjectInspectorList(string label, IVisualElementTemplate<TValue> template)
        {
            _template = template;
            _listContainer = new Foldout();
            _listContainer.text = label;
            _listContainer.contentContainer.AddToClassList("inspector-list-container");
            _listContainer.value = true;
            _addButton = new Button(AddItem);
            _addButton.text = "Add";
            _addButton.AddToClassList("inspector-list-add-button");
            Add(_listContainer);
            if (enabledSelf)
                _listContainer.Add(_addButton);
            var styleSheet = Resources.Load<StyleSheet>("TSRMMS/MSSUIElements/InspectorList");
            styleSheets.Add(styleSheet);
        }

        // Get the reference to the bound serialized object.
        /*public override void HandleEvent(EventBase evt)
        {
            var type = evt.GetType(); //SerializedObjectBindEvent is internal, so need to use reflection here
            if ((type.Name == "SerializedPropertyBindEvent") && !string.IsNullOrWhiteSpace(bindingPath))
            {
                var obj = type.GetProperty("bindProperty")?.GetValue(evt) as SerializedProperty;
                _array = obj;
                if (obj != null)
                {
                    if (_hasFoldingBeenForced) obj.isExpanded = _listContainer.value;
                    else _listContainer.value = obj.isExpanded;
                }
                // Updating it twice here doesn't cause an issue.
                UpdateList();
            }
            base.HandleEvent(evt);
        }*/

        // Refresh/recreate the list.
        public void UpdateList()
        {
            _listContainer.Clear();

            if (_items != null)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    int index = i;
                    var element = _template.CreateVisualElementForObject(_items[i]);
                    var item = new InspectorListItem(element, _items.Count, index, _showElementsButtons);
                    item.removeButton.RegisterCallback<PointerUpEvent>((_) => RemoveItem(index));
                    item.upButton.RegisterCallback<PointerUpEvent>((_) => MoveUpItem(index));
                    item.downButton.RegisterCallback<PointerUpEvent>((_) => MoveDownItem(index));
                    _listContainer.Add(item);
                }
            }
            if (enabledSelf)
                _listContainer.Add(_addButton);
        }

        // Remove an item and refresh the list
        public void RemoveItem(int index)
        {
            _items?.RemoveAt(index);
            UpdateList();
        }

        public void MoveUpItem(int index)
        {
            if (_items != null && index > 0)
            {
                (_items[index - 1], _items[index]) = (_items[index], _items[index - 1]);
                //_items.serializedObject.ApplyModifiedProperties();
            }
            UpdateList();
        }

        public void MoveDownItem(int index)
        {
            if (_items != null && index < _items.Count - 1)
            {
                (_items[index + 1], _items[index]) = (_items[index], _items[index + 1]);
                //_items.serializedObject.ApplyModifiedProperties();
            }
            UpdateList();
        }

        // Add an item and refresh the list
        public void AddItem()
        {
            if (_items == null)
                _items = new List<TValue>();
            _items.Add(new TValue());
            UpdateList();
        }
    }

    public class InspectorListItem : VisualElement
    {
        public Button removeButton;
        public Button upButton;
        public Button downButton;
        public InspectorListItem(VisualElement element, int listSize, int index, bool showButtonsText)
        {
            AddToClassList("inspector-list-item-container");

            VisualElement buttonsArea = new VisualElement();

            this.RegisterCallback<GeometryChangedEvent>(e =>
            {
                buttonsArea.ClearClassList();
                if (e.newRect.height > 60)
                {
                    buttonsArea.AddToClassList("inspector-list-buttons-container-vertical");
                    buttonsArea.Add(removeButton);
                    buttonsArea.Add(upButton);
                    buttonsArea.Add(downButton);
                }
                else
                {
                    buttonsArea.AddToClassList("inspector-list-buttons-container-horizontal");
                    buttonsArea.Add(upButton);
                    buttonsArea.Add(downButton);
                    buttonsArea.Add(removeButton);
                }
            });

            upButton = new Button();
            upButton.name = "UpInspectorListItem";
            upButton.AddToClassList("inspector-list-up-button");
            if (index == 0)
                upButton.SetEnabled(false);
            downButton = new Button();
            downButton.name = "DownInspectorListItem";
            downButton.AddToClassList("inspector-list-down-button");
            if (index >= listSize - 1)
                downButton.SetEnabled(false);
            removeButton = new Button();
            removeButton.name = "RemoveInspectorListItem";
            removeButton.AddToClassList("inspector-list-remove-button");

            if (showButtonsText)
            {
                upButton.text = "up";
                downButton.text = "down";
                removeButton.text = "-";
            }
            element.AddToClassList("inspector-list-item");

            Add(element);
            Add(buttonsArea);


        }
    }
}