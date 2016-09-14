using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrosshairManager : MonoBehaviour 
{
	[Header("-Manage Crosshairs-")]
	public GameObject activeCrosshair;
	public GameObject[] crosshairTypes;
	//public Crosshair activeCrosshair;
	//public Crosshair[] crosshairTypes;

	// Singleton
	public static CrosshairManager instance;
	public static CrosshairManager GetInstance()
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
		FindCrosshairs();
	}

	private void FindCrosshairs()
	{
		if(transform.childCount == 0)
			return;

		crosshairTypes = new GameObject[transform.childCount];
		//crosshairTypes = new Crosshair[transform.childCount];

		for(int i = 0; i < transform.childCount; i++)
		{
			crosshairTypes[i] = transform.GetChild(i).gameObject;
			//crosshairTypes[i] = transform.GetChild(i).GetComponent<Crosshair>();
		}

		InitialiseCrosshairs();
	}

	private void InitialiseCrosshairs() // Set active only one crosshair, by index
	{
		if(activeCrosshair)
			return;

		for(int i = 0; i < crosshairTypes.Length; i++)
		{
			crosshairTypes[i].gameObject.SetActive(false);
		}

		crosshairTypes[0].gameObject.SetActive(true);
		activeCrosshair = crosshairTypes[0];
	}

	public void DefineCrosshairByIndex(int findIndex) // If we want to assign the crosshair by index
	{
		activeCrosshair.SetActive(false);
		activeCrosshair = crosshairTypes[findIndex];
		activeCrosshair.SetActive(true);

	}

	//public void DefineCrosshairByName(string findName) // If we want to assign by name
	//{
	//	for(int i = 0; i < crosshairTypes.Length; i++)
	//	{
	//		if(string.Equals(crosshairTypes[i].name, findName))
	//		{
	//			activeCrosshair = crosshairTypes[i];
	//			break;
	//		}
	//	}
	//}
}
