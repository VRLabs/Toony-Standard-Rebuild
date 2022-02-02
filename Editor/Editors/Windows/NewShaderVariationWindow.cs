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
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;
using Debug = UnityEngine.Debug;

namespace VRLabs.ToonyStandardRebuild
{
    public class NewShaderVariationWindow : EditorWindow
    {
        [NonSerialized]
        public ModularShader shader;
        private List<ShaderModule> _availableModules;
        private List<ShaderModule> _usedModules;
        private List<ShaderModule> _baseUsedModules;
        private ReorderableList _availableModulesList;
        private ReorderableList _usedModulesList;
        private ReorderableList _baseUsedModulesList;
        private ShaderModule _lastSelectedModule;

        private bool _firstCycle = true;
        private Vector2 _firstSettingsViewPosition;
        private Vector2 _secondSettingsViewPosition;
        private Vector2 _thirdSettingsViewPosition;
        private List<string> _moduleErrors;
        private Dictionary<string, Dictionary<string, int>> _uvSets;
        private string _author;
        private string _description;
        private string _id;
        private string _name;
        private string _version;
        private string _shaderPath;

        private void OnGUI()
        {
            if (shader == null)
            {
                EditorGUILayout.HelpBox("Error finding the base shader, this window should only be opened from the \"Create new variation\" button in the shader inspector's settings", MessageType.Error);
                return;
            }
            if (_firstCycle)
            {
                _author = "AUTHOR";
                _description = "DESCRIPTION";
                _id = "ToonyStandardRebuildVariation";
                _name = "Toony Standard Rebuild Variation";
                _version = shader.Version;
                _shaderPath = "VRLabs/Toony Standard RE:Build Variations/Variation";
                InitializeLists();
                _firstCycle = false;
            }

            _id = EditorGUILayout.TextField("ID", _id);
            _name = EditorGUILayout.TextField("Name", _name);
            _author = EditorGUILayout.TextField("Author", _author);
            _version = EditorGUILayout.TextField("Version", _version);
            EditorGUILayout.LabelField("Description");
            _description = EditorGUILayout.TextArea(_description);
            _shaderPath = EditorGUILayout.TextField("Shader Path", _shaderPath);

            DrawModuleSelectorsArea();
            
            if (_moduleErrors?.Count > 0)
            {
                EditorGUILayout.Space(4);
                foreach (string moduleError in _moduleErrors)
                    EditorGUILayout.HelpBox(moduleError, MessageType.Error);
            }
            
            GUILayout.FlexibleSpace();
            DrawInfoArea();
            
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(_moduleErrors?.Count > 0);
            if (GUILayout.Button("Create new shader"))
            {
                SaveNewShader();
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
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void SaveNewShader()
        {
            string modularShaderPath = EditorUtility.SaveFilePanel("Save modular shader to file", "Assets", "TSR modular shader", "asset");
            if (string.IsNullOrWhiteSpace(modularShaderPath) || modularShaderPath.IndexOf("/Assets", StringComparison.Ordinal) == -1)
                return;
            modularShaderPath = modularShaderPath.Substring(modularShaderPath.IndexOf("/Assets", StringComparison.Ordinal) + 1);
            
            if (!Path.GetFileName(Path.GetDirectoryName(modularShaderPath)).Equals("Editor"))
            {
                EditorUtility.DisplayDialog("Error", "The folder must be an \"Editor\" folder", "Ok");
                return;
                
            }

            string shaderPath = EditorUtility.OpenFolderPanel("New Shader path", "Assets", "");
            if (string.IsNullOrWhiteSpace(shaderPath))
                return;
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var newShader = CreateInstance<ModularShader>();

            newShader.Author = _author;
            newShader.Description = _description;
            newShader.Id = _id;
            newShader.Name = _name;
            newShader.Version = _version;
            newShader.ShaderPath = _shaderPath;

            var props = new List<Property>();
            foreach (Property prop in shader.Properties)
            {
                var newProp = new Property
                {
                    Name = prop.Name,
                    Type = prop.Type,
                    DefaultValue = prop.DefaultValue,
                    DisplayName = prop.DisplayName,
                    Attributes = new List<string>(prop.Attributes)
                };

                props.Add(newProp);
            }

            newShader.Properties = props;
            newShader.CustomEditor = shader.CustomEditor;
            newShader.BaseModules = _baseUsedModules;
            newShader.AdditionalModules = _usedModules;
            newShader.ShaderTemplate = shader.ShaderTemplate;
            newShader.AdditionalSerializedData = shader.AdditionalSerializedData;
            newShader.UseTemplatesForProperties = shader.UseTemplatesForProperties;
            newShader.ShaderPropertiesTemplate = shader.ShaderPropertiesTemplate;
            newShader.LockBaseModules = true;

            newShader.AdditionalModules = new List<ShaderModule>(_usedModules);

            _uvSets = TSRUtilities.LoadUvSet(newShader);

            AssetDatabase.CreateAsset(newShader, modularShaderPath);

            string oldModularShaderPath = AssetDatabase.GetAssetPath(shader);
            
            string oldLocalizationPath = $"{Path.GetDirectoryName(oldModularShaderPath)}/Localization";
            string oldSettingsPath = $"{Path.GetDirectoryName(oldModularShaderPath)}/Settings";
            string newLocalizationPath = $"{Path.GetDirectoryName(modularShaderPath)}/Localization";
            string newSettingsPath = $"{Path.GetDirectoryName(modularShaderPath)}/Settings";

            if (!Directory.Exists(newLocalizationPath))
                Directory.CreateDirectory(newLocalizationPath);
            
            if (!Directory.Exists(newSettingsPath))
                Directory.CreateDirectory(newSettingsPath);
            
            File.Copy($"{oldSettingsPath}/{Path.GetFileNameWithoutExtension(oldModularShaderPath)}.json", $"{newSettingsPath}/{Path.GetFileNameWithoutExtension(modularShaderPath)}.json");

            foreach (var file in Directory.GetFiles(oldLocalizationPath).Where(x => !Path.GetExtension(x).Equals(".meta")))
                File.Copy(file, $"{newLocalizationPath}/{Path.GetFileName(file)}");

            try
            {
                ShaderGenerator.GenerateShader(shaderPath, newShader, PostGeneration);
                EditorUtility.SetDirty(newShader);
                stopwatch.Stop();
                Debug.Log($"Toony Standard RE:Build: updated shader modules for \"{newShader.Name}\" in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.Log($"Toony Standard RE:Build: Failed to update shader modules for \"{newShader.Name}\"");
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private void InitializeLists()
        {
            _availableModules = TSRUtilities.FindAssetsByType<ShaderModule>()
                .Where(x => shader.BaseModules.All(y => y != x) &&
                            shader.AdditionalModules.All(y => y != x))
                .ToList();

            _usedModules = new List<ShaderModule>(shader.AdditionalModules);
            _baseUsedModules = new List<ShaderModule>(shader.BaseModules);

            _usedModulesList = new ReorderableList(_usedModules, typeof(ShaderModule), true, false, false, false);
            _usedModulesList.headerHeight = 1;
            _usedModulesList.footerHeight = 1;
            _usedModulesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= _usedModules.Count) return;
                GUI.Label(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _usedModules[index].Name);

                if (isFocused && index < _usedModules.Count && _lastSelectedModule != _usedModules[index])
                {
                    _lastSelectedModule = _usedModules[index];
                }
            };
            
            _baseUsedModulesList = new ReorderableList(_baseUsedModules, typeof(ShaderModule), true, false, false, false);
            _baseUsedModulesList.headerHeight = 1;
            _baseUsedModulesList.footerHeight = 1;
            _baseUsedModulesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= _baseUsedModules.Count) return;
                GUI.Label(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _baseUsedModules[index].Name);

                if (isFocused && index < _baseUsedModules.Count && _lastSelectedModule != _baseUsedModules[index])
                {
                    _lastSelectedModule = _baseUsedModules[index];
                }
            };
            
            _availableModulesList = new ReorderableList(_availableModules, typeof(ShaderModule), true, false, false, false);
            _availableModulesList.headerHeight = 1;
            _availableModulesList.footerHeight = 1;
            _availableModulesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= _availableModules.Count) return;
                GUI.Label(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _availableModules[index].Name);

