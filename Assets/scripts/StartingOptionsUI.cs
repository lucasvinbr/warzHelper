﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingOptionsUI : ModeUI {

    public GameObject initialOptions, templateOptions, gameOptions;
	

    public override void Initialize()
    {
        initialOptions.SetActive(true);
        templateOptions.SetActive(false);
        gameOptions.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

	public override void Cleanup() {
		Initialize();
	}
}
