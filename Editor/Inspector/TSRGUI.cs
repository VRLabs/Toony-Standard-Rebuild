﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.Sections;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.OdinSerializer;
using System.Text;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls;
using Debug = UnityEngine.Debug;

namespace VRLabs.ToonyStandardRebuild
{
    public class TSRGUI : ShaderGUI, ISimpleShaderInspector
    {
        // Array containing all found languages for a specific GUI.
        private string[] _languages;
        // String containing the selected language.
        private string _selectedLanguage;
        // Integer containing the index of the selected language into the languages array.
        private int _selectedLanguageIndex;
        // Path of the shader.
        private string _path;
        private string _settingsPath;
        // Bool that determines if the current OnGUI call is the first one or not.
        private bool _isFirstLoop = true;

        private bool ContainsNonAnimatableProperties => _nonAnimatablePropertyControls.Count > 0;

        private List<INonAnimatableProperty> _nonAnimatablePropertyControls;

        public static Color DefaultBgColor { get; set; } = GUI.backgroundColor;

        public Material[] Materials { get; set; }
        public Shader Shader { get; set; }
        public ModularShader ModularShader { get; set; }
        public List<SimpleControl> Controls { get; set; }

        protected bool NeedsNonAnimatableUpdate { get; set; }

        private Texture2D _logo = (Texture2D)Resources.Load("TSR/Logo");
        private List<string> _startupErrors;
        private Dictionary<string, List<SimpleControl>> _controlsByModule;
        private Dictionary<OrderedSection, UpdateData> _sectionDefaults;
        private OrderedSectionGroup _mainOrderedSection;
        private ControlContainer _staticSections;
        private string _mainSectionLocalizationModulePath;
        
        private MaterialProperty[] _props;
        private MaterialEditor _materialEditor;
        private GUIStyle _aboutLabelStyle;
        private bool _showSettingsGUI;

        private List<ShaderModule> _availableModules;
        private List<ShaderModule> _usedModules;

        private static Vector2 _firstSettingsViewPosition;
        private static Vector2 _secondSettingsViewPosition;

        private static List<ModularShader> _loadedShaders;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _props = properties;
            _materialEditor = materialEditor;
            if (_isFirstLoop)
            {
                //Stopwatch watch = new Stopwatch();
                //watch.Start();
                DefaultBgColor = GUI.backgroundColor;

                _aboutLabelStyle = new GUIStyle(EditorStyles.miniLabel);
                _aboutLabelStyle.alignment = TextAnchor.LowerRight;
                _aboutLabelStyle.fontStyle = FontStyle.Italic;
                _aboutLabelStyle.normal.textColor = new Color(0.44f, 0.44f, 0.44f, 1f);

                NeedsNonAnimatableUpdate = false;
                _startupErrors = new List<string>();
                Controls = new List<SimpleControl>();
                _controlsByModule = new Dictionary<string, List<SimpleControl>>();
                _sectionDefaults = new Dictionary<OrderedSection, UpdateData>();
                Materials = Array.ConvertAll(materialEditor.targets, item => (Material)item);
                Shader = Materials[0].shader;
                /*watch.Stop();
                Debug.Log($"Time spent for initial bs: {watch.ElapsedTicks} ticks");
                watch.Restart();*/
                ModularShader first = null;
                int attempts = 0;
                if (_loadedShaders == null) _loadedShaders = FindAssetsByType<ModularShader>().ToList();
                while (first == null && attempts < 2)
                {
                    attempts++;
                    first = _loadedShaders.FirstOrDefault(x => x.LastGeneratedShaders.Contains(Shader));

                }
                
                ModularShader = first;
                /*watch.Stop();
                Debug.Log($"Time spent finding the modular shader: {watch.ElapsedTicks} ticks");*/
                if (ModularShader == null)
                {
                    _startupErrors.Add("The modular shader asset has not been found for this shader, this inspector works only on Toony Standard RE:Build generated shaders");
                }
                else
                {
                    //watch.Restart();
                    Start();
                    /*watch.Stop();
                    Debug.Log($"Time spent on start function: {watch.ElapsedTicks} ticks");
                    watch.Restart();*/
                    Controls.SetInspector(this);
                    _nonAnimatablePropertyControls = (List<INonAnimatableProperty>)Controls.FindNonAnimatablePropertyControls();
                    Controls.FetchProperties(properties, out List<string> missingProperties);
                    foreach (string missingProperty in missingProperties)
                    {
                        _startupErrors.Add($"The property \"{missingProperty}\" has been defined but is not available in the shader.");
                    }
                    /*watch.Stop();
                    Debug.Log($"Time spent on loading properties of SSI controls: {watch.ElapsedTicks} ticks");*/
                }
                _isFirstLoop = false;
            }
            else
            {
                Controls.FetchProperties(properties);
            }
            Header();

