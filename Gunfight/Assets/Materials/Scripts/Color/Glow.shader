Shader "Hidden/Glow" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowIntensity ("GlowIntensity", Range(0, 10)) = 2
        [NoScaleOffset] _GlowTex ("GlowTexture", 2D) = "white" { }

        _DistortTex ("DistortionTex", 2D) = "white" { }
        _DistortAmount ("DistortionAmount", Range(0, 2)) = 2 
        _DistortTexXSpeed ("DistortTexXSpeed", Range(-50, 50)) = 0 
        _DistortTexYSpeed ("DistortTexYSpeed", Range(-50, 50)) = -5

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
            sampler2D _GlowTex;
            float4 _BaseColor;
            fixed4 _GlowColor;
            float _GlowIntensity;

            sampler2D _DistortTex;
            float4 _DistortTex_ST;
            float _DistortTexXSpeed, _DistortTexYSpeed, _DistortAmount;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 uvOutDistTex : TEXCOORD1;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uvOutDistTex = TRANSFORM_TEX(v.uv, _DistortTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) *  _BaseColor;

                i.uvOutDistTex.x += (_Time * _DistortTexXSpeed) % 1;
                i.uvOutDistTex.y += (_Time * _DistortTexYSpeed) % 1;
                float outDistortAmnt = (tex2D(_DistortTex, i.uvOutDistTex).r - 0.5) * 0.2 * _DistortAmount;
                float2 destUv = (0, 0);
                destUv.x += outDistortAmnt;
                destUv.y += outDistortAmnt;
                float4 noiseCol = tex2D(_DistortTex, destUv);


                fixed4 emission = tex2D(_GlowTex, i.uv);

                emission.rgb *= emission.a * col.a * _GlowIntensity * _GlowColor;
                col.rgb += emission.rgb * noiseCol;

                return col;
            }
            ENDCG
        }
    }
}