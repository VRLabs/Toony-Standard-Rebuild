using System;
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
using UnityEditorInternal;
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
        private bool _isOptimisedShader;

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
        private ReorderableList _availableModulesList;
        private ReorderableList _usedModulesList;
        private ShaderModule _lastSelectedModule;
        private List<string> _moduleErrors;

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
                string shaderName = null;
                if (Materials[0].shader.name.Length > 35 && Materials[0].shader.name.Substring(Materials[0].shader.name.Length - 35, 3).Equals("-g-"))
                {
                    shaderName = Materials[0].shader.name.Substring(0, Materials[0].shader.name.Length - 35);
                    if (shaderName.StartsWith("Hidden/")) 
                        shaderName = shaderName.Substring(7);
                }

                if (string.IsNullOrEmpty(shaderName))
                {
                    while (first == null && attempts < 2)
                    {
                        attempts++;
                        first = _loadedShaders.FirstOrDefault(x => x.LastGeneratedShaders.Contains(Shader));
                    }

                    ModularShader = first;
                    _isOptimisedShader = false;
                }
                else
                {
                    while (first == null && attempts < 2)
                    {
                        attempts++;
                        first = _loadedShaders.FirstOrDefault(x => x.ShaderPath.Equals(shaderName));
                    }

                    ModularShader = first;
                    _isOptimisedShader = true;
                }

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
                        if(!missingProperty.Equals("")) _startupErrors.Add($"The property \"{missingProperty}\" has been defined but is not available in the shader.");
                    }
                    /*watch.Stop();
                    Debug.Log($"Time spent on loading properties of SSI controls: {watch.ElapsedTicks} ticks");*/
                }
                _isFirstLoop = false;
            }
            else
            {
                if (!_showSettingsGUI) Controls.FetchProperties(properties);
            }
            Header();

            if (_showSettingsGUI)
            {
                DrawSettingsGUI();
            }
            else if (_isOptimisedShader)
            {
                EditorGUILayout.HelpBox("This shader is in an optimised state, and settings cannot be edited", MessageType.Warning);
                if (GUILayout.Button("Revert to main shader"))
                {
                    foreach (Material material in Materials)
                        material.shader = Shader.Find(ModularShader.ShaderPath);
                    
                    _isFirstLoop = true;
                }
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
            var newControl = sectionControl.CreateControl(control, ModularShader);
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
            if (ModularShader != null && !_isOptimisedShader)
            {
                ReinitializeListsIfNeeded();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                /*if (GUILayout.Button("Create new base"))
                {

                }*/

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
                DrawModuleSelectorsArea();

                if (_moduleErrors?.Count > 0)
                {
                    EditorGUILayout.Space(4);
                    foreach (string moduleError in _moduleErrors)
                        EditorGUILayout.HelpBox(moduleError, MessageType.Error);
                }

                EditorGUILayout.Space(8);
                DrawInfoArea();
                EditorGUILayout.Space(30);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Regenerate shader"))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    ShaderGenerator.GenerateShader(Path.GetDirectoryName(_path), ModularShader);
                    stopwatch.Stop();
                    Debug.Log($"Toony Standard RE:Build: regenerated shader \"{ModularShader.Name}\" in {stopwatch.ElapsedMilliseconds}ms");
                }

                if (GUILayout.Button("Generate Optimised shader"))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Directory.CreateDirectory("Assets/VRLabs/GeneratedAssets/Shaders");
                    ShaderGenerator.GenerateMinimalShader("Assets/VRLabs/GeneratedAssets/Shaders", ModularShader, Materials);
                    stopwatch.Stop();
                    Debug.Log($"Toony Standard RE:Build: generated optimised shader for {Materials.Length} material{(Materials.Length > 1 ? "s" : "")} in {stopwatch.ElapsedMilliseconds}ms");
                    _isFirstLoop = true;
                    _showSettingsGUI = false;
                }

                GUILayout.FlexibleSpace();
                EditorGUI.BeginDisabledGroup(_moduleErrors?.Count > 0);
                if (GUILayout.Button("Apply module changes"))
                {
                    ModularShader.AdditionalModules = new List<ShaderModule>(_usedModules);
                    ShaderGenerator.GenerateShader(Path.GetDirectoryName(_path), ModularShader);

                    EditorUtility.SetDirty(ModularShader);

                    Debug.Log($"Toony Standard RE:Build: updated shader modules for \"{ModularShader.Name}\"");
                }

                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Reset changes"))
                {
                    _usedModules = null;
                    _usedModulesList = null;
                    _availableModules = null;
                    _availableModulesList = null;
                    _lastSelectedModule = null;
                    _moduleErrors = null;
                    _materialEditor.Repaint();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void ReinitializeListsIfNeeded()
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

            if (_usedModulesList == null)
            {
                _usedModulesList = new ReorderableList(_usedModules, typeof(ShaderModule), true, false, false, false);
                _usedModulesList.headerHeight = 1;
                _usedModulesList.footerHeight = 1;
                _usedModulesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= _usedModules.Count) return;
                    GUI.Label(new Rect(rect.x, rect.y, rect.width - 15, EditorGUIUtility.singleLineHeight), _usedModules[index].Name);
                    if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y + 1, 20, EditorGUIUtility.singleLineHeight + 1), "A"))
                    {
                        _availableModules.Add(_usedModules[index]);
                        _usedModules.Remove(_usedModules[index]);
                        _moduleErrors = ShaderGenerator.CheckShaderIssues(ModularShader.BaseModules.Concat(_usedModules).ToList());
                        _materialEditor.Repaint();
                    }

                    if (isFocused && index < _usedModules.Count && _lastSelectedModule != _usedModules[index])
                    {
                        _lastSelectedModule = _usedModules[index];
                    }
                };
            }

            if (_availableModulesList == null)
            {
                _availableModulesList = new ReorderableList(_availableModules, typeof(ShaderModule), true, false, false, false);
                _availableModulesList.headerHeight = 1;
                _availableModulesList.footerHeight = 1;
                _availableModulesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= _availableModules.Count) return;
                    GUI.Label(new Rect(rect.x, rect.y, rect.width - 15, EditorGUIUtility.singleLineHeight), _availableModules[index].Name);
                    if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y + 1, 20, EditorGUIUtility.singleLineHeight + 1), "B"))
                    {
                        _usedModules.Add(_availableModules[index]);
                        _availableModules.Remove(_availableModules[index]);
                        _moduleErrors = ShaderGenerator.CheckShaderIssues(ModularShader.BaseModules.Concat(_usedModules).ToList());
                        _materialEditor.Repaint();
                    }

                    if (isFocused && index < _availableModules.Count && _lastSelectedModule != _availableModules[index])
                    {
                        _lastSelectedModule = _availableModules[index];
                    }
                };
            }
        }

        private void DrawModuleSelectorsArea()
        {
            float tabWidth = EditorGUIUtility.currentViewWidth / 2 - 20;
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(tabWidth));
            EditorGUILayout.BeginVertical("RL Header", GUILayout.MinWidth(tabWidth));
            GUILayout.Label("Active Modules");
            EditorGUILayout.EndVertical();
            _firstSettingsViewPosition = EditorGUILayout.BeginScrollView(_firstSettingsViewPosition, GUILayout.MaxHeight(Math.Min(200, _usedModules.Count * 21 + 8)));
            _usedModulesList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(tabWidth));
            EditorGUILayout.BeginVertical("RL Header", GUILayout.MinWidth(tabWidth));
            GUILayout.Label("Available Modules");
            EditorGUILayout.EndVertical();
            _secondSettingsViewPosition = EditorGUILayout.BeginScrollView(_secondSettingsViewPosition, GUILayout.MaxHeight(Math.Min(200, _availableModules.Count * 21 + 8)));
            _availableModulesList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawInfoArea()
        {
            if (_lastSelectedModule != null)
            {
                EditorGUILayout.BeginVertical(Styles.BoxHeavyBorder);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(_lastSelectedModule.Name, EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
                GUILayout.Label($"({_lastSelectedModule.Id})", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(3);
                GUILayout.Label(_lastSelectedModule.Description, EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space(5);

                if (_lastSelectedModule.ModuleDependencies.Count > 0)
                {
                    GUILayout.Label("Requires:", EditorStyles.boldLabel);
                    foreach (var s in _lastSelectedModule.ModuleDependencies)
                        GUILayout.Label(s);
                    EditorGUILayout.Space(3);
                }

                if (_lastSelectedModule.IncompatibleWith.Count > 0)
                {
                    GUILayout.Label("Incompatible With:", EditorStyles.boldLabel);
                    foreach (var s in _lastSelectedModule.IncompatibleWith)
                        GUILayout.Label(s);
                }

                EditorGUILayout.Space(3);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Author:", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
                GUILayout.Label(_lastSelectedModule.Author, GUILayout.Height(14));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Version:", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
                GUILayout.Label(_lastSelectedModule.Version, GUILayout.Height(14));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
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
            if( ModularShader != null) GUILayout.Label($"Modular shader loaded: {ModularShader.Name}");
            else  GUILayout.Label($"Modular shader loaded: ");
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
                    _usedModulesList = null;
                    _availableModules = null;
                    _availableModulesList = null;
                    _lastSelectedModule = null;
                    _moduleErrors = null;
                }
                else
                {
                    _isFirstLoop = true;
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