using System;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;

namespace VRLabs.ToonyStandardRebuild
{
    public class ControlsUIElement : VisualElement
    {
        public class Template : IVisualElementTemplate<ControlUI>
        {
            public VisualElement CreateVisualElementForObject(ControlUI obj)
            {
                var element = new ControlsUIElement();
                element.AssignObject(obj);
                return element;
            }
        }

        public static Template ElementTemplate = new Template();
        private TextField _controlName;
        private TextField _appendAfter;
        private PopupField<Type> _controlType;
        private VisualElement _specificControlUI;

        public ControlsUIElement()
        {
            _controlName = new TextField("Name");
            _appendAfter = new TextField("Append after");
            _controlType = new PopupField<Type>("Type", ControlInstancerUtility.ControlInstancers.Select(x => x.Key).ToList(), 0, 
                type => type.Name, type => type.Name);
            //_controlType.Init(ControlType.SpaceControl);
            _specificControlUI = new VisualElement();

            //var controlsList = new ObjectInspectorList<Template, ControlUI>("Controls", ElementTemplate);

            Add(_controlName);
            Add(_appendAfter);
            Add(_controlType);
            Add(_specificControlUI);
        }

        public void AssignObject(ControlUI obj)
        {
            _controlName.SetValueWithoutNotify(obj.Name);
            _appendAfter.SetValueWithoutNotify(obj.AppendAfter);
            if(obj.UIControlType != null)
                _controlType.SetValueWithoutNotify(obj.UIControlType);

            _controlName.RegisterValueChangedCallback(e => obj.Name = e.newValue);
            _appendAfter.RegisterValueChangedCallback(e => obj.AppendAfter = e.newValue);

            _controlType.RegisterValueChangedCallback(e =>
            {
                obj.UIControlType = e.newValue;
                InitializeTypeSpecificArea(obj);
            });

            InitializeTypeSpecificArea(obj);
        }

        private void InitializeTypeSpecificArea(ControlUI obj)
        {
            int index = IndexOf(_specificControlUI);
            
            if (obj.UIControlType != null && ControlInstancerUtility.ControlInstancers.ContainsKey(obj.UIControlType))
            {
                IControlInstancer instancer = ControlInstancerUtility.ControlInstancers[obj.UIControlType];
                _specificControlUI = instancer.InstanceEditorUI(obj);
            }
            else
            {
                obj.UIControlType = typeof(PropertyControl);
                _controlType.SetValueWithoutNotify(obj.UIControlType);
            }

            RemoveAt(index);
            Insert(index, _specificControlUI);
        }
    }
}