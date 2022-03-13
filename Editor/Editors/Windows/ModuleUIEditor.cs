using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.OdinSerializer;

namespace VRLabs.ToonyStandardRebuild
{
    [Serializable]
    public class SerializedUIData
    {
        public string module = "";
        public List<string> unityGUIDReferences = new List<string>();
    }
    public class ModuleUIEditor : EditorWindow
    {
        [MenuItem("VRLabs/Toony Standard RE:Build/Edit UI For module", priority = 10)]
        public static void ShowExample()
        {
            ModuleUIEditor window = GetWindow<ModuleUIEditor>();
            window.titleContent = new GUIContent("Module UI editor");
            window.Show();
        }

        private VisualElement _root;

        private ObjectField _modularShaderField;
        private ObjectField _shaderModuleField;
        private Foldout _modulePropertiesFoldout;
        private ObjectField _modularShaderAliasesSourceField;
        private Foldout _modularShaderAliasesFoldout;
        private ObjectInspectorList<SectionUI> _sectionsList;
        private ObjectInspectorList<UVSet> _uvSetList;

        private bool _currentSelectorUsed;

        private ModuleUI _ui;

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            _root = rootVisualElement;

            var styleSheet = Resources.Load<StyleSheet>("TSR/ModuleUI");
            _root.styleSheets.Add(styleSheet);
            styleSheet = Resources.Load<StyleSheet>(EditorGUIUtility.isProSkin ? "TSR/ModuleUI-dark" : "TSR/ModuleUI-light");
            _root.styleSheets.Add(styleSheet);
            _root.style.flexDirection = FlexDirection.Row;

            _modularShaderField = new ObjectField();
            _shaderModuleField = new ObjectField();
            _modularShaderAliasesSourceField = new ObjectField();
            _modularShaderField.style.flexGrow = 1;
            _shaderModuleField.style.flexGrow = 1;

            string serializedData = null;

            _modulePropertiesFoldout = new Foldout();
            _modulePropertiesFoldout.text = "Module Property Names";
            _modulePropertiesFoldout.AddToClassList("top-foldout");
            
            _modularShaderAliasesFoldout = new Foldout();
            _modularShaderAliasesFoldout.text = "Current Shader Aliases";
            _modularShaderAliasesFoldout.AddToClassList("top-foldout");
            
            _modularShaderAliasesSourceField.AddToClassList("aliases-field");
            _modularShaderAliasesSourceField.objectType = typeof(ModularShader);
            _modularShaderAliasesSourceField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                _modularShaderAliasesFoldout.Clear();
                var sections = new Dictionary<string, Foldout>();
                var ms = (ModularShader)e.newValue;
                if (ms == null) return;
                var shaderData = DeserializeModuleUI(ms.AdditionalSerializedData);
                AddElementsFromModuleUI(shaderData, sections);

                foreach (ShaderModule module in ShaderGenerator.FindAllModules(ms))
                {
                    shaderData = DeserializeModuleUI(module.AdditionalSerializedData);
                    AddElementsFromModuleUI(shaderData, sections);
                }

