namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public static partial class Chainables
    {
        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.PropertyControl AddPropertyControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.PropertyControl(propertyName);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

    }
}