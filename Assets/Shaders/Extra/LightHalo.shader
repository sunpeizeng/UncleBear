//小猪哥友情提供
//2012,8,29
Shader "Triniti/Extra/LightHalo"
{
	Properties
	{
		_HaloColor ("Halo Color", Color) = (1,1,1,1)
		_Halo ("Halo (RGBA)", 2D) = "black" {}
		_MainTex ("MainTex (RGB)", 2D) = "" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}

	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		BindChannels
		{
			Bind "texcoord1", texcoord0
			Bind "texcoord", texcoord1
			Bind "texcoord", texcoord2
			Bind "texcoord", texcoord3
			Bind "texcoord1", texcoord4
		}

		Pass
		{
			Fog{ Mode Off }
			
			SetTexture [_Halo]{ 
			combine texture * texture double

			}
			
			SetTexture [_Halo]{ 
			constantColor [_HaloColor]
			combine constant * previous
			}

			SetTexture [_MainTex]{ combine texture * previous double }
			SetTexture [_MainTex]{ combine previous + texture }

			SetTexture [_LightMap]{ combine texture * previous }
		}
	}

}

