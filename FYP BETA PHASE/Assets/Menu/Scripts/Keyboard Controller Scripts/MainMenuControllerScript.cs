using UnityEngine;
using System;
using System.Collections;

public class MainMenuControllerScript : MonoBehaviour {

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
        if(Input.GetKeyDown("return")) {
            if(selectedIndex == 0) {
                newMenuScript.SelectNewGame();
            }
            if(selectedIndex == 1) {
                newMenuScript.SelectSettings();
            }
            if(selectedIndex == 2) {
                newMenuScript.SelectCredits();
            }
            if(selectedIndex == 3) {
                newMenuScript.SelectQuit();
            }
        }
        if(Input.GetKeyDown("escape")) {
            newMenuScript.SelectQuit();
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

        Debug.Log("selectedMenuIndex: " + selectedIndex);
    }

    void handleSelection() {
        switch (selectedIndex) {
            case 0:
                newMenuScript.MONewGame();
                break;

            case 1:
                newMenuScript.MOSettings();
                break;

            case 2:
                newMenuScript.MOCredits();
                break;

            case 3:
                newMenuScript.MOQuit();
                break; 
        }
    }
}