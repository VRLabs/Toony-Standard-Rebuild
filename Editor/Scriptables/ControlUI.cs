using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls;
using VRLabs.ToonyStandardRebuild.SSICustomControls;

namespace VRLabs.ToonyStandardRebuild
{

    public class ControlUI
    {
        public string Name;
        public string AppendAfter;
        public Type UIControlType;
        public List<object> Parameters;
        public List<ControlUI> Controls;

        public ControlUI()
        {
            Name = "";
            AppendAfter = "";
            UIControlType = typeof(PropertyControl);
            Parameters = new List<object>();
            Controls = new List<ControlUI>();
        }

        public bool CouldHaveControls()
        {
            if (UIControlType == null || !ControlInstancerUtility.ControlInstancers.ContainsKey(UIControlType)) return false;
            IControlInstancer instancer = ControlInstancerUtility.ControlInstancers[UIControlType];
            return instancer.CanHaveChildControls;
        }

        public SimpleControl CreateControl(IControlContainer parentControl, ModularShader modularShader, out string uvSet)
        {
            uvSet = null;
            if (string.IsNullOrWhiteSpace(Name))
                throw new NullReferenceException("The name is empty");
            
            if (UIControlType != null && ControlInstancerUtility.ControlInstancers.ContainsKey(UIControlType))
            {
                IControlInstancer instancer = ControlInstancerUtility.ControlInstancers[UIControlType];
                return instancer.InstanceInspectorControl(this, parentControl, modularShader, out uvSet);
            }
            else
            {
                throw new NullReferenceException($"The following control type has not been found: {UIControlType}");
            }
        }
    }
}