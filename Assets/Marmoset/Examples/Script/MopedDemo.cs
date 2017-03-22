// Marmoset Skyshop
// Copyright 2013 Marmoset LLC
// http://marmoset.co

using UnityEngine;
using System.Collections;

public class MopedDemo : MonoBehaviour {
	public mset.Sky[] skies = null;
	
	private bool spinning = true;
	private bool background = true;
	
	private int currentSky = 0;
	private bool showDiffuse = true;
	private bool showSpecular = true;
	private bool untextured = false;
	private float exposure = 1f;
	private float glow = 0.5f;
	
	public Renderer[] meshes = null;
	public Renderer[] glowMeshes = null;
	public Renderer[] specGlowMeshes = null;
	
	private Texture[] diffTextures = null;
	private Texture[] specTextures = null;
	private Texture[] glowTextures = null;
	
	private Texture2D greyTex = null;
	private Texture2D blackTex = null;
	
	public Texture2D helpTex = null;
	private Color helpColor = new Color(1f,1f,1f,0f);
	
	public bool showGUI = true;
	private bool firstFrame = true;
		
	// Use this for initialization
	void Start () {
		firstFrame = true;
		setDiffuse(true);
		setSpecular(true);
		
		currentSky = 0;
		for( int i=skies.Length-1; i>=0; --i ) {
			setSky(i);
		}
		setBackground(background);
		
		greyTex = new Texture2D(16,16);
		Color grey = new Color(0.95f,0.95f,0.95f,1.0f);
		Color[] pixels = greyTex.GetPixels();
		for( int i=0; i<pixels.Length; ++i) { pixels[i] = grey; }
		greyTex.SetPixels(pixels);
		greyTex.Apply(true);
		
		blackTex = new Texture2D(16,16);
		pixels = blackTex.GetPixels();
		Color black = new Color(0f,0f,0f,0f);
		for( int i=0; i<pixels.Length; ++i) { pixels[i] = black; }
		blackTex.SetPixels(pixels);
		blackTex.Apply(true);
		
		if( meshes != null ) {
			diffTextures = new Texture[meshes.Length];
			specTextures = new Texture[meshes.Length];
			glowTextures = new Texture[meshes.Length];
			for( int i=0; i<meshes.Length; ++i ) {				
				if(meshes[i].material.HasProperty("_MainTex"))	diffTextures[i] = meshes[i].material.GetTexture("_MainTex");
				if(meshes[i].material.HasProperty("_SpecTex"))	specTextures[i] = meshes[i].material.GetTexture("_SpecTex");
				if(meshes[i].material.HasProperty("_Illum"))	glowTextures[i] = meshes[i].material.GetTexture("_Illum");
			}
		}
		setGrey(false);
		setGlow(glow);
		setExposures(1.0f);
	}
	void setDiffuse(bool yes) {
		showDiffuse = yes;
		for( int i=0; i<skies.Length; ++i ) {
			if(skies[i]) skies[i].DiffIntensity = yes ? 1.0f : 0.0f;
		}
	}
	void setSpecular(bool yes) {
		showSpecular = yes;
		for( int i=0; i<skies.Length; ++i ) {
			if(skies[i]) skies[i].SpecIntensity = yes ? 1.0f : 0.0f;
		}
	}
	void setExposures(float val) {
		exposure = val;
		for( int i=0; i<skies.Length; ++i ) {
			if(skies[i]) skies[i].CamExposure = val;
		}
	}
	void setSky(int index) {
		currentSky = index;		
		mset.SkyManager manager = mset.SkyManager.Get();
		if(manager) manager.BlendToGlobalSky(skies[currentSky], 1f);
	}
	void setBackground(bool yes) {
		background = yes;
		mset.SkyManager.Get().ShowSkybox = yes;
	}
	void setGrey(bool yes) {
		if( meshes != null ) {
			for( int i=0; i<meshes.Length; ++i ) {
				if(yes) {
					if(diffTextures[i])		meshes[i].material.SetTexture("_MainTex", greyTex);
					//if(specTextures[i])	meshes[i].material.SetTexture("_SpecTex", null);
					if(glowTextures[i])		meshes[i].material.SetTexture("_Illum", blackTex);
				} else {
					if(diffTextures[i])		meshes[i].material.SetTexture("_MainTex", diffTextures[i]);
					//if(specTextures[i])	meshes[i].material.SetTexture("_SpecTex", specTextures[i]);
					if(glowTextures[i])		meshes[i].material.SetTexture("_Illum", glowTextures[i]);
				}
			}
		}
	}
	void setGlow(float val) {
		glow = val;
		if( glowMeshes != null ) {
			for( int i=0; i<glowMeshes.Length; ++i ) {
				Material mat = glowMeshes[i].material;
				if( mat.HasProperty("_GlowStrength") ) {
					mat.SetFloat("_GlowStrength",12*glow);
				}
			}
		}
		if( specGlowMeshes != null ) {
			for( int i=0; i<specGlowMeshes.Length; ++i ) {
				Material mat = specGlowMeshes[i].material;
				if( mat.HasProperty("_SpecInt") ) {
					mat.SetFloat("_SpecInt",glow);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if( firstFrame ) {
			firstFrame = false;
			setSky(0);
		}
		if( Input.GetKeyDown(KeyCode.Alpha1) ) {
			setExposures(0.25f);
		}
		if( Input.GetKeyDown(KeyCode.Alpha2) ) {
			setExposures(0.5f);
		}
		if( Input.GetKeyDown(KeyCode.Alpha3) ) {
			setExposures(0.75f);
		}
		if( Input.GetKeyDown(KeyCode.Alpha4) ) {
			setExposures(1f);
		}
		if( skies.Length > 0 ) {
			if( Input.GetKeyDown(KeyCode.LeftArrow) ) {
				setSky((currentSky+1)%skies.Length);
			}
			if( Input.GetKeyDown(KeyCode.RightArrow) ) {
				setSky((currentSky+skies.Length-1)%skies.Length);
			}
		}
		if( Input.GetKeyDown(KeyCode.B) ) {
			setBackground(!background);
		}
		if( Input.GetKeyDown(KeyCode.S) ) {
			spinning = !spinning;
		}
		
		if( Input.multiTouchEnabled && Input.touchCount == 2 ) {
			float p0 = Input.GetTouch(0).position.x;
			float p1 = Input.GetTouch(1).position.x;
			float dx0 = Input.GetTouch(0).deltaPosition.x;
			float dx1 = Input.GetTouch(1).deltaPosition.x;
			if(dx0 != 0f && dx1 != 0f ) {
				if(p0 > p1) {
					float temp = dx0;
					dx0 = dx1;
					dx1 = temp;
				}
				float delta = dx1 - dx0;
				setExposures(Mathf.Clamp(exposure + delta*0.0025f,0.01f,10f));
			}
		}
	}
	
	Vector3 angularVel = new Vector3(0,6f,0);
	void FixedUpdate() {
		if( spinning ) {
			mset.SkyManager manager = mset.SkyManager.Get();
			if(manager && manager.GlobalSky) {
				manager.GlobalSky.transform.Rotate(angularVel*Time.fixedDeltaTime);
			}
		}
	}
	
	void OnGUI() {
		Rect menuRect = GetComponent<Camera>().pixelRect;
		menuRect.y =	  GetComponent<Camera>().pixelRect.height * 0.87f;
		menuRect.height = GetComponent<Camera>().pixelRect.height * 0.06f;
		
		if( Input.mousePosition.y < GetComponent<Camera>().pixelHeight*0.13f ) {
			helpColor.a = Mathf.Min(1f,helpColor.a + 0.1f);
		} else {
			helpColor.a = Mathf.Min(1f,0.9f*helpColor.a);
		}
		drawHelp(menuRect.width - helpTex.width, menuRect.y - helpTex.height - 10);
		
		GUI.color = Color.white;
		if(showGUI) {
			Rect cbRect = menuRect;
			cbRect.x = 10;
			cbRect.y += 3;
			cbRect.height = 20;
			cbRect.width = 100;
			
			bool prev = showDiffuse;
			showDiffuse = GUI.Toggle(cbRect, showDiffuse, "Diffuse IBL");
			if( prev != showDiffuse ) setDiffuse(showDiffuse);
			
			cbRect.y += 15;
			prev = showSpecular;
			showSpecular = GUI.Toggle(cbRect, showSpecular, "Specular IBL");
			if( prev != showSpecular ) setSpecular(showSpecular);
			
			cbRect.x += cbRect.width;
			cbRect.y -= 15;
			prev = background;
			cbRect.x += cbRect.width;
			background = GUI.Toggle(cbRect, background, "Skybox");
			if(prev != background) setBackground(background);
			
			cbRect.y += 15;
			spinning = GUI.Toggle(cbRect, spinning, "Spinning");
			
			cbRect.x += cbRect.width;
			cbRect.y -= 15;
			prev = untextured;
			untextured = GUI.Toggle(cbRect, untextured, "Untextured");
			if( prev != untextured ) setGrey(untextured);
			
						
			Rect sliderRect = cbRect;
			sliderRect.x = 15;
			sliderRect.y = menuRect.yMax - 10;
			sliderRect.height = 20;
			sliderRect.width = menuRect.width*0.28f;
			GUI.Label(sliderRect,"Exposure: " + Mathf.CeilToInt(exposure * 100) + "%");
			sliderRect.y += 18;
			float logExposure = Mathf.Sqrt(exposure);
			logExposure = GUI.HorizontalSlider(sliderRect, logExposure, 0, 2f);
			exposure = logExposure*logExposure;
			setExposures(exposure);
			
			///
			
			sliderRect.x = GetComponent<Camera>().pixelRect.width - sliderRect.width - 15;
			sliderRect.y -= 18f;
			GUI.Label(sliderRect,"Moped Lights");
			sliderRect.y += 18f;
			float logGlow = glow*glow;
			float prevGlow = logGlow;
			logGlow = GUI.HorizontalSlider(sliderRect, logGlow, 0, 1f);
			if( logGlow != prevGlow ) {
				setGlow(Mathf.Sqrt(logGlow));
			}
		}
	}
	
	void drawHelp(float x, float y) {
		if( helpTex ) {
			Rect texRect = new Rect(x,y,helpTex.width,helpTex.height);
			GUI.color = helpColor;
			GUI.DrawTexture(texRect, helpTex);
		}
	}
}
