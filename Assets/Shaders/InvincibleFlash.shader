Shader "Custom/InvincibleFlash"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _FlashSpeed ("Flash Speed", Range(1, 20)) = 10
        _MinAlpha ("Min Alpha", Range(0, 1)) = 0.2
        _MaxAlpha ("Max Alpha", Range(0, 1)) = 0.8
        [Toggle(USE_CUSTOM_COLOR)] _UseCustomColor ("Use Custom Color", Float) = 0
        _FlashColor ("Flash Color", Color) = (1,0.5,0.5,1)
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature USE_CUSTOM_COLOR
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _BaseColor;
            float _Invincible;
            float _FlashSpeed;
            float _MinAlpha;
            float _MaxAlpha;
            fixed4 _FlashColor;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _BaseColor;
                
                if (_Invincible > 0.5)
                {
                    float t = sin(_Time.y * _FlashSpeed) * 0.5 + 0.5;
                    float alpha = lerp(_MinAlpha, _MaxAlpha, t);
                    
                    #if USE_CUSTOM_COLOR
                        col.rgb = lerp(col.rgb, _FlashColor.rgb, t * _FlashColor.a);
                    #endif
                    
                    col.a *= alpha;
                }
                
                return col;
            }
            ENDCG
        }
    }
}
