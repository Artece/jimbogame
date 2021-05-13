#define LINE_WIDTH 0.01
#define LINE_SPACE 15.0

float time : register(C0);
float resolutionRatio : register(C1);

float isInGrid(float t, float2 uv)
{
    float modded = fmod((uv.x + uv.y + t) / LINE_WIDTH, LINE_SPACE);
    modded = fmod(modded + LINE_SPACE, LINE_SPACE);
    return 1.0 - clamp(floor(modded), 0, 1);
}

float4 main(float2 uvIn : TEXCOORD) : COLOR
{
    float2 uv = float2(uvIn.x, uvIn.y / resolutionRatio);
    
    float4 col = float4(0, 0, 0, 0);
    
    // colors represented as RGB values from 0 to 1
    float3 color1 = float3(0.05, 0.0, 0.1);
    float3 color2 = float3(0.01, 0.01, 0.15);
    
    float iscolor1 = 0;
    iscolor1 += isInGrid(time * 0.05, uv);
    iscolor1 += isInGrid(3.14, float2(uv.x, -uv.y));
    iscolor1 = clamp(iscolor1, 0, 1);
    col += float4(color1 * iscolor1, iscolor1);
    
    float iscolor2 = 0;
    iscolor2 += isInGrid(time * 0.08, uv);
    iscolor2 += isInGrid(time * 0.05, float2(uv.x, -uv.y));
    iscolor2 = clamp(iscolor2, 0, 1);
    col += float4(color2 * iscolor2, iscolor2);
    
    col /= max(sqrt(col.a), 1);
    return float4(col.xyz, 1);
}