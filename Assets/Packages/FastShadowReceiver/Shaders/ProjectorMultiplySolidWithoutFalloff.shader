Shader "FastShadowReceiver/Projector/Multiply Solid Without Falloff" {
	Properties {
		_ShadowTex ("Cookie", 2D) = "gray" {}
		_ClipScale ("Near Clip Sharpness", Float) = 100
		_Alpha ("Shadow Darkness", Range (0, 1)) = 1.0
		_Offset ("Offset", Range (-1, -10)) = -1.0
		_AlphaTest ("Alpha Test", Range (0, 1)) = 0.5
	}
	Subshader {
		Tags {"Queue"="Transparent-1"}
		Pass {
			ZWrite Off
			Fog { Color (1, 1, 1) }
			ColorMask RGB
			Blend DstColor Zero
			Offset -1, [_Offset]
			Stencil {
				Ref 1
				Comp NotEqual
				Pass Replace
				ZFail Keep
			}
 
			CGPROGRAM
			#pragma vertex fsr_vert_projector_nearclip
			#pragma fragment fsr_frag_projector_shadow_nearclip_alphatest
			#pragma multi_compile FSR_PROJECTOR FSR_RECEIVER
			#pragma multi_compile_fog
			#include "FastShadowReceiver.cginc"

			fixed _AlphaTest;

			fixed4 fsr_frag_projector_shadow_nearclip_alphatest(FSR_V2F_PROJECTOR_NEARCLIP i) : COLOR
			{
				fixed4 col;
				col = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uv));
				clip(col.a - _AlphaTest);
				col.a = 1.0f;
				col.rgb = lerp(fixed3(1,1,1), col.rgb, _Alpha*saturate(i.alpha));
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1));
				return col;
			}
			ENDCG
		}
	}
}
