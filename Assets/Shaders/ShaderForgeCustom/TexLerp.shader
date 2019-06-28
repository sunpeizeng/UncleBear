// Shader created with Shader Forge v1.30 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.30;sub:START;pass:START;ps:flbk:Unlit/Texture,iptp:0,cusa:True,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:True,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:32854,y:32655,varname:node_3138,prsc:2|emission-8654-OUT,alpha-726-OUT;n:type:ShaderForge.SFN_Tex2d,id:63,x:32062,y:32579,ptovrint:False,ptlb:Tex_Src,ptin:_Tex_Src,varname:node_63,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:335d39ac920954c01ac193e693291d4a,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:7875,x:32045,y:33003,ptovrint:False,ptlb:Tex_Dst,ptin:_Tex_Dst,varname:node_7875,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:6e01041934bf04f5a88b0272165bf725,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:1630,x:31805,y:32778,ptovrint:False,ptlb:Slider_Val,ptin:_Slider_Val,varname:node_1630,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Lerp,id:6090,x:32423,y:32680,varname:node_6090,prsc:2|A-63-RGB,B-7875-RGB,T-1630-OUT;n:type:ShaderForge.SFN_Color,id:4813,x:32423,y:32468,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_4813,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:8654,x:32636,y:32558,varname:node_8654,prsc:2|A-4813-RGB,B-6090-OUT;n:type:ShaderForge.SFN_Multiply,id:726,x:32636,y:32819,varname:node_726,prsc:2|A-4813-A,B-1170-OUT;n:type:ShaderForge.SFN_Lerp,id:1170,x:32423,y:32878,varname:node_1170,prsc:2|A-63-A,B-7875-A,T-1630-OUT;proporder:63-7875-1630-4813;pass:END;sub:END;*/

Shader "Shader Forge/TexLerp" {
    Properties {
        _Tex_Src ("Tex_Src", 2D) = "white" {}
        _Tex_Dst ("Tex_Dst", 2D) = "white" {}
        _Slider_Val ("Slider_Val", Range(0, 1)) = 0
        _Color ("Color", Color) = (1,1,1,1)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }
        LOD 200
        Pass {
            Name "FORWARD"
			Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha
			Offset -10,0
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            uniform sampler2D _Tex_Src; uniform float4 _Tex_Src_ST;
            uniform sampler2D _Tex_Dst; uniform float4 _Tex_Dst_ST;
            uniform float _Slider_Val;
            uniform float4 _Color;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 _Tex_Src_var = tex2D(_Tex_Src,TRANSFORM_TEX(i.uv0, _Tex_Src));
                float4 _Tex_Dst_var = tex2D(_Tex_Dst,TRANSFORM_TEX(i.uv0, _Tex_Dst));
                float3 emissive = (_Color.rgb*lerp(_Tex_Src_var.rgb,_Tex_Dst_var.rgb,_Slider_Val));
                float3 finalColor = emissive;
                return fixed4(finalColor,(_Color.a*lerp(_Tex_Src_var.a,_Tex_Dst_var.a,_Slider_Val)));
            }
            ENDCG
        }
    }
    FallBack "Unlit/Texture"
    //CustomEditor "ShaderForgeMaterialInspector"
}
