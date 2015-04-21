struct VS_OUTPUT
{
	float4 pos	:	POSITION;
	float4 color	:	COLOR;
	float2 texCoord	:	TEXCOORD0;
};

float4x4 WorldViewProj	:	WORLDVIEWPROJECTION;
float Opacity = 1.0;

VS_OUTPUT Transform(
	float4 Pos	:	POSITION,
	float2 texCoord	:	TEXCOORD0)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;
	
	Out.pos = mul(Pos, WorldViewProj);
	Out.color = float4(0,0,0,Opacity);
	Out.texCoord = texCoord;
	
	return Out;
}

technique SetOpacity
{
	pass P0
	{
		VertexShader = compile vs_1_0 Transform();
		PixelShader = NULL;
	}
}