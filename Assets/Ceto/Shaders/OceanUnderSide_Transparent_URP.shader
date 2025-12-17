Shader "Ceto/URP/OceanUnderSide_Transparent" 
{
	Properties 
	{
		[HideInInspector] _CullFace ("__cf", Float) = 1.0
	}
	
	SubShader 
	{
		Tags 
		{ 
			"RenderType"="Transparent"
			"RenderPipeline"="UniversalPipeline"
			"Queue"="Transparent-102"
			"OceanMask"="Ceto_ProjectedGrid_Under"
		}
		LOD 300
		
		Pass
		{
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward" }
			
			Cull [_CullFace]
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex OceanVert
			#pragma fragment OceanFrag
			
			// URP Keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			
			// Ceto keywords
			#pragma multi_compile __ CETO_REFLECTION_ON
			#pragma multi_compile __ CETO_UNDERWATER_ON
			#pragma multi_compile __ CETO_USE_OCEAN_DEPTHS_BUFFER
			#pragma multi_compile __ CETO_USE_4_SPECTRUM_GRIDS
			
			#define CETO_NICE_BRDF
			#define CETO_OCEAN_UNDERSIDE
			#define CETO_TRANSPARENT_QUEUE

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			
			#include "OceanShaderHeader.hlsl"
			#include "OceanDisplacement.hlsl"
			#include "OceanBRDF.hlsl"
			#include "OceanUnderWater.hlsl"
			
			struct Attributes
			{
				float4 positionOS : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};
			
			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 wPos : TEXCOORD0;
				float4 screenUV : TEXCOORD1;
				float4 texUV : TEXCOORD2;
				float3 viewDirWS : TEXCOORD3;
			};
			
			Varyings OceanVert(Attributes input)
			{
				Varyings output = (Varyings)0;
				
				float4 uv = float4(input.positionOS.xy, input.texcoord.xy);
				
				float4 oceanPos;
				float3 displacement;
				OceanPositionAndDisplacement(uv, oceanPos, displacement);
				
				float3 positionWS = oceanPos.xyz + displacement;
				output.positionCS = TransformWorldToHClip(positionWS);
				
				output.wPos = float4(positionWS, ComputeDepth01(output.positionCS.z, output.positionCS.w));
				output.texUV = uv;
				output.screenUV = ComputeScreenPos(output.positionCS);
				output.viewDirWS = GetWorldSpaceViewDir(positionWS);
				
				return output;
			}
			
			half4 OceanFrag(Varyings input) : SV_Target
			{
				float4 uv = input.texUV;
				float3 worldPos = input.wPos.xyz;
				float depth = input.wPos.w;
				
				float2 screenUV = input.screenUV.xy / input.screenUV.w;
				
				float4 st = WorldPosToProjectorSpace(worldPos);
				OceanClip(st, worldPos);
				
				half3 norm;
				half3 unmaskedNorm;
				fixed4 foam;
				OceanNormalAndFoam(uv, st, worldPos, norm, unmaskedNorm, foam);
				norm.y *= -1.0;
				
				half3 view = normalize(input.viewDirWS);
				float dist = length(GetCameraPositionWS() - worldPos);
				
				if (dot(view, norm) < 0.0) norm = reflect(norm, view);
				
				float4 distortionUV = DisortScreenUV(norm, float4(screenUV, screenUV), depth, dist, view);
				
				fixed3 sky = SkyColorFromBelow(distortionUV);
				fixed3 sea = DefaultUnderSideColor();
				
				float fresnel = FresnelWaterAir(view, norm);
				fixed foamAmount = FoamAmount(worldPos, foam);
				
				fixed3 col = fixed3(0,0,0);
				col += sea * fresnel;
				col += sky * (1.0-fresnel);
				col = AddFoamColor(foamAmount, col);
				
				// Setup surface for lighting
				SurfaceOutputOcean surface;
				surface.Albedo = col;
				surface.Normal = norm;
				surface.DNormal = norm;
				surface.Emission = 0;
				surface.Fresnel = fresnel;
				surface.Foam = foamAmount;
				surface.Alpha = 1.0;
				surface.LightMask = 0.0;
				
				// Apply lighting
				half4 finalColor = LightingOceanBRDF(surface, view, worldPos);
				
				return finalColor;
			}
			
			ENDHLSL
		}
		
		Pass 
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			Cull [_CullFace]
			ZWrite On
			ZTest LEqual
			ColorMask 0
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment
			
			#pragma multi_compile __ CETO_USE_4_SPECTRUM_GRIDS
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			
			#include "OceanShaderHeader.hlsl"
			#include "OceanDisplacement.hlsl"
			
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float2 texcoord : TEXCOORD0;
			};
			
			struct Varyings
			{
				float4 positionCS : SV_POSITION;
			};
			
			float3 ApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
			{
				float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
				float scale = invNdotL * _ShadowBias.y;
				positionWS = lightDirection * _ShadowBias.xxx + positionWS;
				positionWS = normalWS * scale.xxx + positionWS;
				return positionWS;
			}
			
			Varyings ShadowPassVertex(Attributes input)
			{
				Varyings output;
				
				float4 uv = float4(input.positionOS.xy, input.texcoord.xy);
				
				float4 oceanPos;
				float3 displacement;
				OceanPositionAndDisplacement(uv, oceanPos, displacement);
				
				float3 positionWS = oceanPos.xyz + displacement;
				float3 normalWS = float3(0, -1, 0);
				
				Light mainLight = GetMainLight();
				positionWS = ApplyShadowBias(positionWS, normalWS, mainLight.direction);
				
				output.positionCS = TransformWorldToHClip(positionWS);
				
				#if UNITY_REVERSED_Z
					output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#else
					output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#endif
				
				return output;
			}
			
			half4 ShadowPassFragment(Varyings input) : SV_TARGET
			{
				return 0;
			}
			
			ENDHLSL
		}
	}
	
	FallBack Off
}
