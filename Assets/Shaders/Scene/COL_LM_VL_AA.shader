// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Triniti/Scene/COL_LM_VL_AA"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_LengthFactor ("Length Factor",Float) = 1
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
				
	// uniform float4x4 _CameraToWorld;
	uniform float _LengthFactor;
	
				
	half3 VertexLightsWorldSpace (half3 WP, half3 WN)
	{
		half3 lightColor = half3(0.0,0.0,0.0);

		// preface & optimization
		half3 toLight0 = mul(unity_CameraToWorld, unity_LightPosition[0] * half4(1,1,-1,1)).xyz - WP;
		
		half lengthSq = dot(toLight0, toLight0) * _LengthFactor;

		half atten = 1.0 + lengthSq * unity_LightAtten[0].z;
		
		atten = 1.0 / atten;
					
		// light #0
		half diff = saturate (dot (WN, normalize(toLight0)));
		lightColor += unity_LightColor[0].rgb * (diff * atten);

		return lightColor;
	}	
	
	ENDCG
		
	
	SubShader {
		LOD 190
		Lighting on
		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" "Queue"="Transparent" }	
		Blend  One One
		Blend DstColor One//, One SrcAlpha
		//BlendOp Add
		Offset -10,-10
		Pass {
		
	     
		     BindChannels 
			{
			   Bind "Vertex", vertex
			   Bind "texcoord", texcoord
			   Bind "Color", color
			} 
			
			FOG { Mode OFF }
		
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			uniform float4 _Color;
			
			struct V2In
			{
				half4 vertex : POSITION;
				half3 normal : NORMAL;
			};
									
			struct v2f
			{
				half4 pos : POSITION;
				half3 color : TEXCOORD0;
			};
			
			v2f vert (V2In v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				half3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				half3 worldNormal = mul((half3x3)unity_ObjectToWorld, v.normal.xyz);
				
				o.color = VertexLightsWorldSpace(worldPos, worldNormal);
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				//fixed4 tex = tex2D(_MainTex, i.uv.xy);
				fixed4 outColor = _Color;
				outColor.rgb += (i.color - float3(0.2f,0.2f,0.2f));
				//outColor.a = i.color.r - 0.5f;//1;//outColor.r - 0.7f;
				return outColor;
			}
			
			ENDCG
		}
		

	}	
}

