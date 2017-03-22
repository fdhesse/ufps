// Marmoset Skyshop
// Copyright 2014 Marmoset LLC
// http://marmoset.co

using UnityEngine;
using System.Collections;

public class BlendList : MonoBehaviour {
	public mset.Sky[] skyList = null;
	public float blendTime = 1f;
	public float waitTime = 3f;
	float blendStamp = 0f;
	int currSky = 0;
	mset.SkyManager manager;

	// Use this for initialization
	void Start () {
		manager = mset.SkyManager.Get();
		manager.BlendToGlobalSky(skyList[currSky], blendTime);
		blendStamp = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if((Time.time - blendStamp) > blendTime+waitTime) {
			currSky = (currSky+1) % skyList.Length;
			blendStamp = Time.time;
			manager.BlendToGlobalSky(skyList[currSky], blendTime);
		}
	}
}
