namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public static partial class Chainables
    {
        public static PropertyControl AddPropertyControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName)
        {
            var control = new PropertyControl(propertyName);
            container.AddControl(control);
            return control;
        }

    }
}