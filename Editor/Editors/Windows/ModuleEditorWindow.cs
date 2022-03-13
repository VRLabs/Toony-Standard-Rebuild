using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;
using Debug = UnityEngine.Debug;

namespace VRLabs.ToonyStandardRebuild
{
    public class ModuleEditorWindow : EditorWindow
    {
        [MenuItem("VRLabs/Toony Standard RE:Build/Debug/Edit modules from modular shader", priority = 2)]
        private static void ShowWindow()
        {
            var window = GetWindow<ModuleEditorWindow>();
            window.titleContent = new GUIContent("TSR Module editor");
            window.Show();
        }
        
        private VisualElement _root;
        private ModularShader _shader;
        private List<ShaderModule> _availableModules;
        private List<ShaderModule> _usedModules;
        private ReorderableList _usedModulesList;
        private ShaderModule _lastSelectedModule;
        private ReorderableList _availableModulesList;
        private List<string> _moduleErrors;
        private Vector2 _firstSettingsViewPosition;
        private Vector2 _secondSettingsViewPosition;
        private Dictionary<string, Dictionary<string, int>> _uvSets;
        private string _path;

        private void CreateGUI()
        {
            _root = rootVisualElement;
            
            var modularShaderField = new ObjectField();
            modularShaderField.objectType = typeof(ModularShader);
            modularShaderField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                _shader = e.newValue as ModularShader;
                if (_shader == null) return;
                _uvSets = TSRUtilities.LoadUvSet(_shader);
                if(_shader.LastGeneratedShaders.Count > 0)
                    _path = AssetDatabase.GetAssetPath(_shader.LastGeneratedShaders[0]);
                else
                {
                    _path = EditorUtility.OpenFolderPanel("Select folder to save the the generated shaders", "Assets", "shader");
                }
            });

            IMGUIContainer container = new IMGUIContainer();
            
            container.onGUIHandler += OnGUIHandler;
            
            _root.Add(modularShaderField);
            _root.Add(container);

        }

        private void OnGUIHandler()
        {
            if (_shader == null) return;
            
            EditorGUILayout.HelpBox("Use this windows only when installing a module breaks the shader in a way that makes your unable to use the shader inspector.", MessageType.Warning);
            EditorGUILayout.HelpBox("If you remove modules that manipulate UV sets and some materials are using them, those UV sets will not be reset.", MessageType.Warning);
            
            ReinitializeListsIfNeeded();
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
                ShaderGenerator.GenerateShader(Path.GetDirectoryName(_path), _shader, PostGeneration, false);
                stopwatch.Stop();
                Debug.Log($"Toony Standard RE:Build: regenerated shader \"{_shader.Name}\" in {stopwatch.ElapsedMilliseconds}ms");
            }

            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(_moduleErrors?.Count > 0);
            if (GUILayout.Button("Apply module changes"))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                _shader.AdditionalModules = new List<ShaderModule>(_usedModules);

                var newUvSets = TSRUtilities.LoadUvSet(_shader);

                foreach (KeyValuePair<string, Dictionary<string, int>> uvSet in newUvSets)
                {
                    if (!_uvSets.TryGetValue(uvSet.Key, out Dictionary<string, int> oldSet)) continue;
                    if (uvSet.Value.Count >= oldSet.Count) continue;
                    break;
                }

                _uvSets = newUvSets;

                ShaderGenerator.GenerateShader(Path.GetDirectoryName(_path), _shader, PostGeneration);
                EditorUtility.SetDirty(_shader);

                stopwatch.Stop();
                Debug.Log($"Toony Standard RE:Build: updated shader modules for \"{_shader.Name}\" in {stopwatch.ElapsedMilliseconds}ms");
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
        
        private void ReinitializeListsIfNeeded()
        {
            if (_availableModules == null)
            {
                _availableModules = TSRUtilities.FindAssetsByType<ShaderModule>()
                    .Where(x => _shader.BaseModules.All(y => y != x) &&
                                _shader.AdditionalModules.All(y => y != x))
                    .ToList();
            }

            if (_usedModules == null)
            {
                _usedModules = new List<ShaderModule>(_shader.AdditionalModules);
            }
            
            if (_usedModulesList == null)
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

            if (_availableModulesList == null)
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
                    _moduleErrors = ShaderGenerator.CheckShaderIssues(_shader.BaseModules.Concat(_usedModules).ToList());
                    Repaint();
                }
            }
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("▷", GUILayout.Height(30)))
            {
                if (_usedModulesList.index >= 0 && _usedModulesList.index < _usedModulesList.count)
                {
                    _availableModules.Add(_usedModules[_usedModulesList.index]);
                    _usedModules.Remove(_usedModules[_usedModulesList.index]);
                    _moduleErrors = ShaderGenerator.CheckShaderIssues(_shader.BaseModules.Concat(_usedModules).ToList());
                    Repaint();
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
        
        public void PostGeneration(StringBuilder shaderFile, ShaderGenerator.ShaderContext context)
        {
            TSRUtilities.TSRPostGeneration(shaderFile, _uvSets);
        }

    }
}