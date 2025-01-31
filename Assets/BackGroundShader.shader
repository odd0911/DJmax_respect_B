Shader "Custom/OptimizedGaussianBlur1D"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _BlurSize ("Blur Size", Range(0, 10)) = 1.0
        _Sigma ("Sigma", Range(0.1, 5.0)) = 10.0 // 더 강한 블러를 위해 Sigma 값 증가
        _KernelSize ("Kernel Size", Range(3, 15)) = 15 // 커널 크기 증가
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

            sampler2D _MainTex;
            float _BlurSize;
            float _Sigma;
            float _KernelSize;
            float2 _MainTex_TexelSize;

            // 가우시안 커널 계산 (1D)
            float gaussian(float x, float sigma)
            {
                return exp(-0.5 * (x * x) / (sigma * sigma)) / (sigma * sqrt(2 * 3.14159));
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 color = half4(0, 0, 0, 0);
                float2 offset = _MainTex_TexelSize * _BlurSize;

                // 가우시안 커널 크기 설정
                int kernelSize = int(_KernelSize); // 사용자 정의 커널 크기
                float weightSum = 0.0;
                float kernel[15]; // 최대 커널 크기 15

                // 1D 가우시안 커널 계산
                for (int j = -kernelSize / 2; j <= kernelSize / 2; ++j)
                {
                    kernel[j + kernelSize / 2] = gaussian(float(j), _Sigma);
                    weightSum += kernel[j + kernelSize / 2];
                }

                // 수평 블러 (가우시안 커널 적용)
                for (int j = -kernelSize / 2; j <= kernelSize / 2; ++j)
                {
                    color += tex2D(_MainTex, i.uv + float2(j * offset.x, 0)) * kernel[j + kernelSize / 2];
                }

                color /= weightSum; // 가중치 합으로 정규화

                // 수직 블러 (가우시안 커널 적용)
                half4 verticalColor = half4(0, 0, 0, 0);
                for (int j = -kernelSize / 2; j <= kernelSize / 2; ++j)
                {
                    verticalColor += tex2D(_MainTex, i.uv + float2(0, j * offset.y)) * kernel[j + kernelSize / 2];
                }

                verticalColor /= weightSum; // 가중치 합으로 정규화

                return color * 0.5 + verticalColor * 0.5; // 수평과 수직 블러 결과 결합
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
