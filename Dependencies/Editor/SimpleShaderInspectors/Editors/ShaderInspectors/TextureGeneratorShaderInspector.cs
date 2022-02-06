using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public abstract class TextureGeneratorShaderInspector : ShaderGUI, ISimpleShaderInspector
    {
        private bool _isFirstLoop = true;
        private bool _doesContainControls = true;
        private List<INonAnimatableProperty> _nonAnimatablePropertyControls;
        private bool ContainsNonAnimatableProperties => _nonAnimatablePropertyControls.Count > 0;
        internal bool isFromGenerator = false;

        internal readonly List<Shader> shaderStack = new List<Shader>();
        internal PropertyInfo[] stackedInfo;

        public List<SimpleControl> Controls { get; set; }
        
        public Material[] Materials { get; private set; }
        
        public Shader Shader { get; private set; }
        
        protected abstract void Start();
        
        protected virtual void StartChecks(MaterialEditor materialEditor) { }

        protected virtual void CheckChanges(MaterialEditor materialEditor) { }
        
        
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (!isFromGenerator)
            {
                EditorGUILayout.HelpBox("This shader is meant to be used inside a Texture Generator in the Simple Shader Inspectors library, and not be used for a material, please select another shader", MessageType.Error);
                return;
            }
            
            if (_isFirstLoop)
            {
                Setup(materialEditor, properties);
            }
            else
            {
                Controls.FetchProperties(properties);
            }
            
            DrawGUI(materialEditor, properties);
            if (ContainsNonAnimatableProperties)
                SSIHelper.UpdateNonAnimatableProperties(_nonAnimatablePropertyControls, materialEditor, false);
            
            CheckChanges(materialEditor);
        }

        internal void Setup(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Controls = new List<SimpleControl>();
            Materials = Array.ConvertAll(materialEditor.targets, item => (Material)item);
            Shader = Materials[0].shader;
            Start();
            Controls.SetInspector(this);
            _nonAnimatablePropertyControls = (List<INonAnimatableProperty>)Controls.FindNonAnimatablePropertyControls();
            Controls.FetchProperties(properties);
            StartChecks(materialEditor);
            _isFirstLoop = false;
            if (Controls == null || Controls.Count == 0)
                _doesContainControls = false;
        }

        private void DrawGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (_doesContainControls)
            {
                foreach (var control in Controls)
                    control.DrawControl(materialEditor);
            }
            else
            {
                EditorGUILayout.HelpBox("No controls have been passed to the Start() method, therefore a default inspector has been drawn, if you are an end user of the shader try to reinstall the shader or contact the creator.", MessageType.Error);
                base.OnGUI(materialEditor, properties);
            }
        }

        internal void SetShaderLocalizationFromGenerator(PropertyInfo[] propertyInfos)
        {
            stackedInfo = propertyInfos;
            Localization.SetPropertiesLocalization(Controls, propertyInfos, new List<PropertyInfo>());
        }

        internal List<AdditionalLocalization> GetRequiredLocalization()
        {
            var elements = new List<AdditionalLocalization>();
            foreach (var control in Controls)
            {
                elements.AddRange(GetControlLocalization(control));
            }

            return elements;
        }

        private static List<AdditionalLocalization> GetControlLocalization(SimpleControl control)
        {
            var elements = new List<AdditionalLocalization>();
            var element = new AdditionalLocalization();
            element.Name = control.ControlAlias;
            
            elements.Add(element);
                
            if (control is IAdditionalLocalization additional)
            {
                if (additional is TextureGeneratorControl texture)
                {
                        elements.AddRange(texture.NamesContent.Select(x => new AdditionalLocalization{Name = $"{x.Name}", Content = null}));
                        elements.AddRange(texture.baseContent.Select(x => new AdditionalLocalization { Name = $"{control.ControlAlias}_{x.Name}", Content = null }));
                }
                else
                {
                    elements.AddRange(additional.AdditionalContent.Select(x => new AdditionalLocalization { Name = $"{control.ControlAlias}_{x.Name}", Content = null }));
                }
            }
                

            if(control is IControlContainer container)
                foreach (var childControl in container.GetControlList())
                    elements.AddRange(GetControlLocalization(childControl));

            return elements;
        }
        
        public void AddControl(SimpleControl control, string alias = "") => Controls.AddControl(control, alias);

        public IEnumerable<SimpleControl> GetControlList() => Controls;
    }
}