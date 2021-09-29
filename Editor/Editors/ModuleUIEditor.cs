using System;
using System.Collections.Generic;
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
        [MenuItem("VRLabs/Toony Standard RE:Build/Edit UI For module")]
        public static void ShowExample()
        {
            ModuleUIEditor wnd = GetWindow<ModuleUIEditor>();
            wnd.titleContent = new GUIContent("Module UI editor");
        }

        private VisualElement _root;

        private ObjectField _modularShaderField;
        private ObjectField _shaderModuleField;
        private ObjectInspectorList<SectionUIElement.Template, SectionUI> _sectionsList;

        private bool currentSelectorUsed;

        private ModuleUI _ui;

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            _root = rootVisualElement;

            var styleSheet = Resources.Load<StyleSheet>("TSR/ModuleUI");
            _root.styleSheets.Add(styleSheet);

            _modularShaderField = new ObjectField();
            _shaderModuleField = new ObjectField();
            _modularShaderField.style.flexGrow = 1;
            _shaderModuleField.style.flexGrow = 1;

            string serializedData = null;

            _modularShaderField.objectType = typeof(ModularShader);
            _modularShaderField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                if (e.newValue != null)
                    serializedData = ((ModularShader)e.newValue).AdditionalSerializedData;
                else
                    serializedData = null;


                if (_shaderModuleField.value != null)
                    _shaderModuleField.SetValueWithoutNotify(null);

                currentSelectorUsed = false;
                OpenData(serializedData, e.newValue);
            });

            _shaderModuleField.objectType = typeof(ShaderModule);
            _shaderModuleField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                if (e.newValue != null)
                    serializedData = ((ShaderModule)e.newValue).AdditionalSerializedData;
                else
                    serializedData = null;

                if (_modularShaderField.value != null)
                    _modularShaderField.SetValueWithoutNotify(null);

                currentSelectorUsed = true;
                OpenData(serializedData, e.newValue);
            });

            _sectionsList = new ObjectInspectorList<SectionUIElement.Template, SectionUI>("Sections", SectionUIElement.ElementTemplate);
            _sectionsList.SetEnabled(false);
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
            bottomElement.Add(saveButton);
            _root.Add(topElement);
            _root.Add(view);
            _root.Add(bottomElement);
        }

        private void SaveData()
        {
            if (!currentSelectorUsed && _modularShaderField.value != null)
            {
                SerializedUIData data = new SerializedUIData();
                data.module = Encoding.ASCII.GetString(SerializationUtility.SerializeValue(_ui, DataFormat.JSON, out List<UnityEngine.Object> unityObjectReferences));
                foreach (var reference in unityObjectReferences)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out string guid, out long _))
                        data.unityGUIDReferences.Add(guid);
                    else
                        data.unityGUIDReferences.Add("");
                }
                ((ModularShader)_modularShaderField.value).AdditionalSerializedData = JsonUtility.ToJson(data);
                EditorUtility.SetDirty(_modularShaderField.value);
                AssetDatabase.SaveAssets();
            }

            if (currentSelectorUsed && _shaderModuleField.value != null)
            {
                SerializedUIData data = new SerializedUIData();
                data.module = Encoding.ASCII.GetString(SerializationUtility.SerializeValue(_ui, DataFormat.JSON, out List<UnityEngine.Object> unityObjectReferences));
                foreach (var reference in unityObjectReferences)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out string guid, out long _))
                        data.unityGUIDReferences.Add(guid);
                    else
                        data.unityGUIDReferences.Add("");
                }
                ((ShaderModule)_shaderModuleField.value).AdditionalSerializedData = JsonUtility.ToJson(data);
                EditorUtility.SetDirty(_modularShaderField.value);
                AssetDatabase.SaveAssets();
            }
        }

        private void OpenData(string serializedData, UnityEngine.Object newValue)
        {
            bool enableSectionList = false;
            if (string.IsNullOrWhiteSpace(serializedData))
            {
                _ui = new ModuleUI();
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
                _ui = SerializationUtility.DeserializeValue<ModuleUI>(Encoding.UTF8.GetBytes(data.module), DataFormat.JSON, unityObjectReferences) ?? new ModuleUI();
            }

            if (!currentSelectorUsed && newValue != null)
            {
                _ui.Name = newValue.name;
                enableSectionList = true;
                _sectionsList.Items = _ui.Sections;
            }

            if (currentSelectorUsed && newValue != null)
            {
                _ui.Name = newValue.name;
                enableSectionList = true;
                _sectionsList.Items = _ui.Sections;
            }

            _sectionsList.SetEnabled(enableSectionList);
            _sectionsList.UpdateList();
        }
    }
}