            if (_showSettingsGUI)
            {
                DrawSettingsGUI();
            }
            else if (_startupErrors.Count > 0)
            {
                foreach (string error in _startupErrors)
                    EditorGUILayout.HelpBox(error, MessageType.Error);
            }
            else
            {
                DrawGUI(materialEditor, properties);
                if (ContainsNonAnimatableProperties)
                    SSIHelper.UpdateNonAnimatableProperties(_nonAnimatablePropertyControls, materialEditor, NeedsNonAnimatableUpdate);

                foreach (OrderedSection section in _mainOrderedSection.Controls)
                {
                    if (!section.HasActivatePropertyUpdated || section.Enabled) continue;
                    _sectionDefaults[section].UpdateMaterials(Materials);
                }
            }

            Footer();
        }

        // Contains the ui loading system for shader and modules
        private void Start()
        {
            _staticSections = this.AddControlContainer();
            _mainOrderedSection = this.AddOrderedSectionGroup("MainGroup");
            string modularShaderPath = AssetDatabase.GetAssetPath(ModularShader);

            LoadLocalizationSettings(modularShaderPath);
            
            _mainSectionLocalizationModulePath = modularShaderPath;

            if (File.Exists(modularShaderPath))
                LoadUIModule(ModularShader, modularShaderPath);

            foreach (ShaderModule shaderModule in ModularShader.BaseModules)
            {
                string modulePath = AssetDatabase.GetAssetPath(shaderModule);
                if (File.Exists(modulePath))
                    LoadUIModule(shaderModule, modulePath);
            }

            foreach (ShaderModule shaderModule in ModularShader.AdditionalModules)
            {
                string modulePath = AssetDatabase.GetAssetPath(shaderModule);
                if (File.Exists(modulePath))
                    LoadUIModule(shaderModule, modulePath);
            }
        }

        private void LoadUIModule(ModularShader modularShader, string modulePath)
        {
            ModuleUI module = LoadSerializedData(modularShader.AdditionalSerializedData);
            List<SimpleControl> loadedControls = LoadControls(module);
            _controlsByModule.Add(modulePath, loadedControls);
            LoadMainOrderedSectionLocalization();
            LoadModuleLocalization(loadedControls, modulePath);
        }

        private void LoadUIModule(ShaderModule shaderModule, string modulePath)
        {
            ModuleUI module = LoadSerializedData(shaderModule.AdditionalSerializedData);
            List<SimpleControl> loadedControls = LoadControls(module);
            _controlsByModule.Add(modulePath, loadedControls);
            LoadModuleLocalization(loadedControls, modulePath);
        }

