Shader "Custom/ColorCycleShader"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _ColorSpeed("Color Speed", Float) = 1.0
        _CustomTime("Custom Time", Float) = 0.0
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            uniform float _CustomTime;
            uniform float4 _Color;
            uniform float _ColorSpeed;

            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Calculate distance from the center
                float distance = length(v.vertex.xy);
                float hue = frac(_CustomTime * _ColorSpeed + distance);
                o.color = float4(hue, 1.0, 1.0, 1.0);
                o.color.rgb = hsv2rgb(o.color.rgb);
                o.color.a = 1.0;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
