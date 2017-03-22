// Marmoset Skyshop
// Copyright 2014 Marmoset LLC
// http://marmoset.co

using UnityEngine;
using System.Collections;

public class Oscillate : MonoBehaviour {

	public Vector3 amplitude;
	public Vector3 speed;
	public Vector3 clamp;
	
	public Vector3 basePosition;
	// Use this for initialization
	void Start () {
		basePosition = this.transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Vector3 t = Time.time * speed * 2 * Mathf.PI;
		Vector3 wave = new Vector3(amplitude.x*Mathf.Sin(t.x), amplitude.y*Mathf.Sin(t.y), amplitude.z*Mathf.Sin(t.z));
		wave.x = Mathf.Clamp(wave.x, -clamp.x, clamp.x);
		wave.y = Mathf.Clamp(wave.y, -clamp.y, clamp.y);
		wave.z = Mathf.Clamp(wave.z, -clamp.z, clamp.z);
		this.transform.position = basePosition + wave;
	}
}
