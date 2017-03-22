Shader "FastShadowReceiver/Projector/Multiply Diffuse" {
	Properties {
		_ShadowTex ("Cookie", 2D) = "gray" {}
		_FalloffTex ("FallOff", 2D) = "white" {}
		_Alpha ("Shadow Darkness", Range (0, 1)) = 1.0
		_Ambient ("Ambient", Range (0, 1)) = 0.3
		_Offset ("Offset", Range (-1, -10)) = -1.0
	}
	Subshader {
		Tags {"Queue"="Transparent-1"}
		Pass {
			ZWrite Off
			Fog { Color (1, 1, 1) }
			ColorMask RGB
			Blend DstColor Zero
			Offset -1, [_Offset]
 
			CGPROGRAM
			#pragma vertex fsr_vert_projector_diffuse
			#pragma fragment fsr_frag_projector_shadow_alpha_falloff
			#pragma multi_compile FSR_PROJECTOR FSR_RECEIVER
			#pragma multi_compile_fog
			#include "FastShadowReceiver.cginc"
			ENDCG
		}
	}
	Fallback "FastShadowReceiver/Projector/Multiply Diffuse Without Falloff"
}
