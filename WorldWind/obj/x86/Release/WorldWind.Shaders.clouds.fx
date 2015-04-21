// Fluttering flag vertex shader

// These lines are just for EffectEdit:
string XFile = "flag.x";   // model
int    BCLR = 0xff000000;          // background colour

texture Tex0;

// transformations provided by the app as input:
float4x4 World: WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float4  horzTapOffs[16];		// Gauss blur horizontal texture coordinate offsets
float4  vertTapOffs[16];		// Gauss blur vertical texture coordinate offsets
float4	TexelWeight[16];
float4  pixelSize;
float2  texelSize;
float    ExposureLevel;
float screenWidth;
float screenHeight;


texture RenderMap;
sampler RenderMapSampler = sampler_state
{
   Texture = <RenderMap>;
   MinFilter = LINEAR;
   MagFilter = LINEAR;
   MipFilter = LINEAR;
   AddressU  = Clamp;
   AddressV  = Clamp;
};

texture FullResMap;
sampler FullResMapSampler = sampler_state
{
   Texture = <FullResMap>;
   MinFilter = LINEAR;
   MagFilter = LINEAR;
   MipFilter = LINEAR;
   AddressU  = Clamp;
   AddressV  = Clamp;
};

struct VS_OUTPUT
{
    float4 Pos : POSITION;
	float2 Tex : TEXCOORD0;
};

VS_OUTPUT VS(
	float3 Pos : POSITION,
	float2 Tex : TEXCOORD0)
{
    VS_OUTPUT Out = (VS_OUTPUT)0;

    float4x4 WorldView = mul(World, View);
    
    float3 P = mul(float4(Pos, 1), (float4x3)WorldView);
    
    // transform Position
    Out.Pos = mul(float4(P, 1), Projection);
	Out.Tex = Tex; 

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
	
	//f.w = f.x;
	
	return f;
}

// -------------------------------------------------------------
// Output channels
// -------------------------------------------------------------
struct VS_OUTPUTScaleBuffer
{
    float4 Pos		    : POSITION;
	float2 Tex	        : TEXCOORD0;	
};

// -------------------------------------------------------------
// vertex shader function (input channels)
// -------------------------------------------------------------
VS_OUTPUTScaleBuffer VSScaleBuffer(float4 Pos : POSITION)
{
    VS_OUTPUTScaleBuffer Out = (VS_OUTPUTScaleBuffer)0;        
    Out.Pos.xy = Pos.xy + pixelSize.xy;
	Out.Pos.z = 0.5f;
	Out.Pos.w = 1.0f;
	
	Out.Tex = float2(0.5f, -0.5f) * Pos.xy + 0.5f.xx;	
	
    return Out;
}

// -------------------------------------------------------------
// Pixel Shader (input channels):output channel
// -------------------------------------------------------------
float4 PSScaleBuffer( float2 Tex :TEXCOORD0): COLOR				
{
    return tex2D(RenderMapSampler, Tex);
}

// -------------------------------------------------------------
// Output channels
// -------------------------------------------------------------
struct VS_OUTPUT_GaussX
{
    float4 Pos			: POSITION;
    float2 Tap0		    : TEXCOORD0;
    float2 Tap1		    : TEXCOORD1;    
    float2 Tap2		    : TEXCOORD2;
    float2 Tap3		    : TEXCOORD3;
    float2 Tap1Neg		: TEXCOORD4;
    float2 Tap2Neg		: TEXCOORD5;        
    float2 Tap3Neg		: TEXCOORD6;        
};

// -------------------------------------------------------------
// vertex shader function (input channels)
// -------------------------------------------------------------
VS_OUTPUT_GaussX VSGaussX(float4 Pos      : POSITION)
{
    VS_OUTPUT_GaussX Out = (VS_OUTPUT_GaussX)0;        
    Out.Pos.xy = Pos.xy + pixelSize.xy;
	Out.Pos.z = 0.5f;
	Out.Pos.w = 1.0f;
	
	float2 Tex = float2(0.5f, -0.5f) * Pos.xy + 0.5f.xx;	
	
	Out.Tap0 = Tex;
	Out.Tap1 = Tex + horzTapOffs[1].xy;	
	Out.Tap2 = Tex + horzTapOffs[2].xy;
	Out.Tap3 = Tex + horzTapOffs[3].xy;
	Out.Tap1Neg = Tex - horzTapOffs[1].xy;
	Out.Tap2Neg = Tex - horzTapOffs[2].xy;			
	Out.Tap3Neg = Tex - horzTapOffs[3].xy;			

    return Out;
}


