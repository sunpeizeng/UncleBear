Shader "Triniti/Particle/ABAA_COL_DO_ZTA"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		
		_Color2 ("Main Color 2", Color) = (0.5,0.5,0.5,0.5)
		_MainTex2 ("Particle Texture 2", 2D) = "white" {}
	}

	Category
	{
		Tags { "Queue"="Transparent"}
		Cull Off
		ZWrite Off
		ZTest Always
		Fog { Color (0,0,0,0) }
		
		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
		
		SubShader
		{
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
}

