struct VS_OUTPUT
{
	float4 pos	:	POSITION;
	float4 color	:	COLOR;
	float2 texCoord	:	TEXCOORD0;
};

texture Tex0;
float4x4 WorldViewProj	:	WORLDVIEWPROJECTION;
float Opacity = 1.0;
float Brightness = 0.0;

VS_OUTPUT VS(
	float4 Pos	:	POSITION,
	float4 Norm : NORMAL,
	float2 texCoord	:	TEXCOORD0)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;
	
	// transform Position
    Out.pos = mul(Pos, WorldViewProj);
	Out.color = float4(0,0,0,Opacity);
	Out.texCoord = texCoord;
	
	return Out;
}

sampler Sampler = sampler_state
{
	Texture = (Tex0);
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

float4 PS(
	float2 Tex : TEXCOORD0) : COLOR
{
	float4 f = tex2D(Sampler, Tex);
	float gray = 0.3 * f.x + 0.59 * f.y + 0.11 * f.z;
	f.xyz = gray;
	f.w = Opacity * f.w;
	return f;
}

float4 PS_Brightness(
	float2 Tex : TEXCOORD0) : COLOR
{
	float4 f = tex2D(Sampler, Tex);
	float gray = 0.3 * f.x + 0.59 * f.y + 0.11 * f.z + Brightness / 255.0;
	f.xyz = gray;
	f.w = Opacity * f.w;
	return f;
}

technique RenderGrayscaleBrightness
{
	pass P0
	{
		VertexShader = compile vs_1_1 VS();
		PixelShader = compile ps_2_0 PS_Brightness();
	}
}

technique RenderGrayscale
{
	pass P0
	{
		VertexShader = compile vs_1_1 VS();
		PixelShader = compile ps_1_1 PS();
	}
}