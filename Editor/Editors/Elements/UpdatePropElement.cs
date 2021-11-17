using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRLabs.ToonyStandardRebuild
{
    public class DisabledParameterElement<T> : VisualElement
    {
        public class Template : IVisualElementTemplate<UpdateProp<T>>
        {
            public VisualElement CreateVisualElementForObject(UpdateProp<T> obj)
            {
                var element = new DisabledParameterElement<T>();
                element.AssignObject(obj);
                return element;
            }
        }

        public static Template ElementTemplate = new Template();
        
        private TextField _parameterNameField;
        private FloatField _floatField;
        private ColorField _colorField;
        private ObjectField _textureField;
        private Toggle _keywordField;
        private TextField _overrideTagField;
        
        public DisabledParameterElement()
        {
            _parameterNameField = new TextField("Name");
            _floatField = new FloatField("Value");
            _colorField = new ColorField("Value");
            _textureField = new ObjectField("Value");
            _textureField.objectType = typeof(Texture);
            _keywordField = new Toggle("Enabled");
            _overrideTagField = new TextField("Value");
        }

        public void AssignObject(UpdateProp<T> obj)
        {
            _parameterNameField.SetValueWithoutNotify(obj.Name);
            _parameterNameField.RegisterValueChangedCallback(e => obj.Name = e.newValue);
            Clear();
            Add(_parameterNameField);

            switch (obj)
            {
                case UpdateProp<float> a:
                    _parameterNameField.label = "Property name";
                    _floatField.SetValueWithoutNotify(a.Value);
                    _floatField.RegisterValueChangedCallback(e => a.Value = e.newValue);
                    Add(_floatField);
                    break;
                case UpdateProp<Color> c:
                    _parameterNameField.label = "Property name";
                    _colorField.SetValueWithoutNotify(c.Value);
                    _colorField.RegisterValueChangedCallback(e => c.Value = e.newValue);
                    Add(_colorField);
                    break;
                case UpdateProp<Texture> t:
                    _parameterNameField.label = "Property name";
                    _textureField.SetValueWithoutNotify(t.Value);
                    _textureField.RegisterValueChangedCallback(e => t.Value = (Texture)e.newValue);
                    Add(_textureField);
                    break;
                case UpdateProp<bool> k:
                    _parameterNameField.label = "Keyword name";
                    _keywordField.SetValueWithoutNotify(k.Value);
                    _keywordField.RegisterValueChangedCallback(e => k.Value = e.newValue);
                    Add(_keywordField);
                    break;
                case UpdateProp<string> t:
                    _parameterNameField.label = "Tag name";
                    _overrideTagField.SetValueWithoutNotify(t.Value);
                    _overrideTagField.RegisterValueChangedCallback(e => t.Value = e.newValue);
                    Add(_overrideTagField);
                    break;
            }
        }
    }
}