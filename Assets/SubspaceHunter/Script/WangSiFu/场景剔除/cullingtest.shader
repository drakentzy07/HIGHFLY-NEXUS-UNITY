Shader "Custom/cullingtest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BoundsCenter ("Bounds Center", Vector) = (0, 0, 0)
        _BoundsSize ("Bounds Size", Vector) = (1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 启用透明混合
            ZWrite On // 启用深度写入，以便正确显示在区域内的部分
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex; // 定义 _MainTex 采样器
            fixed4 _MainTex_ST;
            float3 _BoundsCenter;
            float3 _BoundsSize;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // 获取世界坐标
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算物体在区域内的可见性
                float3 pos = i.worldPos;
                float visibility = 1.0;

                // 检查物体是否在正方体区域外
                if (pos.x < _BoundsCenter.x - _BoundsSize.x * 0.5 || 
                    pos.x > _BoundsCenter.x + _BoundsSize.x * 0.5 ||
                    pos.y < _BoundsCenter.y - _BoundsSize.y * 0.5 || 
                    pos.y > _BoundsCenter.y + _BoundsSize.y * 0.5 ||
                    pos.z < _BoundsCenter.z - _BoundsSize.z * 0.5 || 
                    pos.z > _BoundsCenter.z + _BoundsSize.z * 0.5)
                {
                    visibility = 0.0; // 区域外的部分设置为完全透明
                }

                // 读取纹理颜色
                fixed4 texColor = tex2D(_MainTex, i.uv);
                texColor.a *= visibility; // 将可见性应用到 alpha 通道

                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent"
}
