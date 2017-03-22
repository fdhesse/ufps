// Marmoset Skyshop
// Copyright 2014 Marmoset LLC
// http://marmoset.co

//WARNING: causes material property issues in OSX and anything pre 4.5
#if !UNITY_STANDALONE_OSX
#define USE_PROPERTY_BLOCKS
#endif

using UnityEngine;
using System.Collections;

using System;

namespace mset {
	public class SkyAnchor : MonoBehaviour {
		public enum AnchorBindType {Center, Offset, TargetTransform, TargetSky};
		public AnchorBindType BindType = AnchorBindType.Center;

		public Transform	AnchorTransform = null;
		public Vector3		AnchorOffset = Vector3.zero;
		public mset.Sky		AnchorSky = null;
		public Vector3		CachedCenter = Vector3.zero;

		public mset.SkyApplicator	CurrentApplicator = null;
		public mset.Sky 			CurrentSky { 
			get { return Blender.CurrentSky; } 
		}
		public mset.Sky				PreviousSky {
			get { return Blender.PreviousSky; }
		}

		public float BlendTime {
			get { return Blender.BlendTime; }
			set { Blender.BlendTime = value; }
		}

		private bool isStatic = false;
		public bool IsStatic {
			get { return isStatic; }
		}

		//true if this anchor is assigned a local sky and should be calling Blender.Apply()
		//false if using the global sky and blender in SkyManager
		public bool HasLocalSky = false;

		//true if gameObject has moved or needs to research applicators
		public bool HasChanged = true;

		[SerializeField]
		private mset.SkyBlender Blender = new mset.SkyBlender();
		private Vector3 LastPosition = Vector3.zero;

		[NonSerialized]
		public Material[] materials = null;

		// Use this for initialization
		void Start() {
			if(BindType != AnchorBindType.TargetSky) {
				#if USE_PROPERTY_BLOCKS
				//HACK: clear the property block for this renderer, good catch-all for old data
				GetComponent<Renderer>().SetPropertyBlock(new MaterialPropertyBlock());
				#endif

				//instantly register and hook up skies to this anchor on creation
				mset.SkyManager skymgr = mset.SkyManager.Get();
				skymgr.RegisterNewRenderer(GetComponent<Renderer>());
				skymgr.ApplyCorrectSky(GetComponent<Renderer>());
				BlendTime = skymgr.LocalBlendTime;
				if(Blender.CurrentSky)	Blender.SnapToSky(Blender.CurrentSky);
				else 					Blender.SnapToSky(skymgr.GlobalSky);

			}
			//instance and keep around the list of materials in this renderer
			materials = GetComponent<Renderer>().materials;
			LastPosition = transform.position;
			HasChanged = true;
		}

		private bool firstFrame = false;
		void OnEnable() {
			isStatic = this.gameObject.isStatic;
			ComputeCenter(ref CachedCenter);
			firstFrame = true;
		}

		private void LateUpdate() {
			//direct link to a sky
			if(BindType == AnchorBindType.TargetSky) {
				HasChanged = AnchorSky != Blender.CurrentSky;
				if(AnchorSky != null) {
					CachedCenter = AnchorSky.transform.position;
				}
			}
			// use a third-party transform for anchor checks
			else if(BindType == AnchorBindType.TargetTransform) {
				if(AnchorTransform) {
					if(AnchorTransform.position.x != LastPosition.x ||
					   AnchorTransform.position.y != LastPosition.y ||
					   AnchorTransform.position.z != LastPosition.z) {
						HasChanged = true;
						LastPosition = AnchorTransform.position;
						CachedCenter.x = LastPosition.x;
						CachedCenter.y = LastPosition.y;
						CachedCenter.z = LastPosition.z;
					}
				}
			}
			else if(!isStatic) {
				if(LastPosition.x != transform.position.x ||
				   LastPosition.y != transform.position.y ||
				   LastPosition.z != transform.position.z) {
					HasChanged = true;
					LastPosition = transform.position;
					ComputeCenter(ref CachedCenter);
				}
			} else {
				HasChanged = false;
			}

			HasChanged |= firstFrame;
			firstFrame = false;

			//add a final blended apply to the frame after blending is complete
			bool isBlending = Blender.IsBlending || Blender.WasBlending(Time.deltaTime);

			//if we're still blending locally for whatever reason, apply
			if( isBlending ) {
				Apply();
			} 
			//if this is an explicitly targeted sky, apply when necessary
			else if(BindType == AnchorBindType.TargetSky) {
				if(HasChanged || Blender.CurrentSky.Dirty) Apply();
			}
			//if this is local-to-local or local-to-global blending apply it here.
			//global-to-global blending is handled by SkyManager
			else if(HasLocalSky) {
				if(HasChanged || Blender.CurrentSky.Dirty)
					Apply();
			}
		}

