using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.Sections
{
    public class Section : PropertyControl, IControlContainer, INonAnimatableProperty
    {
        protected readonly float ShowValue;

        protected readonly float HideValue;

        protected readonly bool UseDictionary;

        protected string DictionaryKey;

        protected bool FirstCycle = true;

        public List<SimpleControl> Controls { get; set; }

        public bool NonAnimatablePropertyChanged { get; set; }

        public bool Show { get; protected set; }

        [FluentSet] public GUIStyle LabelStyle { get; set; }

        [FluentSet] public GUIStyle BackgroundStyle { get; set; }

        [FluentSet] public bool AreControlsInHeader { get; set; }

        [FluentSet] public bool IsPropertyAnimatable { get; set; }

        [FluentSet] public bool ShowFoldoutArrow { get; set; }

        [FluentSet] public Color BackgroundColor { get; set; }

        public Section(string propertyName, float hideValue = 0, float showValue = 1) : base(propertyName)
        {
            InitSection();
            UseDictionary = false;
            this.HideValue = hideValue;
            this.ShowValue = showValue;
        }

        public Section() : base("SSI_UNUSED_PROP")
        {
            InitSection();
            UseDictionary = true;
            ControlAlias = "Section";
            HideValue = 0;
            ShowValue = 1;
        }

        private void InitSection()
        {
            Controls = new List<SimpleControl>();
            BackgroundColor = new Color(1, 1, 1, 1);
            LabelStyle = Styles.BoldCenter;
            BackgroundStyle = Styles.BoxHeavyBorder;
            AreControlsInHeader = false;
            IsPropertyAnimatable = false;
            ShowFoldoutArrow = true;
        }

        protected void SetupEnabled(MaterialEditor materialEditor)
        {
            if (UseDictionary)
            {
                if (!FirstCycle) return;
                
                if (string.IsNullOrWhiteSpace(DictionaryKey))
                    DictionaryKey = $"{ControlAlias}_{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Inspector.Materials[0]))}_Show";
                
                if (StaticDictionaries.BoolDictionary.TryGetValue(DictionaryKey, out bool show))
                {
                    Show = show;
                }
                else
                {
                    Show = false;
                    StaticDictionaries.BoolDictionary.SetValue(DictionaryKey, Show);
                }
                FirstCycle = false;
            }
            else
            {
                Show = Math.Abs(Property.floatValue - ShowValue) < 0.001;
            }
        }

        protected void UpdateEnabled(MaterialEditor materialEditor)
        {
            if (UseDictionary)
            {
                StaticDictionaries.BoolDictionary.SetValue(DictionaryKey, Show);
            }
            else
            {
                if (IsPropertyAnimatable)
                {
                    materialEditor.RegisterPropertyChangeUndo(Property.displayName);
                    Property.floatValue = Show ? ShowValue : HideValue;
                }
                else
                {
                    NonAnimatablePropertyChanged = true;
                }
            }
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            SetupEnabled(materialEditor);
            EditorGUILayout.Space();

            GUI.backgroundColor = BackgroundColor;
            EditorGUILayout.BeginVertical(BackgroundStyle);
            GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;
            Rect r = EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            if (ShowFoldoutArrow)
                Show = EditorGUILayout.Toggle(Show, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
            
            float rectWidth = ShowFoldoutArrow ? GUILayoutUtility.GetLastRect().width : 0;
            float rectHeight = GUILayoutUtility.GetRect(Content, LabelStyle).height;
            Rect r2 = new Rect(r.x + rectWidth, r.y, r.width - (rectWidth * 2), Math.Max(rectHeight, r.height));
            GUI.Label(r2, Content, LabelStyle);

            Show = GUI.Toggle(r, Show, GUIContent.none, new GUIStyle());
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            if (HasPropertyUpdated)
                UpdateEnabled(materialEditor);

            EditorGUILayout.EndHorizontal();

            if (!AreControlsInHeader)
                EditorGUILayout.EndVertical();
            
            if (Show)
                DrawControls(materialEditor);
            
            if (AreControlsInHeader)
                EditorGUILayout.EndVertical();
        }

        protected void DrawControls(MaterialEditor materialEditor)
        {
            EditorGUILayout.Space();
            foreach (var control in Controls)
                control.DrawControl(materialEditor);
            
            EditorGUILayout.Space();
        }

        public virtual void UpdateNonAnimatableProperty(MaterialEditor materialEditor)
        {
            if (UseDictionary || !HasPropertyUpdated) return;
            materialEditor.RegisterPropertyChangeUndo(Property.displayName);
            Property.floatValue = Show ? ShowValue : HideValue;
        }
        
        public void AddControl(SimpleControl control, string alias = "") => Controls.AddControl(control, alias);

        public IEnumerable<SimpleControl> GetControlList() => Controls;
    }
}