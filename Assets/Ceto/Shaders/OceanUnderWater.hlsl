#ifndef CETO_UNDERWATER_INCLUDED
#define CETO_UNDERWATER_INCLUDED

/*
* Applies the foam to the ocean color.
*/
fixed FoamAmount(float3 worldPos, fixed4 foam)
{
	//foam.x == the wave (spectrum) foam.
	//foam.y == the overlay foam with foam texture.
	//foam.z == the overlay foam with no foam texture.

	fixed foamTexture = 0.0;

	#ifndef CETO_DISABLE_FOAM_TEXTURE
   		foamTexture += SAMPLE_TEXTURE2D(Ceto_FoamTexture0, sampler_Ceto_FoamTexture0, (worldPos.xz + Ceto_FoamTextureScale0.z) * Ceto_FoamTextureScale0.xy).a * 0.5;
		foamTexture += SAMPLE_TEXTURE2D(Ceto_FoamTexture1, sampler_Ceto_FoamTexture1, (worldPos.xz + Ceto_FoamTextureScale1.z) * Ceto_FoamTextureScale1.xy).a * 0.5;
	#else
		foamTexture = 1.0;
	#endif

	//Apply texture to the wave foam if that option is enabled.
    foam.x = lerp(foam.x, foam.x * foamTexture, Ceto_TextureWaveFoam);
	//Apply texture to overlay foam
   	foam.y = foam.y * foamTexture;
   
   	return saturate(max(max(foam.x, foam.y), foam.z));
}

/*
* Applies the foam to the ocean color.
*/
fixed3 AddFoamColor(fixed foamAmount, fixed3 oceanCol)
{
	//apply the absorption coefficient to the foam based on the foam strength.
	fixed3 foamCol = Ceto_FoamTint * foamAmount * exp(-Ceto_AbsCof.rgb * (1.0 - foamAmount) * 1.0);

	return lerp(oceanCol, foamCol, foamAmount);
}

/*
* Calculate a subsurface scatter color based on the view, normal and sun dir.
*/
fixed3 SubSurfaceScatter(fixed3 V, fixed3 N, float surfaceDepth)
{
	fixed3 col = fixed3(0,0,0);

	#ifdef CETO_UNDERWATER_ON
		//The strength based on the view and up direction.
		fixed VU = 1.0 -  max(0.0, dot(V, fixed3(0,1,0)));
		VU *= VU;
		
		//The strength based on the view and sun direction.
		fixed VS = max(0, dot(reflect(V, fixed3(0,1,0)) * -1.0, Ceto_SunDir));
		VS *= VS;
		VS *= VS;
		
		float NX =  abs(dot(N, fixed3(1,0,0)));
		
		fixed s = NX * VU * VS;

		//If sun below horizion remove sss.
		if (dot(Ceto_SunDir, float3(0, 1, 0)) < 0.0) s = 0.0;
		
		//apply a non linear fade to distance.
		fixed d = max(0.2, exp(-max(0.0, surfaceDepth)));

		//Apply the absorption coefficient base on the distance and tint final color.
		col = Ceto_SSSTint * exp(-Ceto_SSSCof.rgb * d * Ceto_SSSCof.a) * s;
	#endif
	
	return col;
}

/*
* Get IVP matrix.
*/
float4x4 GetIVPMatrix()
{
	return Ceto_Camera_IVP0;
}

/*
* Calculates the world position from the depth buffer value.
*/
float3 WorldPosFromDepth(float2 uv, float depth)
{
	#if defined(UNITY_REVERSED_Z)
		depth = 1.0 - depth;
	#endif

	float4 ndc = float4(uv.x * 2.0 - 1.0, uv.y * 2.0 - 1.0, depth * 2.0 - 1.0, 1);
	
	float4 worldPos = mul(GetIVPMatrix(), ndc);
	worldPos /= worldPos.w;

	return worldPos.xyz;
}

/*
* The world position of the first object below the water surface
* reconstructed from the depth buffer.
*/
float3 WorldDepthPos(float2 screenUV)
{
	float3 worldPos = float3(0, 0, 0);

	#ifdef CETO_UNDERWATER_ON
	#ifndef CETO_USE_OCEAN_DEPTHS_BUFFER
	float db = SAMPLE_TEXTURE2D(Ceto_DepthBuffer, sampler_Ceto_DepthBuffer, screenUV).x;
	worldPos = WorldPosFromDepth(screenUV, db);
	#endif
	#endif

	return worldPos;
}