        private static ModuleUI LoadSerializedData(string serializedData)
        {
            if (string.IsNullOrWhiteSpace(serializedData))
            {
                return new ModuleUI();
            }
            else
            {
                var data = JsonUtility.FromJson<SerializedUIData>(serializedData);
                List<UnityEngine.Object> unityObjectReferences = new List<UnityEngine.Object>();
                foreach (var guid in data.unityGUIDReferences)
                {
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        unityObjectReferences.Add(null);
                    }
                    else
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        unityObjectReferences.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
                    }
                }
                return SerializationUtility.DeserializeValue<ModuleUI>(Encoding.UTF8.GetBytes(data.module), DataFormat.JSON, unityObjectReferences) ?? new ModuleUI();
            }
        }

        private List<SimpleControl> LoadControls(ModuleUI module)
        {
            var loadedControls = new List<SimpleControl>();
            foreach (var moduleSection in module.Sections)
            {
                if (moduleSection.IsPermanent)
                {
                    var section = (Section)_staticSections.Controls.FirstOrDefault(x => x.ControlAlias.Equals(moduleSection.SectionName));
                    if (section == null)
                    {
                        section = _staticSections.AddSection().Alias(moduleSection.SectionName);
                        loadedControls.Add(section);
                    }

                    foreach (var sectionControl in moduleSection.Controls)
                        LoadControl(section, sectionControl, loadedControls);
                }
                else
                {
                    var section = _mainOrderedSection.Controls.FirstOrDefault(x => x.ControlAlias.Equals(moduleSection.SectionName));
                    if (section == null)
                    {
                        if (string.IsNullOrEmpty(moduleSection.ActivatePropertyName))
                        {
                            _startupErrors.Add($"Module {module.Name}: section \"{moduleSection.SectionName}\" does not declare a property to use and " +
                                "it's the first module declaring this section. All modules that declare a section first need to declare a property to define when to activate it");
                        }
                        else
                        {
                            try
                            {
                                FindProperty(moduleSection.ActivatePropertyName, _props);
                                section = _mainOrderedSection.AddOrderedSection(moduleSection.ActivatePropertyName, moduleSection.EnableValue).Alias(moduleSection.SectionName);
                                loadedControls.Add(section);
                                _sectionDefaults.Add(section, new UpdateData());
                            }
                            catch (ArgumentException)
                            {
                                _startupErrors.Add($"Module {module.Name}: section \"{moduleSection.SectionName}\" declares a non existent property \"{moduleSection.ActivatePropertyName}\"");
                                continue;
                            }
                        }
                    }

                    foreach (var sectionControl in moduleSection.Controls)
                        LoadControl(section, sectionControl, loadedControls);
                    
                    _sectionDefaults[section].FloatProperties.AddRange(moduleSection.OnSectionDisableData.FloatProperties);
                    _sectionDefaults[section].ColorProperties.AddRange(moduleSection.OnSectionDisableData.ColorProperties);
                    _sectionDefaults[section].TextureProperties.AddRange(moduleSection.OnSectionDisableData.TextureProperties);
                    _sectionDefaults[section].Keywords.AddRange(moduleSection.OnSectionDisableData.Keywords);
                    _sectionDefaults[section].OverrideTags.AddRange(moduleSection.OnSectionDisableData.OverrideTags);
                }
            }

            return loadedControls;
        }

        private void LoadControl(IControlContainer control, ControlUI sectionControl, List<SimpleControl> loadedControls)
        {
            var newControl = sectionControl.CreateControl(control);
            loadedControls.Add(newControl);

            if (sectionControl.CouldHaveControls() && newControl is IControlContainer container)
                foreach (var childControl in sectionControl.Controls)
                    LoadControl(container, childControl, loadedControls);
        }

        private void DrawGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // Draw controls
            foreach (var control in Controls)
                control.DrawControl(materialEditor);

        }

        private void DrawSettingsGUI()
        {
            if (_availableModules == null)
            {
                _availableModules = FindAssetsByType<ShaderModule>()
                    .Where(x => ModularShader.BaseModules.All(y => y != x) &&
                                ModularShader.AdditionalModules.All(y => y != x))
                    .ToList();
            }

            if (_usedModules == null)
            {
                _usedModules = new List<ShaderModule>(ModularShader.AdditionalModules);
            }
            EditorGUILayout.HelpBox("Work in progress", MessageType.Warning);

            float tabWidth = EditorGUIUtility.currentViewWidth / 2 - 20;
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(tabWidth));
            GUILayout.Label("Enabled Modules");
            _firstSettingsViewPosition = EditorGUILayout.BeginScrollView(_firstSettingsViewPosition, Styles.BoxHeavyBorder, GUILayout.MaxHeight(200), GUILayout.MinHeight(20));
            for (int index = 0; index < _usedModules.Count; index++)
            {
                ShaderModule module = _usedModules[index];
                int r = DrawModuleLine(module, true);
                if (r == 0)
                    continue;

                MoveModuleInList(_usedModules, index, r);
                _materialEditor.Repaint();
                //GUILayout.Label(module.Name);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(tabWidth));
            GUILayout.Label("Available Modules");
            _secondSettingsViewPosition = EditorGUILayout.BeginScrollView(_secondSettingsViewPosition, Styles.BoxHeavyBorder, GUILayout.MaxHeight(200), GUILayout.MinHeight(20));
            for (int index = 0; index < _availableModules.Count; index++)
            {
                ShaderModule module = _availableModules[index];
                int r = DrawModuleLine(module, false);
                if (r == 0)
                    continue;

                MoveModuleInList(_availableModules, index, r);
                _materialEditor.Repaint();
                //GUILayout.Label(module.Name);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Regenerate shader"))
            {
                ShaderGenerator.GenerateMainShader(Path.GetDirectoryName(_path), ModularShader);
                Debug.Log($"Toony Standard RE:Build: regenerated shader \"{ModularShader.Name}\"");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply module changes"))
            {
                ModularShader.AdditionalModules = new List<ShaderModule>(_usedModules);
                ShaderGenerator.GenerateMainShader(Path.GetDirectoryName(_path), ModularShader);
                Debug.Log($"Toony Standard RE:Build: updated shader modules for \"{ModularShader.Name}\"");
            }
            if (GUILayout.Button("Reset changes"))
            {
                _availableModules = FindAssetsByType<ShaderModule>()
                    .Where(x => ModularShader.BaseModules.All(y => y != x) &&
                                ModularShader.AdditionalModules.All(y => y != x))
                    .ToList();
                _usedModules = new List<ShaderModule>(ModularShader.AdditionalModules);
            }
            EditorGUILayout.EndHorizontal();
        }

        private int DrawModuleLine(ShaderModule module, bool enabledModule)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(module.Name);
            GUILayout.FlexibleSpace();
            int i = 0;
            if (enabledModule)
            {
                if (GUILayout.Button("", Styles.UpIcon, GUILayout.Width(20.0f), GUILayout.Height(20.0f)))
                {
                    i = -1;
                }
                if (GUILayout.Button("", Styles.DownIcon, GUILayout.Width(20.0f), GUILayout.Height(20.0f)))
                {
                    i = 1;
                }
                if (GUILayout.Button("A"))
                {
                    _availableModules.Add(module);
                    _usedModules.Remove(module);
                    _materialEditor.Repaint();
                }
            }
            else
            {
                if (GUILayout.Button("B"))
                {
                    _availableModules.Remove(module);
                    _usedModules.Add(module);
                    _materialEditor.Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            return i;
        }

        private void MoveModuleInList(List<ShaderModule> modules, int i, int posMov)
        {
            if (posMov + i >= modules.Count || posMov + i < 0)
                return;
            (modules[i + posMov], modules[i]) = (modules[i], modules[i + posMov]);
        }

        private void Header()
        {
            float windowWidth = EditorGUIUtility.currentViewWidth;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            windowWidth -= 40;
            int width;
            int height;
            if (windowWidth < _logo.width)
            {
                width = (int)windowWidth;
                height = _logo.height * width / _logo.width;
            }
            else
            {
                width = _logo.width;
                height = _logo.height;
            }
            GUILayout.Label(_logo, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Modular shader loaded: {ModularShader.Name}");
            GUILayout.FlexibleSpace();
            // Draw the language selector only if there is more than 1 language available.
            if (_languages?.Length > 1)
            {
                int s = EditorGUILayout.Popup(_selectedLanguageIndex, _languages, GUILayout.Width(120));
                if (s != _selectedLanguageIndex)
                {
                    _selectedLanguageIndex = s;
                    _selectedLanguage = _languages[s];
                    foreach (KeyValuePair<string, List<SimpleControl>> pair in _controlsByModule)
                        LoadModuleLocalization(pair.Value, pair.Key);
                }
            }
            if (GUILayout.Button("", Styles.GearIcon, GUILayout.Width(22), GUILayout.Height(22)))
            {
                _showSettingsGUI = !_showSettingsGUI;
                if (_showSettingsGUI)
                {
                    _usedModules = null;
                    _availableModules = null;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void Footer()
        {
            GUILayout.Space(14);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(Styles.SSILogoDark, "Check the official GitHub!"), "label", GUILayout.Width(32), GUILayout.Height(32)))
            {
                Application.OpenURL("https://github.com/Cibbi/Toony-standard");
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            if (GUILayout.Button(new GUIContent(Styles.SSILogoLight, "Join our discord!"), "label", GUILayout.Width(32), GUILayout.Height(32)))
            {
                Application.OpenURL("https://discord.gg/THPSWpP");
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            if (GUILayout.Button(new GUIContent(Styles.SSILogoDark, "Want to gift me pizza every month? Become a patron!"), "label", GUILayout.Width(32), GUILayout.Height(32)))
            {
                Application.OpenURL("https://www.patreon.com/Cibbi");
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Toony Standard RE:Build test1", _aboutLabelStyle, GUILayout.Height(26));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        
        private void LoadMainOrderedSectionLocalization()
        {
            string localizationPath = $"{Path.GetDirectoryName(_mainSectionLocalizationModulePath)}/Localization";
            if (!Directory.Exists(localizationPath))
                Directory.CreateDirectory(localizationPath);
            if (File.Exists($"{localizationPath}/{_selectedLanguage}.json"))
                _mainOrderedSection.ApplyLocalization($"{localizationPath}/{_selectedLanguage}.json", true);
            else
                _mainOrderedSection.ApplyLocalization($"{localizationPath}/English.json", true);
        }

        private void LoadModuleLocalization(List<SimpleControl> loadedControls, string modulePath)
        {
            string localizationPath = $"{Path.GetDirectoryName(modulePath)}/Localization";
            if (!Directory.Exists(localizationPath))
                Directory.CreateDirectory(localizationPath);
            if (File.Exists($"{localizationPath}/{_selectedLanguage}.json"))
                loadedControls.ApplyLocalization($"{localizationPath}/{_selectedLanguage}.json", true);
            else
                loadedControls.ApplyLocalization($"{localizationPath}/English.json", true);
        }

        private void LoadLocalizationSettings(string baseModularShaderPath)
        {
            // Initializes path if it hasn't been initialized.
            if (string.IsNullOrWhiteSpace(_path) || string.IsNullOrWhiteSpace(_settingsPath))
            {
                _path = AssetDatabase.GetAssetPath(Shader);

                _settingsPath = $"{Path.GetDirectoryName(baseModularShaderPath)}/Settings";

                if (!Directory.Exists(_settingsPath))
                    Directory.CreateDirectory(_settingsPath);
            }

            // Get Settings (or create if file is missing).  
            string settingsPath = $"{_settingsPath}/{Path.GetFileNameWithoutExtension(baseModularShaderPath)}.json";
            if (File.Exists(settingsPath))
            {
                var settings = JsonUtility.FromJson<TSRSettingsFile>(File.ReadAllText(settingsPath));
                _selectedLanguage = settings.SelectedLanguage;
                _languages = settings.AvailableLanguages;
                _selectedLanguageIndex = Array.IndexOf(settings.AvailableLanguages, _selectedLanguage);
            }
            else
            {
                var settings = new TSRSettingsFile
                {
                    SelectedLanguage = "English",
                    AvailableLanguages = new[] { "English" }
                };
                File.WriteAllText(settingsPath, JsonUtility.ToJson(settings));
                _selectedLanguage = settings.SelectedLanguage;
                _languages = settings.AvailableLanguages;
                _selectedLanguageIndex = settings.AvailableLanguages.Length - 1;
            }
        }

        public void AddControl(SimpleControl control) => Controls.Add(control);

        public IEnumerable<SimpleControl> GetControlList() => Controls;

        private static T[] FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            AssetDatabase.Refresh();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).ToString().Replace("UnityEngine.", "")}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets.ToArray();
        }
    }

    [Serializable]
    public class TSRSettingsFile
    {
        public string SelectedLanguage;
        public string[] AvailableLanguages;
    }
}