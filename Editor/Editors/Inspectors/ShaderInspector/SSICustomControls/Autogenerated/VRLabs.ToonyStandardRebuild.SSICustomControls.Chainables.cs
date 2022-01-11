namespace VRLabs.ToonyStandardRebuild.SSICustomControls
{
    public static partial class Chainables
    {
        public static ListSelectorControl AddListSelectorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.Collections.Generic.List<VRLabs.ToonyStandardRebuild.SSICustomControls.ListSelectorControl.ListSelectorItem> items, string appendAfterAlias = "")
        {
            var control = new ListSelectorControl(propertyName, items);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static ModuleSelectorControl AddModuleSelectorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, VRLabs.ToonyStandardRebuild.ModularShaderSystem.ModularShader shader, string appendAfterAlias = "")
        {
            var control = new ModuleSelectorControl(propertyName, shader);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static UVSetSelectorControl AddUVSetSelectorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.Collections.Generic.List<System.String> items, string appendAfterAlias = "")
        {
            var control = new UVSetSelectorControl(propertyName, items);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

    }
}
