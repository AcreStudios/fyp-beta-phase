using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class Footsteps : MonoBehaviour 
{
	// Components
	private Transform trans;
	private AudioSource audioSrc;
	private SoundManager soundManager;

	[Header("-Ground Settings-")]
	public GroundMaterialType[] groundTypes;
	public LayerMask groundLayer;

	[Header("-Sounds Settings-"), Range(0f, 1f)]
	public float footstepVolume = .2f;
	public bool randomizePitch = true;
	[Range(0.1f, 1f)]
	public float minPitch = .9f;
	[Range(0.1f, 2f)]
	public float maxPitch = 1.1f;
	[Range(0f, 0.2f)]
	public float stepsDelay = 0f;

	void Awake()
	{
		// Cache components
		trans = GetComponent<Transform>();
		audioSrc = GetComponent<AudioSource>();
	}

	void Start()
	{
		soundManager = SoundManager.GetInstance();

		// Set footsteps volume
		audioSrc.volume = footstepVolume;
	}

	public void PlayFootstepSound(float intensity) // Raycast down from each foot
	{
		RaycastHit hit;
		Vector3 start = trans.position + trans.up;
		Vector3 dir = Vector3.down;

		if(Physics.Raycast(start, dir, out hit, 1.3f, groundLayer))
		{
			MeshRenderer mRend = hit.collider.GetComponent<MeshRenderer>();
			if(mRend)
				StartCoroutine(PlayMeshSound(mRend, intensity, hit.point));
		}
	}

	private IEnumerator PlayMeshSound(MeshRenderer rend, float intensity, Vector3 hitPos) // Compare material to footstep sound
	{
		yield return new WaitForSeconds(stepsDelay);

		if(groundTypes.Length > 0) // If we defined a ground type
		{
			foreach(GroundMaterialType gTypes in groundTypes)
			{
				if(gTypes.footstepSounds.Length > 0) // If we have footsteps
				{
					foreach(Material mat in gTypes.mats) 
					{
						if(rend.material.mainTexture == mat.mainTexture) // Compare
						{
							if(soundManager) // If we have a sound manager
							{
								soundManager.PlaySoundOnce(hitPos,
									gTypes.footstepSounds[Random.Range(0, gTypes.footstepSounds.Length)], 2f,
									randomizePitch,
									minPitch,
									maxPitch, intensity * footstepVolume);
							}
						}
					}
				}
			}
		}
	}
}

[System.Serializable]
public class GroundMaterialType
{
	public string groundName;
	public Material[] mats;
	public AudioClip[] footstepSounds;
}
