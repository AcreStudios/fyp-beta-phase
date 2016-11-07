using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuFromGame : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("p")) {
            SceneManager.LoadScene("Menu_Improvised");
        }
	}
}
