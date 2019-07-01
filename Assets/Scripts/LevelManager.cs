﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] GameObject lobbyPrefab;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
            Instantiate(lobbyPrefab);
    }

    public static void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene(0);
    }

    public void OnPlayClick()
    {
        SceneManager.LoadScene(1);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}
