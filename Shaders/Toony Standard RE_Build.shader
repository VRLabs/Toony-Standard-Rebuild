Shader "VRLabs/Toony Standard RE:Build"
{
	Properties
	{
		_SrcBlend("Src Blend", Float) = 1.0
		_DstBlend("Dst Blend", Float) = 0.0
		_ZWrite("ZWrite", Float) = 1.0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Int) = 2
		_Mode("Blend mode", Int) = 0
		[IntRange] _StencilID("Stencil ID (0-255)", Range(0,255)) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp("Stencil Operation", Int) = 0
		_StencilSection("Stencil Section Display", Int) = 0
		_DirectLightMode("Direct Light Mode", Float) = 0.0
		_UVCount("UV Count", Float) = 0.0
		_UV1Index("UV1 Index", Float) = 0.0
		_MainTex("Main texture", 2D) = "white" {}
		_Color("Albedo color", Color) = (0, 0, 0, 0)
		_BumpMap("Normal map", 2D) = "bump" {}
		_BumpScale("Normal map scale", Float) = 1
		_Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
		_Occlusion("Occlusion", Range(0, 1)) = 1
		_MSSO("MSSO", 2D) = "white" {}
		_MainTex_UV("Main texture UV set", Float) = 0
		_BumpMap_UV("Bump map UV set", Float) = 0
		_MSSO_UV("MSSO UV set", Float) = 0
		_Ramp("Toon Ramp", 2D) = "white" {}
		_RampOffset("Ramp Offset", Range(-1, 1)) = 0
		_ShadowIntensity("Shadow intensity", Range(0, 1)) = 0.6
		_OcclusionOffsetIntensity("Occlusion Offset Intensity", Range(0, 1)) = 0
		_RampMin("Ramp Min", Color) = (0.003921569, 0.003921569, 0.003921569, 0.003921569)
		_RampColor("Ramp Color", Color) = (1, 1, 1, 1)
		_Metallic("Metallic", Range(0, 1)) = 0
		_Glossiness("Glossiness", Range(0, 1)) = 0
		_Specular("Specular", Range(0, 1)) = 0.5
		_SpecularTintTexture("Specular Tint Texture", 2D) = "white" {}
		_SpecularTint("Specular Tint", Color) = (1, 1, 1, 1)
		_ReplaceSpecular("Replace Specular", Float) = 0
		_SpecularMode("Specular Mode", Float) = -1
		_SpecularTintTexture_UV("Specular Tint UV Set", Float) = 0
		_EnableSpecular("Enable Specular", Float) = 0.0
		_TangentMap("Tangent Map", 2D) = "white" {}
		_Anisotropy("Ansotropy", Range(-1, 1)) = 0
		_TangentMap_UV("Tangent Map UV", Float) = 0
		_IndirectFallbackMode("Indirect Fallback Mode", Float) = 0.0
		_IndirectOverride("Indirect Override", Float) = 0.0
		_FallbackCubemap("Fallback Cubemap", Cube) = ""{}
	}
	SubShader
	{
		Blend [_SrcBlend] [_DstBlend]
		ZWrite [_ZWrite]
		Cull [_Cull]
		Stencil
		{
			Ref [_StencilID]
			Comp [_StencilComp]
			Pass [_StencilOp]
		}
		
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
			}
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex Vertex
			#pragma fragment Fragment
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile _ VERTEXLIGHT_ON
			#ifndef UNITY_PASS_FORWARDBASE
			#define UNITY_PASS_FORWARDBASE
			#endif
			
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHAMODULATE_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "UnityStandardUtils.cginc"
			#include "AutoLight.cginc"
			
			struct VertexData
			{
				float4 vertex     : POSITION;
				float2 uv         : TEXCOORD0;
				float2 uv1        : TEXCOORD1;
				float2 uv2        : TEXCOORD2;
				float2 uv3        : TEXCOORD3;
				float3 normal     : NORMAL;
				float4 tangentDir : TANGENT;
				
			};
			
			struct FragmentData
			{
				float4 pos        : SV_POSITION;
				float3 normal     : NORMAL;
				float4 tangentDir : TANGENT;
				float2 uv         : TEXCOORD0;
				float2 uv1        : TEXCOORD1;
				float2 uv2        : TEXCOORD2;
				float2 uv3        : TEXCOORD3;
				float3 worldPos   : TEXCOORD4;
				
				UNITY_SHADOW_COORDS(5)
				UNITY_FOG_COORDS(6)
				
				#if defined(LIGHTMAP_ON)
				float2 lightmapUV : TEXCOORD7;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD8;
				#endif
				
			};
			
			FragmentData FragData;
			float4 FinalColor;
			#if defined(_ALPHATEST_ON)
			float _Cutoff;
			#endif
			
			#if defined(_ALPHAMODULATE_ON)
			sampler3D _DitherMaskLOD;
			#endif
			float LightAttenuation;
			float _DirectLightMode;
			float UnmaxedNdotL;
			float NdotL;
			float NdotV;
			float NdotH;
			float LdotH;
			float _MainTex_UV;
			float _Occlusion;
			float Occlusion;
			float _MSSO_UV;
			float _BumpScale;
			float _BumpMap_UV;
			float Roughness;
			float Attenuation;
			float RampAttenuation;
			float _RampOffset;
			float _ShadowIntensity;
			float _OcclusionOffsetIntensity;
			float _Metallic;
			float Metallic;
			float _Glossiness;
			float Glossiness;
			float _Specular;
			float Specular;
			float SquareRoughness;
			float _ReplaceSpecular;
			float OneMinusReflectivity;
			float _SpecularTintTexture_UV;
			float GFS;
			float _Anisotropy;
			float _TangentMap_UV;
			float _IndirectFallbackMode;
			float _IndirectOverride;
			float _EnableSpecular;
			float _SpecularMode;
			float3 NormalMap;
			float3 NormalDir;
			float3 LightDir;
			float3 ViewDir;
			float3 TangentDir;
			float3 BitangentDir;
			float3 HalfDir;
			float3 ReflectDir;
			float3 Lightmap;
			float3 DynamicLightmap;
			float3 IndirectDiffuse;
			float3 Diffuse;
			float3 VertexDirectDiffuse;
			float3 _RampMin;
			float3 SpecularColor;
			float3 DirectSpecular;
			float3 NDF;
			float3 IndirectSpecular;
			float3 CustomIndirect;
			float4 LightmapDirection;
			float4 DynamicLightmapDirection;
			float4 SpecLightColor;
			float4 LightColor;
			float4 Albedo;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _MSSO_ST;
			float4 Msso;
			float4 _BumpMap_ST;
			float4 _RampColor;
			float4 _SpecularTintTexture_ST;
			float4 _SpecularTint;
			float4 _TangentMap_ST;
			samplerCUBE _FallbackCubemap;
			UNITY_DECLARE_TEX2D(_MainTex);
			UNITY_DECLARE_TEX2D(_Ramp);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_MSSO);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularTintTexture);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_TangentMap);
			
			#define TSR_TRANSFORM_TEX(set,name) (set[name##_UV].xy * name##_ST.xy + name##_ST.zw)
			
			float2 Uvs[16];
			inline void LoadUV0()
			{
				Uvs[0] = FragData.uv;
			}
			
			void LoadUVList()
			{
				LoadUV0();
				
			}
			inline float remap(float value, float oldMin, float oldMax, float newMin, float newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float2 remap(float2 value, float2 oldMin, float2 oldMax, float2 newMin, float2 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float3 remap(float3 value, float3 oldMin, float3 oldMax, float3 newMin, float3 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float4 remap(float4 value, float4 oldMin, float4 oldMax, float4 newMin, float4 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline half Pow5 (half x)
			{
				return x*x * x*x * x;
			}
			
			void LoadUVs()
			{
				LoadUVList();
			}
			void SampleAlbedo()
			{
				Albedo = UNITY_SAMPLE_TEX2D(_MainTex, TSR_TRANSFORM_TEX(Uvs, _MainTex)) * _Color;
			}
			void ClipAlpha()
			{
				#if defined(_ALPHATEST_ON)
				clip(Albedo.a - _Cutoff);
				#else
				#if defined(_ALPHAMODULATE_ON)
				float dither = tex3D(_DitherMaskLOD, float3(FragData.pos.xy * 0.25, Albedo.a * 0.9375)).a; //Dither16x16Bayer(FragData.pos.xy * 0.25) * Albedo.a;
				clip(dither-0.01);
				#endif
				#endif
			}
			void SampleNormal()
			{
				NormalMap = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, TSR_TRANSFORM_TEX(Uvs, _BumpMap)), _BumpScale);
			}
			void SampleMSSO()
			{
				Msso = UNITY_SAMPLE_TEX2D_SAMPLER(_MSSO, _MainTex, TSR_TRANSFORM_TEX(Uvs, _MSSO));
				Occlusion = lerp(1,Msso.a, _Occlusion);
			}
			void GetSampleData()
			{
				Metallic = Msso.r * _Metallic;
				Glossiness = Msso.g * _Glossiness;
				Specular = Msso.b * _Specular;
				
				Roughness = 1 - Glossiness;
				SquareRoughness = max(Roughness * Roughness, 0.002);
			}
			half3 Unity_SafeNormalize(half3 inVec)
			{
				half dp3 = max(0.001f, dot(inVec, inVec));
				return inVec * rsqrt(dp3);
			}
			
			void CalculateNormals(inout float3 normal, inout float3 tangent, inout float3 bitangent, float3 normalmap)
			{
				float3 tspace0 = float3(tangent.x, bitangent.x, normal.x);
				float3 tspace1 = float3(tangent.y, bitangent.y, normal.y);
				float3 tspace2 = float3(tangent.z, bitangent.z, normal.z);
				
				float3 calcedNormal;
				calcedNormal.x = dot(tspace0, normalmap);
				calcedNormal.y = dot(tspace1, normalmap);
				calcedNormal.z = dot(tspace2, normalmap);
				
				calcedNormal = normalize(calcedNormal);
				float3 bumpedTangent = (cross(bitangent, calcedNormal));
				float3 bumpedBitangent = (cross(calcedNormal, bumpedTangent));
				
				normal = calcedNormal;
				tangent = bumpedTangent;
				bitangent = bumpedBitangent;
			}
			
			void GetDirectionVectors()
			{
				NormalDir    = normalize(FragData.normal);
				TangentDir   = normalize(UnityObjectToWorldDir(FragData.tangentDir.xyz));
				BitangentDir = normalize(cross(NormalDir, TangentDir) * FragData.tangentDir.w * unity_WorldTransformParams.w);
				CalculateNormals(NormalDir, TangentDir, BitangentDir, NormalMap);
				LightDir     = normalize(UnityWorldSpaceLightDir(FragData.worldPos));
				ViewDir      = normalize(UnityWorldSpaceViewDir(FragData.worldPos));
				ReflectDir   = reflect(-ViewDir, NormalDir);
				HalfDir      = Unity_SafeNormalize(LightDir + ViewDir);
			}
			half3 GetSHLength()
			{
				half3 x, x1;
				x.r = length(unity_SHAr);
				x.g = length(unity_SHAg);
				x.b = length(unity_SHAb);
				x1.r = length(unity_SHBr);
				x1.g = length(unity_SHBg);
				x1.b = length(unity_SHBb);
				return x + x1;
			}
			
			float FadeShadows (FragmentData i, float attenuation)
			{
				#if HANDLE_SHADOWS_BLENDING_IN_GI && !defined (SHADOWS_SHADOWMASK)
				// UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
				float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
				float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
				float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
				attenuation = saturate(attenuation + shadowFade);
				#endif
				#if defined(LIGHTMAP_ON) && defined (SHADOWS_SHADOWMASK)
				// UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
				float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
				float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
				float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
				float bakedAttenuation = UnitySampleBakedOcclusion(i.lightmapUV, i.worldPos);
				attenuation = UnityMixRealtimeAndBakedShadows(attenuation, bakedAttenuation, shadowFade);
				//attenuation = saturate(attenuation + shadowFade);
				//attenuation = bakedAttenuation;
				
				#endif
				return attenuation;
			}
			
			void GetLightData()
			{
				UNITY_LIGHT_ATTENUATION(attenuation, FragData, FragData.worldPos);
				attenuation = FadeShadows(FragData, attenuation);
				LightAttenuation = attenuation;
				
				//lightmap sampling
				#if defined(LIGHTMAP_ON)
				Lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, FragData.lightmapUV));
				
				//directional map sampling
				#if defined(DIRLIGHTMAP_COMBINED)
				LightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, FragData.lightmapUV);
				#endif
				#endif
				//dynamic Lightmap sampling
				#if defined(DYNAMICLIGHTMAP_ON)
				DynamicLightmap = DecodeRealtimeLightmap( UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, FragData.dynamicLightmapUV));
				
				#if defined(DIRLIGHTMAP_COMBINED)
				DynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, FragData.dynamicLightmapUV);
				#endif
				#endif
				
				LightColor = float4(_LightColor0.rgb, LightAttenuation);
				SpecLightColor = LightColor;
				IndirectDiffuse = 0;
				//only counts it in the forwardBase pass
				#if defined(UNITY_PASS_FORWARDBASE)
				
				LightColor.rgb += float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
				//some random magic to get more tinted by the ambient
				LightColor.rgb = lerp(GetSHLength(), LightColor.rgb, .75);
				SpecLightColor = LightColor;
				
				#if defined(LIGHTMAP_ON)
				IndirectDiffuse = Lightmap;
				#if defined(DIRLIGHTMAP_COMBINED)
				IndirectDiffuse = DecodeDirectionalLightmap(IndirectDiffuse, LightmapDirection, NormalDir);
				#endif
				#endif
				
				#if defined(DYNAMICLIGHTMAP_ON)
				
				#if defined(DIRLIGHTMAP_COMBINED)
				IndirectDiffuse += DecodeDirectionalLightmap(DynamicLightmap, DynamicLightmapDirection, NormalDir);
				#else
				IndirectDiffuse += DynamicLightmap;
				#endif
				#endif
				
				#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
				
				//if there's no direct light, we get the probe light direction to use as direct light direction and
				//we consider the indirect light color as it was the direct light color.
				//also taking into account the case of a really low intensity being considered like non existent due to it no having
				//much relevance anyways and it can cause problems locally on mirrors if the avatat has a very low intensity light
				//just for enabling the depth buffer.
				IndirectDiffuse = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
				if(any(_WorldSpaceLightPos0.xyz) == 0 || _LightColor0.a < 0.01)
				{
					LightDir = normalize(unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
					SpecLightColor.rgb = IndirectDiffuse;
					LightColor.a = 1;
					if(_DirectLightMode > 0)
					{
						LightColor.rgb = IndirectDiffuse;
						IndirectDiffuse = 0;
					}
				}
				#endif
				#endif
			}
			void GetDotProducts()
			{
				UnmaxedNdotL = dot(NormalDir, LightDir);
				NdotL = max(UnmaxedNdotL, 0);
				NdotV = abs(dot(NormalDir, ViewDir));
				NdotH = max(dot(NormalDir, HalfDir),0);
				LdotH = max(dot(LightDir, HalfDir),0);
			}
			void SetupAlbedoAndSpecColor()
			{
				float3 specularTint = (UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularTintTexture, _MainTex, TSR_TRANSFORM_TEX(Uvs, _SpecularTintTexture)).rgb * _SpecularTint).rgb;
				
				float sp = Specular * 0.08;
				SpecularColor = lerp(float3(sp, sp, sp), Albedo.rgb, Metallic);
				if(_ReplaceSpecular == 1)
				{
					SpecularColor = specularTint;
				}
				else
				{
					SpecularColor *= specularTint;
				}
				OneMinusReflectivity = (1 - sp) - (Metallic * (1 - sp));
				Albedo.rgb *= OneMinusReflectivity;
			}
			void PremultiplyAlpha()
			{
				#if defined(_ALPHAPREMULTIPLY_ON)
				Albedo.rgb *= Albedo.a;
				#endif
			}
			//unity's base diffuse based on disney implementation
			float DisneyDiffuse(half perceptualRoughness)
			{
				float fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
				// Two schlick fresnel term
				float lightScatter   = (1 + (fd90 - 1) * Pow5(1 - NdotL));
				float viewScatter    = (1 + (fd90 - 1) * Pow5(1 - NdotV));
				
				return lightScatter * viewScatter;
			}
			
			// Get the direct diffuse contribution using disney's diffuse implementation
			void GetPBRDiffuse()
			{
				Diffuse = 0;
				float ramp = DisneyDiffuse(Roughness) * NdotL;
				Diffuse = Albedo * (LightColor.rgb * LightColor.a * ramp + IndirectDiffuse);
				Attenuation = ramp;
			}
			float4 RampDotL()
			{
				//Adding the occlusion into the offset of the ramp
				float offset = _RampOffset + ( Occlusion * _OcclusionOffsetIntensity) - _OcclusionOffsetIntensity;
				//Calculating ramp uvs based on offset
				float newMin = max(offset, 0);
				float newMax = max(offset + 1, 0);
				float rampUv = remap(UnmaxedNdotL, -1, 1, newMin, newMax);
				float3 ramp = UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv, rampUv)).rgb;
				//Adding the color and remapping it based on the shadow intensity stored into the alpha channel of the ramp color
				ramp *= _RampColor.rgb;
				float intensity = max(_ShadowIntensity, 0.001);
				ramp = remap(ramp, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1));
				
				//getting the modified ramp for highlights and all lights that are not directional
				float3 rmin = remap(_RampMin, 0, 1, 1-intensity, 1);
				float3 rampA = remap(ramp, rmin, 1, 0, 1);
				float rampGrey = max(max(rampA.r, rampA.g), rampA.b);
				#if defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE)
				return float4(ramp,rampGrey);
				#else
				return float4(rampA,rampGrey);
				#endif
				
			}
			
			void GetToonDiffuse()
			{
				Diffuse = 0;
				
				//toon version of the NdotL for the direct light
				float4 ramp = RampDotL();
				Attenuation = ramp.a;
				
				//diffuse color
				#if defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE)
				Diffuse = Albedo * ramp.rgb * (LightColor.rgb + IndirectDiffuse);
				#else
				Diffuse = Albedo * ramp.rgb * LightColor.rgb * LightColor.a;
				#endif
				
			}
			//modified version of Shade4PointLights
			float3 Shade4PointLights(float3 normal, float3 worldPos)
			{
				float4 toLightX = unity_4LightPosX0 - worldPos.x;
				float4 toLightY = unity_4LightPosY0 - worldPos.y;
				float4 toLightZ = unity_4LightPosZ0 - worldPos.z;
				float4 lengthSq = 0;
				lengthSq += toLightX * toLightX;
				lengthSq += toLightY * toLightY;
				lengthSq += toLightZ * toLightZ;
				float4 ndl = 0;
				ndl += toLightX * normal.x;
				ndl += toLightY * normal.y;
				ndl += toLightZ * normal.z;
				// correct NdotL
				float4 corr = rsqrt(lengthSq);
				ndl =  ndl * corr;
				//attenuation
				float4 atten = 1.0 / (1.0 + lengthSq * unity_4LightAtten0);
				
				float4 diff = max(ndl,0) * atten;
				// final color
				float3 col = 0;
				col += unity_LightColor[0] * diff.x;
				col += unity_LightColor[1] * diff.y;
				col += unity_LightColor[2] * diff.z;
				col += unity_LightColor[3] * diff.w;
				return col;
			}
			
			void GetPBRVertexDiffuse()
			{
				VertexDirectDiffuse = 0;
				#if defined(VERTEXLIGHT_ON)
				VertexDirectDiffuse = Shade4PointLights(NormalDir, FragData.worldPos);
				VertexDirectDiffuse *= Albedo;
				#endif
			}
			float3 RampDotLVertLight(float3 normal, float3 worldPos, float occlusion)
			{
				//from Shade4PointLights function to get NdotL + attenuation
				float4 toLightX = unity_4LightPosX0 - worldPos.x;
				float4 toLightY = unity_4LightPosY0 - worldPos.y;
				float4 toLightZ = unity_4LightPosZ0 - worldPos.z;
				float4 lengthSq = 0;
				lengthSq += toLightX * toLightX;
				lengthSq += toLightY * toLightY;
				lengthSq += toLightZ * toLightZ;
				float4 ndl = 0;
				ndl += toLightX * normal.x;
				ndl += toLightY * normal.y;
				ndl += toLightZ * normal.z;
				// correct NdotL
				float4 corr = rsqrt(lengthSq);
				ndl =  ndl * corr;
				//attenuation
				float4 atten = 1.0 / (1.0 + lengthSq * unity_4LightAtten0);
				float4 atten2 = saturate(1 - (lengthSq * unity_4LightAtten0 / 25));
				atten = min(atten, atten2 * atten2);
				
				//ramp calculation for all 4 vertex lights
				float offset = _RampOffset + (occlusion * _OcclusionOffsetIntensity) - _OcclusionOffsetIntensity;
				//Calculating ramp uvs based on offset
				float newMin = max(offset, 0);
				float newMax = max(offset + 1, 0);
				float4 rampUv = remap(ndl, float4(-1,-1,-1,-1), float4(1,1,1,1), float4(newMin, newMin, newMin, newMin), float4(newMax, newMax, newMax, newMax));
				float intensity = max(_ShadowIntensity, 0.001);
				float3 rmin = remap(_RampMin, 0, 1, 1-intensity, 1);
				float3 ramp = remap(remap(UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv.x, rampUv.x)).rgb * _RampColor.rgb, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1)), rmin, 1, 0, 1).rgb * unity_LightColor[0].rgb * atten.r;
				ramp +=       remap(remap(UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv.y, rampUv.y)).rgb * _RampColor.rgb, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1)), rmin, 1, 0, 1).rgb * unity_LightColor[1].rgb * atten.g;
				ramp +=       remap(remap(UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv.z, rampUv.z)).rgb * _RampColor.rgb, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1)), rmin, 1, 0, 1).rgb * unity_LightColor[2].rgb * atten.b;
				ramp +=       remap(remap(UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv.w, rampUv.w)).rgb * _RampColor.rgb, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1)), rmin, 1, 0, 1).rgb * unity_LightColor[3].rgb * atten.a;
				
				return ramp;
			}
			
			// Get the vertex diffuse contribution using a toon ramp
			void GetToonVertexDiffuse()
			{
				VertexDirectDiffuse = 0;
				#if defined(VERTEXLIGHT_ON)
				VertexDirectDiffuse = RampDotLVertLight(NormalDir, FragData.worldPos, Occlusion);
				VertexDirectDiffuse *= Albedo;
				#endif
			}
			float GTR2(float NdotH, float a)
			{
				float a2 = a*a;
				float t = 1 + (a2-1)*NdotH*NdotH;
				return a2 / (UNITY_PI * t*t + 1e-7f);
			}
			
			float smithG_GGX(float NdotV, float alphaG)
			{
				float a = alphaG*alphaG;
				float b = NdotV*NdotV;
				return 1 / (NdotV + sqrt(a + b - a*b));
			}
			
			void StandardDirectSpecular()
			{
				NDF = GTR2(NdotH, SquareRoughness) * UNITY_PI;
				GFS = smithG_GGX(max(NdotL,lerp(0.3,0,SquareRoughness)), Roughness) * smithG_GGX(NdotV, Roughness);
			}
			float sqr(float x) { return x*x; }
			
			float GTR2_aniso(float NdotH, float HdotX, float HdotY, float ax, float ay)
			{
				return 1 / (UNITY_PI * ax*ay * sqr( sqr(HdotX/ax) + sqr(HdotY/ay) + NdotH*NdotH ));
			}
			
			float smithG_GGX_aniso(float NdotV, float VdotX, float VdotY, float ax, float ay)
			{
				return 1 / (NdotV + sqrt( sqr(VdotX*ax) + sqr(VdotY*ay) + sqr(NdotV) ));
			}
			
			float3 GetModifiedTangent(float3 tangentTS, float3 tangentDir)
			{
				float3x3 worldToTangent;
				worldToTangent[0] = float3(1, 0, 0);
				worldToTangent[1] = float3(0, 1, 0);
				worldToTangent[2] = float3(0, 0, 1);
				
				float3 tangentTWS = mul(tangentTS, worldToTangent);
				float3 fTangent;
				if (tangentTS.z < 1)
				fTangent = tangentTWS;
				else
				fTangent = tangentDir;
				
				return fTangent;
			}
			
			void AnisotropicDirectSpecular()
			{
				float4 tangentTS = UNITY_SAMPLE_TEX2D_SAMPLER(_TangentMap, _MainTex, TSR_TRANSFORM_TEX(Uvs, _TangentMap));
				
				float anisotropy = tangentTS.a * _Anisotropy;
				float3 tangent = GetModifiedTangent(tangentTS.rgb, TangentDir);
				
				float3  anisotropyDirection = anisotropy >= 0.0 ? BitangentDir : TangentDir;
				float3  anisotropicTangent  = cross(anisotropyDirection, ViewDir);
				float3  anisotropicNormal   = cross(anisotropicTangent, anisotropyDirection);
				float   bendFactor          = abs(anisotropy) * saturate(1 - (Pow5(1 - SquareRoughness)));
				float3  bentNormal          = normalize(lerp(NormalDir, anisotropicNormal, bendFactor));
				ReflectDir = reflect(-ViewDir, bentNormal);
				
				float TdotH = dot(tangent, HalfDir);
				float TdotL = dot(tangent, LightDir);
				float BdotH = dot(BitangentDir, HalfDir);
				float BdotL = dot(BitangentDir, LightDir);
				float TdotV = dot(ViewDir, tangent);
				float BdotV = dot(ViewDir, BitangentDir);
				
				//float aspect = sqrt(1-anisotropy*.9);
				//float ax = max(.005, SquareRoughness / aspect);
				//float ay = max(.005, SquareRoughness * aspect);
				float ax = max(SquareRoughness * (1.0 + anisotropy), 0.005);
				float ay = max(SquareRoughness * (1.0 - anisotropy), 0.005);
				
				NDF = GTR2_aniso(NdotH, TdotH, BdotH, ax, ay) * UNITY_PI;
				
				GFS  = smithG_GGX_aniso(NdotL, TdotL, BdotL, ax, ay);
				GFS *= smithG_GGX_aniso(NdotV, TdotV, BdotV, ax, ay);
				
				//NDF = GTR2(NdotH, SquareRoughness) * UNITY_PI;
				//GFS = smithG_GGX(max(NdotL,lerp(0.3,0,SquareRoughness)), Roughness) * smithG_GGX(NdotV, Roughness);
			}
			inline half3 FresnelTerm (half3 F0, half cosA)
			{
				half t = Pow5 (1 - cosA);   // ala Schlick interpoliation
				return F0 + (1-F0) * t;
			}
			
			void FinalizeDirectSpecularTerm()
			{
				DirectSpecular = GFS * NDF;
				#ifdef UNITY_COLORSPACE_GAMMA
				DirectSpecular = sqrt(max(1e-4h, DirectSpecular));
				#endif
				DirectSpecular = max(0, DirectSpecular * Attenuation);
				DirectSpecular *= any(SpecularColor) ? 1.0 : 0.0;
				DirectSpecular *= FresnelTerm(SpecularColor, LdotH);
			}
			void GetFallbackCubemap()
			{
				CustomIndirect = texCUBElod(_FallbackCubemap, half4(ReflectDir.xyz, remap(SquareRoughness, 1, 0, 5, 0))).rgb;
			}
			struct Unity_GlossyEnvironmentData
			{
				// - Deferred case have one cubemap
				// - Forward case can have two blended cubemap (unusual should be deprecated).
				
				// Surface properties use for cubemap integration
				half    roughness; // CAUTION: This is perceptualRoughness but because of compatibility this name can't be change :(
				half3   reflUVW;
			};
			half perceptualRoughnessToMipmapLevel(half perceptualRoughness)
			{
				return perceptualRoughness * UNITY_SPECCUBE_LOD_STEPS;
			}
			
			half4 Unity_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, Unity_GlossyEnvironmentData glossIn)
			{
				half perceptualRoughness = glossIn.roughness /* perceptualRoughness */ ;
				
				// TODO: CAUTION: remap from Morten may work only with offline convolution, see impact with runtime convolution!
				// For now disabled
				#if 0
				float m = PerceptualRoughnessToRoughness(perceptualRoughness); // m is the real roughness parameter
				const float fEps = 1.192092896e-07F;        // smallest such that 1.0+FLT_EPSILON != 1.0  (+1e-4h is NOT good here. is visibly very wrong)
				float n =  (2.0/max(fEps, m*m))-2.0;        // remap to spec power. See eq. 21 in --> https://dl.dropboxusercontent.com/u/55891920/papers/mm_brdf.pdf
				
				n /= 4;                                     // remap from n_dot_h formulatino to n_dot_r. See section "Pre-convolved Cube Maps vs Path Tracers" --> https://s3.amazonaws.com/docs.knaldtech.com/knald/1.0.0/lys_power_drops.html
				
				perceptualRoughness = pow( 2/(n+2), 0.25);      // remap back to square root of real roughness (0.25 include both the sqrt root of the conversion and sqrt for going from roughness to perceptualRoughness)
				#else
				// MM: came up with a surprisingly close approximation to what the #if 0'ed out code above does.
				perceptualRoughness = perceptualRoughness*(1.7 - 0.7*perceptualRoughness);
				#endif
				
				half mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
				half3 R = glossIn.reflUVW;
				half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, R, mip);
				
				return float4(DecodeHDR(rgbm, hdr),rgbm.a);
			}
			
			inline half3 FresnelLerp (half3 F0, half3 F90, half cosA)
			{
				half t = Pow5 (1 - cosA);   // ala Schlick interpoliation
				return lerp (F0, F90, t);
			}
			
			void GetIndirectSpecular()
			{
				Unity_GlossyEnvironmentData envData;
				envData.roughness = Roughness;
				envData.reflUVW = BoxProjectedCubemapDirection(ReflectDir, FragData.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
				float4 indirectSpecularRGBA = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
				IndirectSpecular =  indirectSpecularRGBA.rgb;
				if ((_IndirectFallbackMode > 0 && indirectSpecularRGBA.a == 0) || (_IndirectOverride > 0))
				{
					//using the fake specular probe toned down based on the average light, it's not phisically accurate
					//but having a probe that reflects arbitrary stuff isn't accurate to begin with
					half lightColGrey = max((LightColor.r + LightColor.g + LightColor.b) / 3, (IndirectDiffuse.r + IndirectDiffuse.g + IndirectDiffuse.b) / 3);
					IndirectSpecular = CustomIndirect * min(lightColGrey, 1);
				}
				
				float grazingTerm = saturate(1 - SquareRoughness + (1 - OneMinusReflectivity));
				IndirectSpecular *= FresnelLerp(SpecularColor, grazingTerm, NdotV);
			}
			void AddStandardDiffuse()
			{
				FinalColor.rgb += Diffuse + VertexDirectDiffuse;
			}
			void AddToonDiffuse()
			{
				FinalColor.rgb += Diffuse + VertexDirectDiffuse;
			}
			void AddDirectSpecular()
			{
				FinalColor.rgb += DirectSpecular * SpecLightColor.rgb * SpecLightColor.a;
			}
			void AddIndirectSpecular()
			{
				FinalColor.rgb += IndirectSpecular * Occlusion;
			}
			void AddAlpha()
			{
				FinalColor.a = Albedo.a;
			}
			
			FragmentData Vertex (VertexData v)
			{
				FragmentData i;
				UNITY_INITIALIZE_OUTPUT(FragmentData, i);
				
				i.pos        = UnityObjectToClipPos(v.vertex);
				i.normal     = UnityObjectToWorldNormal(v.normal);
				i.worldPos   = mul(unity_ObjectToWorld, v.vertex);
				i.tangentDir = v.tangentDir;
				i.uv         = v.uv;
				i.uv1        = v.uv1;
				i.uv2        = v.uv2;
				i.uv3        = v.uv3;
				
				UNITY_TRANSFER_SHADOW(i, v.uv);
				UNITY_TRANSFER_FOG(i, i.pos);
				
				#if defined(LIGHTMAP_ON)
				i.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				
				#if defined(DYNAMICLIGHTMAP_ON)
				i.dynamicLightmapUV = v.uv2 * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				
				return i;
			}
			
			float4 Fragment (FragmentData i) : SV_TARGET
			{
				FragData = i;
				FinalColor = float4(0,0,0,0);
				
				LoadUVs();
				SampleAlbedo();
				ClipAlpha();
				SampleNormal();
				SampleMSSO();
				if(_EnableSpecular == 1)
				{
					GetSampleData();
				}
				GetDirectionVectors();
				GetLightData();
				GetDotProducts();
				if(_EnableSpecular == 1)
				{
					SetupAlbedoAndSpecColor();
				}
				PremultiplyAlpha();
				if(_DirectLightMode == 0)
				{
					GetPBRDiffuse();
				}
				if(_DirectLightMode == 1)
				{
					GetToonDiffuse();
				}
				if(_DirectLightMode == 0)
				{
					GetPBRVertexDiffuse();
				}
				if(_DirectLightMode == 1)
				{
					GetToonVertexDiffuse();
				}
				if(_SpecularMode == 0)
				{
					StandardDirectSpecular();
				}
				if(_SpecularMode == 1)
				{
					AnisotropicDirectSpecular();
				}
				if(_EnableSpecular == 1)
				{
					FinalizeDirectSpecularTerm();
				}
				if(_IndirectFallbackMode == 1)
				{
					GetFallbackCubemap();
				}
				if(_EnableSpecular == 1)
				{
					GetIndirectSpecular();
				}
				if(_DirectLightMode == 0)
				{
					AddStandardDiffuse();
				}
				if(_DirectLightMode == 1)
				{
					AddToonDiffuse();
				}
				if(_EnableSpecular == 1)
				{
					AddDirectSpecular();
				}
				if(_EnableSpecular == 1)
				{
					AddIndirectSpecular();
				}
				AddAlpha();
				
				return FinalColor;
			}
			
			ENDCG
		}
		
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardAdd"
			}
			
			Blend [_SrcBlend] One
			Fog { Color (0,0,0,0) } // in additive pass fog should be black
			ZWrite Off
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex Vertex
			#pragma fragment Fragment
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
			
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHAMODULATE_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "UnityStandardUtils.cginc"
			#include "AutoLight.cginc"
			
			struct VertexData
			{
				float4 vertex     : POSITION;
				float2 uv         : TEXCOORD0;
				float2 uv1        : TEXCOORD1;
				float2 uv2        : TEXCOORD2;
				float2 uv3        : TEXCOORD3;
				float3 normal     : NORMAL;
				float4 tangentDir : TANGENT;
				
			};
			
			struct FragmentData
			{
				float4 pos        : SV_POSITION;
				float3 normal     : NORMAL;
				float4 tangentDir : TANGENT;
				float2 uv         : TEXCOORD0;
				float2 uv1        : TEXCOORD1;
				float2 uv2        : TEXCOORD2;
				float2 uv3        : TEXCOORD3;
				float3 worldPos   : TEXCOORD4;
				
				UNITY_SHADOW_COORDS(5)
				UNITY_FOG_COORDS(6)
				
				#if defined(LIGHTMAP_ON)
				float2 lightmapUV : TEXCOORD7;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD8;
				#endif
				
			};
			
			FragmentData FragData;
			float4 FinalColor;
			#if defined(_ALPHATEST_ON)
			float _Cutoff;
			#endif
			
			#if defined(_ALPHAMODULATE_ON)
			sampler3D _DitherMaskLOD;
			#endif
			float LightAttenuation;
			float _DirectLightMode;
			float UnmaxedNdotL;
			float NdotL;
			float NdotV;
			float NdotH;
			float LdotH;
			float _MainTex_UV;
			float _Occlusion;
			float Occlusion;
			float _MSSO_UV;
			float _BumpScale;
			float _BumpMap_UV;
			float Roughness;
			float Attenuation;
			float RampAttenuation;
			float _RampOffset;
			float _ShadowIntensity;
			float _OcclusionOffsetIntensity;
			float _Metallic;
			float Metallic;
			float _Glossiness;
			float Glossiness;
			float _Specular;
			float Specular;
			float SquareRoughness;
			float _ReplaceSpecular;
			float OneMinusReflectivity;
			float _SpecularTintTexture_UV;
			float GFS;
			float _Anisotropy;
			float _TangentMap_UV;
			float _IndirectFallbackMode;
			float _IndirectOverride;
			float _EnableSpecular;
			float _SpecularMode;
			float3 NormalMap;
			float3 NormalDir;
			float3 LightDir;
			float3 ViewDir;
			float3 TangentDir;
			float3 BitangentDir;
			float3 HalfDir;
			float3 ReflectDir;
			float3 Lightmap;
			float3 DynamicLightmap;
			float3 IndirectDiffuse;
			float3 Diffuse;
			float3 VertexDirectDiffuse;
			float3 _RampMin;
			float3 SpecularColor;
			float3 DirectSpecular;
			float3 NDF;
			float3 IndirectSpecular;
			float3 CustomIndirect;
			float4 LightmapDirection;
			float4 DynamicLightmapDirection;
			float4 SpecLightColor;
			float4 LightColor;
			float4 Albedo;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _MSSO_ST;
			float4 Msso;
			float4 _BumpMap_ST;
			float4 _RampColor;
			float4 _SpecularTintTexture_ST;
			float4 _SpecularTint;
			float4 _TangentMap_ST;
			samplerCUBE _FallbackCubemap;
			UNITY_DECLARE_TEX2D(_MainTex);
			UNITY_DECLARE_TEX2D(_Ramp);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_MSSO);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularTintTexture);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_TangentMap);
			
			#define TSR_TRANSFORM_TEX(set,name) (set[name##_UV].xy * name##_ST.xy + name##_ST.zw)
			
			float2 Uvs[16];
			inline void LoadUV0()
			{
				Uvs[0] = FragData.uv;
			}
			
			void LoadUVList()
			{
				LoadUV0();
				
			}
			inline float remap(float value, float oldMin, float oldMax, float newMin, float newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float2 remap(float2 value, float2 oldMin, float2 oldMax, float2 newMin, float2 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float3 remap(float3 value, float3 oldMin, float3 oldMax, float3 newMin, float3 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float4 remap(float4 value, float4 oldMin, float4 oldMax, float4 newMin, float4 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline half Pow5 (half x)
			{
				return x*x * x*x * x;
			}
			
			void LoadUVs()
			{
				LoadUVList();
			}
			void SampleAlbedo()
			{
				Albedo = UNITY_SAMPLE_TEX2D(_MainTex, TSR_TRANSFORM_TEX(Uvs, _MainTex)) * _Color;
			}
			void ClipAlpha()
			{
				#if defined(_ALPHATEST_ON)
				clip(Albedo.a - _Cutoff);
				#else
				#if defined(_ALPHAMODULATE_ON)
				float dither = tex3D(_DitherMaskLOD, float3(FragData.pos.xy * 0.25, Albedo.a * 0.9375)).a; //Dither16x16Bayer(FragData.pos.xy * 0.25) * Albedo.a;
				clip(dither-0.01);
				#endif
				#endif
			}
			void SampleNormal()
			{
				NormalMap = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, TSR_TRANSFORM_TEX(Uvs, _BumpMap)), _BumpScale);
			}
			void SampleMSSO()
			{
				Msso = UNITY_SAMPLE_TEX2D_SAMPLER(_MSSO, _MainTex, TSR_TRANSFORM_TEX(Uvs, _MSSO));
				Occlusion = lerp(1,Msso.a, _Occlusion);
			}
			void GetSampleData()
			{
				Metallic = Msso.r * _Metallic;
				Glossiness = Msso.g * _Glossiness;
				Specular = Msso.b * _Specular;
				
				Roughness = 1 - Glossiness;
				SquareRoughness = max(Roughness * Roughness, 0.002);
			}
			half3 Unity_SafeNormalize(half3 inVec)
			{
				half dp3 = max(0.001f, dot(inVec, inVec));
				return inVec * rsqrt(dp3);
			}
			
			void CalculateNormals(inout float3 normal, inout float3 tangent, inout float3 bitangent, float3 normalmap)
			{
				float3 tspace0 = float3(tangent.x, bitangent.x, normal.x);
				float3 tspace1 = float3(tangent.y, bitangent.y, normal.y);
				float3 tspace2 = float3(tangent.z, bitangent.z, normal.z);
				
				float3 calcedNormal;
				calcedNormal.x = dot(tspace0, normalmap);
				calcedNormal.y = dot(tspace1, normalmap);
				calcedNormal.z = dot(tspace2, normalmap);
				
				calcedNormal = normalize(calcedNormal);
				float3 bumpedTangent = (cross(bitangent, calcedNormal));
				float3 bumpedBitangent = (cross(calcedNormal, bumpedTangent));
				
				normal = calcedNormal;
				tangent = bumpedTangent;
				bitangent = bumpedBitangent;
			}
			
			void GetDirectionVectors()
			{
				NormalDir    = normalize(FragData.normal);
				TangentDir   = normalize(UnityObjectToWorldDir(FragData.tangentDir.xyz));
				BitangentDir = normalize(cross(NormalDir, TangentDir) * FragData.tangentDir.w * unity_WorldTransformParams.w);
				CalculateNormals(NormalDir, TangentDir, BitangentDir, NormalMap);
				LightDir     = normalize(UnityWorldSpaceLightDir(FragData.worldPos));
				ViewDir      = normalize(UnityWorldSpaceViewDir(FragData.worldPos));
				ReflectDir   = reflect(-ViewDir, NormalDir);
				HalfDir      = Unity_SafeNormalize(LightDir + ViewDir);
			}
			half3 GetSHLength()
			{
				half3 x, x1;
				x.r = length(unity_SHAr);
				x.g = length(unity_SHAg);
				x.b = length(unity_SHAb);
				x1.r = length(unity_SHBr);
				x1.g = length(unity_SHBg);
				x1.b = length(unity_SHBb);
				return x + x1;
			}
			
			float FadeShadows (FragmentData i, float attenuation)
			{
				#if HANDLE_SHADOWS_BLENDING_IN_GI && !defined (SHADOWS_SHADOWMASK)
				// UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
				float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
				float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
				float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
				attenuation = saturate(attenuation + shadowFade);
				#endif
				#if defined(LIGHTMAP_ON) && defined (SHADOWS_SHADOWMASK)
				// UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
				float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
				float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
				float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
				float bakedAttenuation = UnitySampleBakedOcclusion(i.lightmapUV, i.worldPos);
				attenuation = UnityMixRealtimeAndBakedShadows(attenuation, bakedAttenuation, shadowFade);
				//attenuation = saturate(attenuation + shadowFade);
				//attenuation = bakedAttenuation;
				
				#endif
				return attenuation;
			}
			
			void GetLightData()
			{
				UNITY_LIGHT_ATTENUATION(attenuation, FragData, FragData.worldPos);
				attenuation = FadeShadows(FragData, attenuation);
				LightAttenuation = attenuation;
				
				//lightmap sampling
				#if defined(LIGHTMAP_ON)
				Lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, FragData.lightmapUV));
				
				//directional map sampling
				#if defined(DIRLIGHTMAP_COMBINED)
				LightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, FragData.lightmapUV);
				#endif
				#endif
				//dynamic Lightmap sampling
				#if defined(DYNAMICLIGHTMAP_ON)
				DynamicLightmap = DecodeRealtimeLightmap( UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, FragData.dynamicLightmapUV));
				
				#if defined(DIRLIGHTMAP_COMBINED)
				DynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, FragData.dynamicLightmapUV);
				#endif
				#endif
				
				LightColor = float4(_LightColor0.rgb, LightAttenuation);
				SpecLightColor = LightColor;
				IndirectDiffuse = 0;
				//only counts it in the forwardBase pass
				#if defined(UNITY_PASS_FORWARDBASE)
				
				LightColor.rgb += float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
				//some random magic to get more tinted by the ambient
				LightColor.rgb = lerp(GetSHLength(), LightColor.rgb, .75);
				SpecLightColor = LightColor;
				
				#if defined(LIGHTMAP_ON)
				IndirectDiffuse = Lightmap;
				#if defined(DIRLIGHTMAP_COMBINED)
				IndirectDiffuse = DecodeDirectionalLightmap(IndirectDiffuse, LightmapDirection, NormalDir);
				#endif
				#endif
				
				#if defined(DYNAMICLIGHTMAP_ON)
				
				#if defined(DIRLIGHTMAP_COMBINED)
				IndirectDiffuse += DecodeDirectionalLightmap(DynamicLightmap, DynamicLightmapDirection, NormalDir);
				#else
				IndirectDiffuse += DynamicLightmap;
				#endif
				#endif
				
				#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
				
				//if there's no direct light, we get the probe light direction to use as direct light direction and
				//we consider the indirect light color as it was the direct light color.
				//also taking into account the case of a really low intensity being considered like non existent due to it no having
				//much relevance anyways and it can cause problems locally on mirrors if the avatat has a very low intensity light
				//just for enabling the depth buffer.
				IndirectDiffuse = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
				if(any(_WorldSpaceLightPos0.xyz) == 0 || _LightColor0.a < 0.01)
				{
					LightDir = normalize(unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
					SpecLightColor.rgb = IndirectDiffuse;
					LightColor.a = 1;
					if(_DirectLightMode > 0)
					{
						LightColor.rgb = IndirectDiffuse;
						IndirectDiffuse = 0;
					}
				}
				#endif
				#endif
			}
			void GetDotProducts()
			{
				UnmaxedNdotL = dot(NormalDir, LightDir);
				NdotL = max(UnmaxedNdotL, 0);
				NdotV = abs(dot(NormalDir, ViewDir));
				NdotH = max(dot(NormalDir, HalfDir),0);
				LdotH = max(dot(LightDir, HalfDir),0);
			}
			void SetupAlbedoAndSpecColor()
			{
				float3 specularTint = (UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularTintTexture, _MainTex, TSR_TRANSFORM_TEX(Uvs, _SpecularTintTexture)).rgb * _SpecularTint).rgb;
				
				float sp = Specular * 0.08;
				SpecularColor = lerp(float3(sp, sp, sp), Albedo.rgb, Metallic);
				if(_ReplaceSpecular == 1)
				{
					SpecularColor = specularTint;
				}
				else
				{
					SpecularColor *= specularTint;
				}
				OneMinusReflectivity = (1 - sp) - (Metallic * (1 - sp));
				Albedo.rgb *= OneMinusReflectivity;
			}
			void PremultiplyAlpha()
			{
				#if defined(_ALPHAPREMULTIPLY_ON)
				Albedo.rgb *= Albedo.a;
				#endif
			}
			//unity's base diffuse based on disney implementation
			float DisneyDiffuse(half perceptualRoughness)
			{
				float fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
				// Two schlick fresnel term
				float lightScatter   = (1 + (fd90 - 1) * Pow5(1 - NdotL));
				float viewScatter    = (1 + (fd90 - 1) * Pow5(1 - NdotV));
				
				return lightScatter * viewScatter;
			}
			
			// Get the direct diffuse contribution using disney's diffuse implementation
			void GetPBRDiffuse()
			{
				Diffuse = 0;
				float ramp = DisneyDiffuse(Roughness) * NdotL;
				Diffuse = Albedo * (LightColor.rgb * LightColor.a * ramp + IndirectDiffuse);
				Attenuation = ramp;
			}
			float4 RampDotL()
			{
				//Adding the occlusion into the offset of the ramp
				float offset = _RampOffset + ( Occlusion * _OcclusionOffsetIntensity) - _OcclusionOffsetIntensity;
				//Calculating ramp uvs based on offset
				float newMin = max(offset, 0);
				float newMax = max(offset + 1, 0);
				float rampUv = remap(UnmaxedNdotL, -1, 1, newMin, newMax);
				float3 ramp = UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv, rampUv)).rgb;
				//Adding the color and remapping it based on the shadow intensity stored into the alpha channel of the ramp color
				ramp *= _RampColor.rgb;
				float intensity = max(_ShadowIntensity, 0.001);
				ramp = remap(ramp, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1));
				
				//getting the modified ramp for highlights and all lights that are not directional
				float3 rmin = remap(_RampMin, 0, 1, 1-intensity, 1);
				float3 rampA = remap(ramp, rmin, 1, 0, 1);
				float rampGrey = max(max(rampA.r, rampA.g), rampA.b);
				#if defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE)
				return float4(ramp,rampGrey);
				#else
				return float4(rampA,rampGrey);
				#endif
				
			}
			
			void GetToonDiffuse()
			{
				Diffuse = 0;
				
				//toon version of the NdotL for the direct light
				float4 ramp = RampDotL();
				Attenuation = ramp.a;
				
				//diffuse color
				#if defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE)
				Diffuse = Albedo * ramp.rgb * (LightColor.rgb + IndirectDiffuse);
				#else
				Diffuse = Albedo * ramp.rgb * LightColor.rgb * LightColor.a;
				#endif
				
			}
			//modified version of Shade4PointLights
			float3 Shade4PointLights(float3 normal, float3 worldPos)
			{
				float4 toLightX = unity_4LightPosX0 - worldPos.x;
				float4 toLightY = unity_4LightPosY0 - worldPos.y;
				float4 toLightZ = unity_4LightPosZ0 - worldPos.z;
				float4 lengthSq = 0;
				lengthSq += toLightX * toLightX;
				lengthSq += toLightY * toLightY;
				lengthSq += toLightZ * toLightZ;
				float4 ndl = 0;
				ndl += toLightX * normal.x;
				ndl += toLightY * normal.y;
				ndl += toLightZ * normal.z;
				// correct NdotL
				float4 corr = rsqrt(lengthSq);
				ndl =  ndl * corr;
				//attenuation
				float4 atten = 1.0 / (1.0 + lengthSq * unity_4LightAtten0);
				
				float4 diff = max(ndl,0) * atten;
				// final color
				float3 col = 0;
				col += unity_LightColor[0] * diff.x;
				col += unity_LightColor[1] * diff.y;
				col += unity_LightColor[2] * diff.z;
				col += unity_LightColor[3] * diff.w;
				return col;
			}
			
			void GetPBRVertexDiffuse()
			{
				VertexDirectDiffuse = 0;
				#if defined(VERTEXLIGHT_ON)
				VertexDirectDiffuse = Shade4PointLights(NormalDir, FragData.worldPos);
				VertexDirectDiffuse *= Albedo;
				#endif
			}
			float3 RampDotLVertLight(float3 normal, float3 worldPos, float occlusion)
			{
				//from Shade4PointLights function to get NdotL + attenuation
				float4 toLightX = unity_4LightPosX0 - worldPos.x;
				float4 toLightY = unity_4LightPosY0 - worldPos.y;
				float4 toLightZ = unity_4LightPosZ0 - worldPos.z;
				float4 lengthSq = 0;
				lengthSq += toLightX * toLightX;
				lengthSq += toLightY * toLightY;
				lengthSq += toLightZ * toLightZ;
				float4 ndl = 0;
				ndl += toLightX * normal.x;
				ndl += toLightY * normal.y;
				ndl += toLightZ * normal.z;
				// correct NdotL
				float4 corr = rsqrt(lengthSq);
				ndl =  ndl * corr;
				//attenuation
				float4 atten = 1.0 / (1.0 + lengthSq * unity_4LightAtten0);
				float4 atten2 = saturate(1 - (lengthSq * unity_4LightAtten0 / 25));
				atten = min(atten, atten2 * atten2);
				
				//ramp calculation for all 4 vertex lights
				float offset = _RampOffset + (occlusion * _OcclusionOffsetIntensity) - _OcclusionOffsetIntensity;
				//Calculating ramp uvs based on offset
				float newMin = max(offset, 0);
				float newMax = max(offset + 1, 0);
				float4 rampUv = remap(ndl, float4(-1,-1,-1,-1), float4(1,1,1,1), float4(newMin, newMin, newMin, newMin), float4(newMax, newMax, newMax, newMax));
				float intensity = max(_ShadowIntensity, 0.001);
				float3 rmin = remap(_RampMin, 0, 1, 1-intensity, 1);
				float3 ramp = remap(remap(UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv.x, rampUv.x)).rgb * _RampColor.rgb, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1)), rmin, 1, 0, 1).rgb * unity_LightColor[0].rgb * atten.r;
				ramp +=       remap(remap(UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv.y, rampUv.y)).rgb * _RampColor.rgb, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1)), rmin, 1, 0, 1).rgb * unity_LightColor[1].rgb * atten.g;
				ramp +=       remap(remap(UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv.z, rampUv.z)).rgb * _RampColor.rgb, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1)), rmin, 1, 0, 1).rgb * unity_LightColor[2].rgb * atten.b;
				ramp +=       remap(remap(UNITY_SAMPLE_TEX2D(_Ramp, float2(rampUv.w, rampUv.w)).rgb * _RampColor.rgb, float3(0, 0, 0), float3(1, 1, 1), 1 - intensity, float3(1, 1, 1)), rmin, 1, 0, 1).rgb * unity_LightColor[3].rgb * atten.a;
				
				return ramp;
			}
			
			// Get the vertex diffuse contribution using a toon ramp
			void GetToonVertexDiffuse()
			{
				VertexDirectDiffuse = 0;
				#if defined(VERTEXLIGHT_ON)
				VertexDirectDiffuse = RampDotLVertLight(NormalDir, FragData.worldPos, Occlusion);
				VertexDirectDiffuse *= Albedo;
				#endif
			}
			float GTR2(float NdotH, float a)
			{
				float a2 = a*a;
				float t = 1 + (a2-1)*NdotH*NdotH;
				return a2 / (UNITY_PI * t*t + 1e-7f);
			}
			
			float smithG_GGX(float NdotV, float alphaG)
			{
				float a = alphaG*alphaG;
				float b = NdotV*NdotV;
				return 1 / (NdotV + sqrt(a + b - a*b));
			}
			
			void StandardDirectSpecular()
			{
				NDF = GTR2(NdotH, SquareRoughness) * UNITY_PI;
				GFS = smithG_GGX(max(NdotL,lerp(0.3,0,SquareRoughness)), Roughness) * smithG_GGX(NdotV, Roughness);
			}
			float sqr(float x) { return x*x; }
			
			float GTR2_aniso(float NdotH, float HdotX, float HdotY, float ax, float ay)
			{
				return 1 / (UNITY_PI * ax*ay * sqr( sqr(HdotX/ax) + sqr(HdotY/ay) + NdotH*NdotH ));
			}
			
			float smithG_GGX_aniso(float NdotV, float VdotX, float VdotY, float ax, float ay)
			{
				return 1 / (NdotV + sqrt( sqr(VdotX*ax) + sqr(VdotY*ay) + sqr(NdotV) ));
			}
			
			float3 GetModifiedTangent(float3 tangentTS, float3 tangentDir)
			{
				float3x3 worldToTangent;
				worldToTangent[0] = float3(1, 0, 0);
				worldToTangent[1] = float3(0, 1, 0);
				worldToTangent[2] = float3(0, 0, 1);
				
				float3 tangentTWS = mul(tangentTS, worldToTangent);
				float3 fTangent;
				if (tangentTS.z < 1)
				fTangent = tangentTWS;
				else
				fTangent = tangentDir;
				
				return fTangent;
			}
			
			void AnisotropicDirectSpecular()
			{
				float4 tangentTS = UNITY_SAMPLE_TEX2D_SAMPLER(_TangentMap, _MainTex, TSR_TRANSFORM_TEX(Uvs, _TangentMap));
				
				float anisotropy = tangentTS.a * _Anisotropy;
				float3 tangent = GetModifiedTangent(tangentTS.rgb, TangentDir);
				
				float3  anisotropyDirection = anisotropy >= 0.0 ? BitangentDir : TangentDir;
				float3  anisotropicTangent  = cross(anisotropyDirection, ViewDir);
				float3  anisotropicNormal   = cross(anisotropicTangent, anisotropyDirection);
				float   bendFactor          = abs(anisotropy) * saturate(1 - (Pow5(1 - SquareRoughness)));
				float3  bentNormal          = normalize(lerp(NormalDir, anisotropicNormal, bendFactor));
				ReflectDir = reflect(-ViewDir, bentNormal);
				
				float TdotH = dot(tangent, HalfDir);
				float TdotL = dot(tangent, LightDir);
				float BdotH = dot(BitangentDir, HalfDir);
				float BdotL = dot(BitangentDir, LightDir);
				float TdotV = dot(ViewDir, tangent);
				float BdotV = dot(ViewDir, BitangentDir);
				
				//float aspect = sqrt(1-anisotropy*.9);
				//float ax = max(.005, SquareRoughness / aspect);
				//float ay = max(.005, SquareRoughness * aspect);
				float ax = max(SquareRoughness * (1.0 + anisotropy), 0.005);
				float ay = max(SquareRoughness * (1.0 - anisotropy), 0.005);
				
				NDF = GTR2_aniso(NdotH, TdotH, BdotH, ax, ay) * UNITY_PI;
				
				GFS  = smithG_GGX_aniso(NdotL, TdotL, BdotL, ax, ay);
				GFS *= smithG_GGX_aniso(NdotV, TdotV, BdotV, ax, ay);
				
				//NDF = GTR2(NdotH, SquareRoughness) * UNITY_PI;
				//GFS = smithG_GGX(max(NdotL,lerp(0.3,0,SquareRoughness)), Roughness) * smithG_GGX(NdotV, Roughness);
			}
			inline half3 FresnelTerm (half3 F0, half cosA)
			{
				half t = Pow5 (1 - cosA);   // ala Schlick interpoliation
				return F0 + (1-F0) * t;
			}
			
			void FinalizeDirectSpecularTerm()
			{
				DirectSpecular = GFS * NDF;
				#ifdef UNITY_COLORSPACE_GAMMA
				DirectSpecular = sqrt(max(1e-4h, DirectSpecular));
				#endif
				DirectSpecular = max(0, DirectSpecular * Attenuation);
				DirectSpecular *= any(SpecularColor) ? 1.0 : 0.0;
				DirectSpecular *= FresnelTerm(SpecularColor, LdotH);
			}
			void GetFallbackCubemap()
			{
				CustomIndirect = texCUBElod(_FallbackCubemap, half4(ReflectDir.xyz, remap(SquareRoughness, 1, 0, 5, 0))).rgb;
			}
			struct Unity_GlossyEnvironmentData
			{
				// - Deferred case have one cubemap
				// - Forward case can have two blended cubemap (unusual should be deprecated).
				
				// Surface properties use for cubemap integration
				half    roughness; // CAUTION: This is perceptualRoughness but because of compatibility this name can't be change :(
				half3   reflUVW;
			};
			half perceptualRoughnessToMipmapLevel(half perceptualRoughness)
			{
				return perceptualRoughness * UNITY_SPECCUBE_LOD_STEPS;
			}
			
			half4 Unity_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, Unity_GlossyEnvironmentData glossIn)
			{
				half perceptualRoughness = glossIn.roughness /* perceptualRoughness */ ;
				
				// TODO: CAUTION: remap from Morten may work only with offline convolution, see impact with runtime convolution!
				// For now disabled
				#if 0
				float m = PerceptualRoughnessToRoughness(perceptualRoughness); // m is the real roughness parameter
				const float fEps = 1.192092896e-07F;        // smallest such that 1.0+FLT_EPSILON != 1.0  (+1e-4h is NOT good here. is visibly very wrong)
				float n =  (2.0/max(fEps, m*m))-2.0;        // remap to spec power. See eq. 21 in --> https://dl.dropboxusercontent.com/u/55891920/papers/mm_brdf.pdf
				
				n /= 4;                                     // remap from n_dot_h formulatino to n_dot_r. See section "Pre-convolved Cube Maps vs Path Tracers" --> https://s3.amazonaws.com/docs.knaldtech.com/knald/1.0.0/lys_power_drops.html
				
				perceptualRoughness = pow( 2/(n+2), 0.25);      // remap back to square root of real roughness (0.25 include both the sqrt root of the conversion and sqrt for going from roughness to perceptualRoughness)
				#else
				// MM: came up with a surprisingly close approximation to what the #if 0'ed out code above does.
				perceptualRoughness = perceptualRoughness*(1.7 - 0.7*perceptualRoughness);
				#endif
				
				half mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
				half3 R = glossIn.reflUVW;
				half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, R, mip);
				
				return float4(DecodeHDR(rgbm, hdr),rgbm.a);
			}
			
			inline half3 FresnelLerp (half3 F0, half3 F90, half cosA)
			{
				half t = Pow5 (1 - cosA);   // ala Schlick interpoliation
				return lerp (F0, F90, t);
			}
			
			void GetIndirectSpecular()
			{
				Unity_GlossyEnvironmentData envData;
				envData.roughness = Roughness;
				envData.reflUVW = BoxProjectedCubemapDirection(ReflectDir, FragData.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
				float4 indirectSpecularRGBA = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
				IndirectSpecular =  indirectSpecularRGBA.rgb;
				if ((_IndirectFallbackMode > 0 && indirectSpecularRGBA.a == 0) || (_IndirectOverride > 0))
				{
					//using the fake specular probe toned down based on the average light, it's not phisically accurate
					//but having a probe that reflects arbitrary stuff isn't accurate to begin with
					half lightColGrey = max((LightColor.r + LightColor.g + LightColor.b) / 3, (IndirectDiffuse.r + IndirectDiffuse.g + IndirectDiffuse.b) / 3);
					IndirectSpecular = CustomIndirect * min(lightColGrey, 1);
				}
				
				float grazingTerm = saturate(1 - SquareRoughness + (1 - OneMinusReflectivity));
				IndirectSpecular *= FresnelLerp(SpecularColor, grazingTerm, NdotV);
			}
			void AddStandardDiffuse()
			{
				FinalColor.rgb += Diffuse + VertexDirectDiffuse;
			}
			void AddToonDiffuse()
			{
				FinalColor.rgb += Diffuse + VertexDirectDiffuse;
			}
			void AddDirectSpecular()
			{
				FinalColor.rgb += DirectSpecular * SpecLightColor.rgb * SpecLightColor.a;
			}
			void AddIndirectSpecular()
			{
				FinalColor.rgb += IndirectSpecular * Occlusion;
			}
			void AddAlpha()
			{
				FinalColor.a = Albedo.a;
			}
			
			FragmentData Vertex (VertexData v)
			{
				FragmentData i;
				UNITY_INITIALIZE_OUTPUT(FragmentData, i);
				
				i.pos        = UnityObjectToClipPos(v.vertex);
				i.normal     = UnityObjectToWorldNormal(v.normal);
				i.worldPos   = mul(unity_ObjectToWorld, v.vertex);
				i.tangentDir = v.tangentDir;
				i.uv         = v.uv;
				i.uv1        = v.uv1;
				i.uv2        = v.uv2;
				i.uv3        = v.uv3;
				
				UNITY_TRANSFER_SHADOW(i, v.uv);
				UNITY_TRANSFER_FOG(i, i.pos);
				
				#if defined(LIGHTMAP_ON)
				i.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				
				#if defined(DYNAMICLIGHTMAP_ON)
				i.dynamicLightmapUV = v.uv2 * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				
				return i;
			}
			
			float4 Fragment (FragmentData i) : SV_TARGET
			{
				FragData = i;
				FinalColor = float4(0,0,0,0);
				
				LoadUVs();
				SampleAlbedo();
				ClipAlpha();
				SampleNormal();
				SampleMSSO();
				if(_EnableSpecular == 1)
				{
					GetSampleData();
				}
				GetDirectionVectors();
				GetLightData();
				GetDotProducts();
				if(_EnableSpecular == 1)
				{
					SetupAlbedoAndSpecColor();
				}
				PremultiplyAlpha();
				if(_DirectLightMode == 0)
				{
					GetPBRDiffuse();
				}
				if(_DirectLightMode == 1)
				{
					GetToonDiffuse();
				}
				if(_DirectLightMode == 0)
				{
					GetPBRVertexDiffuse();
				}
				if(_DirectLightMode == 1)
				{
					GetToonVertexDiffuse();
				}
				if(_SpecularMode == 0)
				{
					StandardDirectSpecular();
				}
				if(_SpecularMode == 1)
				{
					AnisotropicDirectSpecular();
				}
				if(_EnableSpecular == 1)
				{
					FinalizeDirectSpecularTerm();
				}
				if(_IndirectFallbackMode == 1)
				{
					GetFallbackCubemap();
				}
				if(_EnableSpecular == 1)
				{
					GetIndirectSpecular();
				}
				if(_DirectLightMode == 0)
				{
					AddStandardDiffuse();
				}
				if(_DirectLightMode == 1)
				{
					AddToonDiffuse();
				}
				if(_EnableSpecular == 1)
				{
					AddDirectSpecular();
				}
				if(_EnableSpecular == 1)
				{
					AddIndirectSpecular();
				}
				AddAlpha();
				
				return FinalColor;
			}
			
			ENDCG
		}
		
		Pass
		{
			Tags
			{
				"LightMode" = "ShadowCaster"
			}
			
			ZWrite On ZTest LEqual
			
			CGPROGRAM
			
			#pragma target 3.0
			
			#pragma multi_compile_shadowcaster
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			
			#pragma vertex Vertex
			#pragma fragment Fragment
			
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHAMODULATE_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			
			#include "UnityCG.cginc"
			
			struct VertexData
			{
				float4 position : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
			};
			
			struct VertexOutput
			{
				float4 position : SV_POSITION;
				#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON)
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
				#endif
				#if defined(SHADOWS_CUBE)
				float3 lightVec : TEXCOORD4;
				#endif
			};
			
			struct FragmentData
			{
				#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON)
				UNITY_VPOS_TYPE pos : VPOS;
				#else
				float4 position : SV_POSITION;
				#endif
				#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON)
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
				#endif
				#if defined(SHADOWS_CUBE)
				float3 lightVec : TEXCOORD4;
				#endif
			};
			
			sampler3D _DitherMaskLOD;
			FragmentData FragData;
			#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON)
			float _MainTex_UV;
			float _Cutoff;
			float _DirectLightMode;
			float _EnableSpecular;
			float _SpecularMode;
			float _IndirectFallbackMode;
			float4 Albedo;
			float4 _MainTex_ST;
			float4 _Color;
			UNITY_DECLARE_TEX2D(_MainTex);
			
			#define TSR_TRANSFORM_TEX(set,name) (set[name##_UV].xy * name##_ST.xy + name##_ST.zw)
			
			float2 Uvs[16];
			inline void LoadUV0()
			{
				Uvs[0] = FragData.uv;
			}
			
			void LoadUVList()
			{
				LoadUV0();
				
			}
			inline float remap(float value, float oldMin, float oldMax, float newMin, float newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float2 remap(float2 value, float2 oldMin, float2 oldMax, float2 newMin, float2 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float3 remap(float3 value, float3 oldMin, float3 oldMax, float3 newMin, float3 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float4 remap(float4 value, float4 oldMin, float4 oldMax, float4 newMin, float4 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline half Pow5 (half x)
			{
				return x*x * x*x * x;
			}
			
			void LoadUVs()
			{
				LoadUVList();
			}
			void SampleAlbedo()
			{
				Albedo = UNITY_SAMPLE_TEX2D(_MainTex, TSR_TRANSFORM_TEX(Uvs, _MainTex)) * _Color;
			}
			void ClipShadowAlpha()
			{
				#if defined(_ALPHATEST_ON)
				clip(Albedo.a - _Cutoff);
				#else
				#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON)
				float dither = tex3D(_DitherMaskLOD, float3(FragData.pos.xy * 0.25, Albedo.a * 0.9375)).a; //Dither16x16Bayer(FragData.pos.xy * 0.25) * Albedo.a;
				clip(dither-0.01);
				#endif
				#endif
			}
			
			#endif
			
			VertexOutput  Vertex (VertexData v)
			{
				VertexOutput  i;
				#if defined(SHADOWS_CUBE)
				i.position = UnityObjectToClipPos(v.position);
				i.lightVec = mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz;
				#else
				i.position = UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal);
				i.position = UnityApplyLinearShadowBias(i.position);
				#endif
				#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON)
				i.uv = v.uv;
				i.uv1 = v.uv1;
				i.uv2 = v.uv2;
				i.uv3 = v.uv3;
				#endif
				return i;
			}
			
			float4 Fragment (FragmentData i) : SV_TARGET
			{
				FragData = i;
				#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON)
				LoadUVs();
				SampleAlbedo();
				ClipShadowAlpha();
				
				#endif
				
				#if defined(SHADOWS_CUBE)
				float depth = length(i.lightVec) + unity_LightShadowBias.x;
				depth *= _LightPositionRange.w;
				return UnityEncodeCubeShadowDepth(depth);
				#else
				return 0;
				#endif
			}
			
			ENDCG
		}
		
		Pass
		{
			Tags
			{
				"LightMode" = "Meta"
			}
			
			Cull Off
			
			CGPROGRAM
			
			#pragma vertex Vertex
			#pragma fragment Fragment
			
			#include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc"
			#include "UnityMetaPass.cginc"
			
			struct VertexData
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
			};
			
			struct FragmentData
			{
				float4 pos : SV_POSITION;
				float2 uv  : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
				
			};
			
			FragmentData FragData;
			float3 Albedo;
			float3 Emission;
			float3 SpecularColor;
			float Roughness;
			float _MainTex_UV;
			float _Occlusion;
			float Occlusion;
			float _MSSO_UV;
			float _Metallic;
			float Metallic;
			float _Glossiness;
			float Glossiness;
			float _Specular;
			float Specular;
			float SquareRoughness;
			float _ReplaceSpecular;
			float OneMinusReflectivity;
			float _SpecularTintTexture_UV;
			float _DirectLightMode;
			float _EnableSpecular;
			float _SpecularMode;
			float _IndirectFallbackMode;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _MSSO_ST;
			float4 Msso;
			float4 _SpecularTintTexture_ST;
			float4 _SpecularTint;
			UNITY_DECLARE_TEX2D(_MainTex);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_MSSO);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularTintTexture);
			
			#define TSR_TRANSFORM_TEX(set,name) (set[name##_UV].xy * name##_ST.xy + name##_ST.zw)
			
			float2 Uvs[16];
			inline void LoadUV0()
			{
				Uvs[0] = FragData.uv;
			}
			
			void LoadUVList()
			{
				LoadUV0();
				
			}
			inline float remap(float value, float oldMin, float oldMax, float newMin, float newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float2 remap(float2 value, float2 oldMin, float2 oldMax, float2 newMin, float2 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float3 remap(float3 value, float3 oldMin, float3 oldMax, float3 newMin, float3 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline float4 remap(float4 value, float4 oldMin, float4 oldMax, float4 newMin, float4 newMax)
			{
				return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
			}
			
			inline half Pow5 (half x)
			{
				return x*x * x*x * x;
			}
			
			void LoadUVs()
			{
				LoadUVList();
			}
			void SampleAlbedo()
			{
				Albedo = UNITY_SAMPLE_TEX2D(_MainTex, TSR_TRANSFORM_TEX(Uvs, _MainTex)) * _Color;
			}
			void SampleMSSO()
			{
				Msso = UNITY_SAMPLE_TEX2D_SAMPLER(_MSSO, _MainTex, TSR_TRANSFORM_TEX(Uvs, _MSSO));
				Occlusion = lerp(1,Msso.a, _Occlusion);
			}
			void GetSampleData()
			{
				Metallic = Msso.r * _Metallic;
				Glossiness = Msso.g * _Glossiness;
				Specular = Msso.b * _Specular;
				
				Roughness = 1 - Glossiness;
				SquareRoughness = max(Roughness * Roughness, 0.002);
			}
			void SetupAlbedoAndSpecColor()
			{
				float3 specularTint = (UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularTintTexture, _MainTex, TSR_TRANSFORM_TEX(Uvs, _SpecularTintTexture)).rgb * _SpecularTint).rgb;
				
				float sp = Specular * 0.08;
				SpecularColor = lerp(float3(sp, sp, sp), Albedo.rgb, Metallic);
				if(_ReplaceSpecular == 1)
				{
					SpecularColor = specularTint;
				}
				else
				{
					SpecularColor *= specularTint;
				}
				OneMinusReflectivity = (1 - sp) - (Metallic * (1 - sp));
				Albedo.rgb *= OneMinusReflectivity;
			}
			
			FragmentData  Vertex (VertexData v)
			{
				FragmentData  i;
				i.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
				i.uv  = v.uv;
				i.uv1 = v.uv1;
				i.uv2 = v.uv2;
				i.uv3 = v.uv3;
				
				return i;
			}
			
			float4 Fragment (FragmentData i) : SV_TARGET
			{
				FragData = i;
				UnityMetaInput surfaceData;
				UNITY_INITIALIZE_OUTPUT(UnityMetaInput, surfaceData);
				
				Albedo = 0;
				Emission = 0;
				SpecularColor = 0;
				Roughness = 1;
				
				LoadUVs();
				SampleAlbedo();
				SampleMSSO();
				if(_EnableSpecular == 1)
				{
					GetSampleData();
				}
				if(_EnableSpecular == 1)
				{
					SetupAlbedoAndSpecColor();
				}
				
				surfaceData.Emission = Emission;
				surfaceData.Albedo = Albedo + SpecularColor * Roughness * Roughness;
				surfaceData.SpecularColor = SpecularColor;
				return UnityMetaFragment(surfaceData);
			}
			
			ENDCG
		}
		
	}
	CustomEditor "VRLabs.ToonyStandardRebuild.TSRGUI"
}
