Shader "Hidden/VRLabs/TSRGeneratorShaders/TangentGen"
{
    Properties
    {
        _TangentMap ("Tangent Map", 2D) = "white" {}
        _AnisotropyMap ("Anisotropy Map", 2D) = "white" {}
        
        _AnisotropyChannel("Anisotropy Channel", Float) = 0
    }
    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM

            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            UNITY_DECLARE_TEX2D(_TangentMap);
            UNITY_DECLARE_TEX2D_NOSAMPLER(_AnisotropyMap);
            float4 _TangentMap_ST;

            float _AnisotropyChannel;

            fixed4 frag (v2f_customrendertexture  i) : SV_Target
            {
                float3 rgb = UNITY_SAMPLE_TEX2D(_TangentMap, i.localTexcoord.xy).rgb;
                float a = UNITY_SAMPLE_TEX2D_SAMPLER(_AnisotropyMap, _TangentMap, i.localTexcoord.xy)[_AnisotropyChannel];
                return float4(rgb, a);
            }
            ENDCG
        }
    }
    CustomEditor "VRLabs.ToonyStandardRebuild.TSRTangentGenGUI"
}
