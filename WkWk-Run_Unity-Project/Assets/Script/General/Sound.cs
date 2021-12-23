using UnityEngine.Audio;
using UnityEngine;

// Custom Class Sound
[System.Serializable]
public class Sound
{
	// Name of the song
	public string name;
	// The audio file
	public AudioClip clip;
	// Volume for this audio (adjustable)
	[Range(0f, 1f)]
	public float volume = .75f;
	// Pitch for this audio (adjustable)
	[Range(.1f, 10f)]
	public float pitch = 1f;
	// Looping or not
	public bool loop = false;
	// 
	[HideInInspector]
	public AudioSource source;
}