#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

Texture2D parallax;
float2 viewSize;
float2 viewPos;
float viewScale;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 mapCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = input.Position;
    
    // Inverse-map the coordinate as we start from view space but want to get into world space.
    output.mapCoord = (input.UV*viewSize+viewPos)/viewScale;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    uint w, h;
    parallax.GetDimensions(w, h);
    int2 mapWH = int2(w, h);
    int2 mapXY = int2(floor(input.mapCoord));
    
    // Flip around to be bottom-bound.
    mapXY.y = mapWH.y - mapXY.y - 1;
    
    // Wrap and clamp
    mapXY.x = abs(mapXY.x) % mapWH.x;
    mapXY.y = clamp(mapXY.y, 0, mapWH.y-1);
    
    return parallax.Load(int3(mapXY, 0));
}

technique Tile
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