                if (isFocused && index < _availableModules.Count && _lastSelectedModule != _availableModules[index])
                {
                    _lastSelectedModule = _availableModules[index];
                }
            };
        }
        
        private void DrawModuleSelectorsArea()
        {
            float tabWidth = EditorGUIUtility.currentViewWidth / 2 - 17;
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(tabWidth + 34));

            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical("RL Header", GUILayout.MinWidth(tabWidth));
            GUILayout.Label("Base Modules");
            EditorGUILayout.EndVertical();
            _thirdSettingsViewPosition = EditorGUILayout.BeginScrollView(_thirdSettingsViewPosition, GUILayout.MaxHeight(Math.Min(200, _baseUsedModules.Count * 21 + 8)));
            _baseUsedModulesList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(2);
            
            EditorGUILayout.BeginVertical(GUILayout.MaxHeight(Math.Min(200, _baseUsedModules.Count * 21 + 8)));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("◁", GUILayout.Height(30), GUILayout.Width(30)))
            {
                if (_availableModulesList.index >= 0 && _availableModulesList.index < _availableModulesList.count)
                {
                    _baseUsedModules.Add(_availableModules[_availableModulesList.index]);
                    _availableModules.Remove(_availableModules[_availableModulesList.index]);
                    _moduleErrors = ShaderGenerator.CheckShaderIssues(_baseUsedModules.Concat(_usedModules).ToList())
                        .Concat(ShaderGenerator.CheckShaderIssues(_baseUsedModules)).Distinct().ToList();
                    Repaint();
                }
            }
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("▷", GUILayout.Height(30), GUILayout.Width(30)))
            {
                if (_baseUsedModulesList.index >= 0 && _baseUsedModulesList.index < _baseUsedModulesList.count)
                {
                    _availableModules.Add(_baseUsedModules[_baseUsedModulesList.index]);
                    _baseUsedModules.Remove(_baseUsedModules[_baseUsedModulesList.index]);
                    _moduleErrors = ShaderGenerator.CheckShaderIssues(_baseUsedModules.Concat(_usedModules).ToList())
                        .Concat(ShaderGenerator.CheckShaderIssues(_baseUsedModules)).Distinct().ToList();
                    Repaint();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            

            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginVertical("RL Header", GUILayout.MinWidth(tabWidth));
            GUILayout.Label("Active Modules");
            EditorGUILayout.EndVertical();
            _firstSettingsViewPosition = EditorGUILayout.BeginScrollView(_firstSettingsViewPosition, GUILayout.MaxHeight(Math.Min(200, _usedModules.Count * 21 + 8)));
            _usedModulesList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            

            EditorGUILayout.Space(2);
            
            EditorGUILayout.BeginVertical(GUILayout.MaxHeight(Math.Min(200, _usedModules.Count * 21 + 8)));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("◁", GUILayout.Height(30), GUILayout.Width(30)))
            {
                if (_availableModulesList.index >= 0 && _availableModulesList.index < _availableModulesList.count)
                {
                    _usedModules.Add(_availableModules[_availableModulesList.index]);
                    _availableModules.Remove(_availableModules[_availableModulesList.index]);
                    _moduleErrors = ShaderGenerator.CheckShaderIssues(_baseUsedModules.Concat(_usedModules).ToList())
                        .Concat(ShaderGenerator.CheckShaderIssues(_baseUsedModules)).Distinct().ToList();
                    Repaint();
                }
            }
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("▷", GUILayout.Height(30), GUILayout.Width(30)))
            {
                if (_usedModulesList.index >= 0 && _usedModulesList.index < _usedModulesList.count)
                {
                    _availableModules.Add(_usedModules[_usedModulesList.index]);
                    _usedModules.Remove(_usedModules[_usedModulesList.index]);
                    _moduleErrors = ShaderGenerator.CheckShaderIssues(_baseUsedModules.Concat(_usedModules).ToList())
                        .Concat(ShaderGenerator.CheckShaderIssues(_baseUsedModules)).Distinct().ToList();
                    Repaint();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(tabWidth));
            EditorGUILayout.BeginVertical("RL Header", GUILayout.MinWidth(tabWidth ));
            GUILayout.Label("Available Modules");
            EditorGUILayout.EndVertical();
            _secondSettingsViewPosition = EditorGUILayout.BeginScrollView(_secondSettingsViewPosition, GUILayout.MaxHeight(Math.Min(400, _availableModules.Count * 21 + 8)));
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
        
        public void PostGeneration(StringBuilder shaderFile, ShaderGenerator.ShaderContext context)
        {
            TSRUtilities.TSRPostGeneration(shaderFile, _uvSets);
        }
    }
}