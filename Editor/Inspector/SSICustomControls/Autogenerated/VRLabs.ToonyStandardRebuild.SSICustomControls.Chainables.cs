using System.Collections.Generic;
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

    }
}