// -------------------------------------------------------------
// Pixel Shader (input channels):output channel
// -------------------------------------------------------------
float4 PSGaussX( float2 Tap0		: TEXCOORD0,
				float2 Tap1		: TEXCOORD1,    
				float2 Tap2		: TEXCOORD2,
				float2 Tap3		: TEXCOORD3,
				float2 Tap1Neg		: TEXCOORD4,
				float2 Tap2Neg		: TEXCOORD5,        
				float2 Tap3Neg		: TEXCOORD6) : COLOR0
{
	float4 Color[7];
	float4 ColorSum = 0.0f;	

	// sample inner taps
	Color[0]  = tex2D(RenderMapSampler, Tap0); 
	Color[1]  = tex2D(RenderMapSampler, Tap1);
	Color[2]  = tex2D(RenderMapSampler, Tap1Neg);
	Color[3]  = tex2D(RenderMapSampler, Tap2); 
	Color[4]  = tex2D(RenderMapSampler, Tap2Neg);
	Color[5]  = tex2D(RenderMapSampler, Tap3);
	Color[6]  = tex2D(RenderMapSampler, Tap3Neg); 
	
	ColorSum = Color[0] * TexelWeight[0];
	ColorSum += Color[1] * TexelWeight[1];	
	ColorSum += Color[2] * TexelWeight[1];
	ColorSum += Color[3] * TexelWeight[2];
	ColorSum += Color[4] * TexelWeight[2];			
	ColorSum += Color[5] * TexelWeight[3];	
	ColorSum += Color[6] * TexelWeight[3];	
	
	// compute texture coordinates for other taps
	float2 Tap4 = Tap0 + horzTapOffs[4].xy;
	float2 Tap5 = Tap0 + horzTapOffs[5].xy;
	float2 Tap6 = Tap0 + horzTapOffs[6].xy;
	float2 Tap4Neg = Tap0 - horzTapOffs[4].xy;
	float2 Tap5Neg = Tap0 - horzTapOffs[5].xy;
	float2 Tap6Neg = Tap0 - horzTapOffs[6].xy;
	
	// sample outer taps
	Color[0]  = tex2D(RenderMapSampler, Tap4); 
	Color[1]  = tex2D(RenderMapSampler, Tap4Neg);
	Color[2]  = tex2D(RenderMapSampler, Tap5); 
	Color[3]  = tex2D(RenderMapSampler, Tap5Neg); 
	Color[4]  = tex2D(RenderMapSampler, Tap6); 
	Color[5]  = tex2D(RenderMapSampler, Tap6Neg); 

	ColorSum += Color[0] * TexelWeight[4];
	ColorSum += Color[1] * TexelWeight[4];
	ColorSum += Color[2] * TexelWeight[5];
	ColorSum += Color[3] * TexelWeight[5];
	ColorSum += Color[4] * TexelWeight[6];
	ColorSum += Color[5] * TexelWeight[6];	

	return ColorSum;				
}

// -------------------------------------------------------------
// Output channels
// -------------------------------------------------------------
struct VS_OUTPUT_GaussY
{
    float4 Pos		: POSITION;
    float2 Tap0		: TEXCOORD0;
    float2 Tap1		: TEXCOORD1;    
    float2 Tap2		: TEXCOORD2;
    float2 Tap3		: TEXCOORD3;
    float2 Tap1Neg		: TEXCOORD4;
    float2 Tap2Neg		: TEXCOORD5;        
    float2 Tap3Neg		: TEXCOORD6;        
};

// -------------------------------------------------------------
// vertex shader function (input channels)
// -------------------------------------------------------------
VS_OUTPUT_GaussY VSGaussY(float4 Pos      : POSITION)
{
    VS_OUTPUT_GaussY Out = (VS_OUTPUT_GaussY)0;        
    Out.Pos.xy = Pos.xy + pixelSize.xy;
	Out.Pos.z = 0.5f;
	Out.Pos.w = 1.0f;
	
	float2 Tex = float2(0.5f, -0.5f) * Pos.xy + 0.5f.xx;	

	Out.Tap0 = Tex;
	Out.Tap1 = Tex + vertTapOffs[1].xy;	
	Out.Tap2 = Tex + vertTapOffs[2].xy;
	Out.Tap3 = Tex + vertTapOffs[3].xy;
	Out.Tap1Neg = Tex - vertTapOffs[1].xy;
	Out.Tap2Neg = Tex - vertTapOffs[2].xy;			
	Out.Tap3Neg = Tex - vertTapOffs[3].xy;			

    return Out;
}

