using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingOptions : MonoBehaviour {

    public GameObject initialOptions, templateOptions, gameOptions;
	

    public void ShowInitialOptions()
    {
        initialOptions.SetActive(true);
        templateOptions.SetActive(false);
        gameOptions.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
