using System.Collections.Generic;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;

namespace VRLabs.ToonyStandardRebuild.SSICustomControls
{
    public static partial class Chainables
    {
        public static ListSelectorControl AddListSelectorControl(this IControlContainer container, System.String propertyName, List<ListSelectorControl.ListSelectorItem> items)
        {
            var control = new ListSelectorControl(propertyName, items);
            container.AddControl(control);
            return control;
        }
        
        public static ModuleSelectorControl AddModuleSelectorControl(this IControlContainer container, System.String propertyName, ModularShader shader)
        {
            var control = new ModuleSelectorControl(propertyName, shader);
            container.AddControl(control);
            return control;
        }

    }
}
