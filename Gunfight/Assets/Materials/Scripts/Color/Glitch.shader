Shader "Hidden/Glitch" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _GlitchAmount ("Glitch Amount", Range(0, 20)) = 3
        _GlitchSize ("Glitch Size", Range(0.25, 5)) = 1

    }
    SubShader {
        Tags { "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            half _GlitchAmount, _GlitchSize;
            float4 _BaseColor;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            half rand2(half2 seed, half offset) {
                return (frac(sin(dot(seed * floor(50 + (_Time % 1.0) * 12), half2(127.1, 311.7))) * 43758.5453123) + offset) % 1.0;
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                half lineNoise = pow(rand2(floor(i.uv * half2(24, 19) * _GlitchSize) * 4, 1), 3.0) * _GlitchAmount
                * pow(rand2(floor(i.uv * half2(38, 14) * _GlitchSize) * 4, 1), 3.0);
                col = tex2D(_MainTex, i.uv + half2(lineNoise * 0.02 * rand2(half2(2.0, 1), 1), 0)) * _BaseColor;
                
                return col;
            }
            ENDCG
        }
    }
}