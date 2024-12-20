﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private InputField playerName = null;
    [SerializeField] private GameObject playerNameChange = null;

    private void Awake()
    {
        if (PlayerInfo.playerName == null)
        {
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetAxis("Submit") > 0)
        {
            PlayerInfo.playerName = playerName.text;
            gameObject.transform.GetChild(1).gameObject.SetActive(false);
        }

        if (playerNameChange.activeSelf)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                playerNameChange.SetActive(false);
            }
        }
    }

    public void ChangeName()
    {
        playerNameChange.SetActive(true);
    }
}

#pragma warning restore 0649