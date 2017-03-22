Shader "FastShadowReceiver/Demo/Terrain/SingleSplatDiffuse" {
	Properties {
		[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
	}
		
	SubShader {
		Tags {
			"SplatCount" = "4"
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}
		CGPROGRAM
		#pragma surface surf Lambert
		struct Input {
			float2 uv_Splat0 : TEXCOORD1;
		};

		sampler2D _Splat0;

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D (_Splat0, IN.uv_Splat0).rgb;
			o.Alpha = 0.0;
		}
		ENDCG  
	}

	Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Lightmap-AddPass"
	Dependency "BaseMapShader" = "Diffuse"
	Dependency "Details0"      = "Hidden/TerrainEngine/Details/Vertexlit"
	Dependency "Details1"      = "Hidden/TerrainEngine/Details/WavingDoublePass"
	Dependency "Details2"      = "Hidden/TerrainEngine/Details/BillboardWavingDoublePass"
	Dependency "Tree0"         = "Hidden/TerrainEngine/BillboardTree"

	Fallback "Diffuse"
}
