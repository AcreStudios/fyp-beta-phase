using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class WeaponHandler : MonoBehaviour 
{
	// Components
	[HideInInspector]
	public Animator animator;
	private SoundManager soundManager;

	[Header("-Weapons-")]
	public Weapon activeWeapon;
	public List<Weapon> weaponsList = new List<Weapon>();

	// Weapon helpers
	private int _weaponIndex;
	private bool _aiming;
	private bool _reloading;
	private bool _switchingWeapon;
	private bool _empty;
	private WaitForSeconds _switchWeaponDelay = new WaitForSeconds(.7f);

	[System.Serializable]
	public class ModelSettings
	{
		public Transform rightHand;
		public Transform spine;
	}
	public ModelSettings modelSettings;

	[System.Serializable]
	public class AnimatorStrings
	{
		public string weaponTypeInt = "WeaponType";
		public string aimingBool = "isAiming";
		public string reloadingBool = "isReloading";
	}
	[SerializeField]
	private AnimatorStrings animatorStrings;


	void Awake()
	{
		// Cache components
		animator = GetComponent<Animator>();
	}

	void OnEnable()
	{
		SetupWeapon();
	}

	void Start() 
	{
		// Cache managers
		soundManager = SoundManager.GetInstance();
	}

	void Update() 
	{
		AnimateWeapon();
	}

	private void SetupWeapon() // Initialise weapon states and update them
	{
		if(activeWeapon)
		{
			activeWeapon.SetEquipState(true);
			activeWeapon.SetOwner(this);
			AddWeaponToList(activeWeapon);

			if(_reloading)
			{
				if(_switchingWeapon)
					_reloading = false;
			}
		}

		if(weaponsList.Count > 0)
		{
			for(int i = 0; i < weaponsList.Count; i++)
			{
				if(weaponsList[i] != activeWeapon)
				{
					weaponsList[i].SetEquipState(false);
					weaponsList[i].SetOwner(this);
				}
			}
		}
	}

	private void AddWeaponToList(Weapon wpn) // Add the weapon to weaponsList
	{
		if(weaponsList.Contains(wpn))
			return;

		weaponsList.Add(wpn);
	}

	private void AnimateWeapon() // Animates the character and weapon with the animator
	{
		animator.SetInteger(animatorStrings.weaponTypeInt, _weaponIndex);
		animator.SetBool(animatorStrings.aimingBool, _aiming);
		animator.SetBool(animatorStrings.reloadingBool, _reloading);

		if(!activeWeapon)
		{
			_weaponIndex = 0;
			return;
		}

		switch(activeWeapon.weaponSettings.weaponType)
		{
			case Weapon.WeaponSettings.WeaponType.PISTOL:
				_weaponIndex = 1;
				break;
			case Weapon.WeaponSettings.WeaponType.OTHER:
				_weaponIndex = 2;
				break;
		}
	}

	public void AimWeapon(bool aim) // Aim logic
	{
		_aiming = aim;
	}

	public void SwitchNextWeapon() // Switch to next weapon on the list
	{
		if(_switchingWeapon || weaponsList.Count == 0 || _reloading)
			return;

		if(activeWeapon)
		{
			int currentWeaponIndex = weaponsList.IndexOf(activeWeapon);
			int nextWeaponIndex = (currentWeaponIndex + 1) % weaponsList.Count;

			activeWeapon = weaponsList[nextWeaponIndex];
		}
		else
			activeWeapon = weaponsList[0];

		StartCoroutine(FinishSwitchWeapon());
		SetupWeapon();
	}

	private IEnumerator FinishSwitchWeapon() // Finish switching weapons
	{
		_switchingWeapon = true;
		yield return _switchWeaponDelay;
		_switchingWeapon = false;
	}

	public void FireCurrentWeapon(Transform mainCamTrans) // Tells the current weapon to fire
	{
		// Fire
		activeWeapon.Shoot(mainCamTrans);

		#region Auto reload?

		if(activeWeapon.weaponSettings.autoReload)
		{
			if(activeWeapon.ammoSettings.currentAmmo == 0)
				ReloadCurrentWeapon();

			return;
		}

		#endregion

		if(activeWeapon.ammoSettings.currentAmmo > 0)
			return;

		
		#region Play empty sound

		if(soundManager)
		{
			if(!_empty && activeWeapon.soundSettings.emptySound && activeWeapon.soundSettings.audioSrc)
			{
				soundManager.PlaySound(activeWeapon.soundSettings.audioSrc,
					activeWeapon.soundSettings.emptySound,
					true,
					activeWeapon.soundSettings.pitchMin,
					activeWeapon.soundSettings.pitchMax);

				StartCoroutine(FinishEmptyFire());
			}
		}

		#endregion
	}

	private IEnumerator FinishEmptyFire()
	{
		_empty = true;
		yield return new WaitForSeconds(.4f);
		_empty = false;
	}

	public void ReloadCurrentWeapon() // Reload logic
	{
		if(_reloading || !activeWeapon || activeWeapon.ammoSettings.totalAmmo <= 0 || activeWeapon.ammoSettings.currentAmmo == activeWeapon.ammoSettings.maxAmmo)
			return;

		#region Play reload sound

		if(soundManager)
		{
			if(activeWeapon.soundSettings.reloadSound && activeWeapon.soundSettings.audioSrc)
			{
				soundManager.PlaySound(activeWeapon.soundSettings.audioSrc,
					activeWeapon.soundSettings.reloadSound,
					true,
					activeWeapon.soundSettings.pitchMin,
					activeWeapon.soundSettings.pitchMax);
			}
		}

		#endregion

		StartCoroutine(FinishReload());
	}

	private IEnumerator FinishReload() // Finish the reloading of the weapon
	{
		_reloading = true;
		yield return new WaitForSeconds(activeWeapon.weaponSettings.reloadDuration);
		activeWeapon.LoadAmmo();
		_reloading = false;
	}
}
