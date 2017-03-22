Shader "FastShadowReceiver/Receiver/Projector/Invisble" {
	SubShader {
		Tags { "RenderType"="Invisible" }
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			struct appdata {
				float4 vertex   : POSITION;
			};
			struct v2f {
				float4 pos      : SV_POSITION;
			};
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = float4(v.vertex.x,v.vertex.y,-1,-1);
				return o;
			}
			fixed4 frag(v2f i) : COLOR
			{
				return 0;
			}
			ENDCG
		}
	} 
}
