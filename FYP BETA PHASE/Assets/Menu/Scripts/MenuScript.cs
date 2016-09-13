using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

    //Start Screen//*****************
    [Header("Start Screen")]
    public GameObject StartCanvas;
    public Button PressToStart;

    //Main Menu//*****************
    [Header("Main Menu")]
    public GameObject MainCanvas;
    public GameObject GORedNewGame;
    public GameObject GORedSettings;
    public GameObject GORedCredits;
    public GameObject GORedQuit;
    public GameObject NewGame;
    public GameObject Settings;
    public GameObject Credits;
    public GameObject Quit;

    //Settings//*****************
    [Header("Settings Menu")]
    public GameObject SettingsCanvas;
    public GameObject GORedGame;
    public GameObject GORedAudio;
    public GameObject GORedVideo;
    public GameObject GORedBack;
    public GameObject Game;
    public GameObject Audio;
    public GameObject Video;
    public GameObject Back;

    //Quit//*****************
    [Header("Quit Menu")]
    public GameObject QuitCanvas;
    public GameObject GORedYes;
    public GameObject GORedNo;
    public GameObject Yes;
    public GameObject No;

    //Credits//*****************
    [Header("Credits Menu")]
    public GameObject CreditsCanvas;
    public GameObject GORedCreditsBack;

    //Loading//*****************
    [Header ("Loading Screen")]
    public GameObject LoadingCanvas;
    public GameObject PressToContinue;
    public GameObject PressToContinueButton;
    public GameObject LoadingText;
    public GameObject DoneText;

    //Loading//*****************
    [Header("Input Controller")]
    public float MenuNumb = 0;
    public float SettignsNumb = 0;
    public float CreditsNumb = 0;
    public float QuitNumb = 0;

    // Use this for initialization *****************
    void Start () {
        //Start Screen//
        PressToStart = PressToStart.GetComponent<Button>();

        StartCanvas.SetActive(true);

        PressToStart.enabled = true;

        //Main Menu//
        MainCanvas.SetActive(false);

        GORedNewGame.SetActive(true);
        GORedSettings.SetActive(false);
        GORedCredits.SetActive(false);
        GORedQuit.SetActive(false);

        NewGame.SetActive(false);
        Settings.SetActive(true);
        Credits.SetActive(true);
        Quit.SetActive(true);

        //Settings//
        SettingsCanvas.SetActive(false);

        GORedGame.SetActive(true);
        GORedAudio.SetActive(false);
        GORedVideo.SetActive(false);
        GORedBack.SetActive(false);

        Game.SetActive(false);
        Audio.SetActive(true);
        Video.SetActive(true);
        Back.SetActive(true);

        //Quit Menu//*****************
        QuitCanvas.SetActive(false);

        GORedYes.SetActive(false);
        GORedNo.SetActive(true);

        Yes.SetActive(true);
        No.SetActive(false);

        //Credits Menu//*****************
        CreditsCanvas.SetActive(false);

        GORedCreditsBack.SetActive(true);

        //Loading Screen//*****************
        LoadingCanvas.SetActive(false);
        PressToContinue.SetActive(false);
        PressToContinueButton.SetActive(false);
        LoadingText.SetActive(false);
        DoneText.SetActive(false);

    }

    //Main Menu Button Handling System *****************
    public void MONewGame() {
        NewGame.SetActive(false);
        Settings.SetActive(true);
        Credits.SetActive(true);
        Quit.SetActive(true);

        GORedNewGame.SetActive(true);
        GORedSettings.SetActive(false);
        GORedCredits.SetActive(false);
        GORedQuit.SetActive(false);
    }
    public void MOSettings() {
        NewGame.SetActive(true);
        Settings.SetActive(false);
        Credits.SetActive(true);
        Quit.SetActive(true);

        GORedNewGame.SetActive(false);
        GORedSettings.SetActive(true);
        GORedCredits.SetActive(false);
        GORedQuit.SetActive(false);

        MenuNumb = 1;
    }
    public void MOCredits() {
        NewGame.SetActive(true);
        Settings.SetActive(true);
        Credits.SetActive(false);
        Quit.SetActive(true);

        GORedNewGame.SetActive(false);
        GORedSettings.SetActive(false);
        GORedCredits.SetActive(true);
        GORedQuit.SetActive(false);

        MenuNumb = 2;
    }
    public void MOQuit() {
        NewGame.SetActive(true);
        Settings.SetActive(true);
        Credits.SetActive(true);
        Quit.SetActive(false);

        GORedNewGame.SetActive(false);
        GORedSettings.SetActive(false);
        GORedCredits.SetActive(false);
        GORedQuit.SetActive(true);

        MenuNumb = 3;
    }

    //Settings Button Handling System *****************
    public void MOGame() {
        GORedGame.SetActive(true);
        GORedAudio.SetActive(false);
        GORedVideo.SetActive(false);
        GORedBack.SetActive(false);

        Game.SetActive(false);
        Audio.SetActive(true);
        Video.SetActive(true);
        Back.SetActive(true);
    }
    public void MOAudio() {
        GORedGame.SetActive(false);
        GORedAudio.SetActive(true);
        GORedVideo.SetActive(false);
        GORedBack.SetActive(false);

        Game.SetActive(true);
        Audio.SetActive(false);
        Video.SetActive(true);
        Back.SetActive(true);
    }
    public void MOVideo() {
        GORedGame.SetActive(false);
        GORedAudio.SetActive(false);
        GORedVideo.SetActive(true);
        GORedBack.SetActive(false);

        Game.SetActive(true);
        Audio.SetActive(true);
        Video.SetActive(false);
        Back.SetActive(true);
    }
    public void MOBack() {
        GORedGame.SetActive(false);
        GORedAudio.SetActive(false);
        GORedVideo.SetActive(false);
        GORedBack.SetActive(true);

        Game.SetActive(true);
        Audio.SetActive(true);
        Video.SetActive(true);
        Back.SetActive(false);
    }

    //Quit Button Handling System *****************
    public void MOYes() {
        GORedYes.SetActive(true);
        GORedNo.SetActive(false);

        Yes.SetActive(false);
        No.SetActive(true);
    }
    public void MONo() {
        GORedYes.SetActive(false);
        GORedNo.SetActive(true);

        Yes.SetActive(true);
        No.SetActive(false);
    }

    //Selecting Individual Buttons Handling System *****************
    public void SelectSettings() {
        SettingsCanvas.SetActive(true);
        MainCanvas.SetActive(false);
    }
    public void SelectBack() {
        SettingsCanvas.SetActive(false);
        CreditsCanvas.SetActive(false);
        MainCanvas.SetActive(true);
    }
    public void SelectNewGame() {
        LoadingCanvas.SetActive(true);
        MainCanvas.SetActive(false);
        LoadingText.SetActive(true);

        StartCoroutine(loadingtime());
    }
    public void SelectCredits() {
        CreditsCanvas.SetActive(true);
        MainCanvas.SetActive(false);
    }
    public void SelectQuit() {
        QuitCanvas.SetActive(true);
        MainCanvas.SetActive(false);
    }
    public void SelectNewQuit() {
        MainCanvas.SetActive(false);
        StartCanvas.SetActive(true);
    }
    public void SelectStart() {
        MainCanvas.SetActive(true);
        StartCanvas.SetActive(false);
    }
    public void SelectNo() {
        QuitCanvas.SetActive(false);
        MainCanvas.SetActive(true);
    }
    public void SelectYes() {
        Application.Quit();
    }
    public void loadApplication() {
        //SceneManager.LoadScene("");
        Debug.Log("Scene Loaded");
    }


    IEnumerator loadingtime() {
        Debug.Log("Scene Loading");
        yield return new WaitForSeconds(3);
        PressToContinue.SetActive(true);
        PressToContinueButton.SetActive(true);
        LoadingText.SetActive(false);
        DoneText.SetActive(true);
    }

}