/*
* Samples the depth buffer with a distortion to the uv.
*/
float SampleDepthBuffer(float2 screenUV)
{
	float depth = SAMPLE_TEXTURE2D(Ceto_DepthBuffer, sampler_Ceto_DepthBuffer, screenUV).x;
	return LinearEyeDepth(depth, _ZBufferParams);
}

/*
* Returns the depth info needed to apply the underwater effect 
* calculated from the depth buffer.
*/
float4 SampleOceanDepthFromDepthBuffer(float2 screenUV)
{
	float depth = SAMPLE_TEXTURE2D(Ceto_DepthBuffer, sampler_Ceto_DepthBuffer, screenUV).x;
	float3 worldPos = WorldPosFromDepth(screenUV, depth);

	float4 oceanDepth = float4(0,0,0,0);
	float ld = LinearEyeDepth(depth, _ZBufferParams);
	
	oceanDepth.x = (worldPos.y-Ceto_OceanLevel) * -1.0;
	oceanDepth.y = ld / Ceto_MaxDepthDist;
	oceanDepth.z = ld;
	oceanDepth.w = 0;
	
	return oceanDepth;
}

/*
* Sample texture taking stereo eye into account (for VR)
*/
float4 SampleOceanDepthTexture(float2 uv)
{
	return SAMPLE_TEXTURE2D(Ceto_OceanDepth0, sampler_Ceto_OceanDepth0, uv);
}

/*
* Returns the depth info from the ocean depths buffer.
*/
float4 SampleOceanDepth(float2 screenUV)
{
	float4 oceanDepth = SampleOceanDepthTexture(screenUV);
	float ld = oceanDepth.y;

	//unnormalize.
	oceanDepth.x *= Ceto_MaxDepthDist;
	oceanDepth.y = ld / Ceto_MaxDepthDist;
	oceanDepth.z = ld;
	oceanDepth.w = 0;

	return oceanDepth;
}

/*
* Computes the depth value used to apply the underwater effect.
*/
float2 OceanDepth(float2 screenUV, float3 worldPos, float depth)
{
	float2 surfaceDepth;
	surfaceDepth.x = (worldPos.y-Ceto_OceanLevel) * -1.0;
	surfaceDepth.y = depth / Ceto_MaxDepthDist;
	
	#ifdef CETO_USE_OCEAN_DEPTHS_BUFFER
		float2 oceanDepth = SampleOceanDepth(screenUV).xy;
	#else
		float2 oceanDepth = SampleOceanDepthFromDepthBuffer(screenUV).xy;
	#endif

	oceanDepth.x = max(0.0, oceanDepth.x - surfaceDepth.x) / Ceto_MaxDepthDist;
	oceanDepth.y = max(0.0, oceanDepth.y - surfaceDepth.y);
	
	return oceanDepth;
}

/*
* Distorts the screen uv by the wave normal. 
*/
float4 DisortScreenUV(half3 normal, float4 screenUV, float surfaceDepth, float dist, half3 view)
{
	//Fade by distance so distortion is less on far away objects.
	float distortionFade = 1.0 - clamp(dist * 0.01, 0.0001, 1.0);
	float3 distortion = normal * Ceto_RefractionDistortion * distortionFade * distortionFade;

	distortion.z *= dot(view, normal);
	float4 distortedUV = saturate(screenUV + distortion.xzxz);

	#ifdef CETO_USE_OCEAN_DEPTHS_BUFFER
		float depth = SampleOceanDepth(distortedUV.xy).z;
	#else
		float depth = SampleDepthBuffer(distortedUV.xy);
	#endif

	//If the distorted depth is less than the ocean mesh depth
	//then the distorted uv is in front of a object.
	if (depth <= surfaceDepth) distortedUV = screenUV;

	//The smaller the depth difference the smaller the distortion
	float distortionMultiplier = saturate((depth - surfaceDepth) * 0.25);
	distortedUV = lerp(screenUV, distortedUV, distortionMultiplier);

	return distortedUV;
}

/*
* The refraction color when see from above the ocean mesh.
*/
fixed3 AboveRefractionColor(float2 grabUV, float3 surfacePos, float depth, fixed3 caustics)
{
	// Use URP's Camera Opaque Texture instead of GrabPass
	fixed3 grab = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, grabUV).rgb * Ceto_AboveRefractionIntensity;

	grab += caustics;
	
	fixed3 col = grab * Ceto_AbsTint * exp(-Ceto_AbsCof.rgb * depth * Ceto_MaxDepthDist * Ceto_AbsCof.a);
	
	return col;
}

