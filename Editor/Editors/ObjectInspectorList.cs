using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRLabs.ToonyStandardRebuild
{
    public interface IVisualElementTemplate<in T>
    {
        VisualElement CreateVisualElementForObject(T obj);
    }
    // Shamelessly taken from here: https://forum.unity.com/threads/custom-bindableelement.989693/    
    public class ObjectInspectorList<TValue> : VisualElement where TValue : new()
    {
        public bool IsFoldoutOpen
        {
            get => _listContainer.value;
            set => _listContainer.value = value;
        }
        
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

        private bool _showElementsButtons = false;
        
        public InspectorListItem<TValue> draggedElement { get; set; }
        public bool _highlightDrops;

        private List<VisualElement> _drops;

        private VisualElement _currentDrop;


        public ObjectInspectorList(string label, IVisualElementTemplate<TValue> template)
        {
            _drops = new List<VisualElement>();
            _template = template;
            _listContainer = new Foldout();
            _listContainer.text = label;
            _listContainer.contentContainer.AddToClassList("inspector-list-container");
            _listContainer.value = true;
            _listContainer.RegisterCallback<MouseUpEvent>(e => Drop());
            _listContainer.RegisterCallback<MouseLeaveEvent>(e => Drop());
            
            _addButton = new Button(AddItem);
            _addButton.text = "Add";
            _addButton.AddToClassList("inspector-list-add-button");
            Add(_listContainer);
            if (enabledSelf)
                _listContainer.Add(_addButton);
            var styleSheet = Resources.Load<StyleSheet>("TSRMMS/MSSUIElements/InspectorList");
            styleSheets.Add(styleSheet);
        }

        private void Drop()
        {
            if (draggedElement == null) return;
            draggedElement.RemoveFromClassList("inspector-list-drag-enabled");

            if (_highlightDrops)
            {
                DeHighlightDrops();
                int dropIndex = _drops.IndexOf(_currentDrop);

                if (dropIndex == -1)
                {
                    draggedElement = null;
                    return;
                }

                if (dropIndex > draggedElement.index) dropIndex--;
                TValue item = _items[draggedElement.index];
                _items.RemoveAt(draggedElement.index);
                _items.Insert(dropIndex, item);
                UpdateList();
            }
            draggedElement = null;
        }
        
        public void HighlightDrops()
        {
            foreach (var item in _drops)
                item.AddToClassList("inspector-list-drop-area-highlight");

            _highlightDrops = true;
        }

        public void DeHighlightDrops()
        {
            foreach (var item in _drops)
                item.RemoveFromClassList("inspector-list-drop-area-highlight");

            _highlightDrops = false;
        }

        // Refresh/recreate the list.
        public void UpdateList()
        {
            _listContainer.Clear();
            _drops.Clear();

            if (_items != null)
            {
                CreateDrop();
                for (int i = 0; i < _items.Count; i++)
                {
                    int index = i;
                    var element = _template.CreateVisualElementForObject(_items[i]);
                    var item = new InspectorListItem<TValue>(this, element, _items.Count, index, _showElementsButtons);
                    item.removeButton.RegisterCallback<PointerUpEvent>((_) => RemoveItem(index));
                    item.upButton.RegisterCallback<PointerUpEvent>((_) => MoveUpItem(index));
                    item.downButton.RegisterCallback<PointerUpEvent>((_) => MoveDownItem(index));
                    _listContainer.Add(item);
                    CreateDrop();
                }
            }
            if (enabledSelf)
                _listContainer.Add(_addButton);
        }

        private void CreateDrop()
        {
            VisualElement dropArea = new VisualElement();
            dropArea.AddToClassList("inspector-list-drop-area");
            dropArea.RegisterCallback<MouseEnterEvent>(e =>
            {
                if (!_highlightDrops) return;
                dropArea.AddToClassList("inspector-list-drop-area-selected");
                _currentDrop = dropArea;
            });
            dropArea.RegisterCallback<MouseLeaveEvent>(e =>
            {
                if (!_highlightDrops) return;
                dropArea.RemoveFromClassList("inspector-list-drop-area-selected");
                if (_currentDrop == dropArea) _currentDrop = null;
            });

            _listContainer.Add(dropArea);
            _drops.Add(dropArea);
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
                (_items[index - 1], _items[index]) = (_items[index], _items[index - 1]);
            UpdateList();
        }

        public void MoveDownItem(int index)
        {
            if (_items != null && index < _items.Count - 1)
                (_items[index + 1], _items[index]) = (_items[index], _items[index + 1]);
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

    public class InspectorListItem<T> : VisualElement where T : new()
    {
        public Button removeButton;
        public Button upButton;
        public Button downButton;
        
        public VisualElement dragArea;
        
        public int index;

        private ObjectInspectorList<T> _list;
        public InspectorListItem(ObjectInspectorList<T> list, VisualElement element, int listSize, int index, bool showButtonsText)
        {
            this.index = index;
            _list = list;
            AddToClassList("inspector-list-item-container");
            
            dragArea = new VisualElement();
            dragArea.AddToClassList("inspector-list-drag-handle");
            
            dragArea.RegisterCallback<MouseDownEvent>(e =>
            {
                if (_list.draggedElement == this)
                {
                    e.StopImmediatePropagation();
                    return;
                }
                else
                {
                    _list.draggedElement = this;
                    _list.HighlightDrops();
                    this.AddToClassList("inspector-list-drag-enabled");
                }
            });

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

            Add(dragArea);
            Add(element);
            Add(buttonsArea);


        }
    }
}