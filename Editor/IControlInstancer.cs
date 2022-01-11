using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;

namespace VRLabs.ToonyStandardRebuild
{
    public static class ControlInstancerUtility
    {
        private static Dictionary<Type, IControlInstancer> _controlInstancers; 
        public static Dictionary<Type, IControlInstancer> ControlInstancers {
            get
            {
                if( _controlInstancers == null )
                {
                    _controlInstancers = new Dictionary<Type,IControlInstancer>();

                    var instancerTypes = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .SelectMany(x => x.GetTypes())
                        .Where(x => x.GetInterface(typeof(IControlInstancer).FullName) != null)
                        .OrderBy(x => x.Name)
                        .ToList();

                    // Create instancers
                    foreach (var type in instancerTypes)
                    {
                        var instancer = Activator.CreateInstance(type) as IControlInstancer;
                        _controlInstancers.Add(instancer.InstanceType, instancer);
                    }
                }
                return _controlInstancers;
            }
        }
    }

    public interface IControlInstancer
    {
        Type InstanceType { get; }
        bool CanHaveChildControls { get; }
        SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet);
        VisualElement InstanceEditorUI(ControlUI uiAsset);
    }
}