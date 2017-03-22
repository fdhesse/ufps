Shader "FastShadowReceiver/Receiver/Shadowmap/VisualizeReceiver" {
	Properties {
		_ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)
		_DebugColor ("Visualize Color", Color) = (1,0,1,0.5)   	
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
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				TRANSFER_SHADOW(o);
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}
			fixed4 _ShadowColor;
			fixed4 _DebugColor;
			fixed4 frag(v2f i) : COLOR
			{
			#if _IS_SHADOW_ENABLED
				fixed4 col = lerp(_ShadowColor, _DebugColor, SHADOW_ATTENUATION(i));
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1));
				return col;
			#else
				return _DebugColor;
			#endif
			}
			ENDCG
		}
	} 
	Fallback "FastShadowReceiver/BlendShadowMap"
}
