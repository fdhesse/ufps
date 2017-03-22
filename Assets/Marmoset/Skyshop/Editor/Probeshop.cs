// Marmoset Skyshop
// Copyright 2014 Marmoset LLC
// http://marmoset.co

using UnityEngine;
using UnityEditor;
using System;
using System.IO;

class Probeshop : EditorWindow {
	public static void ProbeSkies(GameObject[] objects, mset.Sky[] skies, bool probeAll, bool probeIBL, System.Action doneCallback) {
		mset.SkyManager mgr = mset.SkyManager.Get();
		if(mgr) {
			Probeshop window = (Probeshop)EditorWindow.GetWindow (typeof (Probeshop), true);
			window.Show(true);
			window.minSize = window.maxSize = new Vector2(400,200);
			Rect r = window.position;
			r.center = new Vector2(Screen.currentResolution.width/2, Screen.currentResolution.height/2);
			window.position = r;
			window.DoneCallback = doneCallback;
			mgr.ProbeSkies( objects, skies, probeAll, probeIBL);
		} else {
			EditorUtility.DisplayDialog("No Sky Manager found!", "Probing skies requires a SkyManager object in the scene. Add one through the GameObject menu.", "Ok");
			Debug.LogWarning("Probing requires a SkyManager object in the scene.");
		}
	}

	private bool probing = false;
	private bool finishing = false;
	private mset.FreeProbe freeProbe = null;
	private mset.SkyProbe proProbe = null;
	private bool useCubeRT = false;
	private bool firstFrame = true;

	public System.Action DoneCallback = null;

	//No more progress bar
	//private float progress = 0f;
	private void SetProgress(float amt) {
		//progress = Mathf.Clamp01(amt);
		this.Repaint();
	}

	private void Update() {
		//bool probing = UnityEditor.EditorPrefs.GetBool("mset.ProbeOverride");

		if(probing) {
			ProbeUpdate();
		} else if(finishing) {
			ProbeFinish();
			finishing = false;
		} else {
			ProbeStart(); 
		}
	}

	private void OnGUI() {
		//bool probing = UnityEditor.EditorPrefs.GetBool("mset.ProbeOverride");
		if(probing) {
			//Progress bar breaks Unity 5.2.3+, Goodnight sweet prince.
			//EditorUtility.DisplayProgressBar("Probe Progress", Mathf.Floor(0.5f + 100f*progress) + "% Complete", progress);

			GUILayout.Space(160);
			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			if( GUILayout.Button("Cancel", GUILayout.Width(120)) ) {
				ProbeCancel();
			}
			GUILayout.EndHorizontal();
		}
	}

	private void ProbeStart() {
		useCubeRT = TestCubeRTSupport();
		//EditorPrefs.SetBool("mset.ProbeOverride", true);
		probing = true;
		EditorApplication.isPlaying = true;
		firstFrame = true;
	}

	private void ProbeUpdate() {
		mset.SkyManager skmgr = mset.SkyManager.Get();
		if(UnityEditor.EditorApplication.isPlaying && skmgr.SkiesToProbe != null) {

			if(firstFrame) {
				if(skmgr.ProbeOnlyStatic) {
					//hide dynamics
					Renderer[] rends = GameObject.FindObjectsOfType<Renderer>();
					foreach(Renderer rend in rends) {
						if(!rend.gameObject.isStatic) rend.gameObject.SetActive(false);
					}
				}
				firstFrame = false;
			}

			if(useCubeRT) {
				if(proProbe == null) {
					proProbe = new mset.SkyProbe();
				}

				//Run the whole loop in place and finish
				bool success = true;
				int i = 0;
				foreach( mset.Sky targetSky in skmgr.SkiesToProbe ) {
					i++;
					SetProgress((float)i / (float)skmgr.SkiesToProbe.Length);

					//NOTE: we don't allocate new cubemaps at this stage. That's on you, buddy.
					Cubemap targetCube = targetSky.SpecularCube as Cubemap;
					if(targetCube == null) continue;
					
					Transform at = targetSky.transform;
					proProbe.maxExponent = skmgr.ProbeExponent;
					proProbe.exposures = skmgr.ProbeExposures;
					bool linear = UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear;

					bool k = proProbe.capture(targetCube, at.position, at.rotation, targetSky.HDRSpec, linear, true);
					if(!k) {
						Debug.LogWarning("Failed to capture with RenderTextures, falling back to ReadPixels.");
						useCubeRT = false;							
						success = false;
						break;
					}
				}
				if(success) {
					CueProbeFinish();
				}
			} 
			
			if(!useCubeRT) {
				//Create a game object and let the update loop run until DoneCallback gets called
				if(freeProbe == null) {
					GameObject go = new GameObject();
					go.name = "FreeProbe Object";
					go.AddComponent<Camera>();
					freeProbe = go.AddComponent<mset.FreeProbe>();
					skmgr.GameApplySkies(true);
					Shader.SetGlobalVector("_UniformOcclusion", skmgr.ProbeExposures);
					freeProbe.linear = UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear;
					freeProbe.maxExponent = skmgr.ProbeExponent;
					freeProbe.exposures = skmgr.ProbeExposures;
					freeProbe.QueueSkies(skmgr.SkiesToProbe);
					freeProbe.ProgressCallback = this.SetProgress;
					freeProbe.DoneCallback = this.CueProbeFinish;
					freeProbe.RunQueue();
				}
			}
		}
	}

