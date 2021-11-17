using System;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class KeywordToggleControl : SimpleControl
    {
        public bool ToggleEnabled { get; protected set; }

        public bool HasKeywordUpdated { get; protected set; }
        
        protected readonly string Keyword;

        private Material[] _materials;

        public KeywordToggleControl(string keyword) : base(keyword)
        {
            this.Keyword = keyword;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            if (_materials == null)
                _materials = Array.ConvertAll(materialEditor.targets, item => (Material)item);
            
            EditorGUI.showMixedValue = _materials.IsKeywordMixedValue(Keyword);
            ToggleEnabled = _materials[0].IsKeywordEnabled(Keyword);

            EditorGUI.BeginChangeCheck();
            ToggleEnabled = EditorGUILayout.Toggle(Content, ToggleEnabled);
            HasKeywordUpdated = EditorGUI.EndChangeCheck();
            if (HasKeywordUpdated)
            {
                materialEditor.RegisterPropertyChangeUndo(Keyword);
                _materials.SetKeyword(Keyword, ToggleEnabled);
            }

            EditorGUI.showMixedValue = false;
        }
    }
}