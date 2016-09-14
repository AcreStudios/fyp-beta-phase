using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AmmoUIManager : MonoBehaviour 
{
	[Header("-Drag Ammo Texts Here-")]
	public Text currentAmmoText;
	public Text maxAmmoText;
	public Text totalAmmoText;

	//private float currentAmmoCount;
	//private float maxAmmoCount;
	//private float totalAmmoCount;

	// Singleton
	public static AmmoUIManager instance;
	public static AmmoUIManager GetInstance()
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
			if(instance != this)
			{
				Destroy(instance.gameObject);
				instance = this;
			}
		}
	}

	void Start()
	{
		// Unequipped
		currentAmmoText.text = "-";
		maxAmmoText.text = "-";
		totalAmmoText.text = "Unequipped";
	}

	public void SetCurrentAmmo(int amount)
	{
		currentAmmoText.text = amount.ToString();
	}

	public void SetMaxAmmo(int amount)
	{
		maxAmmoText.text = amount.ToString();
	}

	public void SetTotalAmmo(int amount)
	{
		totalAmmoText.text = amount.ToString();
	}
}
