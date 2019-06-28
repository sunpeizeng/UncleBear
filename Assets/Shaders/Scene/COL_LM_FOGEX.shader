
Shader "Triniti/Scene/COL_LM_FOGEX"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex", 2D) = "" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue" = "Geometry" }
	
		Pass
		{
			Lighting Off
			
			BindChannels
			{
				Bind "vertex", vertex
				Bind "texcoord", texcoord0
				Bind "texcoord1", texcoord1
			}
			
			Color [_Color]
			SetTexture [_MainTex]{ combine texture * primary }
			SetTexture [_LightMap] { combine texture * previous }
		}
	}
}

