Shader "Custom/ObjectSilhouette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _OccludedOpacity ("Occluded Object Opacity", Range(0, 1)) = 0.3
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

        // 플레이어와 겹치지 않는 영역
        Pass
        {
            Name "Normal"

            Stencil
            {
                Ref 1
                ReadMask 1
                WriteMask 0

                Comp NotEqual
                Pass Keep
            }

            CGPROGRAM

            #pragma target 2.0
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag

            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnitySprites.cginc"

            ENDCG
        }

        // 플레이어와 겹치는 영역의 오브젝트만 반투명 처리
        Pass
        {
            Name "Occluded"

            Stencil
            {
                Ref 1
                ReadMask 1
                WriteMask 0

                Comp Equal
                Pass Keep
            }

            CGPROGRAM

            #pragma target 2.0
            #pragma vertex SpriteVert
            #pragma fragment OccludedFragment

            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnitySprites.cginc"

            float _OccludedOpacity;
            float _Cutoff;

            fixed4 OccludedFragment(v2f input) : SV_Target
            {
                fixed4 color =
                    SampleSpriteTexture(input.texcoord) * input.color;

                // 오브젝트의 투명한 여백 제외
                clip(color.a - _Cutoff);

                // 겹친 영역의 오브젝트 불투명도를 낮춤
                color.a *= _OccludedOpacity;
                color.rgb *= color.a;

                return color;
            }

            ENDCG
        }
    }

    Fallback Off
}