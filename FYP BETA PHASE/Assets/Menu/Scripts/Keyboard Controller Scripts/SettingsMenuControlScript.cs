using UnityEngine;
using System;
using System.Collections;

public class SettingsMenuControlScript : MonoBehaviour {

    public NewMenuScript newMenuScript;
    public int selectedIndex;
    void Start() {
        selectedIndex = 0;
    }
	// Update is called once per frame
	void Update() {
        if (Input.GetKeyDown("down")) {
            if (selectedIndex != 3) {
                selectedIndex += 1;
            }
        }

        if (Input.GetKeyDown("up")) {
            if (selectedIndex != 0) {
                selectedIndex -= 1;
            }
        }
        if (Input.GetKeyDown("return")) {
            if (selectedIndex == 3) {
                newMenuScript.SelectBack();
            }
        }
        if(Input.GetKeyDown("escape")) {
            newMenuScript.SelectBack();
        }
        if (selectedIndex == 0) {
            handleSelection();
        }
        if (selectedIndex == 1) {
            handleSelection();
        }
        if (selectedIndex == 2) {
            handleSelection();
        }
        if (selectedIndex == 3) {
            handleSelection();
        }

        Debug.Log("selectedSettingsIndex: " + selectedIndex);
    }

    void handleSelection() {
        switch (selectedIndex) {
            case 0:
                newMenuScript.MOGame();
                break;

            case 1:
                newMenuScript.MOAudio();
                break;

            case 2:
                newMenuScript.MOVideo();
                break;

            case 3:
                newMenuScript.MOBack();
                break;
        }
    }
}
