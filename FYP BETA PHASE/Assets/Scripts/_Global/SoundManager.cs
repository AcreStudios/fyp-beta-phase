using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour 
{
	// Singleton
	public static SoundManager instance;
	public static SoundManager GetInstance()
	{
		return instance;
	}


	void Awake()
	{
		// Implement singleton
		if(!instance)
			instance = this;
		else
		{
			// Prevent multiple instances, making sure only one per scene
			if(instance != this)
				Destroy(gameObject);
		}
	}

	public void PlaySound(AudioSource aSrc, AudioClip aClip, bool randomPitch = false, float minRandomPitch = 1, float maxRandomPitch = 1, float vol = 1f)
	{
		// Plays sound on their own audio source
		if(randomPitch)
			aSrc.pitch = Random.Range(minRandomPitch, maxRandomPitch);

		aSrc.volume = vol;
		aSrc.spatialBlend = 1f;
		aSrc.clip = aClip;
		aSrc.Play();
	}

	public void PlaySoundOnce(Vector3 pos, AudioClip aClip, float duration = 2f, bool randomPitch = false, float minRandomPitch = 1, float maxRandomPitch = 1)
	{
		// Creates a 3D sound at target location
		GameObject os = new GameObject("OneShotAudio_" + aClip.name);
		os.transform.position = pos;
		AudioSource a = os.AddComponent<AudioSource>();
		a.spatialBlend = 1f;
		a.clip = aClip;
		a.Play();

		// Destroy sound after specific duration
		Destroy(os, duration);
	}
}
