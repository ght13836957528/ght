Shader "Custom/DiffuseVertexLight"
{
    Properties
    {
        _Diffuse("Diffuse", Color) = (1,1,1)
    }
    SubShader
    {
        Tags { "LightMode"="ForwardBase" }
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Lighting.cginc"
            fixed4 _Diffuse;
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
                float3 col:COLOR;
            };

            v2f vert(a2v data)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(data.vertex);
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                fixed3 worldNormal = UnityObjectToWorldNormal(data.normal);
                fixed3 worldLight = normalize(_WorldSpaceLightPos0.xyz);
                float3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal,worldLight)* 0.5f + 0.5f);
                //半兰伯特，之前是将小于0的结果都变成0，使得背光面无变化，兰半伯特将点积的结果从（-1，1）映射到（0，1），使得背光面也有明暗变化
                o.col = diffuse + ambient;
                return o;
            }

            fixed4 frag(v2f data):SV_Target
            {
                return fixed4(data.col,1);
            }
        
            ENDCG
            
        }
        
    }
    FallBack "Diffuse"
}
