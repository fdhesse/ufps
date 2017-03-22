// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

//
// FastShadowReceiver.cginc
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//

#if !defined(FAST_SHADOW_RECEIVER_CGINC_DEFINED)
#define FAST_SHADOW_RECEIVER_CGINCLUDE_CGINC_DEFINED

#include "UnityCG.cginc"

#if defined(FSR_PROJECTOR) || !defined(FSR_RECEIVER)
float4x4 unity_Projector;
float4x4 unity_ProjectorClip;
float4 fsrProjectVertex(float4 v)
{
	float4 p = mul (unity_Projector, v);
	p.z = mul (unity_ProjectorClip, v).x;
	return p;
}
float3 fsrProjectorDir()
{
	return normalize(float3(unity_Projector[2][0],unity_Projector[2][1], unity_Projector[2][2]));
}
#else
float4x4 _FSRProjector;
float4 _FSRProjectDir;
float4 fsrProjectVertex(float4 v)
{
	return mul (_FSRProjector, v);
}
float3 fsrProjectorDir()
{
	return _FSRProjectDir.xyz;
}
#endif

struct FSR_V2F_PROJECTOR {
	float4 uv        : TEXCOORD0;
	UNITY_FOG_COORDS(1)
	float4 pos       : SV_POSITION;
};

struct FSR_V2F_PROJECTOR_ALPHA {
	float4 uv        : TEXCOORD0;
	fixed  alpha     : TEXCOORD1;
	UNITY_FOG_COORDS(2)
	float4 pos       : SV_POSITION;
};

struct FSR_V2F_PROJECTOR_NEARCLIP {
	float4 uv        : TEXCOORD0;
	UNITY_FOG_COORDS(1)
	half   alpha     : COLOR;
	float4 pos       : SV_POSITION;
};

struct FSR_V2F_PROJECTOR_NEARCLIP_ALPHA {
	float4 uv        : TEXCOORD0;
	UNITY_FOG_COORDS(1)
	half2  alpha     : COLOR;
	float4 pos       : SV_POSITION;
};

uniform float _ClipScale;
uniform fixed _Alpha;
uniform fixed _Ambient;
uniform fixed4 _Color;
uniform sampler2D _ShadowTex;
uniform sampler2D _FalloffTex;

FSR_V2F_PROJECTOR fsr_vert_projector(float4 vertex : POSITION)
{
	FSR_V2F_PROJECTOR o;
	o.pos = mul(UNITY_MATRIX_MVP, vertex);
	o.uv  = fsrProjectVertex(vertex);
	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

FSR_V2F_PROJECTOR_ALPHA fsr_vert_projector_diffuse(float4 vertex : POSITION, float3 normal : NORMAL)
{
	FSR_V2F_PROJECTOR_ALPHA o;
	o.pos = mul(UNITY_MATRIX_MVP, vertex);
	o.uv  = fsrProjectVertex(vertex);
	fixed diffuse = -dot(normal, fsrProjectorDir());
	o.alpha = _Alpha * diffuse/(saturate(diffuse) + _Ambient);
	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

FSR_V2F_PROJECTOR_NEARCLIP fsr_vert_projector_nearclip(float4 vertex : POSITION)
{
	FSR_V2F_PROJECTOR_NEARCLIP o;
	o.pos = mul(UNITY_MATRIX_MVP, vertex);
	o.uv  = fsrProjectVertex(vertex);
	o.alpha = _ClipScale*o.uv.z;
	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

FSR_V2F_PROJECTOR_NEARCLIP_ALPHA fsr_vert_projector_diffuse_nearclip(float4 vertex : POSITION, float3 normal : NORMAL)
{
	FSR_V2F_PROJECTOR_NEARCLIP_ALPHA o;
	o.pos = mul(UNITY_MATRIX_MVP, vertex);
	o.uv  = fsrProjectVertex(vertex);
	o.alpha.x = _ClipScale*o.uv.z;
	half diffuse = -dot(normal, fsrProjectorDir());
	o.alpha.y = _Alpha * diffuse/(saturate(diffuse) + _Ambient);
	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

fixed4 fsr_frag_projector_shadow_falloff(FSR_V2F_PROJECTOR i) : COLOR
{
	fixed4 col;
	fixed alpha = tex2D(_FalloffTex, i.uv.zz).a;
	col.rgb = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uv)).rgb;
	col.a = 1.0f;
	col.rgb = lerp(fixed3(1,1,1), col.rgb, _Alpha*alpha);
	UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1));
	return col;
}

fixed4 fsr_frag_projector_shadow_alpha_falloff(FSR_V2F_PROJECTOR_ALPHA i) : COLOR
{
	fixed4 col;
	fixed alpha = tex2D(_FalloffTex, i.uv.zz).a;
	col.rgb = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uv)).rgb;
	col.a = 1.0f;
	col.rgb = lerp(fixed3(1,1,1), col.rgb, saturate(i.alpha*alpha));
	UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1));
	return col;
}

fixed4 fsr_frag_projector_shadow_nearclip(FSR_V2F_PROJECTOR_NEARCLIP i) : COLOR
{
	fixed4 col;
	col.rgb = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uv)).rgb;
	col.a = 1.0f;
	col.rgb = lerp(fixed3(1,1,1), col.rgb, _Alpha*saturate(i.alpha));
	UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1));
	return col;
}

fixed4 fsr_frag_projector_shadow_alpha_nearclip(FSR_V2F_PROJECTOR_NEARCLIP_ALPHA i) : COLOR
{
	fixed4 col;
	col.rgb = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uv)).rgb;
	col.a = 1.0f;
	col.rgb = lerp(fixed3(1,1,1), col.rgb, saturate(saturate(i.alpha.x)*i.alpha.y));
	UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1));
	return col;
}

fixed4 fsr_frag_projector_shadow_visualize_nearclip(FSR_V2F_PROJECTOR_NEARCLIP i) : COLOR
{
	fixed4 col = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uv));
	col = lerp(_Color, fixed4(0,0,0,1), saturate(i.alpha)*_Alpha*(1.0f - col));
	UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1));
	return col;
}


#endif // !defined(FAST_SHADOW_RECEIVER_CGINCLUDE_CGINC_DEFINED)
