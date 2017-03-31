using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipBtnPlayName : MonoBehaviour {
    public  AudioClip AudioClipName;
    AudioSource audioSource;
	// Use this for initialization
	void Start () {
        if (GameObject.FindGameObjectWithTag("GuiCamera").GetComponent<AudioSource>() ==null)
        {
            GameObject.FindGameObjectWithTag("GuiCamera").AddComponent<AudioSource>();
        }
        audioSource = GameObject.FindGameObjectWithTag("GuiCamera").GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void Play()
    {
        if(AudioClipName!=null && !audioSource.isPlaying)
        {
            audioSource.clip = AudioClipName;
            audioSource.PlayOneShot(AudioClipName);

        }       
    }

}