	private void ProbeCancel() {
		if(freeProbe != null) {
			CueProbeFinish();
		}
	}

	private void CueProbeFinish() {
		this.finishing = true;
		this.probing = false;
		if( this.freeProbe ) {
			GameObject.Destroy( this.freeProbe.gameObject );
		}

	}

	private void ProbeFinish() {
		EditorApplication.isPlaying = false;
		mset.SkyManager skmgr = mset.SkyManager.Get();
		foreach(mset.Sky sky in skmgr.SkiesToProbe) {
			if(sky.SpecularCube as Cubemap) {
				mset.CubeMipProcessor.CreateSubMips(sky.SpecularCube as Cubemap);
				mset.SHUtil.projectCube(ref sky.SH, sky.SpecularCube as Cubemap, 3, true);
				mset.SHUtil.convolve( ref sky.SH );
				sky.SH.copyToBuffer();
			}
			sky.Dirty = true;
		}
		proProbe = null;
		freeProbe = null;
		skmgr.SkiesToProbe = null;
		AssetDatabase.Refresh();
		
		skmgr.ShowSkybox = skmgr.ShowSkybox;		
		skmgr.EditorUpdate(true);
		skmgr.ProbeExposures = Vector4.one;

		EditorUtility.ClearProgressBar();
		mset.SkyInspector.forceRefresh();

		if(DoneCallback != null) DoneCallback();
		DoneCallback = null;

		Close();
	}
	
	private bool TestCubeRTSupport() {			
		mset.SkyManager mgr = mset.SkyManager.Get();
		bool useCubeRT = false;
		if(mgr && mgr.ProbeWithCubeRT) {
			//test for render texture and HDR support
			RenderTexture testRT = RenderTexture.GetTemporary(256,256,24,RenderTextureFormat.ARGBHalf);
			testRT.Release();
			testRT.dimension = UnityEngine.Rendering.TextureDimension.Cube;			
			testRT.useMipMap = true;
			testRT.autoGenerateMips = true;
			testRT.Create();
			if( !testRT.IsCreated() && !testRT.Create() ){
				testRT = RenderTexture.GetTemporary(256,256,24,RenderTextureFormat.ARGB32);
				testRT.Release();
				testRT.dimension = UnityEngine.Rendering.TextureDimension.Cube;
				testRT.useMipMap = true;
				testRT.autoGenerateMips = true;
				testRT.Create();
			}
			useCubeRT = testRT.IsCreated() || testRT.Create();
			if(!useCubeRT) {
				Debug.LogWarning("RenderTextures don't seem to be supported, using ReadPixels capture instead.");
			}

			#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
				#if UNITY_EDITOR_WIN
				if(!UnityEditor.PlayerSettings.useDirect3D11) {
					Debug.LogWarning("RenderTexture cubemaps require Direct3D 11 in Windows, using ReadPixels capture instead.");
					useCubeRT = false;
				}
				#endif
			#endif
			RenderTexture.ReleaseTemporary(testRT);
		}
		return useCubeRT;
	}
}
