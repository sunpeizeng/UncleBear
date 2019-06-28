Shader "Triniti/Particle/RotateUV_AA_COL_DO_FADE"
{
    Properties
    {
        _MainTex("Texture (RGB)", 2D) = "black" {}
		_FadeTex ("Fade Texture",2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _Thta("Angle", Float) = 0
		_ThtaFade("Angle Fade", Float) = 0
		_Center("Center",Vector) = (0.5,0.5,0,0)
    } 
   
 SubShader
    {
		Tags { "Queue"="Transparent"}
		
        Pass
        {
            Cull Off
			Blend SrcAlpha One
			BindChannels 
			{
				Bind "Vertex", vertex
				Bind "texcoord", texcoord0
				Bind "texcoord1", texcoord1
			}
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
               
                #pragma fragmentoption ARB_fog_exp2
                #pragma fragmentoption ARB_precision_hint_fastest
               
                #include "UnityCG.cginc"
               
                uniform sampler2D _MainTex;
                uniform float4 _MainTex_ST;
				uniform sampler2D _FadeTex;
				uniform float4 _FadeTex_ST;
                uniform float4 _Color;
                uniform float _Thta;
				uniform float _ThtaFade;
				uniform fixed4 _Center;
               
			    struct v2i
                {
                    float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 texcoord0 : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
                };

                v2f vert(v2i v)
                {
                    v2f o;
                   
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                    o.texcoord0 = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.texcoord0.xy -= 0.5*(_MainTex_ST - 1);
					half sinThta = sin(_Thta);
					half cosThta = cos(_Thta);

					o.texcoord0 -= _Center.xy;

					o.texcoord0 = float2(o.texcoord0.x*cosThta - o.texcoord0.y*sinThta , o.texcoord0.x*sinThta + o.texcoord0.y*cosThta);

					o.texcoord0 += _Center.xy;

					//fade map

					o.texcoord1 = TRANSFORM_TEX(v.texcoord1, _FadeTex);
					o.texcoord1.xy -= 0.5*(_FadeTex_ST - 1);
					sinThta = sin(_ThtaFade);
					cosThta = cos(_ThtaFade);
					o.texcoord1 -= _Center.xy;

					o.texcoord1 = float2(o.texcoord1.x*cosThta - o.texcoord1.y*sinThta , o.texcoord1.x*sinThta + o.texcoord1.y*cosThta);

					o.texcoord1 += _Center.xy;

                    return o;
                }
              
                float4 frag(v2f i) : COLOR
                {

                    float4 color = tex2D(_MainTex, i.texcoord0) *  tex2D(_FadeTex, i.texcoord1) * _Color;
               
                    return color;
                }
            ENDCG
        }
   
    }
}

