Shader "Triniti/Particle/ABAA_COL_DO_OnePass"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		
		_Color2 ("Main Color 2", Color) = (0.5,0.5,0.5,0.5)
		_MainTex2 ("Particle Texture 2", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue"="Transparent"}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		Fog { Color (0,0,0,0) }

		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
			Bind "TexCoord1", texcoord1
		}

		Pass
		{
		        CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
               
                #pragma fragmentoption ARB_fog_exp2
                #pragma fragmentoption ARB_precision_hint_fastest
               
                #include "UnityCG.cginc"
               
                uniform sampler2D _MainTex;
                uniform float4 _MainTex_ST;
                uniform float4 _Color;

				uniform sampler2D _MainTex2;
                uniform float4 _MainTex2_ST;
                uniform float4 _Color2;
               
			   	struct V2In
				{
					half4 vertex : POSITION;
					half2 texcoord : TEXCOORD0;
					//half2 texcoord1 : TEXCOORD1;
					half4 color : COLOR;
				};

                struct v2f
                {
                    float4 pos : SV_POSITION;
					float2 texcoord1 : TEXCOORD0;
                    float2 texcoord2 : TEXCOORD1;
					half4 color : COLOR;
                };

                v2f vert(V2In v)
                {
                    v2f o;
                   
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);

                    o.texcoord1 = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.texcoord2 = TRANSFORM_TEX(v.texcoord, _MainTex2);

					o.color = v.color;

                    return o;
                }
              
                half4 frag(v2f i) : COLOR
                {

                    half4 color1 = tex2D(_MainTex, i.texcoord1) * _Color * i.color * 2.0;
                    half4 color2 = tex2D(_MainTex2, i.texcoord2) * _Color2 * i.color * 2.0;
					
					half4 color = color1 + color2.a / color1.a * color2;

					color.a = color1.a;

                    return color;
                }
            ENDCG
		}
	}


	SubShader
	{
		Tags { "Queue"="Transparent"}
		Cull Off
		ZWrite Off
		Fog { Color (0,0,0,0) }
		
		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
		
		

		//AB
			Pass
			{
			    Blend SrcAlpha OneMinusSrcAlpha
			
				SetTexture [_MainTex]
				{
					constantColor [_Color]
					combine constant * primary
				}
				SetTexture [_MainTex]
				{
					combine texture * previous double
				}
			}
		//AA
			Pass
			{
				Blend SrcAlpha One
			
				SetTexture [_MainTex2]
				{
					constantColor [_Color2]
					combine constant * primary
				}
				SetTexture [_MainTex2]
				{
					combine texture * previous double
				}
			}
		
	}
}

