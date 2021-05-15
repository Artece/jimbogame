Shader "Hidden/NewImageEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

#define LINE_WIDTH 0.01
#define LINE_SPACE 15.0

            float isInGrid(float t, float2 uv)
            {
                float modded = fmod((uv.x + uv.y + t) / LINE_WIDTH, LINE_SPACE);
                modded = fmod(modded + LINE_SPACE, LINE_SPACE);
                return 1.0 - clamp(floor(modded), 0, 1);
            }

            float4 customGrid(float2 uvIn : TEXCOORD) : COLOR
            {
                float time = _Time.y;
                float resolutionRatio = _ScreenParams.x / _ScreenParams.y;

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

            float4 frag (v2f i) : SV_Target
            {
                return customGrid(i.uv);
            }
            ENDCG
        }
    }
}
