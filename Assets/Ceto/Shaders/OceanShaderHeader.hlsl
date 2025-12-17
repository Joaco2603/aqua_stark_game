#ifndef CETO_SHADER_HEADER_INCLUDED
#define CETO_SHADER_HEADER_INCLUDED

///////////////////////////////////////////////////////////
//                Common Block                          //
//////////////////////////////////////////////////////////

#if !defined (M_PI)
#define M_PI 3.141592657
#endif

#if !defined (M_SQRT_PI)
#define M_SQRT_PI 1.7724538
#endif

#define OBJECT_TO_WORLD unity_ObjectToWorld

///////////////////////////////////////////////////////////
//                Ocean BDRF Block                      //
//////////////////////////////////////////////////////////

TEXTURE2D(Ceto_Reflections0);
SAMPLER(sampler_Ceto_Reflections0);

TEXTURE2D(Ceto_Reflections1);
SAMPLER(sampler_Ceto_Reflections1);

float Ceto_SpecularRoughness;
float Ceto_SpecularIntensity;
float Ceto_MinFresnel;
float Ceto_FresnelPower;
float3 Ceto_ReflectionTint;
float Ceto_ReflectionDistortion;
float3 Ceto_DefaultSkyColor;

///////////////////////////////////////////////////////////
//                Ocean Displacement Block              //
//////////////////////////////////////////////////////////

TEXTURE2D(Ceto_FoamMap0);
SAMPLER(sampler_Ceto_FoamMap0);

TEXTURE2D(Ceto_SlopeMap0);
SAMPLER(sampler_Ceto_SlopeMap0);

TEXTURE2D(Ceto_SlopeMap1);
SAMPLER(sampler_Ceto_SlopeMap1);

TEXTURE2D(Ceto_DisplacementMap0);
SAMPLER(sampler_Ceto_DisplacementMap0);

TEXTURE2D(Ceto_DisplacementMap1);
SAMPLER(sampler_Ceto_DisplacementMap1);

TEXTURE2D(Ceto_DisplacementMap2);
SAMPLER(sampler_Ceto_DisplacementMap2);

TEXTURE2D(Ceto_DisplacementMap3);
SAMPLER(sampler_Ceto_DisplacementMap3);

float3 Ceto_PosOffset;

TEXTURE2D(Ceto_Overlay_NormalMap);
SAMPLER(sampler_Ceto_Overlay_NormalMap);

TEXTURE2D(Ceto_Overlay_HeightMap);
SAMPLER(sampler_Ceto_Overlay_HeightMap);

TEXTURE2D(Ceto_Overlay_FoamMap);
SAMPLER(sampler_Ceto_Overlay_FoamMap);

TEXTURE2D(Ceto_Overlay_ClipMap);
SAMPLER(sampler_Ceto_Overlay_ClipMap);

float4x4 Ceto_Interpolation;
float4x4 Ceto_ProjectorVP;
float4 Ceto_GridSizes;
float4 Ceto_Choppyness;
float2 Ceto_GridScale;
float2 Ceto_ScreenGridSize;
float Ceto_SlopeSmoothing;
float Ceto_FoamSmoothing;
float Ceto_WaveSmoothing;
float Ceto_MapSize;
float Ceto_GridEdgeBorder;
float Ceto_OceanLevel;
float Ceto_MaxWaveHeight;

///////////////////////////////////////////////////////////
//                Ocean Underwater Block                //
//////////////////////////////////////////////////////////

TEXTURE2D(Ceto_OceanDepth0);
SAMPLER(sampler_Ceto_OceanDepth0);

TEXTURE2D(Ceto_OceanDepth1);
SAMPLER(sampler_Ceto_OceanDepth1);

TEXTURE2D(Ceto_DepthBuffer);
SAMPLER(sampler_Ceto_DepthBuffer);

TEXTURE2D(Ceto_NormalFade);
SAMPLER(sampler_Ceto_NormalFade);

// URP Camera Opaque Texture instead of GrabPass
TEXTURE2D(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);

float4x4 Ceto_Camera_IVP0, Ceto_Camera_IVP1;

float3 Ceto_SunDir;
float3 Ceto_SunColor;
float3 Ceto_DefaultOceanColor;
float Ceto_MaxDepthDist;
float Ceto_AboveRefractionIntensity;
float Ceto_BelowRefractionIntensity;
float Ceto_RefractionDistortion;
float3 Ceto_FoamTint;
float Ceto_DepthBlend;
float Ceto_EdgeFade;

float4 Ceto_SSSCof;
float3 Ceto_SSSTint;

float4 Ceto_AbsCof;
float3 Ceto_AbsTint;

float4 Ceto_BelowCof;
float3 Ceto_BelowTint;

float Ceto_AboveInscatterScale;
float3 Ceto_AboveInscatterMode;
float4 Ceto_AboveInscatterColor;

float Ceto_BelowInscatterScale;
float3 Ceto_BelowInscatterMode;
float4 Ceto_BelowInscatterColor;

TEXTURE2D(Ceto_FoamTexture0);
SAMPLER(sampler_Ceto_FoamTexture0);
float4 Ceto_FoamTextureScale0;

TEXTURE2D(Ceto_FoamTexture1);
SAMPLER(sampler_Ceto_FoamTexture1);
float4 Ceto_FoamTextureScale1;

float Ceto_TextureWaveFoam;

TEXTURE2D(Ceto_CausticTexture);
SAMPLER(sampler_Ceto_CausticTexture);
float4 Ceto_CausticTextureScale;
float3 Ceto_CausticTint;
float2 Ceto_CausticDistortion;

///////////////////////////////////////////////////////////
//                Ocean Masking Block                   //
//////////////////////////////////////////////////////////

TEXTURE2D(Ceto_OceanMask0);
SAMPLER(sampler_Ceto_OceanMask0);

TEXTURE2D(Ceto_OceanMask1);
SAMPLER(sampler_Ceto_OceanMask1);

#define EMPTY_MASK 0.0
#define TOP_MASK 0.25
#define UNDER_MASK 0.5
#define BOTTOM_MASK 1.0

#endif
