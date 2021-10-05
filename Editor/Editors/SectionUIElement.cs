using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using System.Reflection;
using UnityEngine;

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
        private FloatField _enableValueField;
        private FloatField _disableValueField;
        private ObjectInspectorList<ControlUI> _controlsList;
        private Foldout _disableSet;
        private ObjectInspectorList<ShaderProperty<float>> _floatsOnDisable;
        private ObjectInspectorList<ShaderProperty<Color>> _colorsOnDisable;
        private ObjectInspectorList<ShaderProperty<Texture>> _texturesOnDisable;

        public SectionUIElement()
        {
            _sectionNameField = new TextField("Section Name");
            _enableNameField = new TextField("Property name");
            _enableValueField = new FloatField("Enable value");
            _disableValueField = new FloatField("Disable value");
            _controlsList = new ObjectInspectorList<ControlUI>("Controls", ControlsUIElement.ElementTemplate);

            _disableSet = new Foldout{ text = "On disable" };

            _floatsOnDisable = new ObjectInspectorList<ShaderProperty<float>>("Floats", DisabledParameterElement<float>.ElementTemplate);
            _colorsOnDisable = new ObjectInspectorList<ShaderProperty<Color>>("Colors", DisabledParameterElement<Color>.ElementTemplate);
            _texturesOnDisable = new ObjectInspectorList<ShaderProperty<Texture>>("Textures", DisabledParameterElement<Texture>.ElementTemplate);
            
            _disableSet.Add(_floatsOnDisable);
            _disableSet.Add(_colorsOnDisable);
            _disableSet.Add(_texturesOnDisable);
            
            Add(_sectionNameField);
            Add(_enableNameField);
            Add(_enableValueField);
            Add(_disableValueField);
            Add(_controlsList);
            Add(_disableSet);
        }

        public void AssignObject(SectionUI obj)
        {
            _sectionNameField.SetValueWithoutNotify(obj.SectionName);
            _enableNameField.SetValueWithoutNotify(obj.ActivatePropertyName);
            _enableValueField.SetValueWithoutNotify(obj.EnableValue);
            _disableValueField.SetValueWithoutNotify(obj.DisableValue);
            _controlsList.Items = obj.Controls;

            if (obj.FloatProperties is null) obj.FloatProperties = new List<ShaderProperty<float>>();
            if (obj.ColorProperties is null) obj.ColorProperties = new List<ShaderProperty<Color>>();
            if (obj.TextureProperties is null) obj.TextureProperties = new List<ShaderProperty<Texture>>();
            _disableSet.value = obj.FloatProperties?.Count + obj.ColorProperties?.Count + obj.TextureProperties?.Count > 0;
            _floatsOnDisable.Items = obj.FloatProperties;
            _colorsOnDisable.Items = obj.ColorProperties;
            _texturesOnDisable.Items = obj.TextureProperties;

            _sectionNameField.RegisterValueChangedCallback(e => obj.SectionName = e.newValue);
            _enableNameField.RegisterValueChangedCallback(e => obj.ActivatePropertyName = e.newValue);
            _enableValueField.RegisterValueChangedCallback(e => obj.EnableValue = e.newValue);
            _disableValueField.RegisterValueChangedCallback(e => obj.DisableValue = e.newValue);
        }
    }
}