using System;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.Sections
{
    public class OrderedSection : Section, IAdditionalProperties, IAdditionalLocalization
    {
        public int PushState;
        private bool _isUp;
        private bool _isDown;

        public AdditionalProperty[] AdditionalProperties { get; set; }
        
        public AdditionalLocalization[] AdditionalContent { get; set; }

        public bool HasActivatePropertyUpdated { get; protected set; }

        public bool HasSectionTurnedOn { get; set; }

        public bool Enabled { get; protected set; }


        private int _sectionPosition;
        public int SectionPosition 
        {
            get => _sectionPosition;
            set
            {
                _sectionPosition = value;
                if (string.IsNullOrWhiteSpace(_positionDictionaryKey))
                    _positionDictionaryKey = $"{ControlAlias}_{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Inspector.Materials[0]))}_SectionPosition";
                StaticDictionaries.IntDictionary.SetValue(_positionDictionaryKey, _sectionPosition);
            }
        }

        public string CustomPopupPath => AdditionalContent[0].Content.text.Equals(ControlAlias + "_" + AdditionalContent[0].Name) ? "" : AdditionalContent[0].Content.text;

        [FluentSet] public GUIStyle UpIcon { get; set; }

        [FluentSet] public GUIStyle DownIcon { get; set; }

        [FluentSet] public GUIStyle DeleteIcon { get; set; }

        [FluentSet] public Color UpColor { get; set; }

        [FluentSet] public Color DownColor { get; set; }

        [FluentSet] public Color DeleteColor { get; set; }

        protected float enableValue;

        protected float disableValue;

        private string _positionDictionaryKey;

        [LimitAccessScope(typeof(OrderedSectionGroup))]
        public OrderedSection(string activatePropertyName, string showPropertyName, float enableValue = 1,
            float disableValue = 0, float showValue = 1, float hideValue = 0) : base(showPropertyName, hideValue, showValue)
        {
            AdditionalProperties = new AdditionalProperty[1];
            AdditionalProperties[0] = new AdditionalProperty(activatePropertyName);

            AdditionalContent = new AdditionalLocalization[1];
            AdditionalContent[0] = new AdditionalLocalization{Name = "Path"};

            UpIcon = Styles.UpIcon;
            DownIcon = Styles.DownIcon;
            DeleteIcon = Styles.DeleteIcon;
            UpColor = Color.white;
            DownColor = Color.white;
            DeleteColor = Color.white;

            this.disableValue = disableValue;
            this.enableValue = enableValue;
        }

        [LimitAccessScope(typeof(OrderedSectionGroup))]
        public OrderedSection(string activatePropertyName, float enableValue = 1, float disableValue = 0)
        {
            AdditionalProperties = new AdditionalProperty[1];
            AdditionalProperties[0] = new AdditionalProperty(activatePropertyName);
            
            AdditionalContent = new AdditionalLocalization[1];
            AdditionalContent[0] = new AdditionalLocalization{Name = "Path"};

            ControlAlias = activatePropertyName;

            UpIcon = Styles.UpIcon;
            DownIcon = Styles.DownIcon;
            DeleteIcon = Styles.DeleteIcon;
            UpColor = Color.white;
            DownColor = Color.white;
            DeleteColor = Color.white;

            this.disableValue = disableValue;
            this.enableValue = enableValue;
        }

        protected void DrawSideButtons()
        {
            Color bgColor = GUI.backgroundColor;
            GUI.backgroundColor = UpColor;
            _isUp = EditorGUILayout.Toggle(_isUp, UpIcon, GUILayout.Width(14.0f), GUILayout.Height(14.0f));
            GUI.backgroundColor = DownColor;
            _isDown = EditorGUILayout.Toggle(_isDown, DownIcon, GUILayout.Width(14.0f), GUILayout.Height(14.0f));
            if (_isUp)
            {
                PushState = -1;
                _isUp = false;
            }
            else if (_isDown)
            {
                PushState = 1;
                _isDown = false;
            }

            EditorGUI.BeginChangeCheck();
            GUI.backgroundColor = DeleteColor;
            Enabled = EditorGUILayout.Toggle(Enabled, DeleteIcon, GUILayout.MaxWidth(14.0f), GUILayout.Height(14.0f));
            if (!Enabled)
            {
                AdditionalProperties[0].Property.floatValue = 0;
                SectionPosition = 0;
            }
            HasActivatePropertyUpdated = EditorGUI.EndChangeCheck();
            GUI.backgroundColor = bgColor;
        }

        public void PredrawUpdate(MaterialEditor materialEditor)
        {
            SetupEnabled(materialEditor);

            if (string.IsNullOrWhiteSpace(_positionDictionaryKey))
            {
                _positionDictionaryKey = $"{ControlAlias}_{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Inspector.Materials[0]))}_SectionPosition";

                if (StaticDictionaries.IntDictionary.TryGetValue(_positionDictionaryKey, out int position))
                {
                    _sectionPosition = position;
                }
                else
                {
                    _sectionPosition = 0;
                    StaticDictionaries.IntDictionary.SetValue(_positionDictionaryKey, _sectionPosition);
                }
                
                if(Math.Abs(AdditionalProperties[0].Property.floatValue - disableValue) > 0.001 && _sectionPosition == 0)
                    _sectionPosition = 753;

                else if(Math.Abs(AdditionalProperties[0].Property.floatValue - disableValue) < 0.001 && _sectionPosition > 0)
                    _sectionPosition = 0;
            }

            Enabled = SectionPosition > 0 && !AdditionalProperties[0].Property.hasMixedValue;
            if(!AdditionalProperties[0].Property.hasMixedValue || HasSectionTurnedOn)
                AdditionalProperties[0].Property.floatValue = Enabled ? enableValue : disableValue;
            HasActivatePropertyUpdated = false;
        }
        
        public bool HasAtLeastOneMaterialDisabled()
        {
            bool yesItHas = false;
            foreach (Material mat in Inspector.Materials)
            {
                yesItHas = Math.Abs(mat.GetFloat(AdditionalProperties[0].Property.name) - disableValue) < 0.001;
                if (yesItHas) break;
            }
            return yesItHas;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
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
            GUILayout.FlexibleSpace();

            DrawSideButtons();

            if (HasSectionTurnedOn)
                HasActivatePropertyUpdated = true;
            
            HasSectionTurnedOn = false;

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
    }
}