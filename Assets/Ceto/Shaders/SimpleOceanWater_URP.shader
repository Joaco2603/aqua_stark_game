Shader "Custom/URP/SimpleOceanWater"
{
    Properties
    {
        _WaterColor ("Water Color", Color) = (0.0, 0.4, 0.7, 1.0)
        _DeepWaterColor ("Deep Water Color", Color) = (0.0, 0.1, 0.3, 1.0)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.9
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        
        [Header(Waves)]
        _WaveNormalMap ("Wave Normal Map", 2D) = "bump" {}
        _WaveSpeed ("Wave Speed", Vector) = (0.05, 0.04, -0.03, -0.06)
        _WaveScale ("Wave Scale", Vector) = (1, 1, 0.5, 0.5)
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.5
        
        [Header(Refraction)]
        _RefractionStrength ("Refraction Strength", Range(0, 0.5)) = 0.1
        
        [Header(Fresnel)]
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 3.0
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.8
        
        [Header(Foam)]
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamDistance ("Foam Distance", Range(0, 1)) = 0.2
        _FoamCutoff ("Foam Cutoff", Range(0, 1)) = 0.7
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 300
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float fogFactor : TEXCOORD5;
            };
            
            TEXTURE2D(_WaveNormalMap);
            SAMPLER(sampler_WaveNormalMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _WaterColor;
                float4 _DeepWaterColor;
                float _Smoothness;
                float _Metallic;
                float4 _WaveNormalMap_ST;
                float4 _WaveSpeed;
                float4 _WaveScale;
                float _WaveStrength;
                float _RefractionStrength;
                float _FresnelPower;
                float _ReflectionStrength;
                float4 _FoamColor;
                float _FoamDistance;
                float _FoamCutoff;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4(normalInput.tangentWS, input.tangentOS.w);
                output.uv = TRANSFORM_TEX(input.uv, _WaveNormalMap);
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            float3 BlendNormals(float3 n1, float3 n2)
            {
                return normalize(float3(n1.xy + n2.xy, n1.z * n2.z));
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample animated wave normals
                float2 uv1 = input.uv * _WaveScale.xy + _Time.y * _WaveSpeed.xy;
                float2 uv2 = input.uv * _WaveScale.zw + _Time.y * _WaveSpeed.zw;
                
                float3 normalMap1 = UnpackNormal(SAMPLE_TEXTURE2D(_WaveNormalMap, sampler_WaveNormalMap, uv1));
                float3 normalMap2 = UnpackNormal(SAMPLE_TEXTURE2D(_WaveNormalMap, sampler_WaveNormalMap, uv2));
                
                float3 tangentNormal = BlendNormals(normalMap1, normalMap2);
                tangentNormal.xy *= _WaveStrength;
                
                // Transform to world space
                float3 bitangent = cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                float3x3 TBN = float3x3(input.tangentWS.xyz, bitangent, input.normalWS);
                float3 normalWS = normalize(mul(tangentNormal, TBN));
                
                // View direction
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);
                
                // Fresnel effect
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                
                // Depth-based effects
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float surfaceDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                float depthDifference = sceneDepth - surfaceDepth;
                
                // Refraction
                float2 distortion = tangentNormal.xy * _RefractionStrength;
                float2 distortedUV = screenUV + distortion;
                
                // Check if distortion goes behind surface
                float distortedDepth = LinearEyeDepth(SampleSceneDepth(distortedUV), _ZBufferParams);
                if (distortedDepth < surfaceDepth)
                    distortedUV = screenUV;
                
                float3 refraction = SampleSceneColor(distortedUV);
                
                // Water color based on depth
                float depthFade = saturate(depthDifference * 0.5);
                float3 waterColor = lerp(_WaterColor.rgb, _DeepWaterColor.rgb, depthFade);
                
                // Mix refraction with water color
                float3 finalColor = lerp(refraction, waterColor, depthFade * 0.7);
                
                // Lighting
                Light mainLight = GetMainLight();
                float3 lightColor = mainLight.color;
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                
                // Specular
                float3 halfVector = normalize(mainLight.direction + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfVector));
                float spec = pow(NdotH, 128.0 * _Smoothness) * _Smoothness;
                float3 specular = spec * lightColor;
                
                // Reflection from environment
                float3 reflectVector = reflect(-viewDirWS, normalWS);
                float3 reflection = GlossyEnvironmentReflection(reflectVector, 1.0 - _Smoothness, 1.0);
                
                // Combine
                finalColor += specular;
                finalColor = lerp(finalColor, reflection, fresnel * _ReflectionStrength);
                
                // Foam
                float foamMask = saturate(1.0 - depthDifference / _FoamDistance);
                foamMask = step(_FoamCutoff, foamMask);
                finalColor = lerp(finalColor, _FoamColor.rgb, foamMask);
                
                // Fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                // Alpha
                float alpha = saturate(depthFade + fresnel * 0.5);
                
                return half4(finalColor, alpha);
            }
            
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
