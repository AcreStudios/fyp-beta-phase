using UnityEngine;
using System.Collections;

public class QuitMenuControllerScript : MonoBehaviour {

    public NewMenuScript newMenuScript;
    public int selectedIndex;

	// Use this for initialization
	void Start() {
        selectedIndex = 0;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown("down")) {
            if (selectedIndex != 1) {
                selectedIndex += 1;
            }
        }

        if (Input.GetKeyDown("up")) {
            if (selectedIndex != 0) {
                selectedIndex -= 1;
            }
        }

        if (Input.GetKeyDown("return")) {
            if (selectedIndex == 0) {
                newMenuScript.SelectNo();
            }
            if (selectedIndex == 1) {
                newMenuScript.SelectYes();
            }
        }

        if(Input.GetKeyDown("escape")) {
            newMenuScript.SelectNo();
        }
        if (selectedIndex == 0) {
            handleSelection();
        }
        if (selectedIndex == 1) {
            handleSelection();
        }

        Debug.Log("selectedQuitIndex" + selectedIndex);
    }

    void handleSelection() {
        switch (selectedIndex) {
            case 0:
                newMenuScript.MONo();
                break;

            case 1:
                newMenuScript.MOYes();
                break;
        }
    }
}
