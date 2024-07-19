Shader "Custom/BasicTexture"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white"{ }
        _Diffuse("Diffuse color",Color) = (1,1,1)
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
            float4 _MainTex_ST;
            sampler2D _MainTex;
            fixed4 _Diffuse;

            struct a2v
            {
                float4 vertex : POSITION;
                float4 texcoord: TEXCOORD0;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
                float2 uv:TEXCOORD1;
            };

            v2f vert(a2v data)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(data.vertex);
                o.uv = TRANSFORM_TEX(data.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f data):SV_Target
            {
                fixed3 albedo = tex2D(_MainTex,data.uv).rgb; //反射率
                fixed3 color = albedo;
                return fixed4(color, 1);
            }
            ENDCG

        }

    }
    FallBack "Diffuse"
}