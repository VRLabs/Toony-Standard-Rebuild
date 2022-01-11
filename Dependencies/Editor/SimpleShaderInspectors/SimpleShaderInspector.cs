using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;


namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public abstract class SimpleShaderInspector : ShaderGUI, ISimpleShaderInspector
    {
        private string[] _languages;
        private string _selectedLanguage;
        private int _selectedLanguageIndex;
        private string _path;
        private bool _isFirstLoop = true;
        private bool _doesContainControls = true;

        private bool ContainsNonAnimatableProperties => _nonAnimatablePropertyControls.Count > 0;

        private List<INonAnimatableProperty> _nonAnimatablePropertyControls;

        private Texture2D _logo;

        public static Color DefaultBgColor { get; set; } = GUI.backgroundColor;

        public Material[] Materials { get; private set; }

        public Shader Shader { get; private set; }

        public List<SimpleControl> Controls { get; set; }

        protected string CustomLocalizationShaderName { get; set; }

        protected bool NeedsNonAnimatableUpdate { get; set; }

        protected abstract void Start();

        protected virtual void Header() { }

        protected virtual void Footer() { }

        protected virtual void StartChecks(MaterialEditor materialEditor) { }

        protected virtual void CheckChanges(MaterialEditor materialEditor) { }

        public sealed override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (_isFirstLoop)
            {
                DefaultBgColor = GUI.backgroundColor;
                _logo = EditorGUIUtility.isProSkin ? Styles.SSILogoDark : Styles.SSILogoLight;
                NeedsNonAnimatableUpdate = false;
                Controls = new List<SimpleControl>();
                Materials = Array.ConvertAll(materialEditor.targets, item => (Material)item);
                Shader = Materials[0].shader;
                Start();
                LoadLocalizations();
                Controls.SetInspector(this);
                _nonAnimatablePropertyControls = (List<INonAnimatableProperty>)Controls.FindNonAnimatablePropertyControls();
                Controls.FetchProperties(properties);
                StartChecks(materialEditor);
                _isFirstLoop = false;
                if (Controls == null || Controls.Count == 0)
                    _doesContainControls = false;
            }
            else
            {
                Controls.FetchProperties(properties);
            }

            Header();
            DrawGUI(materialEditor, properties);
            if (ContainsNonAnimatableProperties)
                SSIHelper.UpdateNonAnimatableProperties(_nonAnimatablePropertyControls, materialEditor, NeedsNonAnimatableUpdate);

            DrawFooter();

            CheckChanges(materialEditor);
        }

        private void LoadLocalizations()
        {
            if (string.IsNullOrWhiteSpace(_path))
            {
                _path = AssetDatabase.GetAssetPath(Shader);
                if (string.IsNullOrWhiteSpace(CustomLocalizationShaderName))
                    CustomLocalizationShaderName = Path.GetFileNameWithoutExtension(_path);

                _path = $"{Path.GetDirectoryName(_path)}/Localization/{CustomLocalizationShaderName}";

                if (!Directory.Exists(_path))
                    Directory.CreateDirectory(_path);
            }

            string settingsPath = $"{_path}/Settings.json";
            if (File.Exists(settingsPath))
            {
                _selectedLanguage = JsonUtility.FromJson<SettingsFile>(File.ReadAllText(settingsPath)).SelectedLanguage;
            }
            else
            {
                File.WriteAllText(settingsPath, JsonUtility.ToJson(new SettingsFile { SelectedLanguage = "English" }));
                _selectedLanguage = "English";
            }

            Controls.ApplyLocalization($"{_path}/{_selectedLanguage}.json", true);

            List<string> names = new List<string>();
            string[] localizations = Directory.GetFiles(_path);
            for (int i = 0; i < localizations.Length; i++)
            {
                if (localizations[i].EndsWith(".json", StringComparison.OrdinalIgnoreCase) && !localizations[i].EndsWith("Settings.json"))
                {
                    string name = Path.GetFileNameWithoutExtension(localizations[i]);
                    names.Add(name);
                    if (name.Equals(_selectedLanguage))
                    {
                        _selectedLanguageIndex = names.Count - 1;
                    }
                }
            }
            _languages = names.ToArray();
        }

        private void DrawGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (_doesContainControls)
            {
                if (_languages.Length > 1)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    int s = EditorGUILayout.Popup(_selectedLanguageIndex, _languages, GUILayout.Width(120));
                    if (s != _selectedLanguageIndex)
                    {
                        _selectedLanguageIndex = s;
                        _selectedLanguage = _languages[s];
                        Controls.ApplyLocalization($"{_path}/{_selectedLanguage}.json", true);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }

                foreach (var control in Controls)
                    control.DrawControl(materialEditor);
            }
            else
            {
                EditorGUILayout.HelpBox("No controls have been passed to the Start() method, therefore a default inspector has been drawn, if you are an end user of the shader try to reinstall the shader or contact the creator.", MessageType.Error);
                base.OnGUI(materialEditor, properties);
            }
        }
        
        private void DrawFooter()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical();
            Footer();
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical(GUILayout.MinHeight(42));
            SSILogo(32);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }

        private void SSILogo(float logoHeight)
        {
            if (GUILayout.Button(new GUIContent(_logo, "Check out Simple Shader Inspectors!"), Styles.BottomCenterLabel, 
                GUILayout.Width(logoHeight), GUILayout.MaxHeight(logoHeight+10), GUILayout.ExpandHeight(true)))
                Application.OpenURL("https://github.com/VRLabs/SimpleShaderInspectors");
        }
        
        public void AddControl(SimpleControl control, string alias = "") => Controls.AddControl(control, alias);
        
        public IEnumerable<SimpleControl> GetControlList() => Controls;
    }
}