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

struct VertexShaderInput
{
   float4 Position : POSITION0;
   uint VertexID: SV_VertexID;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 pos = input.Position;

    output.Position = mul(mul(mul(pos, modelMatrix), viewMatrix), projectionMatrix);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    return float4(1, 0, 0, 0.5);
}

technique Triangle
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};