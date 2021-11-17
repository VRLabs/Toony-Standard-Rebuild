using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class VectorControl : PropertyControl
    {
        private readonly int _visibleCount;
        
        public bool IsXVisible { get; protected set; }
        
        public bool IsYVisible { get; protected set; }
        
        public bool IsZVisible { get; protected set; }
        
        public bool IsWVisible { get; protected set; }
        public VectorControl(string propertyName, bool isXVisible = true, bool isYVisible = true,
             bool isZVisible = true, bool isWVisible = true) : base(propertyName)
        {
            IsXVisible = isXVisible;
            IsYVisible = isYVisible;
            IsZVisible = isZVisible;
            IsWVisible = isWVisible;

            _visibleCount = 0;
            if (IsXVisible) _visibleCount++;
            if (IsYVisible) _visibleCount++;
            if (IsZVisible) _visibleCount++;
            if (IsWVisible) _visibleCount++;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            Rect r = EditorGUILayout.GetControlRect(true);
            EditorGUI.LabelField(r, Content);
            EditorGUI.showMixedValue = Property.hasMixedValue;
            EditorGUI.BeginChangeCheck();

            r = new Rect(r.x + EditorGUIUtility.labelWidth, r.y, r.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            int i = 0;
            int oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            Vector4 vector = new Vector4(0, 0, 0, 0)
            {
                x = IsXVisible ? DrawSingleField("X", Property.vectorValue.x, GetFragmentedRect(r, _visibleCount, i++)) : Property.vectorValue.x,
                y = IsYVisible ? DrawSingleField("Y", Property.vectorValue.y, GetFragmentedRect(r, _visibleCount, i++)) : Property.vectorValue.y,
                z = IsZVisible ? DrawSingleField("Z", Property.vectorValue.z, GetFragmentedRect(r, _visibleCount, i++)) : Property.vectorValue.z,
                w = IsWVisible ? DrawSingleField("W", Property.vectorValue.w, GetFragmentedRect(r, _visibleCount, i)) : Property.vectorValue.w
            };

            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            if (HasPropertyUpdated)
            {
                materialEditor.RegisterPropertyChangeUndo(Property.displayName);
                Property.vectorValue = vector;
            }

            EditorGUI.showMixedValue = false;
            EditorGUI.indentLevel = oldIndentLevel;
        }

        private static Rect GetFragmentedRect(Rect r, int count, int current)
        {
            return new Rect(r.x + (r.width * current / count), r.y, r.width / count, r.height);
        }

        private static float DrawSingleField(string label, float value, Rect r)
        {
            Rect rt = new Rect(r.x, r.y, 15, r.height);
            GUI.Label(rt, label, Styles.CenterLabel);
            rt = new Rect(r.x + 15, r.y, r.width - 15, r.height);
            return EditorGUI.FloatField(rt, value);
        }
    }
}