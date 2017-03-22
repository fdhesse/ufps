Shader "FastShadowReceiver/DecalMul" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}		
		_TileParams("Tile Params",Vector)=(1,1,1,1)   
	}
	SubShader {
		Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		Offset -1, -1
		LOD 200
		Blend Zero SrcColor		
		
		CGPROGRAM
		#pragma surface surf SimpleLambert vertex:vert finalcolor:mycolor noforwardadd

		sampler2D _MainTex;		
		uniform float4	_TileParams;

		struct Input {
			float4 customUV;
			float4 tileParams;
			float  brightness;
			float  lightscale;
		};

		void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
		{
		//color.a = 1;
	      //color.rgb *= fixed3(IN.brightness, IN.brightness, IN.brightness);
		  
		  color = lerp(half4(1,1,1,1), color, color.a);
		}

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.customUV.xy = v.texcoord.xy;			
			o.customUV.zw = v.texcoord1.xy;
			o.tileParams.xy = v.texcoord2.xy;
			o.tileParams.zw = v.texcoord3.xy;
			o.brightness = v.color.r;
			o.lightscale = v.color.g * 100;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2Dproj(_MainTex, UNITY_PROJ_COORD(IN.customUV) );		
			
			half4 projUV = UNITY_PROJ_COORD(IN.customUV);
			half2 uv = projUV.xy / projUV.w;
			uv.x = uv.x * IN.tileParams.x +  IN.tileParams.x * IN.tileParams.z;
			uv.y = uv.y * IN.tileParams.y +  IN.tileParams.y * IN.tileParams.w;
			half4 c = tex2D(_MainTex,  uv);			

			//fixed alpha = tex2D(_FalloffTex, IN.customUV.zz).a;
			//o.Albedo = c.rgb;
			//o.Alpha = c.a * alpha;
			//fixed4 color = lerp(half4(1,1,1,1), c, c.a);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			//o.Emission = IN.brightness;
			
			o.Emission = IN.brightness;
			o.Specular = IN.lightscale;

			//o.Albedo *= fixed3(5, 5, 5);
		}

		half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) { //2
			half NdotL = dot (s.Normal, lightDir);  //3
			half4 c;   //4s
			c.rgb = s.Albedo * (_LightColor0.rgb * (NdotL * atten * 2) + s.Specular);
			//c.rgb += half3(0.5, 0.5, 0.5);
			//c.rgb = s.Albedo;
			c.a = s.Alpha; //6
			return c; //7
		}
		ENDCG
	} 
}