		//Call this whenever the material list on a renderer has changed (added materials, changed material references, etc).
		public void UpdateMaterials() {
			materials = GetComponent<Renderer>().materials;
		}

		//Called by Destroy, cleaning up all the instanced material assets Unity has created
		public void CleanUpMaterials() {
			if(materials != null) {
				foreach (Material mat in materials) {
					Destroy(mat);
				}
				materials = new Material[] {};
			}
		}

		public void SnapToSky(mset.Sky nusky) {
			if(nusky == null) return;
			if(BindType == AnchorBindType.TargetSky) return;			
			Blender.SnapToSky(nusky);
			HasLocalSky = true;
		}
		public void BlendToSky(mset.Sky nusky) {
			if(nusky == null) return;
			//ignore if swaps if we are glued to a specific sky
			if(BindType == AnchorBindType.TargetSky) return;
			Blender.BlendToSky(nusky);
			HasLocalSky = true;
		}

		public void SnapToGlobalSky(mset.Sky nusky) {
			SnapToSky(nusky);
			HasLocalSky = false;
		}
		public void BlendToGlobalSky(mset.Sky nusky) {
			if(HasLocalSky) BlendToSky(nusky);
			HasLocalSky = false;
		}

		public void Apply() {
			if(BindType == AnchorBindType.TargetSky) {
				//we don't want to check for null skies every frame for every object but for
				//targeted skies, we do a global sky backup here
				if(AnchorSky)	Blender.SnapToSky(AnchorSky);
				else			Blender.SnapToSky(SkyManager.Get().GlobalSky);			
			}
			Blender.Apply(GetComponent<Renderer>(), materials);
		}

		//Center is cached in Anchors of static objects
		public void GetCenter(ref Vector3 _center) {
			_center.x = CachedCenter.x;
			_center.y = CachedCenter.y;
			_center.z = CachedCenter.z;
		}

		private void ComputeCenter(ref Vector3 _center) {
			_center.x = transform.position.x;
			_center.y = transform.position.y;
			_center.z = transform.position.z;

			switch(BindType) {
			case AnchorBindType.TargetTransform:
				if(AnchorTransform) {
					_center.x = AnchorTransform.position.x;
					_center.y = AnchorTransform.position.y;
					_center.z = AnchorTransform.position.z;
				}
				break;
			case AnchorBindType.Center:
				_center.x = GetComponent<Renderer>().bounds.center.x;
				_center.y = GetComponent<Renderer>().bounds.center.y;
				_center.z = GetComponent<Renderer>().bounds.center.z;
				break;
			case AnchorBindType.Offset:
				Vector3 p = transform.localToWorldMatrix.MultiplyPoint3x4(this.AnchorOffset);
				_center.x = p.x;
				_center.y = p.y;
				_center.z = p.z;
				break;
			case AnchorBindType.TargetSky:
				if(AnchorSky) {
					_center.x = AnchorSky.transform.position.x;
					_center.y = AnchorSky.transform.position.y;
					_center.z = AnchorSky.transform.position.z;
				}
				break;
			};
		}

		//clean up the instanced material list Unity spawned
		void OnDestroy() {
			CleanUpMaterials();
		}

		void OnApplicationQuit() {
			CleanUpMaterials();
		}

	#if UNITY_EDITOR
		public void OnDrawGizmosSelected() {
			Gizmos.color = Color.cyan;
			ComputeCenter(ref CachedCenter);
			Gizmos.DrawLine(transform.position, CachedCenter);
			if(BindType == AnchorBindType.Offset) {
				Gizmos.color = new Color(0f,4f,4f);
				Gizmos.DrawSphere(CachedCenter, 0.15f);
			}
		}
	#endif
	}
}
