using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public interface ISimpleShaderInspector : IControlContainer
    {
        Material[] Materials { get; }

        Shader Shader { get; }
    }
}