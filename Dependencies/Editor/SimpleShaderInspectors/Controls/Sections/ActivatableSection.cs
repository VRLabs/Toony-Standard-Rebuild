using System;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.Sections
{
    public class ActivatableSection : Section, IAdditionalProperties
    {
        protected float enableValue;

        protected float disableValue;

        public AdditionalProperty[] AdditionalProperties { get; set; }

        public bool HasActivatePropertyUpdated { get; protected set; }

        public bool Enabled { get; protected set; }
        public ActivatableSection(string activatePropertyName, string showPropertyName, float enableValue = 1,
            float disableValue = 0, float hideValue = 0, float showValue = 1) : base(showPropertyName, hideValue, showValue)
        {
            AdditionalProperties = new AdditionalProperty[1];
            AdditionalProperties[0] = new AdditionalProperty(activatePropertyName);
            this.disableValue = disableValue;
            this.enableValue = enableValue;
        }

        public ActivatableSection(string activatePropertyName, float enableValue = 1, float disableValue = 0)
        {
            AdditionalProperties = new AdditionalProperty[1];
            AdditionalProperties[0] = new AdditionalProperty(activatePropertyName);
            ControlAlias = activatePropertyName;
            this.disableValue = disableValue;
            this.enableValue = enableValue;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            SetupEnabled(materialEditor);

            EditorGUILayout.Space();

            Enabled = Math.Abs(AdditionalProperties[0].Property.floatValue - enableValue) < 0.001;

            GUI.backgroundColor = BackgroundColor;
            EditorGUILayout.BeginVertical(BackgroundStyle);
            GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;

            Rect r = EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            if (ShowFoldoutArrow)
                Show = EditorGUILayout.Toggle(Show, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
            
            EditorGUI.BeginChangeCheck();
            Enabled = EditorGUILayout.Toggle(Enabled, GUILayout.MaxWidth(20.0f));
            HasActivatePropertyUpdated = EditorGUI.EndChangeCheck();
            float rectWidth = ShowFoldoutArrow ? GUILayoutUtility.GetLastRect().width : 0;
            float rectHeight = GUILayoutUtility.GetRect(Content, LabelStyle).height;
            Rect r2 = new Rect(r.x + rectWidth, r.y, r.width - (rectWidth * 2), Math.Max(rectHeight, r.height));
            GUI.Label(r2, Content, LabelStyle);

            Show = GUI.Toggle(r, Show, GUIContent.none, new GUIStyle());
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            if (HasPropertyUpdated)
                UpdateEnabled(materialEditor);

            if (HasActivatePropertyUpdated)
            {
                materialEditor.RegisterPropertyChangeUndo(AdditionalProperties[0].Property.displayName);
                AdditionalProperties[0].Property.floatValue = Enabled ? enableValue : disableValue;
            }
            EditorGUILayout.EndHorizontal();

            if (!AreControlsInHeader)
                EditorGUILayout.EndVertical();
            
            if (Show)
            {
                EditorGUI.BeginDisabledGroup(!Enabled);
                DrawControls(materialEditor);
                EditorGUI.EndDisabledGroup();
            }
            
            if (AreControlsInHeader)
                EditorGUILayout.EndVertical();
        }
    }
}