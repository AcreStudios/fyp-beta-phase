using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AmmoUIManager : MonoBehaviour 
{
	[Header("-Drag Ammo Bar Here-")]
	public Transform AmmobarTrans;

	private GameObject[] ammobar;

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
		// Cache ammobar children
		AmmobarTrans = transform;
		ammobar = new GameObject[AmmobarTrans.childCount];

		for(int i = 0; i < AmmobarTrans.childCount; i++)
		{
			ammobar[i] = AmmobarTrans.GetChild(i).gameObject;
			ammobar[i].SetActive(false);
		}

		// Turn off at start
		ToggleAmmobar(false);
	}
	
	public void UpdateAmmobar(int curAmmo) // Call this to update ammobar 
	{
		if(curAmmo == 12)
		{
			foreach(GameObject b in ammobar)
				b.SetActive(true);
		}
		else
			ammobar[curAmmo].SetActive(false);

		print(curAmmo);

	}

	public void ToggleAmmobar(bool enabled) // Turn it on/off
	{
		transform.parent.gameObject.SetActive(enabled);
	}

}
