using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public class PropertyControl : SimpleControl
    {
        private int _propertyIndex = -2;

        public string PropertyName { get; protected set; }
        public MaterialProperty Property { get; protected set; }

        public bool HasPropertyUpdated { get; protected set; }

        public PropertyControl(string propertyName) : base(propertyName)
        {
            PropertyName = propertyName;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(Property, Content);
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
        }

        internal void SetPropertyIndex(MaterialProperty[] properties)
        {
            _propertyIndex = SSIHelper.FindPropertyIndex(PropertyName, properties);
        }

        internal void FetchProperty(MaterialProperty[] properties)
        {
            if (_propertyIndex == -2)
                SetPropertyIndex(properties);

            Property = _propertyIndex != -1 ? properties[_propertyIndex] : null;
        }
    }
}