/*
* The refraction color when see from below the ocean mesh (under water).
*/
fixed3 BelowRefractionColor(float2 grabUV)
{
	// Use URP's Camera Opaque Texture instead of GrabPass
	fixed3 grab = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, grabUV).rgb * Ceto_BelowRefractionIntensity;
	
	return grab;
}

/*
* The inscatter when seen from above the ocean mesh.
*/
fixed3 AddAboveInscatter(fixed3 col, float depth)
{
	//There are 3 methods used to apply the inscatter.
	half3 inscatterScale;
	inscatterScale.x = saturate(depth * Ceto_AboveInscatterScale);
	inscatterScale.y = saturate(1.0-exp(-depth * Ceto_AboveInscatterScale));
	inscatterScale.z = saturate(1.0-exp(-depth * depth * Ceto_AboveInscatterScale));
	
	//Apply mask to pick which methods result to use.
	half a = dot(inscatterScale, Ceto_AboveInscatterMode);
	
	return lerp(col, Ceto_AboveInscatterColor.rgb, a * Ceto_AboveInscatterColor.a);
}

/*
* The inscatter when seen from below the ocean mesh.
*/
fixed3 AddBelowInscatter(fixed3 col, float depth)
{
	//There are 3 methods used to apply the inscatter.
	half3 inscatterScale;
	inscatterScale.x = saturate(depth * Ceto_BelowInscatterScale);
	inscatterScale.y = saturate(1.0-exp(-depth * Ceto_BelowInscatterScale));
	inscatterScale.z = saturate(1.0-exp(-depth * depth * Ceto_BelowInscatterScale));
	
	//Apply mask to pick which methods result to use.
	half a = dot(inscatterScale, Ceto_BelowInscatterMode);
	
	return lerp(col, Ceto_BelowInscatterColor.rgb, a * Ceto_BelowInscatterColor.a);
}

/*
* The ocean color when seen from above the ocean mesh.
*/
fixed3 OceanColorFromAbove(float4 distortedUV, float3 surfacePos, float surfaceDepth, fixed3 caustics)
{
	fixed3 col = Ceto_DefaultOceanColor;

	#ifdef CETO_UNDERWATER_ON
		float2 oceanDepth = OceanDepth(distortedUV.xy, surfacePos, surfaceDepth);

		float depthBlend = lerp(oceanDepth.x, oceanDepth.y, Ceto_DepthBlend);
		
		fixed3 refraction = AboveRefractionColor(distortedUV.zw, surfacePos, depthBlend, caustics);
		
		col = AddAboveInscatter(refraction, depthBlend);
	#endif
	
	return col;
}

/*
* This is the color of the underside of the mesh.
*/
fixed3 DefaultUnderSideColor()
{
	return Ceto_BelowInscatterColor.rgb;
}

/*
* The sky color when seen from below the ocean mesh.
*/
fixed3 SkyColorFromBelow(float4 distortedUV)
{
	fixed3 col = Ceto_DefaultOceanColor;

	#ifdef CETO_UNDERWATER_ON
		col = BelowRefractionColor(distortedUV.zw);
	#endif
	
	return col;
}

/*
* Returns a blend value to use as the alpha to fade ocean into shoreline.
*/
float EdgeFade(float2 screenUV, float3 view, float3 surfacePos, float3 worldDepthPos)
{
	float edgeFade = 1.0;

	#ifdef CETO_UNDERWATER_ON
	#ifndef CETO_DISABLE_EDGE_FADE
	//Fade based on dist between ocean surface and bottom
	#ifdef CETO_USE_OCEAN_DEPTHS_BUFFER
		float surfaceDepth = (surfacePos.y - Ceto_OceanLevel) * -1.0;
		float oceanDepth = SampleOceanDepthTexture(screenUV).x * Ceto_MaxDepthDist;
		float dist = oceanDepth - surfaceDepth;
	#else
		float dist = surfacePos.y - worldDepthPos.y;
	#endif

		dist = max(0.0, dist);
		edgeFade = 1.0 - saturate(exp(-dist * Ceto_EdgeFade) * 2.0);

		//Restrict blending when viewing ocean from a shallow angle
		float viewMaskStr = 10.0;
		float viewMask = saturate(dot(view, fixed3(0, 1, 0)) * viewMaskStr);

		edgeFade = lerp(1.0, edgeFade, viewMask);
	#endif
	#endif

	return edgeFade;
}

