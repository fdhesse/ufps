// Marmoset Skyshop
// Copyright 2013 Marmoset LLC
// http://marmoset.co

//WARNING: causes material property issues in OSX and anything pre 4.5
#define USE_PROPERTY_BLOCKS

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace mset {
	public class Sky : MonoBehaviour {
		[SerializeField]
		private Texture specularCube = null;
		public Texture SpecularCube {
			get{ return specularCube; }
			set{ specularCube = value; }
		}

		[SerializeField]
		private Texture skyboxCube = null;
		public Texture SkyboxCube {
			get{ return skyboxCube; }
			set{ skyboxCube = value; }
		}

		//helper flag for deciding which skies in the scene to re-probe
		public bool IsProbe = false;

		[SerializeField]
		private Bounds dimensions = new Bounds(Vector3.zero, Vector3.one); 

		//[NonSerialized]
		public Bounds Dimensions {
			get{ return dimensions; }
			set{ _dirty = true; dimensions = value; }
		}

		private bool _dirty = false;
		public bool Dirty {
			get{ return _dirty; }
			set{ _dirty = value; }
		}

		[SerializeField]
		private float masterIntensity = 1f;
		public float MasterIntensity
		{
			get{ return masterIntensity; }
			set{ _dirty = true; masterIntensity = value; }
		}
		[SerializeField]
		private float skyIntensity = 1f;
		public float SkyIntensity
		{
			get{ return skyIntensity; }
			set{ _dirty = true; skyIntensity = value; }
		}
		[SerializeField]
		private float specIntensity = 1f;
		public float SpecIntensity
		{
			get{ return specIntensity; }
			set{ _dirty = true; specIntensity = value; }
		}
		[SerializeField]
		private float diffIntensity = 1f;
		public float DiffIntensity
		{
			get{ return diffIntensity; }
			set{ _dirty = true; diffIntensity = value; }
		}
		[SerializeField]
		private float camExposure = 1f;
		public float CamExposure
		{
			get{ return camExposure; }
			set{ _dirty = true; camExposure = value; }
		}
		[SerializeField]
		private float specIntensityLM = 1f;
		public float SpecIntensityLM
		{
			get{ return specIntensityLM; }
			set{ _dirty = true; specIntensityLM = value; }
		}
		[SerializeField]
		private float diffIntensityLM = 1f;
		public float DiffIntensityLM
		{
			get{ return diffIntensityLM; }
			set{ _dirty = true; diffIntensityLM = value; }
		}
		[SerializeField]
		private bool hdrSky = true;
		public bool HDRSky
		{
			get{ return hdrSky; }
			set{ _dirty = true; hdrSky = value; }
		}
		[SerializeField]
		private bool hdrSpec = true;
		public bool HDRSpec
		{
			get{ return hdrSpec; }
			set{ _dirty = true; hdrSpec = value; }
		}

		[SerializeField]
		private bool linearSpace = true;
		public bool LinearSpace
		{
			get{ return linearSpace; }
			set{ _dirty = true; linearSpace = value; }
		}

		[SerializeField]
		private bool autoDetectColorSpace = true; //for inspector use
		public bool AutoDetectColorSpace
		{
			get{ return autoDetectColorSpace; }
			set{ _dirty = true; autoDetectColorSpace = value; }
		}

		[SerializeField]
		private bool hasDimensions = false;
		public bool HasDimensions
		{
			get{ return hasDimensions; }
			set{ _dirty = true; hasDimensions = value; }
		}

		public mset.SHEncoding SH = new mset.SHEncoding();
		public mset.SHEncodingFile CustomSH = null;

		private Matrix4x4 skyMatrix = Matrix4x4.identity;
		private Matrix4x4 invMatrix = Matrix4x4.identity;
		private Vector4 exposures = Vector4.one;
		private Vector4 exposuresLM = Vector4.one;

		private Vector4 skyMin = -Vector4.one;
		private Vector4 skyMax =  Vector4.one;

		//sets of shader parameters for each layer of blending
		private ShaderIDs[] blendIDs = { new ShaderIDs(), new ShaderIDs() };
		#if USE_PROPERTY_BLOCKS
		private static MaterialPropertyBlock propBlock = null;
		#endif

		[SerializeField]
		private Cubemap _blackCube;
		private Cubemap blackCube {
			get {
				if(_blackCube == null) {
					_blackCube = Resources.Load<Cubemap>("blackCube");
				}
				return _blackCube;
			}
		}

		[SerializeField]
		private Material _SkyboxMaterial = null;
		private Material SkyboxMaterial {
			get {
				if(_SkyboxMaterial == null) {
					_SkyboxMaterial = Resources.Load<Material>("skyboxMat");
				}
				return _SkyboxMaterial;
			}
		}

		//pulls the appropriate material reference from the renderer for sky binding
		private static Material[] getTargetMaterials(Renderer target) {
			mset.SkyAnchor anchor = target.gameObject.GetComponent<mset.SkyAnchor>();
			if(anchor != null) return anchor.materials;
			//shared materials are now used everywhere except SkyAnchor, or we're leaking memory
			return target.sharedMaterials;
		}

		// Public Interface //

		//Apply functions are shader-level bindings to variable and texture slots in the material system.
		//Calls to Apply are handled through the SkyManager and Applicator system now.

		//global apply
		public void Apply() { Apply(0); }
		public void Apply(int blendIndex) {
			ShaderIDs bids = this.blendIDs[blendIndex];	
			ApplyGlobally(bids);
		}
		//per-renderer apply
		public void Apply(Renderer target) { Apply(target, 0); }

		public void Apply(Renderer target, int blendIndex) {
			if(target && this.enabled && this.gameObject.activeInHierarchy) {
				ApplyFast(target, blendIndex);
			}
		}

		public void ApplyFast(Renderer target, int blendIndex) {
			// Binds IBL data, exposure, and a skybox texture globally or to a specific game object
			#if USE_PROPERTY_BLOCKS
				#if UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
					if(propBlock == null) propBlock = new MaterialPropertyBlock();
					target.GetPropertyBlock(propBlock);		
					ApplyToBlock(ref propBlock, this.blendIDs[blendIndex]);
					target.SetPropertyBlock(propBlock);
				#else
					if(propBlock == null) propBlock = new MaterialPropertyBlock();
					if(blendIndex == 0) {
						propBlock.Clear();
					} else {
						target.GetPropertyBlock(propBlock);						
					}
					ApplyToBlock(ref propBlock, this.blendIDs[blendIndex]);
					target.SetPropertyBlock(propBlock);
				#endif
			#else
				//SharedMaterials are now used everywhere except through SkyAnchor
				foreach(Material mat in target.sharedMaterials) {
					Apply(mat, blendIndex);
				}				
			#endif
		}
		//per-renderer apply
		public void Apply(Material target) { Apply(target, 0); }
		public void Apply(Material target, int blendIndex) {
			// Binds IBL data, exposure, and a skybox texture globally or to a specific game object
			if(target && this.enabled && this.gameObject.activeInHierarchy) {
				ApplyToMaterial(target, this.blendIDs[blendIndex]);
			}
		}

		private void ApplyToBlock(ref MaterialPropertyBlock block, ShaderIDs bids) {
		#if USE_PROPERTY_BLOCKS
			#if UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2

				block.SetVector(bids.exposureIBL,	exposures);
				block.SetVector(bids.exposureLM,	exposuresLM);
				block.SetMatrix(bids.skyMatrix,		skyMatrix);
				block.SetMatrix(bids.invSkyMatrix,	invMatrix);

				block.SetVector(bids.skyMin, skyMin);
				block.SetVector(bids.skyMax, skyMax);			

				if(specularCube) block.SetTexture(bids.specCubeIBL, specularCube);
				else 			 block.SetTexture(bids.specCubeIBL, blackCube);

				block.SetVector(bids.SH[0],	SH.cBuffer[0]);
				block.SetVector(bids.SH[1],	SH.cBuffer[1]);
				block.SetVector(bids.SH[2],	SH.cBuffer[2]);
				block.SetVector(bids.SH[3],	SH.cBuffer[3]);
				block.SetVector(bids.SH[4],	SH.cBuffer[4]);
				block.SetVector(bids.SH[5],	SH.cBuffer[5]);
				block.SetVector(bids.SH[6],	SH.cBuffer[6]);
				block.SetVector(bids.SH[7],	SH.cBuffer[7]);
				block.SetVector(bids.SH[8], SH.cBuffer[8]);

			#else 

				block.AddVector(bids.exposureIBL,	exposures);
				block.AddVector(bids.exposureLM,	exposuresLM);

				block.AddMatrix(bids.skyMatrix,		skyMatrix);
				block.AddMatrix(bids.invSkyMatrix,	invMatrix);

				block.AddVector(bids.skyMin, skyMin);
				block.AddVector(bids.skyMax, skyMax);			

				if(specularCube) block.AddTexture(bids.specCubeIBL, specularCube);
				else 			 block.AddTexture(bids.specCubeIBL, blackCube);

				block.AddVector(bids.SH[0],	SH.cBuffer[0]);
				block.AddVector(bids.SH[1],	SH.cBuffer[1]);
				block.AddVector(bids.SH[2],	SH.cBuffer[2]);
				block.AddVector(bids.SH[3],	SH.cBuffer[3]);
				block.AddVector(bids.SH[4],	SH.cBuffer[4]);
				block.AddVector(bids.SH[5],	SH.cBuffer[5]);
				block.AddVector(bids.SH[6],	SH.cBuffer[6]);
				block.AddVector(bids.SH[7],	SH.cBuffer[7]);
				block.AddVector(bids.SH[8], SH.cBuffer[8]);

			#endif
		#endif
		}

		private void ApplyToMaterial(Material mat, ShaderIDs bids) {
			mat.SetVector(bids.exposureIBL,	this.exposures);
			mat.SetVector(bids.exposureLM,	this.exposuresLM);			
			mat.SetMatrix(bids.skyMatrix,		this.skyMatrix);
			mat.SetMatrix(bids.invSkyMatrix,	this.invMatrix);

			mat.SetVector(bids.skyMin, skyMin);
			mat.SetVector(bids.skyMax, skyMax);
					
			if(specularCube) mat.SetTexture(bids.specCubeIBL,	this.specularCube);
			else 			 mat.SetTexture(bids.specCubeIBL,	this.blackCube);
			if(skyboxCube)	 mat.SetTexture(bids.skyCubeIBL, 	this.skyboxCube);
			for(int i=0; i<9; ++i) {
				mat.SetVector(bids.SH[i], this.SH.cBuffer[i]);
			}
		}

		private void ApplySkyTransform(ShaderIDs bids) {
			Shader.SetGlobalMatrix(bids.skyMatrix, skyMatrix);
			Shader.SetGlobalMatrix(bids.invSkyMatrix, invMatrix);

			Shader.SetGlobalVector(bids.skyMin, skyMin);
			Shader.SetGlobalVector(bids.skyMax, skyMax);
		}
		private void ApplyGlobally(ShaderIDs bids) {
			//Transform
			Shader.SetGlobalMatrix(bids.skyMatrix, skyMatrix);
			Shader.SetGlobalMatrix(bids.invSkyMatrix, invMatrix);

			Shader.SetGlobalVector(bids.skyMin, skyMin);
			Shader.SetGlobalVector(bids.skyMax, skyMax);

			//Exposures
			Shader.SetGlobalVector(bids.exposureIBL, exposures);
			Shader.SetGlobalVector(bids.exposureLM, exposuresLM);			
			Shader.SetGlobalFloat("_EmissionLM", 1f); 					//a hint for the Beast Lightmapper, rendering is unaffected
			Shader.SetGlobalVector("_UniformOcclusion", Vector4.one); 	//set only on individual renderers and outside the scope of the sky system
			
			//IBL
			if(specularCube)Shader.SetGlobalTexture(bids.specCubeIBL, specularCube);
			else 			Shader.SetGlobalTexture(bids.specCubeIBL, blackCube);
			if(skyboxCube)	Shader.SetGlobalTexture(bids.skyCubeIBL, skyboxCube);
			//bind spherical harmonics
			for(int i=0; i<9; ++i) { Shader.SetGlobalVector(bids.SH[i], this.SH.cBuffer[i]); }
		}

		
		public static void ScrubGlobalKeywords() {
			Shader.DisableKeyword("MARMO_SKY_BLEND_ON");
			Shader.DisableKeyword( "MARMO_SKY_BLEND_OFF");

			Shader.DisableKeyword("MARMO_BOX_PROJECTION_ON");
			Shader.DisableKeyword( "MARMO_BOX_PROJECTION_OFF");
			
			Shader.DisableKeyword("MARMO_TERRAIN_BLEND_ON");
			Shader.DisableKeyword("MARMO_TERRAIN_BLEND_OFF");
		}
		
		public static void ScrubKeywords(Material[] materials) {
			foreach( Material mat in materials) {
				//SpeedTree likes to keep null materials around
				if(mat != null) {
					mat.DisableKeyword("MARMO_SKY_BLEND_ON");
					mat.DisableKeyword("MARMO_SKY_BLEND_OFF");
					
					mat.DisableKeyword("MARMO_BOX_PROJECTION_ON");
					mat.DisableKeyword("MARMO_BOX_PROJECTION_OFF");

					mat.DisableKeyword("MARMO_TERRAIN_BLEND_ON");
					mat.DisableKeyword("MARMO_TERRAIN_BLEND_OFF");
				}
			}
		}

		//////////////////
		/// PROJECTION ///
		//////////////////

		//Support
		//NOTE: This toggles support for all box projection, overriding material and renderer settings
		private static bool internalProjectionSupport = false;
		public static void EnableProjectionSupport(bool enable) {
			//Support deals only with _OFF keywords, which will override local and global _ON keywords
			if(enable)	Shader.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
			else 		Shader.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
			internalProjectionSupport = enable;
		}

		public static void EnableGlobalProjection(bool enable) {
			if(!internalProjectionSupport) return;
			//globally, we can only turn projection on, changing OFF would override local settings
			if(enable) 	Shader.EnableKeyword("MARMO_BOX_PROJECTION_ON");
			else 		Shader.DisableKeyword("MARMO_BOX_PROJECTION_ON");
		}

		//renderer
		public static void EnableProjection(Renderer target, Material[] mats, bool enable) {
			if(!internalProjectionSupport) return;
			if(mats == null) return;
			if(enable) {
				foreach(Material mat in mats) {
					if(mat) {
						mat.EnableKeyword("MARMO_BOX_PROJECTION_ON");
						mat.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
					}
				}
			} else {
				foreach(Material mat in mats) {
					if(mat) {
						mat.DisableKeyword("MARMO_BOX_PROJECTION_ON");
						mat.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
					}
				}
			}
		}

		//material
		public static void EnableProjection(Material mat, bool enable) {
			if(!internalProjectionSupport) return;
			if(enable) {
				mat.EnableKeyword("MARMO_BOX_PROJECTION_ON");
				mat.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
			} else {
				mat.DisableKeyword("MARMO_BOX_PROJECTION_ON");
				mat.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
			}
		}

		////////////////
		/// BLENDING ///
		////////////////

		//Support
		private static bool internalBlendingSupport = false;
		public static void EnableBlendingSupport(bool enable) {
			//Support deals only with _OFF keywords, which will override local and global _ON keywords
			if(enable) 	Shader.DisableKeyword( "MARMO_SKY_BLEND_OFF");
			else 		Shader.EnableKeyword( "MARMO_SKY_BLEND_OFF");
			internalBlendingSupport = enable;
		}		

		//global
		//Terrain blending cannot be enabled per material so it's only ever done globally.
		//because global keywords override local ones, terrain needs its own keyword.
		public static void EnableTerrainBlending(bool enable) {
			if(!internalBlendingSupport) return;
			if(enable) {
				Shader.EnableKeyword( "MARMO_TERRAIN_BLEND_ON");
				Shader.DisableKeyword("MARMO_TERRAIN_BLEND_OFF");
			} else {
				Shader.DisableKeyword("MARMO_TERRAIN_BLEND_ON");
				Shader.EnableKeyword("MARMO_TERRAIN_BLEND_OFF");
			}
		}

		public static void EnableGlobalBlending(bool enable) {
			if(!internalBlendingSupport) return;
			//globally, we can only turn sky blending on, messing with OFF would override local settings
			if(enable)	Shader.EnableKeyword("MARMO_SKY_BLEND_ON");
			else 		Shader.DisableKeyword("MARMO_SKY_BLEND_ON");
		}

		//renderer
		public static void EnableBlending(Renderer target, Material[] mats, bool enable) {
			//NOTE: renderer and material enabling fragments materials! These should be in PropertyBlocks but they're not
			if(!internalBlendingSupport) return;
			if(mats == null) return;
			if(enable) {
				foreach(Material mat in mats) {
					if(mat) {
						//locally we want to control every aspect of the keyword, including off
						mat.EnableKeyword("MARMO_SKY_BLEND_ON");
						mat.DisableKeyword("MARMO_SKY_BLEND_OFF");
					}
				}
			} else {
				foreach(Material mat in mats) {
					if(mat) {
						mat.DisableKeyword("MARMO_SKY_BLEND_ON");
						mat.EnableKeyword("MARMO_SKY_BLEND_OFF");
					}
				}
			}
		}

		//material
		public static void EnableBlending(Material mat, bool enable) {
			if(!internalBlendingSupport) return;
			if(enable) {
				mat.EnableKeyword("MARMO_SKY_BLEND_ON");
				mat.DisableKeyword("MARMO_SKY_BLEND_OFF");
			} else {
				mat.DisableKeyword("MARMO_SKY_BLEND_ON");
				mat.EnableKeyword("MARMO_SKY_BLEND_OFF");
			}
		}

		/////////////////////
		/// BLEND WEIGHTS ///
		/////////////////////

		//global
		public static void SetBlendWeight(float weight) {
			Shader.SetGlobalFloat("_BlendWeightIBL", weight);
		}

		//renderer
		public static void SetBlendWeight(Renderer target, float weight) {
		#if USE_PROPERTY_BLOCKS
			#if UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
				if( propBlock == null ) propBlock = new MaterialPropertyBlock();
				target.GetPropertyBlock(propBlock);
				propBlock.SetFloat("_BlendWeightIBL", weight);
				target.SetPropertyBlock(propBlock);
			#else
				if( propBlock == null ) propBlock = new MaterialPropertyBlock();
				else propBlock.Clear();

				//NOTE: this expects the property block to be cleared prior to being called or the weight property will accumulate every frame!
				//MaterialPropertyBlock block = new MaterialPropertyBlock();
				target.GetPropertyBlock(propBlock);
				propBlock.AddFloat("_BlendWeightIBL", weight);
				target.SetPropertyBlock(propBlock);
			#endif
		#else
			Material[] mats = getTargetMaterials(target);
			foreach(Material mat in mats) {
				mat.SetFloat("_BlendWeightIBL", weight);
			}
		#endif
		}

		//material
		public static void SetBlendWeight(Material mat, float weight) {
			mat.SetFloat("_BlendWeightIBL", weight);
		}

		//NOTE: this fragments materials, should be in a property block but those are dangerous
		public static void SetUniformOcclusion(Renderer target, float diffuse, float specular) {
			if(target != null) {
				//Sets a custom multiplier on the diffuse and specular intensities from the active Sky.			
				Vector4 occlusion = Vector4.one;
				occlusion.x = diffuse;
				occlusion.y = specular;
				Material[] mats = getTargetMaterials(target);
				foreach(Material mat in mats) {
					mat.SetVector("_UniformOcclusion", occlusion);
				}
			}
		}

		public void SetCustomExposure(float diffInt, float specInt, float skyInt, float camExpo) { SetCustomExposure(null, diffInt, specInt, skyInt, camExpo); }
		public void SetCustomExposure(Renderer target, float diffInt, float specInt, float skyInt, float camExpo) {
			Vector4 expo = Vector4.one;
			ComputeExposureVector(ref expo, diffInt, specInt, skyInt, camExpo);
			if( target == null ) {
				Shader.SetGlobalVector(this.blendIDs[0].exposureIBL, expo);
			} else {
				Material[] mats = getTargetMaterials(target);
				foreach(Material mat in mats) {
					mat.SetVector(this.blendIDs[0].exposureIBL, expo);
				}
			}
		}

		public void ToggleChildLights(bool enable) {
			//Enable/disable all lights that are child objects of this Sky
			//NOTE: this causes scene changes on sky selection, may not be desireable in the editor!
			Light[] lights = this.GetComponentsInChildren<Light>();
			for(int i = 0; i < lights.Length; ++i) {
				lights[i].enabled = enable;
			}
		}

		private void UpdateSkySize() {
			if(this.HasDimensions) {
				skyMin = this.Dimensions.center - this.Dimensions.extents;
				skyMax = this.Dimensions.center + this.Dimensions.extents;
				Vector3 scale = this.transform.localScale;
				skyMin.x *= scale.x;	skyMin.y *= scale.y;	skyMin.z *= scale.z;
				skyMax.x *= scale.x;	skyMax.y *= scale.y;	skyMax.z *= scale.z;
			} else {
				skyMax = Vector4.one * 100000f;
				skyMin = Vector4.one *-100000f;
			}
		}

		private void UpdateSkyTransform() {
			skyMatrix.SetTRS(transform.position, transform.rotation, Vector3.one);
			invMatrix = skyMatrix.inverse;
		}

		private void ComputeExposureVector(ref Vector4 result, float diffInt, float specInt, float skyInt, float camExpo) {
			//build exposure values for shader, HDR skies need the RGBM expansion constant 6.0 in there
			result.x = masterIntensity * diffInt;
			result.y = masterIntensity * specInt;
			result.z = masterIntensity * skyInt * camExpo; //exposure baked right into skybox exposure
			result.w = camExpo;

			float ldr2hdr = 6f;
			if(linearSpace) ldr2hdr = Mathf.Pow(ldr2hdr, 2.2f);
			if(!hdrSpec) result.y /= ldr2hdr;
			if(!hdrSky)  result.z /= ldr2hdr;
		}

		private void UpdateExposures() {
			ComputeExposureVector(ref exposures, diffIntensity, specIntensity, skyIntensity, camExposure);
			exposuresLM.x = diffIntensityLM;
			exposuresLM.y = specIntensityLM;
		}

		// Link shader property IDs to the shader system, call any time these are likely to change (game start, and pretty much whenever in the editor) 
		private void UpdatePropertyIDs() {
			blendIDs[0].Link();
			blendIDs[1].Link("1");
		}


		// Run-Time //		
		// Constructor
		public void Awake() {
			UpdatePropertyIDs();
			#if USE_PROPERTY_BLOCKS
			propBlock = new MaterialPropertyBlock();
			#endif
		}

		private void Reset() {
			skyMatrix = invMatrix = Matrix4x4.identity;
			exposures = Vector4.one;
			exposuresLM = Vector4.one;

			specularCube = skyboxCube = null;
			masterIntensity = skyIntensity = specIntensity = diffIntensity = 1f;
			hdrSky = hdrSpec = false;
		}

		//on enable or activate
		private void OnEnable() {
			//finalize or allocate serialized properties here
			if(SH == null) SH = new mset.SHEncoding();
			if(this.CustomSH != null) SH.copyFrom(this.CustomSH.SH);
			SH.copyToBuffer();
			SceneManager.sceneLoaded += SceneStart;
		}

		private void OnDisable() {			
			SceneManager.sceneLoaded -= SceneStart;
		}

		private void SceneStart(Scene scene, LoadSceneMode mode)
		{
			UpdateExposures();
			UpdateSkyTransform();
			UpdateSkySize();
		}

		private void Start() {
			UpdateExposures();
			UpdateSkyTransform();
			UpdateSkySize();

#if UNITY_ANDROID
			if(!Application.isEditor) {
				// on mobile devices all gloss levels are discarded
				Cubemap cube = this.specularCube as Cubemap;
				if( cube ) cube.Apply(true);
			}
#endif
		}

		private void Update() {
			if(transform.hasChanged) {
				Dirty = true;
				UpdateSkyTransform();
				UpdateSkySize(); //this shouldn't change at runtime, skies are supposed to be static
				transform.hasChanged = false;
			}
			UpdateExposures();
		}

#if UNITY_EDITOR
		public void EditorStart() {
			if(!this.blendIDs[0].valid || !this.blendIDs[1].valid) {
				this.UpdatePropertyIDs();
			}
		}

		public void EditorUpdate() {
			UpdateSkyTransform();
			UpdateExposures();
			UpdateSkySize();
			if(specularCube && specularCube.filterMode != FilterMode.Trilinear) {
				specularCube.filterMode = FilterMode.Trilinear;
			}
		}
#endif
		//script instance is destroyed
		private void OnDestroy() {
			SH = null;
			_blackCube = null;
			specularCube = null;
			skyboxCube = null;
		}

		// Editor Functions //
		private Material projMaterial = null;
		public void DrawProjectionCube(Vector3 center, Vector3 radius) {
			if(projMaterial == null ) {
				projMaterial = Resources.Load<Material>("projectionMat");
				if(!projMaterial) Debug.LogError("Failed to find projectionMat material in Resources folder!");
			}

			Vector4 exposureIBL = Vector4.one;
			exposureIBL.z = this.CamExposure;
			exposureIBL *= this.masterIntensity;

			ShaderIDs bids = this.blendIDs[0];

			projMaterial.color = new Color(0.7f,0.7f,0.7f,1f);
			projMaterial.SetVector(bids.skyMin, -this.Dimensions.extents);
			projMaterial.SetVector(bids.skyMax, this.Dimensions.extents);
			projMaterial.SetVector(bids.exposureIBL, exposureIBL);
			projMaterial.SetTexture(bids.skyCubeIBL, this.specularCube);
			projMaterial.SetMatrix(bids.skyMatrix, skyMatrix);
			projMaterial.SetMatrix(bids.invSkyMatrix, invMatrix);
			projMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(this.transform.localToWorldMatrix);

			mset.GLUtil.DrawCube(center, -radius);

			GL.End();
			GL.PopMatrix();
		}

	#if UNITY_EDITOR
		private static bool SkyManagerCheck = true;
		private void OnDrawGizmos() {
			//The most reliable place to bind shader variables in the editor

			if(SkyManagerCheck && !Application.isPlaying) {
				SkyManagerCheck = false;
				mset.SkyManager mgr = mset.SkyManager.Get();
				if(mgr == null) {
					string title = "Missing SkyManager";
					string text = "An object with a Sky Manager component is required for Skyshop to function properly.\n\nWould you like to create one?";
					if(UnityEditor.EditorUtility.DisplayDialog(title, text, "Create", "Ignore")) {
						GameObject go = new GameObject("Sky Manager");
						go.AddComponent<mset.SkyManager>();
						go.transform.position = this.transform.position + Vector3.up * 10f;
					}
				}
			}

			if(!this.blendIDs[0].valid || !this.blendIDs[1].valid) {
				this.UpdatePropertyIDs();
			}

			Gizmos.DrawIcon(transform.position, "cubelight.tga", true);
			if(this.HasDimensions) {
				Color c = new Color(0.4f, 0.7f, 1f, 0.333f);
				Gizmos.color = c;

				Matrix4x4 mat = new Matrix4x4();
				mat.SetTRS(this.transform.position, this.transform.rotation, this.transform.localScale);
				Gizmos.matrix = mat;
				Gizmos.DrawWireCube(dimensions.center, dimensions.size);
			}
		}

		private void OnDrawGizmosSelected() {
			if(this.HasDimensions) {
				Color c = new Color(0.4f, 0.7f, 1f, 0.5f);
				Gizmos.color = c;

				Matrix4x4 mat = this.transform.localToWorldMatrix;
				Gizmos.matrix = mat;

				if(UnityEditor.Selection.activeGameObject == this.gameObject) {
					Gizmos.DrawCube(dimensions.center, -1f*dimensions.size);
					Vector3 dim_with_scale = new Vector3(dimensions.extents.x * transform.localScale.x, dimensions.extents.y * transform.localScale.y, dimensions.extents.z * transform.localScale.z);
					DrawProjectionCube(dimensions.center, dim_with_scale);
				}
				c.a = 1f;
				Gizmos.DrawWireCube(dimensions.center, dimensions.size);
			}
		}
	#endif

		//deprecated: use SkyApplicator to apply skies in trigger volumes
		private void OnTriggerEnter(Collider other) {
			if(other.GetComponent<Renderer>()) {
				this.Apply(other.GetComponent<Renderer>(), 0);
			}
		}

		private void OnPostRender() {
		}
	}
}