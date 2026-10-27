Shader "Custom/PlayerStencil"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Cutoff ("Alpha Cutoff", Range(0.001, 1)) = 0.1

        [MaterialToggle] PixelSnap ("Pixel Snap", Float) = 0

        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)

        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest LEqual

        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "PlayerStencil"

            Stencil
            {
                Ref 1
                ReadMask 1
                WriteMask 1

                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }

            CGPROGRAM

            #pragma target 2.0
            #pragma vertex SpriteVert
            #pragma fragment PlayerFragment

            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnitySprites.cginc"

            float _Cutoff;

            fixed4 PlayerFragment(v2f input) : SV_Target
            {
                fixed4 color =
                    SampleSpriteTexture(input.texcoord) * input.color;

                // 투명한 여백은 버림.
                // 버려진 픽셀에는 스텐실도 기록되지 않음.
                clip(color.a - _Cutoff);

                // 기본 스프라이트와 같은 방식으로 색상 합성
                color.rgb *= color.a;

                return color;
            }

            ENDCG
        }
    }

    Fallback Off
}