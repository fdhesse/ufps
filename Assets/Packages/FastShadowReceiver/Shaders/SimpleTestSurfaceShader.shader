Shader "FastShadowReceiver/SimpleTestSurfaceShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}		
		_TileParams("Tile Params",Vector)=(1,1,1,1)   
	}
	SubShader {
		Tags {"RenderType"="Opaque" }
		//ZWrite Off
		Offset -1, -1
		LOD 200
		//Blend Zero SrcColor	
		
			
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
			  //color = lerp(half4(1,1,1,1), color, color.a);
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
			
				//half4 c = tex2D(_MainTex,  IN.customUV);			
				//o.Albedo = c.rgb;
				//o.Alpha = c.a;			
				o.Albedo = fixed3(1,1,1);
				o.Alpha = 1.0;
			
				//o.Emission = IN.brightness;
				//o.Specular = IN.lightscale;			
			}

			half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) 
			{
				half NdotL = dot (s.Normal, lightDir); 
				half4 c;  
				c.rgb = s.Albedo * (_LightColor0.rgb * (NdotL * atten * 2) + s.Specular);			
				//c.rgb =  (_LightColor0.rgb) ;
				
				c.a = s.Alpha;
				//c.a = 1;
				return c; 
			}

			ENDCG
		}
	
}
