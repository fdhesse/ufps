Shader "FastShadowReceiver/Receiver/Shadowmap/Blend" {
	Properties {
		_ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent-500" }
		Pass {
			Tags { "LightMode"="ForwardBase" }
			Cull Back
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#define _IS_SHADOW_ENABLED (defined(SHADOWS_SCREEN) || defined(SHADOWS_DEPTH) || defined(SHADOWS_CUBE))

			struct appdata {
				float4 vertex   : POSITION;
			};
			struct v2f {
				float4 pos       : SV_POSITION;
				SHADOW_COORDS(0)
				UNITY_FOG_COORDS(1)
			};
			v2f vert(appdata v)
			{
				v2f o;
			#if _IS_SHADOW_ENABLED
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				TRANSFER_SHADOW(o);
			#else
				o.pos = float4(v.vertex.x,v.vertex.y,-1,-1);
			#endif
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}
			fixed4 _ShadowColor;
			fixed4 frag(v2f i) : COLOR
			{
			#if _IS_SHADOW_ENABLED
				fixed4 col = _ShadowColor;
				col.a *= (1.0f - SHADOW_ATTENUATION(i));
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			#else
				return 0;
			#endif
			}
			ENDCG
		}
		Pass {
			Tags { "LightMode" = "ShadowCollector" }
		
			Fog {Mode Off}
			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define SHADOW_COLLECTOR_PASS
			#include "UnityCG.cginc"
			struct appdata {
				float4 vertex   : POSITION;
			};
			struct v2f { 
				V2F_SHADOW_COLLECTOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				TRANSFER_SHADOW_COLLECTOR(o)
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				SHADOW_COLLECTOR_FRAGMENT(i)
			}
			ENDCG
		}
	} 
}
