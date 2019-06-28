Shader "Triniti/Model/ModelEdge_Alphe"
{
    Properties
    {
        _MainTex("Texture (RGBA)", 2D) = "white" {}
		_BlendTex("Blend Texture (RGBA)", 2D) = "black" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _AtmoColor("Atmosphere Color", Color) = (0.5, 0.5, 1.0, 1)
		_Pow("Edge Length",Range (0.5,8)) = 1
    }
   
 SubShader
    {
    
		Tags {"Queue" = "Transparent"}
        Pass
        {
            Name "PlanetBase"
           
            Cull back

			BindChannels 
			{
				Bind "Vertex", vertex
				Bind "texcoord", texcoord0
				Bind "normal", normal
			}
           	Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
               
                #pragma fragmentoption ARB_fog_exp2
                #pragma fragmentoption ARB_precision_hint_fastest
               
                #include "UnityCG.cginc"
               
                uniform sampler2D _MainTex;
                uniform float4 _MainTex_ST;
                uniform float4 _Color;
                uniform float4 _AtmoColor;

				uniform sampler2D _BlendTex;
                uniform float4 _BlendTex_ST;
				uniform float _Pow;
               
                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float3 normal : TEXCOORD0;
                    float3 worldvertpos : TEXCOORD1;
                    float2 texcoord : TEXCOORD2;
					float amount : TEXCOORD3;
                };

                v2f vert(appdata_base v)
                {
                    v2f o;
                   
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                    o.normal = mul (UNITY_MATRIX_MV, float4(v.normal,0));//mul (UNITY_MATRIX_MVP,v.normal.xyzz).xyz;
					o.normal = normalize(o.normal);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.amount =  saturate((o.normal.x*o.normal.x + o.normal.y*o.normal.y - o.normal.z*o.normal.z*0.5));
					o.amount = pow(o.amount,_Pow);
                    return o;
                }
              
                float4 frag(v2f i) : COLOR
                {
                    float4 color = tex2D(_MainTex, i.texcoord) * _Color;
                    color.rgba = lerp(color.rgba, _AtmoColor, i.amount);
               
                    return color;
                }
            ENDCG
        }
   
    }//end subshader

	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			Lighting Off

			BindChannels 
			{
				Bind "Vertex", vertex
				Bind "texcoord", texcoord0
				Bind "texcoord1", texcoord1
			}
			Color[_Color]
			SetTexture [_MainTex]
			{
				combine texture * primary
			}

			SetTexture [_MainTex2]
			{
				ConstantColor [_AtmoColor]
				combine previous * constant
			}
		}
	}
}

