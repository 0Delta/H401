using UnityEngine;
using System.Collections;

public class TapPlaySE : MonoBehaviour {

    private AudioSource audioSource;
	// Use this for initialization
	void Start () {
        audioSource = GetComponent<AudioSource>();
	}


	void OnMouseUp () {
        audioSource.Play();
	}
}