// -------------------------------------------------------------
// Pixel Shader (input channels):output channel
// -------------------------------------------------------------
float4 PSGaussY( float2 Tap0		: TEXCOORD0,
				float2 Tap1		: TEXCOORD1,    
				float2 Tap2		: TEXCOORD2,
				float2 Tap3		: TEXCOORD3,
				float2 Tap1Neg		: TEXCOORD4,
				float2 Tap2Neg		: TEXCOORD5,        
				float2 Tap3Neg		: TEXCOORD6) : COLOR0
{
	float4 Color[7];
	float4 ColorSum = 0.0f;
	
	// sample inner taps
	Color[0] = tex2D(RenderMapSampler, Tap0);
	Color[1] = tex2D(RenderMapSampler, Tap1);
	Color[2] = tex2D(RenderMapSampler, Tap1Neg);
	Color[3] = tex2D(RenderMapSampler, Tap2);
	Color[4] = tex2D(RenderMapSampler, Tap2Neg);
	Color[5] = tex2D(RenderMapSampler, Tap3);
	Color[6] = tex2D(RenderMapSampler, Tap3Neg);
	
	ColorSum = Color[0] * TexelWeight[0];
	ColorSum += Color[1] * TexelWeight[1];	
	ColorSum += Color[2] * TexelWeight[1];
	ColorSum += Color[3] * TexelWeight[2];
	ColorSum += Color[4] * TexelWeight[2];			
	ColorSum += Color[5] * TexelWeight[3];	
	ColorSum += Color[6] * TexelWeight[3];	

	// compute texture coordinates for other taps
	float2 Tap4 = Tap0 + vertTapOffs[4].xy;
	float2 Tap5 = Tap0 + vertTapOffs[5].xy;
	float2 Tap6 = Tap0 + vertTapOffs[6].xy;
	float2 Tap4Neg = Tap0 - vertTapOffs[4].xy;
	float2 Tap5Neg = Tap0 - vertTapOffs[5].xy;
	float2 Tap6Neg = Tap0 - vertTapOffs[6].xy;
	
	// sample outer taps
	Color[0] = tex2D(RenderMapSampler, Tap4);
	Color[1] = tex2D(RenderMapSampler, Tap4Neg);
	Color[2] = tex2D(RenderMapSampler, Tap5);
	Color[3] = tex2D(RenderMapSampler, Tap5Neg);
	Color[4] = tex2D(RenderMapSampler, Tap6);
	Color[5] = tex2D(RenderMapSampler, Tap6Neg);									

	ColorSum += Color[0] * TexelWeight[4];
	ColorSum += Color[1] * TexelWeight[4];
	ColorSum += Color[2] * TexelWeight[5];
	ColorSum += Color[3] * TexelWeight[5];
	ColorSum += Color[4] * TexelWeight[6];
	ColorSum += Color[5] * TexelWeight[6];	
	
	return ColorSum;				
}


// -------------------------------------------------------------
// Output channels
// -------------------------------------------------------------
struct VS_OUTPUTScreen
{
    float4 Pos			: POSITION;
	float2 TopLeft	    : TEXCOORD0;	
	float2 TopRight	    : TEXCOORD1;	
	float2 BottomLeft	: TEXCOORD2;	
	float2 BottomRight	: TEXCOORD3;	
};

// -------------------------------------------------------------
// vertex shader function (input channels)
// -------------------------------------------------------------
VS_OUTPUTScreen VSScreen(float4 Pos      : POSITION)
{
    VS_OUTPUTScreen Out = (VS_OUTPUTScreen)0;        
    Out.Pos.xy = Pos.xy; // + pixelSize.xy;
	Out.Pos.z = 0.5f;
	Out.Pos.w = 1.0f;
	
	float2 Tex = float2(0.5f, -0.5f) * Pos.xy + 0.5f.xx;	
	
    const float2 oneZero   = float2(1.0f, 0.0f);

	Out.TopLeft     = Tex + oneZero.yy * texelSize;	    // Top Left
	Out.TopRight    = Tex + oneZero.xy * texelSize;		// Top Right
	Out.BottomLeft  = Tex + oneZero.yx * texelSize;		// Bottom Left
	Out.BottomRight = Tex + oneZero.xx * texelSize;		// Bottom Right
	
    return Out;
}

// -------------------------------------------------------------
// Pixel Shader (input channels):output channel
// -------------------------------------------------------------
float4 PSScreen(float2 Tex : TEXCOORD0) : COLOR0
{
	float4 FullScreenImage = tex2D(FullResMapSampler, Tex);
	float4 BlurredImage = tex2D(RenderMapSampler, Tex);
	
	float4 color = lerp(FullScreenImage, BlurredImage, 0.4f);

	Tex -= 0.5f;					     // range -0.5..0.5	
	float vignette = 1 - dot(Tex, Tex);	
	
	// multiply color with vignette^4
	color = color * vignette * vignette * vignette * vignette;
	
	color *= ExposureLevel;		    // apply simple exposure	
    return pow(color, 0.55f);
}

technique VSClouds
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
		PixelShader = compile ps_1_1 PS();
    }
}

technique ScaleBuffer
{
    pass P0
    {
        VertexShader = compile vs_1_1 VSScaleBuffer();
        PixelShader  = compile ps_1_1 PSScaleBuffer();
    }
}

technique GaussX
{
    pass P0
    {
        VertexShader = compile vs_1_1 VSGaussX();
        PixelShader  = compile ps_2_0 PSGaussX();
    }
}

technique GaussY
{
    pass P0
    {
        VertexShader = compile vs_1_1 VSGaussY();
        PixelShader  = compile ps_2_0 PSGaussY();
    }
}


technique Screenblit
{
    pass P0
    {
        VertexShader = compile vs_1_1 VSScreen();
        PixelShader  = compile ps_2_0 PSScreen();
    }
}
