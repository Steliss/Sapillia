Shader "Custom/TitleWaterReflect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.7, 0.85, 1, 0.4)
        _WaveStrength ("Wave Strength", Range(0, 0.03)) = 0.005
        _WaveFrequency ("Wave Frequency", Float) = 40
        _WaveSpeed ("Wave Speed", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _WaveStrength;
            float _WaveFrequency;
            float _WaveSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                uv.x += sin(
                    uv.y * _WaveFrequency
                    + _Time.y * _WaveSpeed
                ) * _WaveStrength;

                fixed4 col = tex2D(_MainTex, uv) * i.color;

                // 이미지 좌우 경계를 벗어난 부분 숨기기
                col.a *= step(0, uv.x) * step(uv.x, 1);

                return col;
            }
            ENDCG
        }
    }
}
