Shader "Custom/WhiteOutLine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 1
        
        // 외곽선 설정
        [MaterialToggle] _Outline ("Outline Enabled", Float) = 1
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThick ("Outline Thickness (Pixels)", Float) = 1
        _AlphaThreshold ("Alpha Threshold", Range(0.01, 0.9)) = 0.1 // 알파 찌꺼기 제거용 기준값
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineThick;
            float _Outline;
            float _AlphaThreshold;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 원본 픽셀의 알파가 임계값보다 낮으면 아예 투명(0) 처리하여 찌꺼기 제거
                if (c.a < _AlphaThreshold)
                {
                    c.a = 0;
                }
                
                // 이제 깨끗해진 알파를 기준으로 외곽선 연산 진행
                if (_Outline > 0 && c.a == 0)
                {
                    float2 pixelStep = _MainTex_TexelSize.xy * _OutlineThick;
                    
                    // 주변 4방향 픽셀 샘플링
                    half aUp    = tex2D(_MainTex, IN.texcoord + float2(0, pixelStep.y)).a;
                    half aDown  = tex2D(_MainTex, IN.texcoord - float2(0, pixelStep.y)).a;
                    half aRight = tex2D(_MainTex, IN.texcoord + float2(pixelStep.x, 0)).a;
                    half aLeft  = tex2D(_MainTex, IN.texcoord - float2(pixelStep.x, 0)).a;

                    // 주변 픽셀들 역시 임계값보다 높은 '진짜 캐릭터 픽셀'인지 검사
                    half validUp    = aUp    >= _AlphaThreshold ? 1 : 0;
                    half validDown  = aDown  >= _AlphaThreshold ? 1 : 0;
                    half validRight = aRight >= _AlphaThreshold ? 1 : 0;
                    half validLeft  = aLeft  >= _AlphaThreshold ? 1 : 0;

                    if (validUp + validDown + validRight + validLeft > 0)
                    {
                        c = _OutlineColor;
                        c.rgb *= c.a; 
                    }
                }
                else
                {
                    c.rgb *= c.a;
                }

                return c;
            }
        ENDCG
        }
    }
}
