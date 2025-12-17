#ifndef CETO_OCEAN_BRDF_INCLUDED
#define CETO_OCEAN_BRDF_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct SurfaceOutputOcean 
{
    half3 Albedo;
    half3 Normal; //Normal for specular
    half3 DNormal; //Normal for diffuse
    half3 Emission;
    half Fresnel;
	half Foam;
    half Alpha;
	half LightMask;
};

fixed FresnelAirWater(fixed3 V, fixed3 N) 
{
	float str = Ceto_FresnelPower;

	#ifdef CETO_BRDF_FRESNEL
	    fixed2 v = V.xz; // view direction in wind space
	    fixed2 t = v * v / (1.0 - V.y * V.y); // cos^2 and sin^2 of view direction
	    fixed sigmaV2 = dot(t, 0.004); // slope variance in view direction
	    
	    fixed sigmaV = 0.063;
	    fixed cosThetaV = dot(V, N);
	    
	    return saturate(Ceto_MinFresnel + (1.0-Ceto_MinFresnel) * pow(1.0 - cosThetaV, str * exp(-2.69 * sigmaV)) / (1.0 + 22.7 * pow(sigmaV, 1.5)));
    #else
    	return saturate(Ceto_MinFresnel + (1.0-Ceto_MinFresnel) * pow(1.0 - dot(V, N), str));
    #endif
}

fixed FresnelWaterAir(fixed3 V, fixed3 N) 
{
    return saturate(pow(1.0 - dot(V, N), Ceto_FresnelPower));
}

fixed3 SampleReflectionTexture(half2 reflectUV)
{
	return SAMPLE_TEXTURE2D_LOD(Ceto_Reflections0, sampler_Ceto_Reflections0, reflectUV, 0).xyz;
}

fixed3 ReflectionColor(half3 N, half2 reflectUV)
{
	fixed3 col = Ceto_DefaultSkyColor;

	#ifdef CETO_REFLECTION_ON
		reflectUV += N.xz * Ceto_ReflectionDistortion;
		col = SampleReflectionTexture(reflectUV);
		col *= Ceto_ReflectionTint;
	#endif

	return col;
}

half Lambda(half cosTheta, half sigma) 
{
	half v = cosTheta / sqrt((1.0 - cosTheta * cosTheta) * (2.0 * sigma));
	return (exp(-v * v)) / (2.0 * v * M_SQRT_PI);
}

half3 ReflectedSunRadianceNice(half3 V, half3 N, half3 L, fixed fresnel) 
{
	half3 Ty = half3(0.0, N.z, -N.y);
	half3 Tx = cross(Ty, N);
    
    half3 H = normalize(L + V);
	half dhn = dot(H, N);
	half idhn = 1.0 / dhn;
    half zetax = dot(H, Tx) * idhn;
    half zetay = dot(H, Ty) * idhn;

	half p = exp(-0.5 * (zetax * zetax / Ceto_SpecularRoughness + zetay * zetay / Ceto_SpecularRoughness)) / (2.0 * M_PI * Ceto_SpecularRoughness);

    half zL = dot(L, N); // cos of source zenith angle
    half zV = dot(V, N); // cos of receiver zenith angle
    half zH = dhn; // cos of facet normal zenith angle
    half zH2 = zH * zH;

    half tanV = atan2(dot(V, Ty), dot(V, Tx));
    half cosV2 = 1.0 / (1.0 + tanV * tanV);
    half sigmaV2 = Ceto_SpecularRoughness * cosV2 + Ceto_SpecularRoughness * (1.0 - cosV2);

    half tanL = atan2(dot(L, Ty), dot(L, Tx));
    half cosL2 = 1.0 / (1.0 + tanL * tanL);
    half sigmaL2 = Ceto_SpecularRoughness * cosL2 + Ceto_SpecularRoughness * (1.0 - cosL2);

    zL = max(zL, 0.01);
    zV = max(zV, 0.01);
    
    return (L.y < 0) ? 0.0 : Ceto_SpecularIntensity * p / ((1.0 + Lambda(zL, sigmaL2) + Lambda(zV, sigmaV2)) * zV * zH2 * zH2 * 4.0);
}

half ReflectedSunRadianceFast(half3 V, half3 N, half3 L, fixed fresnel) 
{
    half3 H = normalize(L + V);

    half hn = dot(H, N);
    half p = exp(-2.0 * ((1.0 - hn * hn) / Ceto_SpecularRoughness) / (1.0 + hn)) / (4.0 * M_PI * Ceto_SpecularRoughness);

    half zL = dot(L, N);
    half zV = dot(V, N);
    zL = max(zL,0.01);
    zV = max(zV,0.01);

    return (L.y < 0 || zL <= 0.0) ? 0.0 : max(Ceto_SpecularIntensity * p * sqrt(abs(zL / zV)), 0.0);
}

// URP Lighting function
half4 OceanBRDFLight(SurfaceOutputOcean s, half3 viewDir, Light light)
{
	half4 c = half4(0,0,0,1);
	
	half3 V = viewDir;
	half3 N = s.Normal;
	half3 DN = s.DNormal;

	#ifdef CETO_OCEAN_UNDERSIDE
		N.y *= -1.0;
		DN.y *= -1.0;
		V.y *= -1.0;
	#endif

	#ifdef CETO_NICE_BRDF
		half3 spec = ReflectedSunRadianceNice(V, N, light.direction, s.Fresnel);
	#else
		half3 spec = ReflectedSunRadianceFast(V, N, light.direction, s.Fresnel);
	#endif
	
	half diff = max(0, dot(DN, light.direction));

	#ifndef CETO_DISABLE_NO_DIFFUSE_IN_REFLECTIONS
		half a = s.Fresnel * (1.0 - s.Foam);
		half3 SpecAndDiffuse = s.Albedo * light.color * diff + light.color * spec;
		half3 SpecNoDiffuse = s.Albedo + light.color * spec;
		c.rgb = SpecNoDiffuse * a + SpecAndDiffuse * (1.0 - a);
	#else
		c.rgb = s.Albedo * light.color * diff + light.color * spec;
	#endif

	return c;
}

// Main lighting function for URP
half4 LightingOceanBRDF(SurfaceOutputOcean s, half3 viewDir, half3 positionWS)
{
	half4 c = half4(0,0,0,0);

	// Get main light
	Light mainLight = GetMainLight();
	c = OceanBRDFLight(s, viewDir, mainLight);

	// Add ambient/indirect lighting
	half3 ambient = SampleSH(s.DNormal);
	
	#ifndef CETO_DISABLE_NO_DIFFUSE_IN_REFLECTION
		c.rgb += s.Albedo * ambient * (1.0 - s.Fresnel);
	#else
		c.rgb += s.Albedo * ambient;
	#endif

	c.a = s.Alpha;
	c.rgb = lerp(c.rgb, s.Albedo, s.LightMask);

	// Add emission
	c.rgb += s.Emission;

	return c;
}

#endif
