using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public interface INonAnimatableProperty
    {
        bool NonAnimatablePropertyChanged { get; set; }

        void UpdateNonAnimatableProperty(MaterialEditor materialEditor);
    }
}