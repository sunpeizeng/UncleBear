Shader "Triniti/Scene/COL_LM_BR_BM2"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex (RGB)", 2D) = "white" {}
		_BrightMap ("Brightmap (RGB)", 2D) = "white" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}
		_Channel1 ("ChannelMap1 (A)", 2D) = "black" {}
		_BlendTex1 ("BlendTex1 (RGB)", 2D) = "black" {}
		_Channel2 ("ChannelMap2 (A)", 2D) = "black" {}
		_BlendTex2 ("BlendTex2 (RGB)", 2D) = "black" {}	
	}

	SubShader
	{
		Tags { "Queue" = "Geometry" "RenderType"="Opaque" }

		BindChannels
		{
			Bind "vertex", vertex
			Bind "texcoord", texcoord0
			Bind "texcoord", texcoord1
			Bind "texcoord", texcoord2
			Bind "texcoord", texcoord3
			Bind "texcoord", texcoord4
			Bind "texcoord1", texcoord5
			Bind "texcoord1", texcoord6
		}
		
		Pass
		{
			Fog{ Mode Off }

			Color [_Color]
			SetTexture [_MainTex]{ combine texture }
			SetTexture [_Channel2]{ combine previous, texture }
			SetTexture [_BlendTex2]{ combine texture lerp (previous) previous }
			SetTexture [_Channel1]{ combine previous, texture }
			SetTexture [_BlendTex1]{ combine texture lerp (previous) previous }
			SetTexture [_BrightMap]{ combine texture * previous double }
			SetTexture [_LightMap]{ combine texture * previous }
		}
	}
}