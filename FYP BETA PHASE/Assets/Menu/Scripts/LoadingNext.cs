using UnityEngine;
using System.Collections;

public class LoadingNext : MonoBehaviour {

    public NewMenuScript menuScript;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown("space")) {
            menuScript.loadApplication();
        }
	}
}
