#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

float4x4 viewMatrix;
float4x4 modelMatrix;
Texture2D tileset;
Texture2D tilemap;
int tileSize;
float2 viewSize;

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
    float2 mapCoord = input.UV*viewSize;
    mapCoord = mul(mul(float4(mapCoord, 0, 1), viewMatrix), modelMatrix).xy;
    
    // Offset to be centered.
    output.mapCoord = mapCoord+tileSize/2;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    uint w, h;
    tilemap.GetDimensions(w, h);
    int2 mapWH = int2(w, h)*tileSize;
    int2 mapXY = int2(floor(input.mapCoord));
    
    // Flip around to be bottom-bound.
    mapXY.y = mapWH.y - mapXY.y;
    
    // Bounds check to ensure we're still within the tilemap.
    if(mapXY.x < 0 || mapXY.y < 0 | mapWH.x <= mapXY.x || mapWH.y <= mapXY.y)
      return float4(0, 0, 0, 0);
    
    // Calculate tile position of and offset within the tile.
    uint2 tileXY   = uint2((uint)mapXY.x / (uint)tileSize, (uint)mapXY.y / (uint)tileSize);
    uint2 offsetXY = uint2((uint)mapXY.x % (uint)tileSize, (uint)mapXY.y % (uint)tileSize);
    
    // Look up tile and subsequent image data at tile position.
    float4 tile = tilemap.Load(int3(tileXY, 0));
    float4 color = tileset.Load(int3(tile.xy*256*tileSize+offsetXY, 0));

    return color;
}

technique Tile
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};