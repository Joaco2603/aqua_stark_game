Shader "Custom/URP/BasicWater"
{
    Properties
    {
        _ShallowColor ("Shallow Water Color", Color) = (0.325, 0.807, 0.971, 0.725)
        _DeepColor ("Deep Water Color", Color) = (0.086, 0.407, 1, 0.749)
        
        [Header(Surface)]
        _Smoothness ("Smoothness", Range(0,1)) = 0.95
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0.3
        _NormalMap ("Normal Map", 2D) = "bump" {}
        
        [Header(Animation)]
        _WaveSpeed ("Wave Speed", Float) = 0.1
        _WaveTiling ("Wave Tiling", Float) = 1.0
        
        [Header(Depth Fade)]
        _DepthFadeDistance ("Depth Fade Distance", Float) = 1.0
        
        [Header(Fresnel)]
        _FresnelBias ("Fresnel Bias", Range(0, 1)) = 0.0
        _FresnelStrength ("Fresnel Strength", Range(0, 5)) = 2.0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
            
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float _Smoothness;
                float _NormalStrength;
                float4 _NormalMap_ST;
                float _WaveSpeed;
                float _WaveTiling;
                float _DepthFadeDistance;
                float _FresnelBias;
                float _FresnelStrength;
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
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Animated wave normals
                float2 uv1 = input.uv * _WaveTiling + _Time.y * _WaveSpeed * float2(1, 0.5);
                float2 uv2 = input.uv * _WaveTiling * 0.7 - _Time.y * _WaveSpeed * float2(0.5, 1);
                
                float3 normal1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv1));
                float3 normal2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv2));
                float3 tangentNormal = normalize(normal1 + normal2);
                tangentNormal.xy *= _NormalStrength;
                
                // Transform normal to world space
                float3 bitangent = cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                float3x3 TBN = float3x3(input.tangentWS.xyz, bitangent, input.normalWS);
                float3 normalWS = normalize(mul(tangentNormal, TBN));
                
                // Calculate depth
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float surfaceDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                float depthDifference = sceneDepth - surfaceDepth;
                
                // Depth-based color
                float depthFade = saturate(depthDifference / _DepthFadeDistance);
                float3 waterColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFade);
                
                // View direction and fresnel
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);
                float fresnel = _FresnelBias + (1.0 - _FresnelBias) * pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelStrength);
                
                // Get scene color for refraction
                float3 sceneColor = SampleSceneColor(screenUV);
                
                // Mix refraction with water color
                float3 finalColor = lerp(sceneColor, waterColor, saturate(depthFade * 0.8));
                
                // Add specular highlights
                Light mainLight = GetMainLight();
                float3 halfVector = normalize(mainLight.direction + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfVector));
                float specular = pow(NdotH, 256.0 * _Smoothness) * _Smoothness;
                finalColor += specular * mainLight.color;
                
                // Add reflection based on fresnel
                float3 reflectVector = reflect(-viewDirWS, normalWS);
                float3 reflection = GlossyEnvironmentReflection(reflectVector, 1.0 - _Smoothness, 1.0);
                finalColor = lerp(finalColor, reflection, fresnel * 0.5);
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                // Alpha based on depth
                float alpha = lerp(_ShallowColor.a, _DeepColor.a, depthFade);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
