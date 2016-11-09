using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseMenuScript : MonoBehaviour {

    public GameObject PauseCanvas;
    public GameObject RED_Resume;
    public GameObject RED_Reload;
    public GameObject RED_ExitMenu;
    public GameObject RED_ExitGame;
    public GameObject BLK_Resume;
    public GameObject BLK_Reload;
    public GameObject BLK_ExitMenu;
    public GameObject BLK_ExitGame;

	// Use this for initialization
	void Start () {
        PauseCanvas.SetActive(false);
        RED_Resume.SetActive(true);
        RED_Reload.SetActive(false);
        RED_ExitMenu.SetActive(false);
        RED_ExitGame.SetActive(false);
        BLK_Resume.SetActive(false);
        BLK_Reload.SetActive(true);
        BLK_ExitMenu.SetActive(true);
        BLK_ExitGame.SetActive(true);
	}
    #region MouseOver Events
    public void MOResume() {
        RED_Resume.SetActive(true);
        RED_Reload.SetActive(false);
        RED_ExitMenu.SetActive(false);
        RED_ExitGame.SetActive(false);
        BLK_Resume.SetActive(false);
        BLK_Reload.SetActive(true);
        BLK_ExitMenu.SetActive(true);
        BLK_ExitGame.SetActive(true);
    }
    public void MOReload() {
        RED_Resume.SetActive(false);
        RED_Reload.SetActive(true);
        RED_ExitMenu.SetActive(false);
        RED_ExitGame.SetActive(false);
        BLK_Resume.SetActive(true);
        BLK_Reload.SetActive(false);
        BLK_ExitMenu.SetActive(true);
        BLK_ExitGame.SetActive(true);
    }
    public void MOExitMenu() {
        RED_Resume.SetActive(false);
        RED_Reload.SetActive(false);
        RED_ExitMenu.SetActive(true);
        RED_ExitGame.SetActive(false);
        BLK_Resume.SetActive(true);
        BLK_Reload.SetActive(true);
        BLK_ExitMenu.SetActive(false);
        BLK_ExitGame.SetActive(true);
    }
    public void MOExitGame() {
        RED_Resume.SetActive(false);
        RED_Reload.SetActive(false);
        RED_ExitMenu.SetActive(false);
        RED_ExitGame.SetActive(true);
        BLK_Resume.SetActive(true);
        BLK_Reload.SetActive(true);
        BLK_ExitMenu.SetActive(true);
        BLK_ExitGame.SetActive(false);
    }
    #endregion

    #region Select Events
    public void SelectResume() {
        //Time.timeScale = 1;
        //PauseCanvas.SetActive(false);
        print("Resume Game");
    }
    public void SelectReload() {
        print("Reloading Last Checkpoint");
    }
    public void SelectExitMenu() {
        SceneManager.LoadScene("Menu_Improvised");
    }
    public void SelectExitGame() {
        Application.Quit();
    }
    #endregion
}
