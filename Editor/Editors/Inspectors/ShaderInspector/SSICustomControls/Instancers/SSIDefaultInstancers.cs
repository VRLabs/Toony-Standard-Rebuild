using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls;

namespace VRLabs.ToonyStandardRebuild
{
    public class LabelControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(LabelControl);
        public bool CanHaveChildControls => false;

        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            return parentControl.AddLabelControl(uiAsset.Name, uiAsset.AppendAfter);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new LabelControlUIElement(uiAsset.Parameters);
    }
    
    public class SpaceControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(SpaceControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            return parentControl.AddSpaceControl(uiAsset.Parameters.Count < 1 || !(uiAsset.Parameters[0] is int a) ? 0 : a, uiAsset.AppendAfter);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new SpaceControlUIElement(uiAsset.Parameters);
    }
    
    public class HelpBoxControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(HelpBoxControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 2 || !(uiAsset.Parameters[0] is MessageType) || !(uiAsset.Parameters[1] is bool))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddHelpBoxControl(uiAsset.Name, uiAsset.AppendAfter).SetBoxType((MessageType)uiAsset.Parameters[0]).SetIsWideBox((bool)uiAsset.Parameters[1]);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new HelpBoxControlUIElement(uiAsset.Parameters);
    }
    
    public class PropertyControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(PropertyControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 1 || !(uiAsset.Parameters[0] is string))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddPropertyControl((string)uiAsset.Parameters[0], uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new PropetyControlUIElement(uiAsset.Parameters);
    }
    
    public class ColorControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(ColorControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 2 || !(uiAsset.Parameters[0] is string) || !(uiAsset.Parameters[1] is bool))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddColorControl((string)uiAsset.Parameters[0], (bool)uiAsset.Parameters[1], uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new ColorControlUIElement(uiAsset.Parameters);
    }
    
    public class VectorControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(VectorControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 5 || !(uiAsset.Parameters[0] is string) ||
                !(uiAsset.Parameters[1] is bool) || !(uiAsset.Parameters[2] is bool) ||
                !(uiAsset.Parameters[3] is bool) || !(uiAsset.Parameters[4] is bool))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddVectorControl((string)uiAsset.Parameters[0], (bool)uiAsset.Parameters[1], (bool)uiAsset.Parameters[2], 
                (bool)uiAsset.Parameters[3], (bool)uiAsset.Parameters[4], uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new VectorControlUIElement(uiAsset.Parameters);
    }
    
    public class TextureControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(TextureControl);
        public bool CanHaveChildControls => true;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            if (uiAsset.Parameters.Count < 4 || !(uiAsset.Parameters[0] is string) ||
                !(uiAsset.Parameters[1] is string) || !(uiAsset.Parameters[2] is string) ||
                !(uiAsset.Parameters[3] is string))
                throw new TypeAccessException("The parameter given was not of the right type");

            uvSet = (string)uiAsset.Parameters[3];
            return parentControl.AddTextureControl((string)uiAsset.Parameters[0], (string)uiAsset.Parameters[1], 
                (string)uiAsset.Parameters[2], uiAsset.AppendAfter).Alias(uiAsset.Name).SetShowTilingAndOffset(true);

        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new TextureControlUIElement(uiAsset.Parameters, uiAsset.Controls);
    }
    
    public class TextureGeneratorControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(TextureGeneratorControl);
        public bool CanHaveChildControls => true;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            if (uiAsset.Parameters.Count < 6 || !(uiAsset.Parameters[0] is ComputeShader) ||
                !(uiAsset.Parameters[1] is string) || !(uiAsset.Parameters[2] is string) ||
                !(uiAsset.Parameters[3] is string) || !(uiAsset.Parameters[4] is string) ||
                !(uiAsset.Parameters[5] is string))
            {
                if (uiAsset.Parameters.Count < 4 || !(uiAsset.Parameters[0] is string) ||
                    !(uiAsset.Parameters[1] is string) || !(uiAsset.Parameters[2] is string) ||
                    !(uiAsset.Parameters[3] is string))
                    throw new TypeAccessException("The parameter given was not of the right type");
            }

            if (uiAsset.Parameters.Count < 6)
            {
                uvSet = (string)uiAsset.Parameters[3];
                return parentControl.AddTextureGeneratorControl((string)uiAsset.Parameters[0], (string)uiAsset.Parameters[1],
                    (string)uiAsset.Parameters[2], uiAsset.AppendAfter).Alias(uiAsset.Name).SetShowTilingAndOffset(true);
            }

            uvSet = (string)uiAsset.Parameters[5];
            return parentControl.AddTextureGeneratorControl((ComputeShader)uiAsset.Parameters[0], (string)uiAsset.Parameters[1],
                (string)uiAsset.Parameters[2], (string)uiAsset.Parameters[3], (string)uiAsset.Parameters[4],
                uiAsset.AppendAfter).Alias(uiAsset.Name).SetShowTilingAndOffset(true);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new TextureGeneratorControlUIElement(uiAsset.Parameters, uiAsset.Controls);
    }
    
    public class GradientTextureControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(GradientTextureControl);
        public bool CanHaveChildControls => true;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 4 || !(uiAsset.Parameters[0] is string) ||
                !(uiAsset.Parameters[1] is string) || !(uiAsset.Parameters[2] is string) ||
                !(uiAsset.Parameters[3] is string))
            {
                if (uiAsset.Parameters.Count < 2 || !(uiAsset.Parameters[0] is string) ||
                    !(uiAsset.Parameters[1] is string))
                    throw new TypeAccessException("The parameter given was not of the right type");
            }
            if (uiAsset.Parameters.Count < 4)
                return parentControl.AddGradientTextureControl((string)uiAsset.Parameters[0], (string)uiAsset.Parameters[1],
                    uiAsset.AppendAfter).Alias(uiAsset.Name);
            else
                return parentControl.AddGradientTextureControl((string)uiAsset.Parameters[0], (string)uiAsset.Parameters[1],
                    (string)uiAsset.Parameters[2], (string)uiAsset.Parameters[3], uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new GradientTextureControlUIElement(uiAsset.Parameters, uiAsset.Controls);
    }
    
    public class TilingAndOffsetControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(TilingAndOffsetControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 1 || !(uiAsset.Parameters[0] is string))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddTilingAndOffsetControl((string)uiAsset.Parameters[0], uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new TilingAndOffsetControlUIElement(uiAsset.Parameters);
    }
    
    public class ConditionalControlContainerInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(ConditionalControlContainer);
        public bool CanHaveChildControls => true;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 2 || !(uiAsset.Parameters[0] is string) || !(uiAsset.Parameters[1] is float))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddConditionalControlContainer((string)uiAsset.Parameters[0], (float)uiAsset.Parameters[1], uiAsset.AppendAfter)
                .Alias(uiAsset.Name).SetIndent(true);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new ConditionalControlContainerUIElement(uiAsset.Parameters, uiAsset.Controls);
    }
    
    public class ToggleControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(ToggleControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 3 || !(uiAsset.Parameters[0] is string) ||
                !(uiAsset.Parameters[1] is float) || !(uiAsset.Parameters[2] is float))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddToggleControl((string)uiAsset.Parameters[0], (float)uiAsset.Parameters[1],
                (float)uiAsset.Parameters[2], uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new ToggleControlUIElement(uiAsset.Parameters);
    }
    
    public class ToggleListControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(ToggleListControl);
        public bool CanHaveChildControls => true;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 3 || !(uiAsset.Parameters[0] is string) ||
                !(uiAsset.Parameters[1] is float) || !(uiAsset.Parameters[2] is float))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddToggleListControl((string)uiAsset.Parameters[0], (float)uiAsset.Parameters[1],
                (float)uiAsset.Parameters[2], uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new ToggleListControlUIElement(uiAsset.Parameters, uiAsset.Controls);
    }
    
    public class KeywordToggleControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(KeywordToggleControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 1 || !(uiAsset.Parameters[0] is string))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddKeywordToggleControl((string)uiAsset.Parameters[0], uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new KeywordToggleControlUIElement(uiAsset.Parameters);
    }
    
    public class KeywordToggleListControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(KeywordToggleListControl);
        public bool CanHaveChildControls => true;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 1 || !(uiAsset.Parameters[0] is string))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddKeywordToggleListControl((string)uiAsset.Parameters[0], uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new KeywordToggleListControlUIElement(uiAsset.Parameters, uiAsset.Controls);
    }
    
    public class LightmapEmissionControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(LightmapEmissionControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            return parentControl.AddLightmapEmissionControl(uiAsset.AppendAfter).Alias(uiAsset.Name);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new LightmapEmissionControlUIElement(uiAsset.Parameters);
    }
    
    public class VertexStreamsControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(VertexStreamsControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            return parentControl.AddVertexStreamsControl(uiAsset.Name, uiAsset.AppendAfter);
        }

        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new VisualElement();
    }
}