Shader "Custom/SpecularVertextLight"
{
    Properties
    {
        _Specular ("Specular", Color) = (1,1,1,1)
        _Gloss ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Diffuse("Diffuse",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "LightMode"="ForwardBase"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "Lighting.cginc"
            
            fixed4 _Diffuse;
            fixed4 _Specular;
            float _Gloss;
            
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
				//顶点信息转换至世界空间
				o.pos = UnityObjectToClipPos(data.vertex);
				//得到环境光部分
				fixed3 ambient =  UNITY_LIGHTMODEL_AMBIENT.xyz;
				//顶点法线转换至世界空间
				fixed3 worldnormal = normalize(mul(data.normal,(float3x3)unity_WorldToObject));
				//光源方向
				fixed3 worldlight = normalize(_WorldSpaceLightPos0.xyz);
				//漫反射部分
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldnormal,worldlight));
				//计算反射方向
				fixed3 reflectDir = normalize(reflect(-worldlight,worldnormal));
				//计算视角方向
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld,data.vertex).xyz);
				//得到高光发射部分
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(reflectDir,viewDir)),_Gloss);
				o.col = ambient + diffuse + specular;
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