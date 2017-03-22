Shader "FastShadowReceiver/Projector/Visualize Receiver" {
	Properties {
		_Color ("Main Color", Color) = (1,0,1,0.5)   	
		_ShadowTex ("Cookie", 2D) = "gray" {}
		_ClipScale ("Near Clip Scale", Float) = 100
		_Alpha ("Shadow Darkness", Range (0, 1)) = 1.0
	}

	Subshader {
		Tags { "Queue"="Transparent-1" }
		Pass {
			ZWrite Off
			Fog { Color (1, 1, 1) }
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1
 
			CGPROGRAM
			#pragma vertex fsr_vert_projector_nearclip
			#pragma fragment fsr_frag_projector_shadow_visualize_nearclip
			#pragma multi_compile FSR_PROJECTOR FSR_RECEIVER
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			#include "FastShadowReceiver.cginc"
			ENDCG
		}
	}
}
