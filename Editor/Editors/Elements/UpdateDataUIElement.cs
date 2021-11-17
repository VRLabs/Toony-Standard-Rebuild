using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRLabs.ToonyStandardRebuild
{
    public class UpdateDataUIElement : VisualElement
    {
        public string FoldoutText
        {
            get => _changedSet.text;
            set => _changedSet.text = value;
        }
        
        private Foldout _changedSet;
        private ObjectInspectorList<UpdateProp<float>> _floats;
        private ObjectInspectorList<UpdateProp<Color>> _colors;
        private ObjectInspectorList<UpdateProp<Texture>> _textures;
        private ObjectInspectorList<UpdateProp<bool>> _keywords;
        private ObjectInspectorList<UpdateProp<string>> _overrideTags;

        private IntegerField _renderQueue;
        private Toggle _enableRenderQueueField;

        public UpdateDataUIElement()
        {
            _changedSet = new Foldout();

            _floats = new ObjectInspectorList<UpdateProp<float>>("Floats", DisabledParameterElement<float>.ElementTemplate);
            _colors = new ObjectInspectorList<UpdateProp<Color>>("Colors", DisabledParameterElement<Color>.ElementTemplate);
            _textures = new ObjectInspectorList<UpdateProp<Texture>>("Textures", DisabledParameterElement<Texture>.ElementTemplate);
            _keywords = new ObjectInspectorList<UpdateProp<bool>>("Keywords", DisabledParameterElement<bool>.ElementTemplate);
            _overrideTags = new ObjectInspectorList<UpdateProp<string>>("Override Tags", DisabledParameterElement<string>.ElementTemplate);
            _enableRenderQueueField = new Toggle("Override Render Queue");
            _renderQueue = new IntegerField("Render Queue");
                
            _changedSet.Add(_floats);
            _changedSet.Add(_colors);
            _changedSet.Add(_textures);
            _changedSet.Add(_keywords);
            _changedSet.Add(_overrideTags);
            _changedSet.Add(_enableRenderQueueField);
            _changedSet.Add(_renderQueue);
            
            Add(_changedSet);
        }

        public void AssignObject(UpdateData obj)
        {
            if (obj.FloatProperties is null) obj.FloatProperties = new List<UpdateProp<float>>();
            if (obj.ColorProperties is null) obj.ColorProperties = new List<UpdateProp<Color>>();
            if (obj.TextureProperties is null) obj.TextureProperties = new List<UpdateProp<Texture>>();
            if (obj.Keywords is null) obj.Keywords = new List<UpdateProp<bool>>();
            if (obj.OverrideTags is null) obj.OverrideTags = new List<UpdateProp<string>>();
            _changedSet.value = false;
            
            _floats.IsFoldoutOpen = obj.FloatProperties.Count > 0;
            _colors.IsFoldoutOpen = obj.ColorProperties.Count > 0;
            _textures.IsFoldoutOpen = obj.TextureProperties.Count > 0;
            _keywords.IsFoldoutOpen = obj.Keywords.Count > 0;
            _overrideTags.IsFoldoutOpen = obj.OverrideTags.Count > 0;
            
            _floats.Items = obj.FloatProperties;
            _colors.Items = obj.ColorProperties;
            _textures.Items = obj.TextureProperties;
            _keywords.Items = obj.Keywords;
            _overrideTags.Items = obj.OverrideTags;

            _enableRenderQueueField.SetValueWithoutNotify(obj.SetRenderQueue);
            _enableRenderQueueField.RegisterValueChangedCallback(e =>
            {
                obj.SetRenderQueue = e.newValue;
                _renderQueue.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            
            _renderQueue.SetValueWithoutNotify(obj.RenderQueue);
            _renderQueue.RegisterValueChangedCallback(e => obj.RenderQueue = e.newValue);
            
            _renderQueue.style.display = _enableRenderQueueField.value ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}