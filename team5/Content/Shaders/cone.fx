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
float2 angles;
float radius;
int triangles;

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
    
    // Compute vertices based on index if not a midpoint vertex.
    int id = input.VertexID % 3;
    if(0 < id){
        int tri = input.VertexID / 3;
        int curve = tri+(id-1);
        float ease = ((float)curve)/triangles;
        
        // Now that we have an ease factor in [0..1] we can get the real angle.
		if(angles.x < angles.y){
			angles.x += 2*3.14159265358979323;
		}
		
        float angle = lerp(angles.x, angles.y, ease);
        
        // Turn polar coordinates into cartesian.
        pos.x = radius * cos(angle);
        pos.y = radius * sin(angle);
    }

    output.Position = mul(mul(mul(pos, modelMatrix), viewMatrix), projectionMatrix);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    return float4(1, 0, 0, 0.5);
}

technique Cone
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};