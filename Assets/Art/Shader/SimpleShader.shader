Shader "Custom/SimpleShader"
{
    Properties
    {
        _Color ("Color222", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Geometry"}
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag //编译指令
            fixed4 _Color;
            struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 pos : SV_POSITION; // 裁剪空间中的顶点坐标
            };
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            half4 frag(v2f i) : SV_Target //结果存储到渲染目标，即默认的帧缓存
			{
			    fixed3 c = _Color;
                return half4(c,1);
            }
            ENDCG
        }
    } 
   
}
