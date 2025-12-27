Shader "Custom/URP/RealisticWater"
{
    Properties
{
    [Header(Colors)]
    // Reducir a�n m�s el alpha para mayor transparencia
    _ShallowColor ("Shallow Water Color", Color) = (0.32, 0.8, 0.97, 0.01)  // era 0.02
    _DeepColor ("Deep Water Color", Color) = (0.08, 0.4, 1.0, 0.02)         // era 0.05
    _FoamColor ("Foam Color", Color) = (1, 1, 1, 0.3)                       // Reducir foam tambi�n
    
    [Header(Surface)]
    _Smoothness ("Smoothness", Range(0,1)) = 0.95
    _NormalMap ("Normal Map", 2D) = "bump" {}
    _NormalStrength ("Normal Strength", Range(0, 2)) = 0.5                  // Reducir para menos distorsi�n
    _NormalTiling ("Normal Tiling", Float) = 1.0
    _NormalSpeed ("Normal Speed", Float) = 0.1
    
    [Header(Waves Vertex)]
    _WaveHeight ("Wave Height", Range(0, 1)) = 0.05                         // Olas mas sutiles
    _WaveFrequency ("Wave Frequency", Float) = 1.0
    _WaveSpeed ("Wave Speed", Float) = 1.0
    
    [Header(Depth and Foam)]
    _DepthFadeDistance ("Depth Fade Distance", Float) = 10.0                 // Aumentar para mas transparencia
    _RefractionStrength ("Refraction Strength", Range(0, 0.5)) = 0.02      // Reducir refraccion
    _FoamSize ("Foam Size", Range(0, 2)) = 0.3                              // Menos foam visible
}

    SubShader
    {
        Tags 
        { 
            "RenderPipeline"="UniversalPipeline" 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
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
                float3 viewDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float fogFactor : TEXCOORD5;
                float3 tangentWS : TEXCOORD6;
                float3 bitangentWS : TEXCOORD7;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float _Smoothness;
                float _NormalStrength;
                float _NormalTiling;
                float _NormalSpeed;
                float _WaveHeight;
                float _WaveFrequency;
                float _WaveSpeed;
                float _DepthFadeDistance;
                float _FoamSize;
                float _RefractionStrength;
                float4 _NormalMap_ST;
            CBUFFER_END

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                // Simple Vertex Waves
                float time = _Time.y * _WaveSpeed;
                float wave = sin(positionWS.x * _WaveFrequency + time) * 0.5 + 
                             cos(positionWS.z * _WaveFrequency * 0.8 + time * 0.8) * 0.5;
                positionWS.y += wave * _WaveHeight;

                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.uv = TRANSFORM_TEX(input.uv, _NormalMap);

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Normal Mapping
                float2 uvScroll1 = input.uv * _NormalTiling + float2(_Time.y * _NormalSpeed, _Time.y * _NormalSpeed * 0.5);
                float2 uvScroll2 = input.uv * _NormalTiling * 0.7 - float2(_Time.y * _NormalSpeed * 0.8, _Time.y * _NormalSpeed);
                
                half3 normal1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvScroll1));
                half3 normal2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvScroll2));
                half3 tangentNormal = normalize(normal1 + normal2);
                tangentNormal.xy *= _NormalStrength;
                
                float3 normalWS = TransformTangentToWorld(tangentNormal, half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
                normalWS = normalize(normalWS);

                // 2. Depth & Refraction
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                // Distort UVs for refraction
                float2 distortedUV = screenUV + (tangentNormal.xy * _RefractionStrength);
                
                float rawDepth = SampleSceneDepth(distortedUV);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfaceDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                float depthDiff = sceneDepth - surfaceDepth;
                
                // Fix artifacts if distorted depth is in front of water
                if (depthDiff < 0)
                {
                    distortedUV = screenUV;
                    rawDepth = SampleSceneDepth(distortedUV);
                    sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                    depthDiff = sceneDepth - surfaceDepth;
                }

                float depthFade = saturate(depthDiff / _DepthFadeDistance);

                // 3. Color & Absorption
                half3 sceneColor = SampleSceneColor(distortedUV);
                half3 waterColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFade);
                
                // Ajuste de transparencia para ver el interior
                //float alpha = lerp(_ShallowColor.a, _DeepColor.a, depthFade);
                float alpha = lerp(_ShallowColor.a * 0.7, _DeepColor.a * 0.7, depthFade);
                half3 finalColor = lerp(sceneColor, waterColor, alpha);

                // 4. Foam
                float foamMask = 1.0 - saturate(depthDiff / _FoamSize);
                foamMask = pow(foamMask, 4.0); // Sharpen foam
                // Add some noise to foam using normal map channels
                float foamNoise = (normal1.x + normal2.y) * 0.5 + 0.5; 
                foamMask *= foamNoise;
                finalColor = lerp(finalColor, _FoamColor.rgb, foamMask * _FoamColor.a);

                // 5. Lighting (Specular + Fresnel)
                Light mainLight = GetMainLight();
                float3 viewDir = normalize(input.viewDirWS);
                float3 halfDir = normalize(mainLight.direction + viewDir);
                
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float NdotH = saturate(dot(normalWS, halfDir));
                float specular = pow(NdotH, 500.0 * _Smoothness) * _Smoothness; // High specular for wet look
                
                // Fresnel
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), 4.0);
                
                finalColor += specular * mainLight.color;
                finalColor += fresnel * 0.2 * mainLight.color; // Add some sky reflection fake

                // Fog
                finalColor = MixFog(finalColor, input.fogFactor);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
