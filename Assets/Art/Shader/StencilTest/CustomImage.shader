Shader "StencilTest/Mask" {
       Properties{
     
        _Stencil("Stencil ID", Float) = 2
        _StencilComp("Stencil Comparison", Float) = 8
        _StencilOp("Stencil Operation", Float) = 2
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Geometry"}
        Pass {
            Stencil {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
            }
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 pos : SV_POSITION;
            };
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            half4 frag(v2f i) : SV_Target {
                return half4(1,1,1,1);
            }
            ENDCG
        }
    } 
}