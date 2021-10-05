using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRLabs.ToonyStandardRebuild
{
    public class DisabledParameterElement<T> : VisualElement
    {
        public class Template : IVisualElementTemplate<ShaderProperty<T>>
        {
            public VisualElement CreateVisualElementForObject(ShaderProperty<T> obj)
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
        
        public DisabledParameterElement()
        {
            _parameterNameField = new TextField("Parameter Name");
            _floatField = new FloatField("Value");
            _colorField = new ColorField("Value");
            _textureField = new ObjectField("Value");
            _textureField.objectType = typeof(Texture);
        }

        public void AssignObject(ShaderProperty<T> obj)
        {
            _parameterNameField.SetValueWithoutNotify(obj.PropertyName);
            _parameterNameField.RegisterValueChangedCallback(e => obj.PropertyName = e.newValue);
            Clear();
            Add(_parameterNameField);

            switch (obj)
            {
                case ShaderProperty<float> a:
                    _floatField.SetValueWithoutNotify(a.Value);
                    _floatField.RegisterValueChangedCallback(e => a.Value = e.newValue);
                    Add(_floatField);
                    break;
                case ShaderProperty<Color> c:
                    _colorField.SetValueWithoutNotify(c.Value);
                    _colorField.RegisterValueChangedCallback(e => c.Value = e.newValue);
                    Add(_colorField);
                    break;
                case ShaderProperty<Texture> t:
                    _textureField.SetValueWithoutNotify(t.Value);
                    _textureField.RegisterValueChangedCallback(e => t.Value = (Texture)e.newValue);
                    Add(_textureField);
                    break;
            }
        }
    }
}