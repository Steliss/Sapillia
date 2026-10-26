Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size (px)", Range(0, 10)) = 1
        [Toggle] _OutlineOn ("Outline On", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True"
               "RenderType"="Transparent" "PreviewType"="Plane"
               "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _OutlineOn;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                c.rgb *= c.a; // 프리멀티플라이드 알파

                float2 d = _MainTex_TexelSize.xy * _OutlineSize;
                float a = 0;
                a = max(a, tex2D(_MainTex, i.uv + float2( d.x, 0)).a);
                a = max(a, tex2D(_MainTex, i.uv + float2(-d.x, 0)).a);
                a = max(a, tex2D(_MainTex, i.uv + float2(0,  d.y)).a);
                a = max(a, tex2D(_MainTex, i.uv + float2(0, -d.y)).a);
                a = max(a, tex2D(_MainTex, i.uv + float2( d.x,  d.y)).a);
                a = max(a, tex2D(_MainTex, i.uv + float2(-d.x,  d.y)).a);
                a = max(a, tex2D(_MainTex, i.uv + float2( d.x, -d.y)).a);
                a = max(a, tex2D(_MainTex, i.uv + float2(-d.x, -d.y)).a);

                float outline = (1 - c.a) * a * _OutlineOn;
                return c + fixed4(_OutlineColor.rgb * outline, outline);
            }
            ENDCG
        }
    }
}