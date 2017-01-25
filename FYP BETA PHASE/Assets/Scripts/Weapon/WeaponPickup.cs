using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class WeaponPickup : MonoBehaviour 
{
	[Header("-Drag weapon to be picked up here-")]
	public Weapon weapon;
	public AudioClip pickupSound;

	private SphereCollider sphereCol;
	private WeaponHandler wpnHandler;
	private PlayerInput playerInput;

	void Awake()
	{
		sphereCol = GetComponent<SphereCollider>();
	}

	void Start()
	{
		// Try get weapon
		Weapon wpn = transform.parent.GetComponent<Weapon>();
		if(wpn)
			weapon = wpn;
		
		// Setup sphere collider
		sphereCol.isTrigger = true;
		if(sphereCol.radius < 1f)
			sphereCol.radius = 1f;
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.CompareTag("Player"))
		{
			wpnHandler = col.GetComponent<WeaponHandler>();
			playerInput = col.GetComponent<PlayerInput>();

			if(playerInput._keyE)
				AddWeaponToPlayer(wpnHandler);
		}
	}

	void OnTriggerStay(Collider col)
	{
		if(col.CompareTag("Player"))
		{
			if(playerInput._keyE)
				AddWeaponToPlayer(wpnHandler);
		}
	}

	void OnTriggerExit(Collider col)
	{
		if(col.CompareTag("Player"))
		{
			wpnHandler = null;
			playerInput = null;
		}
	}

	private void AddWeaponToPlayer(WeaponHandler player)
	{
		if(!weapon)
			return;

		wpnHandler.weaponsList.Add(weapon);
		wpnHandler.SetupWeapon();
		wpnHandler.SwitchNextWeapon();

		if(pickupSound)
			SoundManager.GetInstance().PlaySoundOnce(transform.position, pickupSound);

		// Destroy self after pickup
		Destroy(gameObject);
	}
}
