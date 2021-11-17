using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public interface IAdditionalProperties
    {
        AdditionalProperty[] AdditionalProperties { get; set; }
    }

    public class AdditionalProperty
    {
        public string PropertyName { get; }
        
        public bool IsPropertyMandatory { get; }

        private int _propertyIndex = -2;

        public MaterialProperty Property { get; private set; }

        public AdditionalProperty(string propertyName, bool isPropertyMandatory = true)
        {
            IsPropertyMandatory = isPropertyMandatory;
            PropertyName = propertyName;
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