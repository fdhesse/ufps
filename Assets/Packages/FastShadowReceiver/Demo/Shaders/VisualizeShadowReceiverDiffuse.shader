Shader "FastShadowReceiver/Demo/VisualizeShadowReceiverDiffuse" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DebugColor ("Visualize Color", Color) = (1,0,1,0.5)   	
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;
		fixed4 _DebugColor;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
		#if defined(SHADOWS_SCREEN) || defined(SHADOWS_DEPTH) || defined(SHADOWS_CUBE)
			o.Albedo = lerp(c.rgb, _DebugColor.rgb, _DebugColor.a);
		#else
			o.Albedo = c.rgb;
		#endif
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
