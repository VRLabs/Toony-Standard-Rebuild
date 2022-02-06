using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls;

namespace VRLabs.ToonyStandardRebuild
{
    internal class TSRTangentGenGUI : TextureGeneratorShaderInspector
    {
        protected override void Start()
        {
            this.AddTextureControl("_TangentMap").WithAlias("TangentGen_Tangent");
            this.AddTextureControl("_AnisotropyMap").WithAlias("TangentGen_Anisotropy");
            this.AddRGBASelectorControl("_AnisotropyChannel").WithAlias("TangentGen_Channel");
        }
    }
}