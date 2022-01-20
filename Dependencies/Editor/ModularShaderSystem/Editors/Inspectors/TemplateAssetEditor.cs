using System;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.ModularShaderSystem.UI
{
    [CustomEditor(typeof(TemplateAsset))]
    public class TemplateAssetEditor : Editor
    {
        private string _templateText;
        private GUIStyle _style;

        public void OnEnable()
        {
            _templateText = serializedObject.FindProperty("Template").stringValue;
            _style = new GUIStyle(EditorStyles.label);
            _style.wordWrap = true;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label(_templateText, _style);
        }
    }
}