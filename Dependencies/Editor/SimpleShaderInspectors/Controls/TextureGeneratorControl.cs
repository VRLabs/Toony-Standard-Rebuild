using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class TextureGeneratorControl : TextureControl, IAdditionalLocalization
    {
        internal readonly AdditionalLocalization[] baseContent;
        internal AdditionalLocalization[] namesContent;
        
        private bool _isGeneratorOpen;
        private readonly Shader _shader;
        private Resolution _resolution;
        private bool _linearSpace;
        
        private CustomRenderTexture _crt;
        private Material _crtMaterial;
        private MaterialEditor _crtMaterialEditor;
        private DefaultGeneratorGUI _crtEditorGUI;
        private PropertyInfo[] _propertyInfos;
        private Texture2D[] _previousTextures;
        private float _width;
        private float _windowWidth;
        private List<Shader> _shaderStack;
        private static readonly string[] _baseNames =
        {
            "GeneratorOpen",        //[0]
            "GeneratorSaveButton",  //[1]
            "GeneratorCancelButton",//[2]
            "GeneratorTextureSize", //[3]
            "GeneratorPreview",     //[4]
            "GeneratorLinearSpace"  //[5]
        };
        
        private ISimpleShaderInspector _inspector;

        [FluentSet] public GUIStyle GeneratorButtonStyle { get; set; }
        
        [FluentSet] public GUIStyle GeneratorSaveButtonStyle { get; set; }
        
        [FluentSet] public GUIStyle GeneratorCloseButtonStyle { get; set; }

        [FluentSet] public GUIStyle GeneratorStyle { get; set; }

        [FluentSet] public GUIStyle GeneratorInputStyle { get; set; }

        [FluentSet] public Color GeneratorButtonColor { get; set; }

        [FluentSet] public Color GeneratorSaveButtonColor { get; set; }
        
        [FluentSet] public Color GeneratorCloseButtonColor { get; set; }

        [FluentSet] public Color GeneratorColor { get; set; }

        [FluentSet] public Color GeneratorInputColor { get; set; }

        public AdditionalLocalization[] AdditionalContent
        {
            get
            {
                List<AdditionalLocalization> content = new List<AdditionalLocalization>();
                content.AddRange(baseContent);
                content.AddRange(namesContent);

                return content.ToArray();
            }
            set { }
        }
        public TextureGeneratorControl(string propertyName, string extraPropertyName1 = null, string extraPropertyName2 = null) : this(Shaders.RGBAPacker, propertyName, extraPropertyName1, extraPropertyName2)
        {
        }

        public TextureGeneratorControl(Shader shader, string propertyName, string extraPropertyName1 = null, string extraPropertyName2 = null) : base(propertyName, extraPropertyName1, extraPropertyName2)
        {
            HasCustomInlineContent = true;
            _resolution = Resolution.M_512x512;
            _shader = shader;

            GeneratorButtonStyle = Styles.Bubble;
            GeneratorStyle = Styles.TextureBoxHeavyBorder;
            GeneratorInputStyle = Styles.Box;
            GeneratorSaveButtonStyle = Styles.Bubble;
            GeneratorCloseButtonStyle = Styles.Bubble;

            GeneratorButtonColor = Color.white;
            GeneratorColor = Color.white;
            GeneratorInputColor = Color.white;
            GeneratorSaveButtonColor = Color.white;
            GeneratorCloseButtonColor = Color.white;

            baseContent = AdditionalContentExtensions.CreateLocalizationArrayFromNames(_baseNames);
        }

        public override void Initialization()
        {
            _crtMaterial = new Material(_shader);
            _crtMaterialEditor = Editor.CreateEditor(_crtMaterial) as MaterialEditor;
            _shaderStack = new List<Shader>();
            bool isRoot = true;
            if (Inspector is TextureGeneratorShaderInspector generatorInspector)
            {
                _shaderStack.AddRange(generatorInspector.shaderStack);
                isRoot = false;
            }
            _shaderStack.Add(Inspector.Shader);
            
            if (_crtMaterialEditor.customShaderGUI != null && _crtMaterialEditor.customShaderGUI is TextureGeneratorShaderInspector compliantInspector)
            {
                if (_shaderStack.Contains(((Material)_crtMaterialEditor.target).shader))
                {
                    namesContent = Array.Empty<AdditionalLocalization>();
                }
                else
                {
                    compliantInspector.shaderStack.AddRange(_shaderStack);
                    compliantInspector.Setup(_crtMaterialEditor, MaterialEditor.GetMaterialProperties(_crtMaterialEditor.targets));
                    namesContent = compliantInspector.GetRequiredLocalization().Where(x => !string.IsNullOrWhiteSpace(x.Name)).Distinct().ToArray();
                }
            }
            else
            {
                namesContent = Array.Empty<AdditionalLocalization>();
            }

            if(isRoot)
                foreach (AdditionalLocalization t in namesContent)
                    t.Name = "Input_" + t.Name;

            Object.DestroyImmediate(_crtMaterial);
            Object.DestroyImmediate(_crtMaterialEditor);
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            DrawTextureSingleLine(materialEditor);

            if (_isGeneratorOpen)
            {
                GUI.backgroundColor = GeneratorColor;
                EditorGUILayout.BeginHorizontal();
                int previousIndent = EditorGUI.indentLevel;
                GUILayout.Space(EditorGUI.indentLevel * 15);
                EditorGUI.indentLevel = 0;
                EditorGUILayout.BeginVertical(GeneratorStyle);
                GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;
                DrawGenerator();
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel = previousIndent;
                EditorGUILayout.EndHorizontal();
            }
        }
        
        protected override void DrawSideContent(MaterialEditor materialEditor)
        {
            if (_isGeneratorOpen) return;
            
            GUI.backgroundColor = GeneratorButtonColor;
            if (GUILayout.Button(baseContent[0].Content, GeneratorButtonStyle))
            {
                _isGeneratorOpen = true;
                _previousTextures = new Texture2D[materialEditor.targets.Length];
                for (int i = 0; i < Inspector.Materials.Length; i++)
                    _previousTextures[i] = (Texture2D)Inspector.Materials[i].GetTexture(PropertyName);
                Selection.selectionChanged += GeneratorCloseCleanup;
            }
            GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;
        }

        private void DrawGenerator()
        {
            if (_crtMaterialEditor == null)
            {
                _crt = new CustomRenderTexture(
                    (int)_resolution, (int)_resolution, RenderTextureFormat.ARGB32, 
                    _linearSpace ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
                Property.textureValue = _crt;
                _crtMaterial = new Material(_shader);
                _crt.material = _crtMaterial;
                _crtMaterialEditor = Editor.CreateEditor(_crtMaterial) as MaterialEditor;
                switch (_crtMaterialEditor.customShaderGUI)
                {
                    case null:
                        _crtEditorGUI = new DefaultGeneratorGUI();
                        break;
                    case TextureGeneratorShaderInspector compliantInspector:
                        SetupInspector(compliantInspector);
                        break;
                }
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(_crtMaterial, true);
                
                
            }
            
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            if (_crtEditorGUI != null)
                _crtEditorGUI.OnGUI(_crtMaterialEditor, MaterialEditor.GetMaterialProperties(_crtMaterialEditor.targets));
            else
                _crtMaterialEditor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(20);
            
            if(Event.current.type != EventType.Repaint)
            {
                float oldWindowWidth = _windowWidth;
                _windowWidth = EditorGUIUtility.currentViewWidth;
                float difference = _windowWidth - oldWindowWidth;
                _width += difference;
            }

            GUI.backgroundColor = GeneratorInputColor;
            EditorGUILayout.BeginHorizontal(GeneratorInputStyle);
            EditorGUILayout.BeginVertical();
            if (_width <= 400)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
            }
            GUILayout.Label(baseContent[4].Content,GUILayout.MinWidth(60));
            Rect windowRect = GUILayoutUtility.GetRect(10, 126, 10, 132);
            float squareSize = Mathf.Min(windowRect.width - 6, windowRect.height - 12);
            var previewRect = new Rect(windowRect.x + 7, windowRect.y + 9, squareSize, squareSize);
            GUI.DrawTexture(previewRect, _crt, ScaleMode.StretchToFill, true);
            
            if (_width <= 400)
            {
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
            }
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(baseContent[5].Content,GUILayout.MinWidth(20));
            GUILayout.FlexibleSpace();
            _linearSpace = EditorGUILayout.Toggle( _linearSpace, GUILayout.Width(16));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(baseContent[3].Content, GUILayout.MinWidth(20));
            GUILayout.FlexibleSpace();
            _resolution = (Resolution)EditorGUILayout.EnumPopup(_resolution,GUILayout.Width(130));
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                _crt.Release();
                Object.DestroyImmediate(_crt);
                _crt = new CustomRenderTexture(
                    (int)_resolution, (int)_resolution, RenderTextureFormat.ARGB32, 
                    _linearSpace ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
                _crt.material = _crtMaterial;
                Property.textureValue = _crt;
            }
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = GeneratorSaveButtonColor;
            
            if (GUILayout.Button(baseContent[1].Content, GeneratorSaveButtonStyle,GUILayout.MinWidth(60)))
            {
                GenerateTexture();
                _previousTextures = null;
                _isGeneratorOpen = false;
                GeneratorCloseCleanup();
                
            }
            GUI.backgroundColor = GeneratorCloseButtonColor;
            if (GUILayout.Button(baseContent[2].Content, GeneratorCloseButtonStyle,GUILayout.MinWidth(60)))
            {
                _isGeneratorOpen = false;
                GeneratorCloseCleanup();
                
            }
            
            GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            if (Event.current.type == EventType.Repaint)
            {
                _windowWidth = EditorGUIUtility.currentViewWidth;
                _width = GUILayoutUtility.GetLastRect().width;
            }


        }

        private void GeneratorCloseCleanup()
        {
            if (_crt != null)
            {
                _crt.Release();
                Object.DestroyImmediate(_crt);
                _crt = null;
            }

            Object.DestroyImmediate(_crtMaterial);
            Object.DestroyImmediate(_crtMaterialEditor);
            _crtMaterial = null;
            _crtMaterialEditor = null;
            _crtEditorGUI = null;
            
            if (_previousTextures != null)
            {
                for (int i = 0; i < Inspector.Materials.Length; i++)
                {
                    if (Inspector.Materials[i] == null) continue;
                    Inspector.Materials[i].SetTexture(PropertyName, _previousTextures[i]);
                }
                    

                _previousTextures = null;
            }
            
            Selection.selectionChanged -= GeneratorCloseCleanup;
        }

        private void SetupInspector(TextureGeneratorShaderInspector compliantInspector)
        {
            compliantInspector.isFromGenerator = true;
            compliantInspector.shaderStack.AddRange(_shaderStack);
            compliantInspector.Setup(_crtMaterialEditor, MaterialEditor.GetMaterialProperties(_crtMaterialEditor.targets));
            
            if (Inspector is TextureGeneratorShaderInspector ins)
                _propertyInfos = ins.stackedInfo;
            else
            {
                _propertyInfos = new PropertyInfo[namesContent.Length];
                for (int i = 0; i < _propertyInfos.Length; i++)
                {
                    _propertyInfos[i] = new PropertyInfo
                    {
                        Name = namesContent[i].Name.Substring(6),
                        DisplayName = namesContent[i].Content.text,
                        Tooltip = namesContent[i].Content.tooltip
                    };
                    
                }
            }
            compliantInspector.SetShaderLocalizationFromGenerator(_propertyInfos);
        }

        private void GenerateTexture()
        {
            string path = SSIHelper.GetTextureDestinationPath((Material)Property.targets[0], PropertyName + ".png");
            Property.textureValue = SSIHelper.SaveAndGetTexture(_crt, path, TextureWrapMode.Repeat, _linearSpace);
        }
    }

    public enum Resolution
    {
        XXS_64x64 = 64,
        XS_128x128 = 128,
        S_256x256 = 256,
        M_512x512 = 512,
        L_1024x1024 = 1024,
        XL_2048x2048 = 2048,
        XXL_4096x4096 = 4096
    }
}