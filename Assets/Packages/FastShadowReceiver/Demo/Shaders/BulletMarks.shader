Shader "FastShadowReceiver/Demo/BulletMarks" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FalloffTex ("FallOff", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		Offset -1, -1
		LOD 200		
		
		CGPROGRAM
		#pragma surface surf Lambert decal:blend vertex:vert

		sampler2D _MainTex;
		sampler2D _FalloffTex;

		struct Input {
			float4 customUV;
		};

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.customUV.xy = v.texcoord.xy;
			o.customUV.zw = v.texcoord1.xy;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2Dproj(_MainTex, UNITY_PROJ_COORD(IN.customUV));
			fixed alpha = tex2D(_FalloffTex, IN.customUV.zz).a;
			o.Albedo = c.rgb;
			o.Alpha = c.a * alpha;
		}
		ENDCG
	} 
}