/*
* Fades the edge of the water where it meets other objects.
*/
fixed3 ApplyEdgeFade(fixed3 col, float2 grabUV, float edgeFade, out fixed alpha, out fixed lightMask)
{
	alpha = 1.0;
	lightMask = 0.0;

	#ifdef CETO_UNDERWATER_ON
	#ifndef CETO_DISABLE_EDGE_FADE
		#ifdef CETO_OPAQUE_QUEUE
			fixed3 grab = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, grabUV).rgb;
			col = lerp(grab, col, edgeFade);
			alpha = 1.0;
			lightMask = 1.0 - edgeFade;
		#endif

		#ifdef CETO_TRANSPARENT_QUEUE
			alpha = edgeFade;
			lightMask = 0.0;
		#endif
	#endif
	#endif

	return col;
}

/*
* Calculate the caustic color when above the water.
*/
fixed3 CausticsFromAbove(float2 disortionUV, half3 unmaskedNorm, float3 surfacePos, float3 distortedWorldDepthPos, float dist)
{
	fixed3 col = fixed3(0, 0, 0);

	#ifdef CETO_UNDERWATER_ON
	#ifndef CETO_USE_OCEAN_DEPTHS_BUFFER
	#ifndef CETO_DISABLE_CAUSTICS
	float2 uv = distortedWorldDepthPos.xz * Ceto_CausticTextureScale.xy + unmaskedNorm.xz * Ceto_CausticDistortion.x;

	//Depth fade
	float depthFadeScale = Ceto_CausticTextureScale.w * Ceto_CausticTextureScale.w;
	float depthFade = exp(-max(0.0, surfacePos.y - distortedWorldDepthPos.y) * depthFadeScale);

	fixed3 caustic = SAMPLE_TEXTURE2D(Ceto_CausticTexture, sampler_Ceto_CausticTexture, uv).rgb;

	//Normal fade
	float nf = SAMPLE_TEXTURE2D(Ceto_NormalFade, sampler_Ceto_NormalFade, disortionUV).x;

	//Distance fade
	float distFade = 1.0 - saturate(dist * 0.001);

	col = caustic * Ceto_CausticTint * nf * distFade * depthFade;
	#endif
	#endif
	#endif

	return col;
}

/*
* Calculate the caustic color when below the water.
*/
fixed3 CausticsFromBelow(float2 screenUV, half3 normal, float3 worldDepthPos, float dist)
{
	fixed3 col = fixed3(0, 0, 0);

	#ifdef CETO_UNDERWATER_ON
	#ifndef CETO_USE_OCEAN_DEPTHS_BUFFER
	#ifndef CETO_DISABLE_CAUSTICS
	float2 uv = worldDepthPos.xz * Ceto_CausticTextureScale.xy + normal.xz * Ceto_CausticDistortion.y;

	//Depth fade
	float depthFadeScale = Ceto_CausticTextureScale.w * Ceto_CausticTextureScale.w;
	float depthFade = exp(-max(0.0, Ceto_OceanLevel - worldDepthPos.y) * depthFadeScale);

	fixed3 caustic = SAMPLE_TEXTURE2D(Ceto_CausticTexture, sampler_Ceto_CausticTexture, uv).rgb;

	//Normal fade
	float nf = SAMPLE_TEXTURE2D(Ceto_NormalFade, sampler_Ceto_NormalFade, screenUV).x;

	col = caustic * Ceto_CausticTint * nf * depthFade;
	#endif
	#endif
	#endif

	return col;
}

/*
* The underwater color used in the post effect shader.
*/ 
fixed3 UnderWaterColor(fixed3 belowColor, float dist)
{
	fixed3 col = belowColor;
	
	#ifdef CETO_UNDERWATER_ON
		col = belowColor * Ceto_BelowTint * exp(-Ceto_BelowCof.rgb * dist * Ceto_BelowCof.a);
		
		//For inscatter dist should be normalized to max dist.
		dist = dist / Ceto_MaxDepthDist;
		//Need to rescale otherwise the inscatter is too strong.
		dist *= 0.1;

		col = AddBelowInscatter(col, dist);
	#endif
	
	return col;
}

#endif
