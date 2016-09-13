using UnityEngine;
using System.Collections;

public class CreditsMenuControlScript : MonoBehaviour {

    public NewMenuScript newMenuScript;
    int selectedIndex = 0;
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("return")) {
            if (selectedIndex == 0) {
                newMenuScript.SelectBack();
            }
        }
        if(Input.GetKeyDown("escape")) {
            newMenuScript.SelectBack();
        }
    }
}