                foreach (var foldout in sections.Select(x => x.Value).OrderBy(x => x.text))
                {
                    _modularShaderAliasesFoldout.Add(foldout);
                }
            });
            


            _modularShaderField.objectType = typeof(ModularShader);
            _modularShaderField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                serializedData = e.newValue != null ? ((ModularShader)e.newValue).AdditionalSerializedData : null;
                
                if (_shaderModuleField.value != null)
                    _shaderModuleField.SetValueWithoutNotify(null);

                _currentSelectorUsed = false;
                OpenData(serializedData, e.newValue);
                SetProperties(((ModularShader)e.newValue).Properties);
            });

            _shaderModuleField.objectType = typeof(ShaderModule);
            _shaderModuleField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                serializedData = e.newValue != null ? ((ShaderModule)e.newValue).AdditionalSerializedData : null;

                if (_modularShaderField.value != null)
                    _modularShaderField.SetValueWithoutNotify(null);

                _currentSelectorUsed = true;
                OpenData(serializedData, e.newValue);
                ShaderModule module = (ShaderModule)e.newValue;
                var props = new List<Property>();
                if (module != null)
                {
                    if (module.EnableProperties.Count > 0)
                        props.AddRange(module.EnableProperties.Where(x => !string.IsNullOrWhiteSpace(x.Name)));
                    props.AddRange(module.Properties);
                }

                SetProperties(props);
            });

            _sectionsList = new ObjectInspectorList<SectionUI>("Sections", SectionUIElement.ElementTemplate);
            _sectionsList.SetEnabled(false);
            _uvSetList = new ObjectInspectorList<UVSet>("UV Sets", UVSetUIElement.ElementTemplate);
            _uvSetList.SetEnabled(false);
            var topElement = new VisualElement();
            topElement.style.flexDirection = FlexDirection.Row;
            topElement.style.minHeight = 20;

            var bottomElement = new VisualElement();
            bottomElement.style.flexDirection = FlexDirection.RowReverse;
            bottomElement.style.minHeight = 25;
            var saveButton = new Button(SaveData);
            saveButton.text = "Save";
            saveButton.style.flexGrow = 0;
            saveButton.style.paddingBottom = 3;
            saveButton.style.paddingLeft = 9;
            saveButton.style.paddingRight = 9;
            saveButton.style.paddingTop = 3;

            topElement.Add(_modularShaderField);
            topElement.Add(_shaderModuleField);
            var view = new ScrollView(ScrollViewMode.Vertical);
            view.style.flexGrow = 1;
            view.Add(_sectionsList);
            view.Add(_uvSetList);
            bottomElement.Add(saveButton);

            var leftColumn = new VisualElement();
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            leftColumn.name = "LeftColumn";
            var leftColumnTitle = new Label("Clippy");
            leftColumnTitle.AddToClassList("column-title");
            leftColumn.Add(leftColumnTitle);
            leftColumn.Add(scroll);
            scroll.Add(_modulePropertiesFoldout);
            scroll.Add(_modularShaderAliasesSourceField);
            scroll.Add(_modularShaderAliasesFoldout);

            var centerColumn = new VisualElement();
            centerColumn.name = "CenterColumn";
            centerColumn.Add(topElement);
            centerColumn.Add(view);
            centerColumn.Add(bottomElement);

            _root.Add(leftColumn);
            _root.Add(centerColumn);
        }

        private void SaveData()
        {
            if (!_currentSelectorUsed && _modularShaderField.value != null)
            {
                SerializedUIData data = new SerializedUIData();
                data.module = Encoding.ASCII.GetString(SerializationUtility.SerializeValue(_ui, DataFormat.JSON, out List<UnityEngine.Object> unityObjectReferences));
                foreach (var reference in unityObjectReferences)
                    data.unityGUIDReferences.Add(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out string guid, out long _) ? guid : "");
                
                ((ModularShader)_modularShaderField.value).AdditionalSerializedData = JsonUtility.ToJson(data);
                EditorUtility.SetDirty(_modularShaderField.value);
                AssetDatabase.SaveAssets();
            }

            if (_currentSelectorUsed && _shaderModuleField.value != null)
            {
                SerializedUIData data = new SerializedUIData();
                data.module = Encoding.ASCII.GetString(SerializationUtility.SerializeValue(_ui, DataFormat.JSON, out List<UnityEngine.Object> unityObjectReferences));
                foreach (var reference in unityObjectReferences)
                    data.unityGUIDReferences.Add(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out string guid, out long _) ? guid : "");
                
                ((ShaderModule)_shaderModuleField.value).AdditionalSerializedData = JsonUtility.ToJson(data);
                EditorUtility.SetDirty(_shaderModuleField.value);
                AssetDatabase.SaveAssets();
            }
        }

        private void OpenData(string serializedData, UnityEngine.Object newValue)
        {
            bool enableSectionList = false;
            _ui = DeserializeModuleUI(serializedData);

            if (!_currentSelectorUsed && newValue != null)
            {
                _ui.Name = newValue.name;
                enableSectionList = true;
                _sectionsList.Items = _ui.Sections;
                _uvSetList.Items = _ui.UVSets;
            }

            if (_currentSelectorUsed && newValue != null)
            {
                _ui.Name = newValue.name;
                enableSectionList = true;
                _sectionsList.Items = _ui.Sections;
                _uvSetList.Items = _ui.UVSets;
            }

            _sectionsList.SetEnabled(enableSectionList);
            _sectionsList.UpdateList();
            _uvSetList.SetEnabled(enableSectionList);
            _uvSetList.UpdateList();
        }

        private static ModuleUI DeserializeModuleUI(string serializedData)
        {
            if (string.IsNullOrWhiteSpace(serializedData))
            {
                return new ModuleUI();
            }

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
        
        private static void AddElementsFromModuleUI(ModuleUI shaderData, Dictionary<string, Foldout> sections)
        {
            foreach (var section in shaderData.Sections)
            {
                if (!sections.ContainsKey(section.SectionName))
                {
                    var f = new Foldout();
                    f.text = section.SectionName;
                    sections.Add(section.SectionName, f);
                }

                AddElementNames(section, section.Controls, sections);
            }
        }

        private static void AddElementNames(SectionUI section, IEnumerable<ControlUI> controls, Dictionary<string, Foldout> sections)
        {
            foreach (ControlUI control in controls)
            {
                sections[section.SectionName].Add(new NameCopyElement(control.Name, control.UIControlType?.Name));
                if (control.CouldHaveControls())
                    AddElementNames(section, control.Controls, sections);
            }
        }

        private void SetProperties(IEnumerable<Property> props)
        {
            _modulePropertiesFoldout.Clear();
            foreach (var prop in props)
            {
                if (string.IsNullOrEmpty(prop.Name))
                    continue;

                _modulePropertiesFoldout.Add(new NameCopyElement(prop.Name, prop.Type));
            }
        }
    }

    public class NameCopyElement : VisualElement
    {
        public NameCopyElement(string name, string type)
        {
            var nameLabel = new Label(name);
            nameLabel.AddToClassList("copy-name");
            var typeLabel = new Label($"({type})");
            typeLabel.AddToClassList("copy-type");
            var button = new Button();
            button.AddToClassList("copy-button");
            var buttonContent = new VisualElement();

            button.clicked += () => GUIUtility.systemCopyBuffer = name;

            Add(nameLabel);
            Add(typeLabel);
            Add(button);
            button.Add(buttonContent);
        }
    }
}
