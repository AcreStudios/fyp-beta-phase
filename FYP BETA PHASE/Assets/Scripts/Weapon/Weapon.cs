using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class Weapon : MonoBehaviour 
{
	// Components
	private BoxCollider col;
	private Rigidbody rb;
	private Animator animator;
	private Transform trans;
	private WeaponHandler weaponHandler;
	private TPCamera tpCamera;

	[System.Serializable]
	public class WeaponSettings
	{
		public enum WeaponType { PISTOL, RIFLE, SHOTGUN }
		public WeaponType weaponType;

		[Header("-Weapon Options-"), Range(1, 5)]
		public int bulletsPerShot = 1;
		public float bulletDamage = 10f;
		public float bulletSpread = .02f;
		public float fireRate = .3f;
		public float fireRange = 100f;
		public float reloadDuration = 2f;
		public bool autoReload = false;
		public LayerMask bulletLayer;

		[Header("-Effects Prefabs-")]
		public GameObject bulletImpactPrefab;
		public GameObject muzzleFlashPrefab;
		public GameObject bulletCasingPrefab;

		[Header("-Effects Transforms-")]
		public Transform bulletSpawnPoint;
		public Transform casingSpawnPoint;

		public float casingSpawnSpeed = 5f;

		[Header("-Weapon Positions-")]
		public Vector3 equipPositionR = new Vector3(-.02f, .15f, .02f);
		public Vector3 equipRotationR = new Vector3(270f, 45f, 45f);
		public Vector3 equipPositionL = new Vector3(-.02f, .15f, .02f);
		public Vector3 equipRotationL = new Vector3(270f, 45f, 45f);
		public Vector3 unequipPosition = new Vector3(.2f, -.2f, -.1f);
		public Vector3 unequipRotation = new Vector3(90f, 270f, 270f);

		[Header("-Weapon Animation-")]
		public bool useAnimation = true;
		public int fireLayer = 0;
		public string fireName = "PistolFire";
	}
	public WeaponSettings weaponSettings;

	[System.Serializable]
	public class AmmoSettings
	{
		public int totalAmmo = 1200;
		public int currentAmmo = 12;
		public int maxAmmo = 12;
	}
	public AmmoSettings ammoSettings;

	[System.Serializable]
	public class SoundSettings
	{
		public AudioClip reloadSound;
		public AudioClip emptySound;
		public AudioClip[] gunshotSounds;
		[Range(0.1f, 1f)]
		public float pitchMin = .9f;
		[Range(1f, 2f)]
		public float pitchMax = 1.1f;
		[HideInInspector]
		public AudioSource audioSrc;
	}
	public SoundSettings soundSettings;

	[System.Serializable]
	public class ModelSettings
	{
		public Vector3 spineRotationRight = new Vector3(-5f, -10f, 0f);
		public Vector3 spineRotationLeft = new Vector3(0f, 15f, 0f);
		public Transform leftHandIKTarget;
		public Transform rightHandIKTarget;
	}
	public ModelSettings modelSettings;

	// Weapon states helpers
	private bool _equipped;
	private bool _loading;


	void Awake()
	{
		col = GetComponent<BoxCollider>();
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		trans = GetComponent<Transform>();
		soundSettings.audioSrc = GetComponent<AudioSource>();
	}

	void Start() 
	{
		tpCamera = TPCamera.GetInstance();

		EquipWeapon();
	}

	void Update() 
	{
		CheckEquipState();
	}

	public void Shoot(Transform mainCamTrans)
	{
		if(ammoSettings.currentAmmo <= 0 || _loading || !weaponSettings.bulletSpawnPoint || !_equipped)
			return;

		for(int i = 0; i < weaponSettings.bulletsPerShot; i++)
		{
			#region Each bullet logic

			RaycastHit hit;
			Vector3 start = mainCamTrans.position;
			Vector3 dir = mainCamTrans.forward;
			dir += (Vector3)Random.insideUnitCircle * weaponSettings.bulletSpread; // Spread the bullets, reduces accuracy
			Debug.DrawRay(start, dir, Color.green);
			if(Physics.Raycast(start, dir, out hit, weaponSettings.fireRange, weaponSettings.bulletLayer))
			{
				//CHAR_Health hp = hit.transform.root.GetComponent<CHAR_Health>();
				//if(hp && hp.isActiveAndEnabled)
				//	hp.ReceiveDamage(weaponSettings.bulletDamage);

				#region Spawn bullet impact

				if(weaponSettings.bulletImpactPrefab)
				{
					if(hit.collider.gameObject.isStatic)
					{
						Vector3 hitPoint = hit.point;
						Quaternion lookRot = Quaternion.LookRotation(hit.normal);
						GameObject decal = (GameObject)Instantiate(weaponSettings.bulletImpactPrefab, hitPoint, lookRot);
						Transform decalTrans = decal.transform;
						Transform hitTrans = hit.transform;
						decalTrans.SetParent(hitTrans);
						Destroy(decal, 20f);
					}
				}

				#endregion
			}

			#endregion
		}

		#region Spawn muzzle flash

		if(weaponSettings.muzzleFlashPrefab)
		{
			Vector3 bSpawnPos = weaponSettings.bulletSpawnPoint.position;
			GameObject mFlash = (GameObject)Instantiate(weaponSettings.muzzleFlashPrefab, bSpawnPos, Quaternion.identity);
			Transform mFlashTrans = mFlash.transform;
			mFlashTrans.SetParent(weaponSettings.bulletSpawnPoint);
			Destroy(mFlash, .5f);
		}

		#endregion

		#region Spawn casing

		if(weaponSettings.bulletCasingPrefab && weaponSettings.casingSpawnPoint)
		{
			Vector3 casingPos = weaponSettings.casingSpawnPoint.position;
			Quaternion casingRot = weaponSettings.casingSpawnPoint.rotation;
			GameObject shell = (GameObject)Instantiate(weaponSettings.bulletCasingPrefab, casingPos, casingRot);
			if(shell.GetComponent<Rigidbody>())
			{
				Rigidbody shellRb = shell.GetComponent<Rigidbody>();
				shellRb.AddForce(weaponSettings.casingSpawnPoint.forward * weaponSettings.casingSpawnSpeed, ForceMode.Impulse);
				shellRb.AddTorque(weaponSettings.casingSpawnPoint.forward * weaponSettings.casingSpawnSpeed, ForceMode.Impulse);
			}
			Destroy(shell, 10f);
		}

		#endregion

		#region Play weapon sound

		if(SoundManager.instance && soundSettings.audioSrc)
		{
			if(soundSettings.gunshotSounds.Length > 0)
			{
				SoundManager.instance.PlaySoundOnce(weaponSettings.bulletSpawnPoint.position,
				soundSettings.gunshotSounds[Random.Range(0, soundSettings.gunshotSounds.Length)],
				2f,
				true,
				soundSettings.pitchMin,
				soundSettings.pitchMax);
			}
		}

		#endregion

		// Animation
		if(weaponSettings.useAnimation)
		{
			if(animator.runtimeAnimatorController)
				animator.Play(weaponSettings.fireName, weaponSettings.fireLayer);
		}

		weaponHandler.animator.SetTrigger("Fire");

		// Shoot cooldown
		ammoSettings.currentAmmo--;
		StartCoroutine(FinishShooting());
	}

	private IEnumerator FinishShooting() // Loads next bullet; Cooldown interval between bullets
	{
		_loading = true;
		yield return new WaitForSeconds(weaponSettings.fireRate);
		_loading = false;
	}

	public void LoadAmmo() // Calculate and reload ammo
	{
		int ammoNeeded = ammoSettings.maxAmmo - ammoSettings.currentAmmo;

		if(ammoNeeded < ammoSettings.totalAmmo) // If we have enough total ammo
		{
			ammoSettings.totalAmmo -= ammoNeeded;
			ammoSettings.currentAmmo = ammoSettings.maxAmmo;
		}
		else
		{
			ammoSettings.currentAmmo = ammoSettings.totalAmmo;
			ammoSettings.totalAmmo = 0;
		}
	}

	private void CheckEquipState() // Check if the weapon should be equipped or not
	{
		if(weaponHandler)
		{
			DisableEnableComponents(false);
			if(_equipped)
				EquipWeapon();
			else
				UnequipWeapon();
		}
		else // If no owner
		{
			DisableEnableComponents(true);
			trans.SetParent(null);
		}
	}

	private void DisableEnableComponents(bool enabled) // Disable or enable rigidbody and collider
	{
		if(!enabled)
		{
			rb.isKinematic = true;
			col.enabled = false;
		}
		else
		{
			rb.isKinematic = false;
			col.enabled = true;
		}
	}

	private void EquipWeapon() // Equip this weapon to the hand
	{
		if(!weaponHandler)
			return;
		else if(!weaponHandler.modelSettings.rightHand)
			return;

		trans.SetParent(weaponHandler.modelSettings.rightHand);

		switch(tpCamera.cameraSettings.shoulder)
		{
			case TPCamera.CameraSettings.Shoulder.RIGHT:
				trans.localPosition = weaponSettings.equipPositionR;
				Quaternion equipRotR = Quaternion.Euler(weaponSettings.equipRotationR);
				trans.localRotation = equipRotR;
				break;
			case TPCamera.CameraSettings.Shoulder.LEFT:
				trans.localPosition = weaponSettings.equipPositionL;
				Quaternion equipRotL = Quaternion.Euler(weaponSettings.equipRotationL);
				trans.localRotation = equipRotL;
				break;
		}
	}

	private void UnequipWeapon() // Unequip and place weapon to the desired location
	{
		// Fix muzzle flash staying
		if(weaponSettings.bulletSpawnPoint != null)
		{
			if(weaponSettings.bulletSpawnPoint.childCount > 0)
			{
				foreach(Transform t in weaponSettings.bulletSpawnPoint.GetComponentsInChildren<Transform>())
				{
					if(t != weaponSettings.bulletSpawnPoint)
						Destroy(t.gameObject);
				}
			}
		}

		if(!weaponHandler)
			return;

		trans.SetParent(weaponHandler.modelSettings.spine);
		trans.localPosition = weaponSettings.unequipPosition;
		Quaternion unequipRot = Quaternion.Euler(weaponSettings.unequipRotation);
		trans.localRotation = unequipRot;
	}

	public void SetEquipState(bool eq) // Sets the weapon equip state
	{
		_equipped = eq;
	}

	public void SetOwner(WeaponHandler wh) // Sets the owner of this weapon
	{
		weaponHandler = wh;
	}
}
