Shader "Hidden/AnalogGlitchImageShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HorizontalShake ("Horizontal Shake", float) = 1
        _ScanLineJitterDisplacement ("Scan Line Jitter Displacement", range(0, 1)) = 0
        _ScanLineJitterThreshold ("Scan Line Jitter Threshold", range(0, 1)) = 0
        _VerticalJumpAmount("Vertical Jump Amount", float) = 0
        _VerticalJumpTime("Veritcal Jump Time", float) = 0
        _ColorDrift("Color Drift", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
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

            float _ScanLineJitterDisplacement;
            float _ScanLineJitterThreshold;
            float _VerticalJumpAmount;
            float _VerticalJumpTime;
            float _HorizontalShake;
            float2 _ColorDrift;     // (amount, time)

            float nrand(float x, float y)
            {
                return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float u = i.uv.x;
                float v = i.uv.y;

                // Scan line jitter
                float jitter = nrand(v, _Time.x) * 2 - 1;
                jitter *= step(_ScanLineJitterThreshold, abs(jitter)) *_ScanLineJitterDisplacement;

                // Vertical jump
                float jump = lerp(v, frac(v + _VerticalJumpTime), _VerticalJumpAmount);

                // Horizontal shake
                float shake = (nrand(_Time.x, 2) - 0.5) * _HorizontalShake;

                // Color drift
                float drift = sin(jump + _ColorDrift.y) * _ColorDrift.x;

                half4 src1 = tex2D(_MainTex, frac(float2(u + jitter + shake, jump)));
                half4 src2 = tex2D(_MainTex, frac(float2(u + jitter + shake + drift, jump)));

                return half4(src1.r, src2.g, src1.b, src2.a);
            }
            ENDCG
        }
    }
}
