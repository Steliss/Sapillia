Shader "Custom/SpriteShoutWave"
{
    Properties
    {
        _Strength ("Distortion Strength", Range(0, 0.2)) = 0.04
        _RingWidth ("Ring Width", Range(0.01, 0.5)) = 0.15
        _Progress ("Progress (0~1)", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off

        GrabPass { "_ShockGrab" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _ShockGrab;
            float _Strength;
            float _RingWidth;
            float _Progress;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 grabPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 스프라이트 중심 기준 좌표 (-1 ~ 1)
                float2 p = i.uv * 2 - 1;
                float dist = length(p);

                // 진행도에 따라 퍼지는 링 (바깥 가장자리는 1.0)
                float ringCenter = _Progress;
                float ring = 1 - saturate(abs(dist - ringCenter) / _RingWidth);
                ring = smoothstep(0, 1, ring);

                // 원 바깥은 제외
                ring *= step(dist, 1.0);

                // 중심에서 바깥쪽으로 밀기
                float2 dir = normalize(p + 0.0001);
                float fade = 1 - _Progress; // 퍼질수록 약해짐
                float2 offset = dir * ring * _Strength * fade;

                float4 grabUV = i.grabPos;
                grabUV.xy += offset * grabUV.w;

                return tex2Dproj(_ShockGrab, UNITY_PROJ_COORD(grabUV));
            }
            ENDCG
        }
    }
}
