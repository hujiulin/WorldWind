// Code adapted from OpenGL-based Atmospheric Scattering from Sean O'Neil

uniform float4 v3CameraPos;		// The camera's current position
uniform float4 v3LightPos;		// The direction vector to the light source
uniform float4 v3InvWavelength;	// 1 / pow(wavelength, 4) for the red, green, and blue channels
uniform float fCameraHeight;	// The camera's current height
uniform float fCameraHeight2;	// fCameraHeight^2
uniform float fOuterRadius;		// The outer (atmosphere) radius
uniform float fOuterRadius2;	// fOuterRadius^2
uniform float fInnerRadius;		// The inner (planetary) radius
uniform float fInnerRadius2;	// fInnerRadius^2
uniform float fKrESun;			// Kr * ESun
uniform float fKmESun;			// Km * ESun
uniform float fKr4PI;			// Kr * 4 * PI
uniform float fKm4PI;			// Km * 4 * PI
uniform float fScale;			// 1 / (fOuterRadius - fInnerRadius)
uniform float fScaleDepth;		// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
uniform float fScaleOverScaleDepth;	// fScale / fScaleDepth
float4x4 WorldViewProj	:	WORLDVIEWPROJECTION;
uniform int nSamples;
uniform float fSamples;
uniform float g;
uniform float g2;

struct VS_OUTPUT
{
	float4 pos	:	POSITION;
	float2 texCoord	:	TEXCOORD0;
	float3 direction : TEXCOORD1;
	float3 color	:	TEXCOORD2;
	float3 secondaryColor : TEXCOORD3;
};

float scale(float fCos)
{
	float x = 1.0 - fCos;
	return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
}

VS_OUTPUT VS(
	float4 Pos	:	POSITION)
{
	float3 v3Pos = Pos.xyz;
	float3 cameraPos = v3CameraPos.xyz;
	float3 light = v3LightPos.xyz;
	
	// Get the ray from the camera to the vertex, and its length (which is the far point of the ray passing through the atmosphere)
	float3 v3Ray = v3Pos - cameraPos;
	float fFar = length(v3Ray);
	v3Ray /= fFar;

	// Calculate the ray's starting position, then calculate its scattering offset
	float3 v3Start = cameraPos;
	float fHeight = length(v3Start);
	float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fCameraHeight));
	float fStartAngle = dot(v3Ray, v3Start) / fHeight;
	float fStartOffset = fDepth*scale(fStartAngle);

	// Initialize the scattering loop variables
	//gl_FrontColor = vec4(0.0, 0.0, 0.0, 0.0);
	float fSampleLength = fFar / fSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Ray * fSampleLength;
	float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

	// Now loop through the sample rays
	float3 v3FrontColor = float3(0.0, 0.0, 0.0);
	for(int i=0; i<nSamples; i++)
	{
		float fHeight = length(v3SamplePoint);
		float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
		float fLightAngle = dot(light, v3SamplePoint) / fHeight;
		float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
		float fScatter = (fStartOffset + fDepth*(scale(fLightAngle) - scale(fCameraAngle)));
		float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
		v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
		v3SamplePoint += v3SampleRay;
	}

	VS_OUTPUT Out = (VS_OUTPUT)0;
	// Finally, scale the Mie and Rayleigh colors and set up the varying variables for the pixel shader
	Out.pos = mul(Pos, WorldViewProj);
	Out.direction = cameraPos - v3Pos;
	Out.secondaryColor = v3FrontColor * fKmESun;
	Out.color = v3FrontColor * (v3InvWavelength * fKrESun);
	
	return Out;
}


float4 PS(
	float2 Tex : TEXCOORD0,
	float3 direction : TEXCOORD1,
	float3 color	:	TEXCOORD2,
	float3 secondaryColor : TEXCOORD3) : COLOR
{
	float3 light = v3LightPos.xyz;
	float fCos = dot(light, direction) / length(direction);
	float fRayleighPhase = 0.75 * (1.0 + fCos*fCos);
	float fMiePhase = 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos*fCos) / pow(1.0 + g2 - 2.0*g*fCos, 1.5);
    float3 f = fRayleighPhase * color + fMiePhase * secondaryColor;
	return float4(f.x, f.y, f.z, 1.0);
	
}

technique Sky
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}