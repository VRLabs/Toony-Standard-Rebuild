using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class TextureControl : PropertyControl, IAdditionalProperties, IControlContainer
    {
        protected bool HasExtra1;

        protected bool HasExtra2;
        
        protected bool HasCustomInlineContent = false;

        protected bool IsOptionsButtonPressed;
        
        public List<SimpleControl> Controls { get; set; }

        public AdditionalProperty[] AdditionalProperties { get; set; }

        [Chainable] public bool ShowTilingAndOffset { get; set; }

        [Chainable] public bool HasHDRColor { get; set; }

        [Chainable] public GUIStyle OptionsButtonStyle { get; set; }
        
        [Chainable] public GUIStyle OptionsAreaStyle { get; set; }

        [Chainable] public Color OptionsButtonColor { get; set; }
        
        [Chainable] public Color OptionsAreaColor { get; set; }

        public TextureControl(string propertyName, string extraPropertyName1 = null, string extraPropertyName2 = null) : base(propertyName)
        {
            AdditionalProperties = new AdditionalProperty[2];
            AdditionalProperties[0] = new AdditionalProperty(extraPropertyName1, false);
            if (!string.IsNullOrWhiteSpace(extraPropertyName1))
                HasExtra1 = true;
            
            AdditionalProperties[1] = new AdditionalProperty(extraPropertyName2, false);
            if (!string.IsNullOrWhiteSpace(extraPropertyName2))
                HasExtra2 = true;
            
            Controls = new List<SimpleControl>();

            OptionsButtonStyle = Styles.GearIcon;
            OptionsAreaStyle = Styles.TextureBoxHeavyBorder;
            OptionsButtonColor = Color.white;
            OptionsAreaColor = Color.white;

            ShowTilingAndOffset = false;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            DrawTextureSingleLine(materialEditor);
        }

        protected void DrawTextureSingleLine(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();
            if (ShowTilingAndOffset || Controls.Count > 0 || HasCustomInlineContent)
                EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical();
            if (HasExtra2)
            {
                materialEditor.TexturePropertySingleLine(Content, Property, AdditionalProperties[0].Property, AdditionalProperties[1].Property);
            }
            else if (HasExtra1)
            {
                if (AdditionalProperties[0].Property.type == MaterialProperty.PropType.Color && HasHDRColor)
                    materialEditor.TexturePropertyWithHDRColorFixed(Content, Property, AdditionalProperties[0].Property, true);
                else
                    materialEditor.TexturePropertySingleLine(Content, Property, AdditionalProperties[0].Property);
            }
            else
            {
                materialEditor.TexturePropertySingleLine(Content, Property);
            }
            EditorGUILayout.EndVertical();
            
            if (HasCustomInlineContent)
                DrawSideContent(materialEditor);
            
            if (ShowTilingAndOffset || Controls.Count > 0)
            {
                GUI.backgroundColor = OptionsButtonColor;
                IsOptionsButtonPressed = EditorGUILayout.Toggle(IsOptionsButtonPressed, OptionsButtonStyle, GUILayout.Width(19.0f), GUILayout.Height(19.0f));
                GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;
                
            }
            if (ShowTilingAndOffset || Controls.Count > 0 || HasCustomInlineContent)
                EditorGUILayout.EndHorizontal();
            
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            
            if (IsOptionsButtonPressed)
            {
                GUI.backgroundColor = OptionsAreaColor;
                EditorGUILayout.BeginHorizontal();
                int previousIndent = EditorGUI.indentLevel;
                GUILayout.Space(EditorGUI.indentLevel * 15);
                EditorGUI.indentLevel = 0;
                EditorGUILayout.BeginVertical(OptionsAreaStyle);
                GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;
                if (ShowTilingAndOffset)
                    materialEditor.TextureScaleOffsetProperty(Property);
                foreach (var control in Controls)
                    control.DrawControl(materialEditor);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel = previousIndent;
                EditorGUILayout.EndHorizontal();
            }
        }
        
        protected virtual void DrawSideContent(MaterialEditor materialEditor)
        {
        }

        public void AddControl(SimpleControl control, string alias = "") => Controls.AddControl(control, alias);


        public IEnumerable<SimpleControl> GetControlList() => Controls;
    }
}