using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.OdinSerializer;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.Sections;
using VRLabs.ToonyStandardRebuild.SSICustomControls;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace VRLabs.ToonyStandardRebuild
{
    public class TSRGUI : ShaderGUI, ISimpleShaderInspector
    {
        private static Vector2 _firstSettingsViewPosition;
        private static Vector2 _secondSettingsViewPosition;

        private static List<ModularShader> _loadedShaders;
        private GUIStyle _aboutLabelStyle;

        private List<ShaderModule> _availableModules;
        private ReorderableList _availableModulesList;
        private Dictionary<string, List<SimpleControl>> _controlsByModule;

        private Dictionary<string, int> _enablers;

        // Bool that determines if the current OnGUI call is the first one or not.
        private bool _isFirstLoop = true;
        private bool _refreshModules = true;
        private bool _isOptimisedShader;

        // Array containing all found languages for a specific GUI.
        private string[] _languages;
        private ShaderModule _lastSelectedModule;
        private Texture2D _logo = (Texture2D)Resources.Load("TSR/Logo");
        private OrderedSectionGroup _mainOrderedSection;
        private string _mainSectionLocalizationModulePath;
        private MaterialEditor _materialEditor;
        private List<string> _moduleErrors;
        private List<INonAnimatableProperty> _nonAnimatablePropertyControls;

        // Path of the shader.
        private string _path;
        private int[] _previousEnablerValues;

        private MaterialProperty[] _props;
        private Dictionary<OrderedSection, UpdateData> _sectionDefaults;

        // String containing the selected language.
        private string _selectedLanguage;

        // Integer containing the index of the selected language into the languages array.
        private int _selectedLanguageIndex;

        private string _settingsPath;
        private bool _showSettingsGUI;
        private List<string> _startupErrors;
        private ControlContainer _staticSections;
        private Dictionary<string, List<string>> _uiUVSets;
        private bool _updateShowSettingsGUI;
        private List<ShaderModule> _usedModules;
        private ReorderableList _usedModulesList;
        private Dictionary<string, string> _uvPropsSet;
        private Dictionary<string, Dictionary<string, int>> _uvSets;
        public ModularShader ModularShader { get; set; }
        public List<SimpleControl> Controls { get; set; }
        public static Color DefaultBgColor { get; set; } = GUI.backgroundColor;

        protected bool NeedsNonAnimatableUpdate { get; set; }

        private bool ContainsNonAnimatableProperties => _nonAnimatablePropertyControls.Count > 0;
        public Material[] Materials { get; set; }
        public Shader Shader { get; set; }

        public void AddControl(SimpleControl control, string alias = "") => Controls.AddControl(control, alias);

        public IEnumerable<SimpleControl> GetControlList() => Controls;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _props = properties;
            _materialEditor = materialEditor;
            if (Materials != null && Shader != Materials[0].shader) _isFirstLoop = _refreshModules = true;
            
            if (_isFirstLoop)
            {
                InitializeFields(materialEditor);
                LoadModularShader();

                if (ModularShader == null)
                {
                    _startupErrors.Add("The modular shader asset has not been found for this shader, this inspector works only on Toony Standard RE:Build generated shaders");
                }
                else
                {
                    Start();
                    Controls.SetInspector(this);
                    _nonAnimatablePropertyControls = (List<INonAnimatableProperty>)Controls.FindNonAnimatablePropertyControls();
                    Controls.FetchProperties(properties, out List<string> missingProperties);
                    foreach (string missingProperty in missingProperties)
                    {
                        if (!string.IsNullOrEmpty(missingProperty)) _startupErrors.Add($"The property \"{missingProperty}\" has been defined but is not available in the shader.");
                    }
                }

                _isFirstLoop = false;
            }
            else
            {
                if (!_showSettingsGUI) Controls.FetchProperties(properties);
            }

            Header();

            if (_isOptimisedShader)
            {
                EditorGUILayout.HelpBox("This shader is in an optimised state, and settings cannot be edited", MessageType.Warning);
            }
            else if (_showSettingsGUI)
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

                CheckIfShaderSwapNeeded();
            }

            Footer();

            UpdateShowSettingsUI();
        }

        private void InitializeFields(MaterialEditor materialEditor)
        {
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
            _uvSets = new Dictionary<string, Dictionary<string, int>>();
            _uiUVSets = new Dictionary<string, List<string>>();
            _uvPropsSet = new Dictionary<string, string>();

            Materials = Array.ConvertAll(materialEditor.targets, item => (Material)item);
            Shader = Materials[0].shader;
        }

        private void LoadModularShader()
        {
            if (_loadedShaders == null) _loadedShaders = TSRUtilities.FindAssetsByType<ModularShader>().ToList();
            string shaderName = null;
            _isOptimisedShader = false;
            if (Shader.name.Length > 35 && Shader.name.Substring(Materials[0].shader.name.Length - 35, 3).Equals("-g-"))
            {
                shaderName = Materials[0].shader.name.Substring(0, Materials[0].shader.name.Length - 35);
                if (shaderName.StartsWith("Hidden/"))
                    shaderName = shaderName.Substring(7);
                _isOptimisedShader = true;
            }

            if (string.IsNullOrEmpty(shaderName))
            {
                if (Shader.name.Contains("-v-"))
                {
                    if (Shader.name.StartsWith("Hidden/"))
                        shaderName = Shader.name.Substring(7);

                    shaderName = shaderName?.Replace(shaderName.Substring(shaderName.IndexOf("-v-", StringComparison.Ordinal)), "");
                }
            }

            ModularShader = string.IsNullOrEmpty(shaderName) ? 
                _loadedShaders.FirstOrDefault(x => x.LastGeneratedShaders.Contains(Shader)) : 
                _loadedShaders.FirstOrDefault(x => x.ShaderPath.Equals(shaderName));

            if (ModularShader != null) return;
            
            _loadedShaders = TSRUtilities.FindAssetsByType<ModularShader>().ToList();
            ModularShader = string.IsNullOrEmpty(shaderName) ? 
                _loadedShaders.FirstOrDefault(x => x.LastGeneratedShaders.Contains(Shader)) : 
                _loadedShaders.FirstOrDefault(x => x.ShaderPath.Equals(shaderName));
            
        }

        // Contains the ui loading system for shader and modules
        private void Start()
        {
            // Initialize main sections containers
            _staticSections = this.AddControlContainer();
            _mainSectionLocalizationModulePath = AssetDatabase.GetAssetPath(ModularShader);
            _path = AssetDatabase.GetAssetPath(Shader);
            _settingsPath = $"{Path.GetDirectoryName(_mainSectionLocalizationModulePath)}/Settings";
            _mainOrderedSection = this.AddOrderedSectionGroup("MainGroup");
            
            var loadedUVControls = new List<(string, UVSetSelectorControl)>();
            LoadLocalizationSettings(_mainSectionLocalizationModulePath);
            

            // Load main UIModule from the modular shader
            if (File.Exists(_mainSectionLocalizationModulePath))
            {
                LoadUIModule(ModularShader, _mainSectionLocalizationModulePath, out List<(string, UVSetSelectorControl)> loadedControls);
                loadedUVControls.AddRange(loadedControls);
            }
            _enablers = new Dictionary<string, int>();

            // Load all UI modules from the shader modules
            LoadModules(ModularShader.BaseModules, loadedUVControls);
            LoadModules(ModularShader.AdditionalModules, loadedUVControls);

            // Setup UvSets
            foreach ((string id, UVSetSelectorControl control) in loadedUVControls)
            {
                if (_uiUVSets.TryGetValue(id, out List<string> items))
                {
                    control.SetNewOptions(items);
                    if (_uvPropsSet.TryGetValue(control.PropertyName, out string s) && !s.Equals(id))
                        _startupErrors.Add($"UV property \"{control.PropertyName}\" has been assigned 2 different UV sets \"{s}\" and \"{id}\", this is not allowed");
                    else
                        _uvPropsSet.Add(control.PropertyName, id);
                }
                else
                {
                    _startupErrors.Add($"UV Set ID \"{id}\" has been declared in a texture control uv but has not been found in the list of available UV Sets");
                }
            }

            _previousEnablerValues = new int[_enablers.Count];
            var keys = _enablers.Keys.ToArray();
            for (int i = 0; i < _enablers.Count; i++)
                _previousEnablerValues[i] = _enablers[keys[i]];

            string variantName = ShaderGenerator.GetVariantCode(_enablers);
            string shaderName = !string.IsNullOrEmpty(variantName) ? $"Hidden/{ModularShader.ShaderPath}-v{variantName}" : $"{ModularShader.ShaderPath}";

            if (_isOptimisedShader) return;
            if (Materials[0].shader == Shader.Find(shaderName)) return;
            
            Materials.SetShader(Shader.Find(shaderName));
        }

        private void LoadModules(List<ShaderModule> modules, List<(string, UVSetSelectorControl)> loadedUVControls)
        {
            foreach (ShaderModule shaderModule in modules)
            {
                string modulePath = AssetDatabase.GetAssetPath(shaderModule);
                if (File.Exists(modulePath))
                {
                    LoadUIModule(shaderModule, modulePath, out List<(string, UVSetSelectorControl)> loadedControls);
                    loadedUVControls.AddRange(loadedControls);
                }

                if (shaderModule == null || shaderModule.EnableProperties.Count == 0 ||
                    shaderModule.EnableProperties.Any(x => string.IsNullOrWhiteSpace(x.Name)) ||
                    !(shaderModule.Templates?.Any(x => x.NeedsVariant) ?? false)) continue;

                foreach (var property in shaderModule.EnableProperties)
                {
                    if (!_enablers.ContainsKey(property.Name))
                        _enablers.Add(property.Name, (int)Materials[0].GetFloat(property.Name));
                }
            }
        }

        private void LoadUIModule(ModularShader modularShader, string modulePath, out List<(string, UVSetSelectorControl)> loadedUVControls)
        {
            ModuleUI module = LoadSerializedData(modularShader.AdditionalSerializedData);
            List<SimpleControl> loadedControls = LoadControls(module, out List<(string, IControlContainer)> uvSetControls);
            _controlsByModule.Add(modulePath, loadedControls);
            loadedUVControls = new List<(string, UVSetSelectorControl)>();
            foreach ((string key, IControlContainer uvSetControl) in uvSetControls)
            {
                if (uvSetControl is PropertyControl prop)
                {
                    var uv = uvSetControl.AddUVSetSelectorControl(prop.PropertyName + "_UV", new List<string>(new[] { "uv0" })).WithAlias(prop.ControlAlias + "_UV");
                    _controlsByModule[modulePath].Add(uv);
                    loadedUVControls.Add((key, uv));
                }
            }

            LoadMainOrderedSectionLocalization();
            loadedControls.SetInspector(this);
            loadedControls.Initialize();
            LoadModuleLocalization(loadedControls, modulePath);
        }

        private void LoadUIModule(ShaderModule shaderModule, string modulePath, out List<(string, UVSetSelectorControl)> loadedUVControls)
        {
            ModuleUI module = LoadSerializedData(shaderModule.AdditionalSerializedData);
            List<SimpleControl> loadedControls = LoadControls(module, out List<(string, IControlContainer)> uvSetControls);
            _controlsByModule.Add(modulePath, loadedControls);
            loadedUVControls = new List<(string, UVSetSelectorControl)>();
            foreach ((string key, IControlContainer uvSetControl) in uvSetControls)
            {
                if (uvSetControl is PropertyControl prop)
                {
                    var uv = uvSetControl.AddUVSetSelectorControl(prop.PropertyName + "_UV", new List<string>(new[] { "uv0" })).WithAlias(prop.ControlAlias + "_UV");
                    _controlsByModule[modulePath].Add(uv);
                    loadedUVControls.Add((key, uv));
                }
            }

            loadedControls.SetInspector(this);
            loadedControls.Initialize();
            LoadModuleLocalization(loadedControls, modulePath);
        }

        internal static ModuleUI LoadSerializedData(string serializedData)
        {
            if (string.IsNullOrWhiteSpace(serializedData))
            {
                return new ModuleUI();
            }
            else
            {
                var data = JsonUtility.FromJson<SerializedUIData>(serializedData);
                List<Object> unityObjectReferences = new List<Object>();
                foreach (var guid in data.unityGUIDReferences)
                {
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        unityObjectReferences.Add(null);
                    }
                    else
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        unityObjectReferences.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
                    }
                }

                return SerializationUtility.DeserializeValue<ModuleUI>(Encoding.UTF8.GetBytes(data.module), DataFormat.JSON, unityObjectReferences) ?? new ModuleUI();
            }
        }

        private List<SimpleControl> LoadControls(ModuleUI module, out List<(string, IControlContainer)> uvSetControls)
        {
            var loadedControls = new List<SimpleControl>();
            uvSetControls = new List<(string, IControlContainer)>();
            foreach (var moduleSection in module.Sections)
                LoadSection(module, uvSetControls, moduleSection, loadedControls);

            // UV set load
            if (module.UVSets == null) return loadedControls;
            foreach (UVSet uvSet in module.UVSets)
            {
                Dictionary<string, int> uvSetDictionary;
                List<string> uiUVSet;
                if (_uvSets.TryGetValue(uvSet.ID, out Dictionary<string, int> foundSet))
                {
                    uvSetDictionary = foundSet;
                    uiUVSet = _uiUVSets[uvSet.ID];
                }
                else
                {
                    uvSetDictionary = new Dictionary<string, int>();
                    uiUVSet = new List<string>();
                    _uvSets.Add(uvSet.ID, uvSetDictionary);
                    _uiUVSets.Add(uvSet.ID, uiUVSet);
                }

                foreach (UVItem uvItem in uvSet.Items)
                {
                    if (!uvSetDictionary.ContainsKey(uvItem.ID))
                    {
                        uvSetDictionary.Add(uvItem.ID, uvSetDictionary.Count);
                        uiUVSet.Add(uvItem.Name);
                    }
                }
            }

            return loadedControls;
        }

        private void LoadSection(ModuleUI module, List<(string, IControlContainer)> uvSetControls, SectionUI moduleSection, List<SimpleControl> loadedControls)
        {
            if (moduleSection.IsPermanent)
            {
                var section = (Section)_staticSections.Controls.FirstOrDefault(x => x.ControlAlias.Equals(moduleSection.SectionName));
                if (section == null)
                {
                    section = _staticSections.AddSection().WithAlias(moduleSection.SectionName);
                    loadedControls.Add(section);
                }

                foreach (var sectionControl in moduleSection.Controls)
                    LoadControl(section, sectionControl, loadedControls, uvSetControls);
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
                            section = _mainOrderedSection.AddOrderedSection(moduleSection.ActivatePropertyName, moduleSection.EnableValue).WithAlias(moduleSection.SectionName);
                            loadedControls.Add(section);
                            _sectionDefaults.Add(section, new UpdateData());
                        }
                        catch (ArgumentException)
                        {
                            _startupErrors.Add($"Module {module.Name}: section \"{moduleSection.SectionName}\" declares a non existent property \"{moduleSection.ActivatePropertyName}\"");
                            return;
                        }
                    }
                }

                foreach (var sectionControl in moduleSection.Controls)
                    LoadControl(section, sectionControl, loadedControls, uvSetControls);

                // Section on disable load
                if (section == null) return;
                _sectionDefaults[section].FloatProperties.AddRange(moduleSection.OnSectionDisableData.FloatProperties);
                _sectionDefaults[section].ColorProperties.AddRange(moduleSection.OnSectionDisableData.ColorProperties);
                _sectionDefaults[section].TextureProperties.AddRange(moduleSection.OnSectionDisableData.TextureProperties);
                _sectionDefaults[section].Keywords.AddRange(moduleSection.OnSectionDisableData.Keywords);
                _sectionDefaults[section].OverrideTags.AddRange(moduleSection.OnSectionDisableData.OverrideTags);
            }
        }

        private void LoadControl(IControlContainer control, ControlUI sectionControl, List<SimpleControl> loadedControls, List<(string, IControlContainer)> uvSetControls)
        {
            var newControl = sectionControl.CreateControl(control, ModularShader, out string uvSet);

            if (!string.IsNullOrWhiteSpace(uvSet) && newControl is IControlContainer ct)
                uvSetControls.Add((uvSet, ct));

            loadedControls.Add(newControl);

            if (sectionControl.CouldHaveControls() && newControl is IControlContainer container)
                foreach (var childControl in sectionControl.Controls)
                    LoadControl(container, childControl, loadedControls, uvSetControls);
        }

        private void DrawGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // Draw controls
            foreach (var control in Controls)
                control.DrawControl(materialEditor);
        }

        private void DrawSettingsGUI()
        {
            if (ModularShader == null || _isOptimisedShader) return;
            
            ReinitializeListsIfNeeded();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create new variation"))
            {
                var window = ScriptableObject.CreateInstance<NewShaderVariationWindow>();
                window.titleContent = new GUIContent("TSR New Variation");
                window.shader = ModularShader;
                window.Show();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
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
                ShaderGenerator.GenerateShader(Path.GetDirectoryName(_path), ModularShader, PostGeneration, true);
                stopwatch.Stop();
                Debug.Log($"Toony Standard RE:Build: regenerated shader \"{ModularShader.Name}\" in {stopwatch.ElapsedMilliseconds}ms");
            }

            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(_moduleErrors?.Count > 0);
            if (GUILayout.Button("Apply module changes"))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var removedEnablerValues = ModularShader.AdditionalModules
                    .Where(x => !_usedModules.Contains(x) && x.EnableProperties.Any(y => y.EnableValue != 0))
                    .SelectMany(x => x.EnableProperties.Where(y => y.EnableValue != 0)).ToList();
                ModularShader.AdditionalModules = new List<ShaderModule>(_usedModules);

                bool refreshMaterialsUVSet = false;
                var newUvSets = TSRUtilities.LoadUvSet(ModularShader);

                foreach (KeyValuePair<string, Dictionary<string, int>> uvSet in newUvSets)
                {
                    if (!_uvSets.TryGetValue(uvSet.Key, out Dictionary<string, int> oldSet)) continue;
                    if (uvSet.Value.Count >= oldSet.Count) continue;
                    refreshMaterialsUVSet = true;
                    break;
                }

                _uvSets = newUvSets;

                var materials = TSRUtilities.FindAssetsByType<Material>().Where(x => ModularShader.LastGeneratedShaders.Contains(x.shader)).ToArray();

                foreach (Material material in materials)
                {
                    if (refreshMaterialsUVSet)
                    {
                        foreach (KeyValuePair<string, string> valuePair in _uvPropsSet.Where(valuePair => material.GetFloat(valuePair.Key) >= _uvSets[valuePair.Value].Count))
                            material.SetFloat(valuePair.Key, 0);
                    }

                    if (removedEnablerValues.Count > 0)
                    {
                        foreach (EnableProperty property in removedEnablerValues.Where(property => Math.Abs(material.GetFloat(property.Name) - property.EnableValue) < 0.01))
                            material.SetFloat(property.Name, 0);
                    }

                    EditorUtility.SetDirty(material);
                }

                ShaderGenerator.GenerateShader(Path.GetDirectoryName(_path), ModularShader, PostGeneration, true);
                foreach (Material material in materials)
                {
                    if (!material.shader.name.Equals("Hidden/InternalErrorShader")) continue;
                    material.shader = ModularShader.LastGeneratedShaders[0];
                    EditorUtility.SetDirty(material);
                }
                EditorUtility.SetDirty(ModularShader);

                stopwatch.Stop();
                Debug.Log($"Toony Standard RE:Build: updated shader modules for \"{ModularShader.Name}\" in {stopwatch.ElapsedMilliseconds}ms");
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

        private void ReinitializeListsIfNeeded()
        {
            if (_availableModules == null || _refreshModules)
            {
                _availableModules = TSRUtilities.FindAssetsByType<ShaderModule>()
                    .Where(x => ModularShader.BaseModules.All(y => y != x) &&
                                ModularShader.AdditionalModules.All(y => y != x))
                    .ToList();
            }

            if (_usedModules == null || _refreshModules)
            {
                _usedModules = new List<ShaderModule>(ModularShader.AdditionalModules);
            }

            if (_usedModulesList == null || _refreshModules)
            {
                _usedModulesList = new ReorderableList(_usedModules, typeof(ShaderModule), true, false, false, false);
                _usedModulesList.headerHeight = 1;
                _usedModulesList.footerHeight = 1;
                _usedModulesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= _usedModules.Count) return;
                    GUI.Label(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _usedModules[index].Name);
                    /*if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y + 1, 20, EditorGUIUtility.singleLineHeight + 1), "►"))
                    {
                        _availableModules.Add(_usedModules[index]);
                        _usedModules.Remove(_usedModules[index]);
                        _moduleErrors = ShaderGenerator.CheckShaderIssues(ModularShader.BaseModules.Concat(_usedModules).ToList());
                        _materialEditor.Repaint();
                    }*/

                    if (isFocused && index < _usedModules.Count && _lastSelectedModule != _usedModules[index])
                    {
                        _lastSelectedModule = _usedModules[index];
                    }
                };
            }

            if (_availableModulesList == null || _refreshModules)
            {
                _availableModulesList = new ReorderableList(_availableModules, typeof(ShaderModule), true, false, false, false);
                _availableModulesList.headerHeight = 1;
                _availableModulesList.footerHeight = 1;
                _availableModulesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= _availableModules.Count) return;
                    GUI.Label(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _availableModules[index].Name);
                    /*if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y + 1, 20, EditorGUIUtility.singleLineHeight + 1), "◄"))
                    {
                        _usedModules.Add(_availableModules[index]);
                        _availableModules.Remove(_availableModules[index]);
                        _moduleErrors = ShaderGenerator.CheckShaderIssues(ModularShader.BaseModules.Concat(_usedModules).ToList());
                        _materialEditor.Repaint();
                    }*/

                    if (isFocused && index < _availableModules.Count && _lastSelectedModule != _availableModules[index])
                    {
                        _lastSelectedModule = _availableModules[index];
                    }
                };
            }

            _refreshModules = false;
        }

        private void DrawModuleSelectorsArea()
        {
            float tabWidth = EditorGUIUtility.currentViewWidth / 2 - 34;
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(tabWidth));
            EditorGUILayout.BeginVertical("RL Header", GUILayout.MinWidth(tabWidth));
            GUILayout.Label("Active Modules");
            EditorGUILayout.EndVertical();
            _firstSettingsViewPosition = EditorGUILayout.BeginScrollView(_firstSettingsViewPosition, GUILayout.MaxHeight(Math.Min(200, _usedModules.Count * 21 + 8)));
            _usedModulesList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(2);

            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("◁", GUILayout.Height(30)))
            {
                if (_availableModulesList.index >= 0 && _availableModulesList.index < _availableModulesList.count)
                {
                    _usedModules.Add(_availableModules[_availableModulesList.index]);
                    _availableModules.Remove(_availableModules[_availableModulesList.index]);
                    _moduleErrors = ShaderGenerator.CheckShaderIssues(ModularShader.BaseModules.Concat(_usedModules).ToList());
                    _materialEditor.Repaint();
                }
            }
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("▷", GUILayout.Height(30)))
            {
                if (_usedModulesList.index >= 0 && _usedModulesList.index < _usedModulesList.count)
                {
                    _availableModules.Add(_usedModules[_usedModulesList.index]);
                    _usedModules.Remove(_usedModules[_usedModulesList.index]);
                    _moduleErrors = ShaderGenerator.CheckShaderIssues(ModularShader.BaseModules.Concat(_usedModules).ToList());
                    _materialEditor.Repaint();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(2);

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
            if (ModularShader != null) GUILayout.Label($"Modular shader loaded: {ModularShader.Name}", GUILayout.MinWidth(0));
            else GUILayout.Label($"Modular shader loaded: ", GUILayout.MinWidth(0));
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
                _updateShowSettingsGUI = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (!_isOptimisedShader)
            {
                if (GUILayout.Button("Generate Optimised shader"))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Directory.CreateDirectory("Assets/VRLabs/GeneratedAssets/Shaders");
                    ShaderGenerator.GenerateMinimalShader("Assets/VRLabs/GeneratedAssets/Shaders", ModularShader, Materials, PostGeneration);
                    stopwatch.Stop();
                    Debug.Log($"Toony Standard RE:Build: generated optimised shader for {Materials.Length} material{(Materials.Length > 1 ? "s" : "")} in {stopwatch.ElapsedMilliseconds}ms");
                    _isFirstLoop = true;
                    _showSettingsGUI = false;
                }

                EditorGUILayout.Space();
            }
            else
            {
                if (GUILayout.Button("Revert to main shader"))
                {
                    Materials.SetShader(Shader.Find(ModularShader.ShaderPath));

                    _isFirstLoop = true;
                }
            }
        }

        private void Footer()
        {
            GUILayout.Space(14);
            GUILayout.BeginHorizontal();
            /*
            if (GUILayout.Button(new GUIContent(Styles.SSILogoDark, "Check the official GitHub"), "label", GUILayout.Width(32), GUILayout.Height(32)))
            {
                Application.OpenURL(TSRConstants.GITHUB_LINK);
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            if (GUILayout.Button(new GUIContent(Styles.SSILogoLight, "Check out Discord"), "label", GUILayout.Width(32), GUILayout.Height(32)))
            {
                Application.OpenURL(TSRConstants.DISCORD_LINK);
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            if (GUILayout.Button(new GUIContent(Styles.SSILogoDark, "Become a patron"), "label", GUILayout.Width(32), GUILayout.Height(32)))
            {
                Application.OpenURL(TSRConstants.PATREON_LINK);
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);*/

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(TSRConstants.TSR_VERSION, _aboutLabelStyle, GUILayout.Height(26));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        // Check if there's a need to swap shader with a variant
        private void CheckIfShaderSwapNeeded()
        {
            if (_isOptimisedShader) return;
            int[] currentValues = new int[_enablers.Count];

            var keys = _enablers.Keys.ToArray();
            for (int i = 0; i < _enablers.Count; i++)
                currentValues[i] = _enablers[keys[i]] = (int)Materials[0].GetFloat(keys[i]);

            if (currentValues.SequenceEqual(_previousEnablerValues)) return;

            _previousEnablerValues = currentValues;
            string variantName = ShaderGenerator.GetVariantCode(_enablers);
            string shaderName = !string.IsNullOrEmpty(variantName) ? $"Hidden/{ModularShader.ShaderPath}-v{variantName}" : $"{ModularShader.ShaderPath}";
            
            Materials.SetShader(Shader.Find(shaderName));
        }

        private void UpdateShowSettingsUI()
        {
            if (!_updateShowSettingsGUI) return;

            _updateShowSettingsGUI = false;
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

        public void PostGeneration(StringBuilder shaderFile, ShaderGenerator.ShaderContext context)
        {
            TSRUtilities.TSRPostGeneration(shaderFile, _uvSets);
        }
    }

    [Serializable]
    public class TSRSettingsFile
    {
        public string SelectedLanguage;
        public string[] AvailableLanguages;
    }
}