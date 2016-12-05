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
	}
	
	public void UpdateAmmobar(int curAmmo) // Call this to update ammobar 
	{
		switch(curAmmo)
		{
			case 0:
				ammobar[0].SetActive(false);
				break;
			case 1:
				ammobar[1].SetActive(false);
				break;
			case 2:
				ammobar[2].SetActive(false);
				break;
			case 3:
				ammobar[3].SetActive(false);
				break;
			case 4:
				ammobar[4].SetActive(false);
				break;
			case 5:
				ammobar[5].SetActive(false);
				break;
			case 6:
				ammobar[6].SetActive(false);
				break;
			case 7:
				ammobar[7].SetActive(false);
				break;
			case 8:
				ammobar[8].SetActive(false);
				break;
			case 9:
				ammobar[9].SetActive(false);
				break;
			case 10:
				ammobar[10].SetActive(false);
				break;
			case 11:
				ammobar[11].SetActive(false);
				break;
			default:
				// Reset bar to full
				foreach(GameObject b in ammobar)
					b.SetActive(true);
				break;
		}
	}

}
