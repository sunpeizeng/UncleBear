// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'


Shader "Triniti/Scene/Shader_Lightmap_Normal-BlendLv4"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("MainTex", 2D) = "white" {}
		_NormalTex ("NormalTex (RGB)", 2D) = "blue" {}

		_BlendTex ("BlendMap (A)", 2D) = "white" {}

		_BlendColor1 ("Blend Color 1", Color) = (0.5,0.5,0.5,1)
		_MainTex1 ("BlendTex1 (RGB)", 2D) = "white" {}
		_NormalTex1 ("NormalTex1 (RGB)", 2D) = "blue" {}
		
		_BlendColor2 ("Blend Color 2", Color) = (0.5,0.5,0.5,0.5)
		_MainTex2 ("BlendTex2 (RGB)", 2D) = "white" {}
		_NormalTex2 ("NormalTex2 (RGB)", 2D) = "blue" {}
		
		_BlendColor3 ("Blend Color 3", Color) = (0.5,0.5,0.5,0.5)
		_MainTex3 ("BlendTex3 (RGB)", 2D) = "white" {}
		_NormalTex3 ("NormalTex3 (RGB)", 2D) = "blue" {}
		
		_BlendColor4 ("Blend Color 4", Color) = (0.5,0.5,0.5,0.5)
		_MainTex4 ("BlendTex4 (RGB)", 2D) = "white" {}
		_NormalTex4 ("NormalTex4 (RGB)", 2D) = "blue" {}
		
		
		//_LightMapBright ("Bright (RGB)", 2D) = "white" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}

		_SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
		_Gloss("Gloss", Range (0.0, 2.0)) = 1.0
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	}

		SubShader {

		Tags { "Queue"="Geometry" "RenderType"="Opaque"  "LightMode" = "ForwardBase" }
		//Fog {Linear }	
		Pass {
		


			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			
			uniform half4 _Color;
			

			uniform half4 _BlendTex_ST;
			uniform sampler2D _BlendTex;
			

			uniform half4 _MainTex1_ST;
			uniform sampler2D _MainTex1;
			uniform half4 _BlendColor1;
			uniform sampler2D _NormalTex1;

			uniform half4 _MainTex2_ST;
			uniform sampler2D _MainTex2;
			uniform half4 _BlendColor2;
			uniform sampler2D _NormalTex2;

			uniform half4 _MainTex3_ST;
			uniform sampler2D _MainTex3;
			uniform half4 _BlendColor3;
			uniform sampler2D _NormalTex3;

			uniform half4 _MainTex4_ST;
			uniform sampler2D _MainTex4;
			uniform half4 _BlendColor4;
			uniform sampler2D _NormalTex4;

			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			uniform sampler2D _NormalTex;

			uniform half4 _LightMap_ST;
			uniform sampler2D _LightMap;

			uniform fixed4 _SpecularColor;
			uniform half _Gloss;
			uniform half _Shininess;
			
			struct V2In
			{
				half4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
				half3 normal :NORMAL;
			};
									
			struct v2f
			{
				half4 pos : POSITION;
				half2 uv_BlendTex : TEXCOORD0;
				half2 uv_MainTex : TEXCOORD1;
				half2 uv_LightMap : TEXCOORD2;
				half4 uv_MainTex1_2 : TEXCOORD3;
				half4 uv_MainTex3_4 : TEXCOORD4;
				half3 lightDir	: TEXCOORD5;
			  	half3 viewDir 	: TEXCOORD6;
				half3 normal :NORMAL;
			};
			
			inline float3 ObjSpaceLightDirLocal( in float4 v )
			{
				float3 objSpaceLightPos = normalize(mul(unity_WorldToObject, _WorldSpaceLightPos0).xyz);
				#ifndef USING_LIGHT_MULTI_COMPILE
					return objSpaceLightPos.xyz - v.xyz * _WorldSpaceLightPos0.w;
				#else
					#ifndef USING_DIRECTIONAL_LIGHT
					return objSpaceLightPos.xyz - v.xyz;
					#else
					return objSpaceLightPos.xyz;
					#endif
				#endif

				//return normalize(mul(_Object2World, _WorldSpaceLightPos0).xyz);

			}

			v2f vert (V2In v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				o.uv_BlendTex = TRANSFORM_TEX(v.texcoord,_BlendTex);

				o.uv_MainTex1_2.xy = TRANSFORM_TEX(v.texcoord,_MainTex1);
				o.uv_MainTex1_2.zw = TRANSFORM_TEX(v.texcoord,_MainTex2);
				o.uv_MainTex3_4.xy = TRANSFORM_TEX(v.texcoord,_MainTex3);
				o.uv_MainTex3_4.zw = TRANSFORM_TEX(v.texcoord,_MainTex4);

				o.uv_MainTex = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv_LightMap = TRANSFORM_TEX(v.texcoord1,_LightMap);

				o.lightDir = ObjSpaceLightDirLocal(v.vertex);

				o.viewDir = ObjSpaceViewDir(v.vertex);

				o.normal = v.normal;
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{		
				fixed4 BlendTex = (1 - tex2D(_BlendTex, i.uv_BlendTex.xy)) * fixed4(_BlendColor1.a,_BlendColor2.a,_BlendColor3.a,_BlendColor4.a);

				fixed4 MainTex1 = tex2D(_MainTex1, i.uv_MainTex1_2.xy) * _BlendColor1;
				fixed4 MainTex2 = tex2D(_MainTex2, i.uv_MainTex1_2.zw) * _BlendColor2;
				fixed4 MainTex3 = tex2D(_MainTex3, i.uv_MainTex3_4.xy) * _BlendColor3;
				fixed4 MainTex4 = tex2D(_MainTex4, i.uv_MainTex3_4.zw) * _BlendColor4;

				fixed4 MainTex = tex2D(_MainTex, i.uv_MainTex.xy) * _Color;
				fixed4 LightMap = tex2D(_LightMap, i.uv_LightMap.xy);

				fixed4 outColor = MainTex1 * BlendTex.r + MainTex * (1 - BlendTex.r);
				outColor = MainTex2 * BlendTex.g + outColor * (1 - BlendTex.g);
				outColor = MainTex3 * BlendTex.b + outColor * (1 - BlendTex.b);
				outColor = MainTex4 * BlendTex.a + outColor * (1 - BlendTex.a);

				//fixed4 outColor = MainTex1 * (1 - BlendTex.r) + MainTex * BlendTex.r;
				//outColor = MainTex2 * (1 - BlendTex.g) + outColor * BlendTex.g;
				//outColor = MainTex3 * (1 - BlendTex.b) + outColor * BlendTex.b;
				//outColor = MainTex4 * (1 - BlendTex.a) + outColor * BlendTex.a;

				outColor *= LightMap;

				half3 nomMain = tex2D(_NormalTex, i.uv_MainTex.xy).yzx;
				nomMain = nomMain * 2 - 1;

				half3 nom1 = tex2D(_NormalTex1, i.uv_MainTex1_2.xy).yzx;
				nom1 = nom1 * 2 - 1;

				half3 nom2 = tex2D(_NormalTex2, i.uv_MainTex1_2.zw).yzx;
				nom2 = nom2 * 2 - 1;

				half3 nom3 = tex2D(_NormalTex3, i.uv_MainTex3_4.xy).yzx;
				nom3 = nom3 * 2 - 1;

				half3 nom4 = tex2D(_NormalTex1, i.uv_MainTex3_4.zw).yzx;
				nom4 = nom4 * 2 - 1;

				half3 nom = nom1 * BlendTex.r + nomMain * (1 - BlendTex.r);
				nom = nom2 * BlendTex.g + nom * (1 - BlendTex.g);
				nom = nom3 * BlendTex.b + nom * (1 - BlendTex.b);
				nom = nom4 * BlendTex.a + nom * (1 - BlendTex.a);

				nom = normalize(nom);

				half3 L = i.lightDir;

				half diff = dot(nom, L);
				diff = diff * 0.5 + 0.5;

		  		half3 V = normalize(i.viewDir);
		  		half3 h = normalize(L + V);
		  		half nh = max(0, dot(nom, h));

		  		half spec = pow(nh, _Shininess * 128.0) * _Gloss;

		  		half4 fc = outColor;// + outColor * _LightColor0 * diff + _LightColor0 * spec * _SpecularColor;
		  		fc *= 2.0;

				//half4 fc = half4(L,1);//_LightColor0 * spec * _SpecularColor;


				return fc;
			}
			
			ENDCG
		}
		

	}	

}

