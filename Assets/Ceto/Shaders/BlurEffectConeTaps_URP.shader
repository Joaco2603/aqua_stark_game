Shader "Ceto/URP/BlurEffectConeTap" 
{
	Properties { _MainTex ("", 2D) = "" {} }
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
		
		Pass 
		{
			Name "BlurPass"
			
			ZTest Always
			Cull Off
			ZWrite Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 taps[4] : TEXCOORD1; 
			};
			
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float4 _MainTex_TexelSize;
			float4 _BlurOffsets;
			
			Varyings vert(Attributes input) 
			{
				Varyings output;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = input.uv - _BlurOffsets.xy * _MainTex_TexelSize.xy;

				output.taps[0] = output.uv + _MainTex_TexelSize.xy * _BlurOffsets.xy;
				output.taps[1] = output.uv - _MainTex_TexelSize.xy * _BlurOffsets.xy;
				output.taps[2] = output.uv + _MainTex_TexelSize.xy * _BlurOffsets.xy * half2(1,-1);
				output.taps[3] = output.uv - _MainTex_TexelSize.xy * _BlurOffsets.xy * half2(1,-1);
				
				return output;
			}
			
			half4 frag(Varyings input) : SV_Target 
			{
				half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.taps[0]);
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.taps[1]);
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.taps[2]);
				color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.taps[3]); 

				color *= 0.25;

				//If the shader for a object writes neg or nan number as the
				//color then when blurring the objects reflection the error
				//will get amplified and show as a black flickering patch.
				color = max(0.0, color);

				return color;
			}
			
			ENDHLSL
		}
	}
	
	Fallback Off
}
