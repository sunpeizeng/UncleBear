Shader "Triniti/Model/RotateUV"
{
    Properties
    {
        _MainTex("Texture (RGB)", 2D) = "black" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _Thta("Angle", Float) = 0
		_Center("Center",Vector) = (0.5,0.5,0,0)
    } 
   
 SubShader
    {
    
        Pass
        {
            Cull back
			Blend SrcAlpha OneMinusSrcAlpha
			BindChannels 
			{
				Bind "Vertex", vertex
				Bind "texcoord", texcoord0
				Bind "normal", normal
			}
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
               
                #pragma fragmentoption ARB_fog_exp2
                #pragma fragmentoption ARB_precision_hint_fastest
               
                #include "UnityCG.cginc"
               
                uniform sampler2D _MainTex;
                uniform float4 _MainTex_ST;
                uniform float4 _Color;
                uniform float _Thta;
				uniform fixed4 _Center;
               
                struct v2f
                {
                    float4 pos : SV_POSITION;
                    //float3 normal : TEXCOORD0;
                    float2 texcoord : TEXCOORD2;
                };

                v2f vert(appdata_base v)
                {
                    v2f o;
                   
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                    ///o.normal = mul (UNITY_MATRIX_MV, float4(v.normal,0));
					//o.normal = normalize(o.normal);
                    o.texcoord = v.texcoord;//TRANSFORM_TEX(v.texcoord, _MainTex);
					half sinThta = sin(_Thta);
					half cosThta = cos(_Thta);

					o.texcoord -= _Center.xy;

					o.texcoord = float2(o.texcoord.x*cosThta - o.texcoord.y*sinThta , o.texcoord.x*sinThta + o.texcoord.y*cosThta);

					o.texcoord += _Center.xy;
					
					o.texcoord = TRANSFORM_TEX(o.texcoord, _MainTex);

                    return o;
                }
              
                float4 frag(v2f i) : COLOR
                {

                    float4 color = tex2D(_MainTex, i.texcoord) * _Color;
                    //color.rgba = lerp(color.rgba, _AtmoColor, i.amount);
               
                    return color;
                }
            ENDCG
        }
   
    }
}

