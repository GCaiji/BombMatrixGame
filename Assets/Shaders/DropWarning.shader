Shader "Custom/DropWarning"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,0,0,1)   // 预警圈主色
        _FlashColor ("Flash Color", Color) = (1,0.5,0.5,1) // 闪烁警示色
        _Progress ("Progress", Range(0,1)) = 1      // 进度控制(1开始->0结束)
        _RingWidth ("Ring Width", Range(0.01,0.5)) = 0.05  // 圆环宽度
        _FlashSpeed ("Flash Speed", Range(0,10)) = 2.0    // 闪烁速度
        _Smoothness ("Edge Smoothness", Range(0,0.1)) = 0.01 // 边缘平滑度
    }
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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

            fixed4 _Color;
            fixed4 _FlashColor;
            float _Progress;
            float _RingWidth;
            float _FlashSpeed;
            float _Smoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 圆心在中心 (0.5, 0.5)
                float2 center = float2(0.5, 0.5);
                float2 delta = i.uv - center;
                float dist = length(delta);
                
                // 计算圆环区域 (平滑过渡)
                float ringMask = smoothstep(
                    0.5 - _RingWidth - _Smoothness, 
                    0.5 - _RingWidth, 
                    dist
                ) - smoothstep(
                    0.5, 
                    0.5 + _Smoothness, 
                    dist
                );
                
                // 丢弃圆环外的像素
                if (ringMask <= 0) discard;
                
                // 计算当前角度 (0 = 顶部，顺时针增加)
                float angle = atan2(delta.y, delta.x) * 0.15915 + 0.5; // [-π,π] -> [0,1]
                angle = fmod(angle - 0.25, 1); // 0.25偏移使0°位于顶部
                angle = angle < 0 ? angle + 1 : angle;
                
                // 计算进度条裁剪
                float progressMask = step(angle, _Progress);
                if (progressMask <= 0) discard;
                
                // 闪烁效果 (正弦波变化)
                float flash = sin(_Time.y * _FlashSpeed) * 0.5 + 0.5;
                fixed4 finalColor = lerp(_Color, _FlashColor, flash);
                
                // 进度条尾部衰减效果
                float fade = 1 - saturate((_Progress - angle) * 20);
                finalColor = lerp(finalColor, _FlashColor, fade);
                
                finalColor.a = ringMask;
                return finalColor;
            }
            ENDCG
        }
    }
}