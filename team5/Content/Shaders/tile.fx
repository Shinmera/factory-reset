#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

float4x4 projectionMatrix;
float4x4 viewMatrix;
float4x4 modelMatrix;
float4 offset;
Texture2D tileset;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4x4 wvp = mul(modelMatrix, mul(viewMatrix, projectionMatrix));
    float4 pos = input.Position;
    pos.xy *= offset.zw;
    output.Position = mul(pos, wvp);
    output.UV = offset.xy + input.UV*offset.zw;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    return tileset.Load(int3(input.UV.x, input.UV.y, 0));
}

technique Tile
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};