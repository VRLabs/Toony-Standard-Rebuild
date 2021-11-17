using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class TextureGeneratorControl : TextureControl, IAdditionalLocalization
    {
        private readonly AdditionalLocalization[] _baseContent;
        private readonly AdditionalLocalization[] _textureContent;
        private readonly AdditionalLocalization[] _colorContent;
        private readonly AdditionalLocalization[] _namesContent;
        private bool _isGeneratorOpen;
        private readonly ComputeShader _compute;
        private readonly string _kernelName;
        private Resolution _resolution;
        private readonly List<ComputeInputBase> _inputs;
        private RenderTexture _result;
        private Texture2D _resultTex;
        private readonly bool _containsTextures;

        private static readonly string[] _baseNames =
                        {
            "GeneratorOpen",        //[0]
            "GeneratorSaveButton",  //[1]
            "GeneratorCancelButton",//[2]
            "GeneratorTextureSize"  //[3]
        };
        private static readonly string[] _textureNames =
                {
            "TextureInvert"         //[0]
        };

        private static readonly string[] _colorNames =
                        {
            "ColorSpace"            //[0]
        };

        private readonly bool _containsColors;

        [Chainable] public GUIStyle GeneratorButtonStyle { get; set; }
        
        [Chainable] public GUIStyle GeneratorSaveButtonStyle { get; set; }

        [Chainable] public GUIStyle GeneratorStyle { get; set; }

        [Chainable] public GUIStyle GeneratorInputStyle { get; set; }

        [Chainable] public Color GeneratorButtonColor { get; set; }

        [Chainable] public Color GeneratorSaveButtonColor { get; set; }

        [Chainable] public Color GeneratorColor { get; set; }

        [Chainable] public Color GeneratorInputColor { get; set; }

        public AdditionalLocalization[] AdditionalContent
        {
            get
            {
                List<AdditionalLocalization> content = new List<AdditionalLocalization>();
                content.AddRange(_baseContent);
                if (_containsTextures) content.AddRange(_textureContent);
                if (_containsColors) content.AddRange(_colorContent);
                content.AddRange(_namesContent);

                return content.ToArray();
            }
            set { }
        }
        public TextureGeneratorControl(string propertyName, string extraPropertyName1 = null, string extraPropertyName2 = null) : this(ComputeShaders.RGBAPacker, ComputeShaders.RGBAPackerSettings, propertyName, extraPropertyName1, extraPropertyName2)
        {
        }

        public TextureGeneratorControl(ComputeShader compute, string computeOptionsJson, string propertyName, string extraPropertyName1 = null, string extraPropertyName2 = null) : base(propertyName, extraPropertyName1, extraPropertyName2)
        {
            HasCustomInlineContent = true;
            _resolution = Resolution.M_512x512;
            _result = new RenderTexture((int)_resolution, (int)_resolution, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
            {
                enableRandomWrite = true
            };
            _result.Create();

            _compute = compute;
            (_kernelName, _inputs) = TextureGeneratorHelper.GetInputs(computeOptionsJson);

            foreach (var input in _inputs)
            {
                switch (input)
                {
                    case ComputeTextureInput _:
                        _containsTextures = true;
                        break;
                    case ComputeColorInput _:
                        _containsColors = true;
                        break;
                }

                if (_containsColors && _containsTextures)
                {
                    break;
                }
            }

            GeneratorButtonStyle = Styles.Bubble;
            GeneratorStyle = Styles.TextureBoxHeavyBorder;
            GeneratorInputStyle = Styles.Box;
            GeneratorSaveButtonStyle = Styles.Bubble;

            GeneratorButtonColor = Color.white;
            GeneratorColor = Color.white;
            GeneratorInputColor = Color.white;
            GeneratorSaveButtonColor = Color.white;

            _baseContent = AdditionalContentExtensions.CreateLocalizationArrayFromNames(_baseNames);

            if (_containsTextures)
                _textureContent = AdditionalContentExtensions.CreateLocalizationArrayFromNames(_textureNames);

            if (_containsTextures)
                _colorContent = AdditionalContentExtensions.CreateLocalizationArrayFromNames(_colorNames);

            _namesContent = new AdditionalLocalization[_inputs.Count];
            for (int i = 0; i < _inputs.Count; i++)
                _namesContent[i] = new AdditionalLocalization { Name = "Input" + (i + 1) };
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
            if (!_isGeneratorOpen)
            {
                GUI.backgroundColor = GeneratorButtonColor;
                if (GUILayout.Button(_baseContent[0].Content, GeneratorButtonStyle))
                {
                    _isGeneratorOpen = true;
                }
                GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;
            }
        }

        private void DrawGenerator()
        {
            int columns = (int)((EditorGUIUtility.currentViewWidth - 20) / 90) - 1;
            if (columns == 0) columns = 1;
            for (int i = 0; i < _inputs.Count; i++)
            {
                if (i % columns == 0)
                {
                    if (i == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                    }
                }

                AdditionalLocalization[] selectedArray = _inputs[i] is ComputeTextureInput ? _textureContent : _colorContent;
                
                GUI.backgroundColor = GeneratorInputColor;
                EditorGUILayout.BeginVertical(GeneratorInputStyle);
                GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;
                _inputs[i].InputGUI(_namesContent[i].Content, selectedArray);
                EditorGUILayout.EndVertical();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            _resolution = (Resolution)EditorGUILayout.EnumPopup(_baseContent[3].Content, _resolution); //_baseContent[3]

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = GeneratorSaveButtonColor;
            
            if (GUILayout.Button(_baseContent[1].Content, GeneratorSaveButtonStyle))
            {
                GenerateTexture();
                _isGeneratorOpen = false;
            }
            
            if (GUILayout.Button(_baseContent[2].Content, GeneratorSaveButtonStyle))
                _isGeneratorOpen = false;
            
            GUI.backgroundColor = SimpleShaderInspector.DefaultBgColor;
            EditorGUILayout.EndHorizontal();
        }

        private void GenerateTexture()
        {
            if (_result.width != (int)_resolution || _result.height != (int)_resolution)
            {
                _result = new RenderTexture((int)_resolution, (int)_resolution, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
                {
                    enableRandomWrite = true
                };
                _result.Create();
            }

            int kernel = _compute.FindKernel(_kernelName);
            _compute.SetTexture(kernel, "Result", _result);
            _compute.SetFloat("width", (float)_resolution);
            _compute.SetFloat("height", (float)_resolution);

            var computeData = new ComputeInputs();
            foreach (var input in _inputs)
                input.AssignInputsToCompute(computeData, kernel);
            
            ComputeBuffer textureParamsBuffer = null;
            ComputeBuffer colorParamsBuffer = null;
            if (computeData.TexturesMeta.Count > 0)
            {
                textureParamsBuffer = new ComputeBuffer(computeData.TexturesMeta.Count, 20);
                textureParamsBuffer.SetData(computeData.TexturesMeta.ToArray());
                _compute.SetBuffer(kernel, "TexturesMeta", textureParamsBuffer);
            }
            if (computeData.Colors.Count > 0)
            {
                colorParamsBuffer = new ComputeBuffer(computeData.Colors.Count, 16);
                colorParamsBuffer.SetData(computeData.Colors.ToArray());
                _compute.SetBuffer(kernel, "Colors", colorParamsBuffer);
            }

            foreach (var texture in computeData.Textures)
                _compute.SetTexture(kernel, texture.Name, texture.Texture);
            
            _compute.Dispatch(kernel, (int)_resolution / 16, (int)_resolution / 16, 1);

            textureParamsBuffer?.Release();
            colorParamsBuffer?.Release();

            RenderTexture.active = _result;
            _resultTex = new Texture2D(_result.width, _result.height, TextureFormat.RGBA32, false);
            _resultTex.ReadPixels(new Rect(0, 0, _result.width, _result.height), 0, 0);
            RenderTexture.active = null;
            _resultTex.Apply(true);
            Property.textureValue = SSIHelper.SaveAndGetTexture(_resultTex, SSIHelper.GetTextureDestinationPath((Material)Property.targets[0], PropertyName + ".png"));
        }
    }

    public enum Resolution
    {
        XS_128x128 = 128,
        S_256x256 = 256,
        M_512x512 = 512,
        L_1024x1024 = 1024,
        XL_2048x2048 = 2048,
        XXL_4096x4096 = 4096
    }
}