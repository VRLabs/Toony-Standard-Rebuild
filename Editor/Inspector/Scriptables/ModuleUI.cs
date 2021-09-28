using System;
using System.Collections.Generic;
using UnityEngine;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls;
using Chainables = VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.Chainables;

namespace VRLabs.ToonyStandardRebuild
{
    public class ModuleUI
    {
        public string Name;
        public List<SectionUI> Sections;

        public ModuleUI()
        {
            Sections = new List<SectionUI>();
        }
    }

    public class SectionUI
    {
        public string SectionName;
        public string ActivatePropertyName;
        public float ActivateValue;
        public List<ControlUI> Controls;

        public SectionUI()
        {
            SectionName = "";
            ActivatePropertyName = "";
            ActivateValue = 0;
            Controls = new List<ControlUI>();
        }
    }

    public class ControlUI
    {
        public string Name;
        public ControlType ControlType;
        public List<object> Parameters;
        public List<ControlUI> Controls;

        public ControlUI()
        {
            Name = "";
            ControlType = ControlType.SpaceControl;
            Parameters = new List<object>();
            Controls = new List<ControlUI>();
        }

        public bool CouldHaveControls()
        {
            switch (ControlType)
            {
                case ControlType.ControlContainer:
                case ControlType.ToggleListControl:
                case ControlType.KeywordToggleListControl:
                    return true;
                default:
                    return false;
            }
        }

        public SimpleControl CreateControl(IControlContainer parentControl)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new NullReferenceException("The name is empty");
            switch (ControlType)
            {
                case ControlType.LabelControl:
                    return parentControl.AddLabelControl(Name);
                case ControlType.SpaceControl:
                    return parentControl.AddSpaceControl(Parameters.Count < 1 || !(Parameters[0] is int a) ? 0 : a);
                case ControlType.PropertyControl:
                    if (Parameters.Count < 1 || !(Parameters[0] is string))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddPropertyControl((string)Parameters[0]).Alias(Name);
                case ControlType.ColorControl:
                    if (Parameters.Count < 2 || !(Parameters[0] is string) || !(Parameters[1] is bool))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddColorControl((string)Parameters[0], (bool)Parameters[1]).Alias(Name);
                case ControlType.VectorControl:
                    if (Parameters.Count < 5 || !(Parameters[0] is string) ||
                        !(Parameters[1] is bool) || !(Parameters[2] is bool) ||
                        !(Parameters[3] is bool) || !(Parameters[4] is bool))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddVectorControl((string)Parameters[0], (bool)Parameters[1], (bool)Parameters[2], (bool)Parameters[3], (bool)Parameters[4]).Alias(Name);
                case ControlType.TextureControl:
                    if (Parameters.Count < 3 || !(Parameters[0] is string) ||
                        !(Parameters[1] is string) || !(Parameters[2] is string))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddTextureControl((string)Parameters[0], (string)Parameters[1], (string)Parameters[2]).Alias(Name);
                case ControlType.TextureGeneratorControl:
                    if (Parameters.Count < 5 || !(Parameters[0] is ComputeShader) ||
                        !(Parameters[1] is string) || !(Parameters[2] is string) ||
                        !(Parameters[3] is string) || !(Parameters[4] is string))
                    {
                        if (Parameters.Count < 3 || !(Parameters[0] is string) ||
                            !(Parameters[1] is string) || !(Parameters[2] is string))
                            throw new TypeAccessException("The parameter given was not of the right type");
                    }
                    if (Parameters.Count < 5)
                        return parentControl.AddTextureGeneratorControl((string)Parameters[0], (string)Parameters[1], (string)Parameters[2]).Alias(Name);
                    else
                        return parentControl.AddTextureGeneratorControl((ComputeShader)Parameters[0], (string)Parameters[1], (string)Parameters[2], (string)Parameters[3], (string)Parameters[4]).Alias(Name);
                case ControlType.GradientTextureControl:
                    if (Parameters.Count < 2 || !(Parameters[0] is string) ||
                        !(Parameters[1] is string))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddGradientTextureControl((string)Parameters[0], (string)Parameters[1]).Alias(Name);
                case ControlType.TilingAndOffsetControl:
                    if (Parameters.Count < 1 || !(Parameters[0] is string))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddTilingAndOffsetControl((string)Parameters[0]).Alias(Name);
                case ControlType.ControlContainer:
                    return parentControl.AddControlContainer().Alias(Name);
                case ControlType.EnumControl:
                    if (Parameters.Count < 2 || !(Parameters[0] is Type) ||
                        !(Parameters[1] is string))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    var methodInfo = typeof(Chainables).GetMethod("AddEnumControl");
                    if (!(methodInfo is null))
                    {
                        var genericMethodInfo = methodInfo.MakeGenericMethod((Type)Parameters[0]);
                        var result = genericMethodInfo.Invoke(parentControl, new object[] { (string)Parameters[1] });
                        return ((SimpleControl)result).Alias(Name);
                    }
                    throw new MethodAccessException("The method given was not found");
                case ControlType.ToggleControl:
                    if (Parameters.Count < 3 || !(Parameters[0] is string) ||
                        !(Parameters[1] is float) || !(Parameters[2] is float))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddToggleControl((string)Parameters[0], (float)Parameters[1], (float)Parameters[2]).Alias(Name);
                case ControlType.ToggleListControl:
                    if (Parameters.Count < 3 || !(Parameters[0] is string) ||
                        !(Parameters[1] is float) || !(Parameters[2] is float))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddToggleListControl((string)Parameters[0], (float)Parameters[1], (float)Parameters[2]).Alias(Name);
                case ControlType.KeywordToggleControl:
                    if (Parameters.Count < 1 || !(Parameters[0] is string))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddKeywordToggleControl((string)Parameters[0]).Alias(Name);
                case ControlType.KeywordToggleListControl:
                    if (Parameters.Count < 1 || !(Parameters[0] is string))
                        throw new TypeAccessException("The parameter given was not of the right type");
                    return parentControl.AddKeywordToggleListControl((string)Parameters[0]).Alias(Name);
                case ControlType.LightmapEmissionControl:
                    return parentControl.AddLightmapEmissionControl().Alias(Name);
                case ControlType.VertexStreamsControl:
                    return parentControl.AddVertexStreamsControl(Name);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum ControlType
    {
        SpaceControl,
        LabelControl,
        PropertyControl,
        ColorControl,
        VectorControl,
        TextureControl,
        TextureGeneratorControl,
        GradientTextureControl,
        TilingAndOffsetControl,
        ControlContainer,
        EnumControl,
        ToggleControl,
        ToggleListControl,
        KeywordToggleControl,
        KeywordToggleListControl,
        LightmapEmissionControl,
        VertexStreamsControl

    }
}