Shader "Map/Destructible" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BaseTex ("Base Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        _Progress ("Destroy Progress", Range(0,1)) = 0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _MainTex, _BaseTex, _NoiseTex, _DissolveMap;
        float _Progress;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // 采样破坏遮罩
            fixed4 dissolve = tex2D(_DissolveMap, IN.uv_MainTex);
            
            // 根据遮罩值混合纹理
            if (dissolve.r > _Progress) {
                o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
            } else {
                o.Albedo = tex2D(_BaseTex, IN.uv_MainTex).rgb;
            }
            
            o.Metallic = 0;
            o.Smoothness = 0.5;
        }
        ENDCG
    }
    FallBack "Diffuse"
}