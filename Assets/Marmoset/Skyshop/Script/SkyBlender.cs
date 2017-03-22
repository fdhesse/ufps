// Marmoset Skyshop
// Copyright 2014 Marmoset LLC
// http://marmoset.co

using UnityEngine;
//using UnityEditor;
using System.Collections;

using System;

namespace mset {
	[Serializable]
	public class SkyBlender {
		public mset.Sky CurrentSky = null;
		public mset.Sky PreviousSky = null;

		[SerializeField]
		private float blendTime = 0.25f;
		public float BlendTime {
			get { return blendTime; }
			set { blendTime = value; }
		}

		private float currentBlendTime = 0.25f;
		private float endStamp = 0f;

		private float blendTimer {
			get { return endStamp - Time.time; }
			set { endStamp = Time.time + value; }
		}
		public float BlendWeight {
			get { return 1f - Mathf.Clamp01(blendTimer/currentBlendTime); }
		}

		public bool IsBlending {
			get { return Time.time < endStamp; }
		}

		public bool WasBlending(float secAgo) {
			return (Time.time-secAgo) < endStamp;
		}

		//global apply
		public void Apply() {
			if(IsBlending) {
				mset.Sky.EnableGlobalProjection(CurrentSky.HasDimensions || PreviousSky.HasDimensions);
				mset.Sky.EnableGlobalBlending(true);
				CurrentSky.Apply(0);
				PreviousSky.Apply(1);
				mset.Sky.SetBlendWeight(BlendWeight);
			} else {
				mset.Sky.EnableGlobalProjection(CurrentSky.HasDimensions);
				mset.Sky.EnableGlobalBlending(false);
				CurrentSky.Apply(0);
			}
		}
		//call on as many materials as appropriate
		public void Apply(Material target) {
			if(IsBlending) {
				mset.Sky.EnableBlending(target, true);
				//projection is enabled if either blend target needs it
				mset.Sky.EnableProjection(target, CurrentSky.HasDimensions || PreviousSky.HasDimensions);
				CurrentSky.Apply(target, 0);
				PreviousSky.Apply(target, 1);
				mset.Sky.SetBlendWeight(target, BlendWeight);
			} else {
				mset.Sky.EnableBlending(target, false);
				mset.Sky.EnableProjection(target, CurrentSky.HasDimensions);
				CurrentSky.Apply(target,0);
			}
		}

		//call on as many renderers as appropriate
		public void Apply(Renderer target, Material[] materials) {
			if(IsBlending) {
				mset.Sky.EnableBlending(target, materials, true);
				mset.Sky.EnableProjection(target, materials, CurrentSky.HasDimensions || PreviousSky.HasDimensions);
				CurrentSky.ApplyFast(target, 0);
				PreviousSky.ApplyFast(target, 1);
				mset.Sky.SetBlendWeight(target, BlendWeight);
			} else {
				mset.Sky.EnableBlending(target, materials, false);
				mset.Sky.EnableProjection(target, materials, CurrentSky.HasDimensions);
				CurrentSky.ApplyFast(target,0);
			}
		}
		//call in addition to Apply()
		public void ApplyToTerrain() {
			if(IsBlending) {
				mset.Sky.EnableTerrainBlending(true);
				//TODO: tell tree billboards to update here
			} else {
				mset.Sky.EnableTerrainBlending(false);
			}
		}
		//call once
		public void SnapToSky(mset.Sky nusky) {
			if(nusky == null) return;
			CurrentSky = PreviousSky = nusky;
			blendTimer = 0f;
		}

		//call once
		public void BlendToSky(mset.Sky nusky) {
			if(nusky == null) return;
			if(CurrentSky != nusky) {
				//do some blending
				if(CurrentSky == null) {
					//nothing to blend from
					PreviousSky = CurrentSky = nusky;
					blendTimer = 0f;
				}
				else {
					PreviousSky = CurrentSky;
					CurrentSky = nusky;
					currentBlendTime = blendTime;
					blendTimer = currentBlendTime;
				}
			}
		}

		public void SkipTime(float sec) {
			blendTimer = blendTimer - sec;
		}
	}
}

