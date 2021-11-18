namespace VRLabs.ToonyStandardRebuild.SSICustomControls
{
    public static partial class Chainables
    {
        public static ListSelectorControl AddListSelectorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.Collections.Generic.List<ListSelectorControl.ListSelectorItem> items)
        {
            var control = new ListSelectorControl(propertyName, items);
            container.AddControl(control);
            return control;
        }

        public static ModuleSelectorControl AddModuleSelectorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, VRLabs.ToonyStandardRebuild.ModularShaderSystem.ModularShader shader)
        {
            var control = new ModuleSelectorControl(propertyName, shader);
            container.AddControl(control);
            return control;
        }

        public static UVSetSelectorControl AddUVSetSelectorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.Collections.Generic.List<string> items)
        {
            var control = new UVSetSelectorControl(propertyName, items);
            container.AddControl(control);
            return control;
        }

    }
}
