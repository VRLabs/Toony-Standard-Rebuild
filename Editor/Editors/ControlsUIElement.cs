using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using System.Reflection;
using System;

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
        private EnumField _controlType;
        private VisualElement _specificControlUI;

        public ControlsUIElement()
        {
            _controlName = new TextField("Name");
            _controlType = new EnumField("Type");
            _controlType.Init(ControlType.SpaceControl);
            _specificControlUI = new VisualElement();

            //var controlsList = new ObjectInspectorList<Template, ControlUI>("Controls", ElementTemplate);

            Add(_controlName);
            Add(_controlType);
            Add(_specificControlUI);
        }

        public void AssignObject(ControlUI obj)
        {
            _controlName.SetValueWithoutNotify(obj.Name);
            _controlType.SetValueWithoutNotify(obj.ControlType);

            _controlName.RegisterValueChangedCallback(e => obj.Name = e.newValue);

            _controlType.RegisterValueChangedCallback(e =>
            {
                obj.ControlType = (ControlType)e.newValue;
                InitializeTypeSpecificArea(obj);
            });

            InitializeTypeSpecificArea(obj);
        }

        private void InitializeTypeSpecificArea(ControlUI obj)
        {
            int index = IndexOf(_specificControlUI);
            switch (obj.ControlType)
            {
                case ControlType.SpaceControl:
                    _specificControlUI = new SpaceControlUIElement(obj.Parameters);
                    break;
                case ControlType.LabelControl:
                    _specificControlUI = new LabelControlUIElement(obj.Parameters);
                    break;
                case ControlType.PropertyControl:
                    _specificControlUI = new PropetyControlUIElement(obj.Parameters);
                    break;
                case ControlType.ColorControl:
                    _specificControlUI = new ColorControlUIElement(obj.Parameters);
                    break;
                case ControlType.VectorControl:
                    _specificControlUI = new VectorControlUIElement(obj.Parameters);
                    break;
                case ControlType.TextureControl:
                    _specificControlUI = new TextureControlUIElement(obj.Parameters);
                    break;
                case ControlType.TextureGeneratorControl:
                    _specificControlUI = new TextureGeneratorControlUIElement(obj.Parameters);
                    break;
                case ControlType.GradientTextureControl:
                    _specificControlUI = new GradientTextureControlUIElement(obj.Parameters);
                    break;
                case ControlType.TilingAndOffsetControl:
                    _specificControlUI = new TilingAndOffsetControlUIElement(obj.Parameters);
                    break;
                case ControlType.ControlContainer:
                    _specificControlUI = new ControlContainerUIElement(obj.Controls);
                    break;
                case ControlType.ToggleControl:
                    _specificControlUI = new ToggleControlUIElement(obj.Parameters);
                    break;
                case ControlType.ToggleListControl:
                    _specificControlUI = new ToggleListControlUIElement(obj.Parameters, obj.Controls);
                    break;
                case ControlType.KeywordToggleControl:
                    _specificControlUI = new KeywordToggleControlUIElement(obj.Parameters);
                    break;
                case ControlType.KeywordToggleListControl:
                    _specificControlUI = new KeywordToggleListControlUIElement(obj.Parameters, obj.Controls);
                    break;
                case ControlType.LightmapEmissionControl:
                    _specificControlUI = new LightmapEmissionControlUIElement(obj.Parameters);
                    break;
                case ControlType.EnumControl:
                case ControlType.VertexStreamsControl:
                default:
                    _specificControlUI = new VisualElement();
                    break;
            }

            RemoveAt(index);
            Insert(index, _specificControlUI);
        }
    }
}