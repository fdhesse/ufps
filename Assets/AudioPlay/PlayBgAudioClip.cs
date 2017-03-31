using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayBgAudioClip : MonoBehaviour {
    public AudioClip BgAudioClip;
    AudioSource audioSource;
	// Use this for initialization
	void Start () {
        if (GameObject.Find("GameManager").GetComponent<AudioSource>() == null)
        {
            GameObject.Find("GameManager").AddComponent<AudioSource>();
        }
        audioSource = GameObject.Find("GameManager").GetComponent<AudioSource>();
        audioSource.clip = BgAudioClip;
        if ( BgAudioClip!=null)
        {
            audioSource.PlayOneShot(BgAudioClip);
        }
        audioSource.loop = true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
