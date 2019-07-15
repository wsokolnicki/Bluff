using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
#pragma warning disable 618
#pragma warning disable 0649

public class LobbyPlayerInfo : NetworkLobbyPlayer
{
    public static LobbyPlayerInfo _instace;

    [SerializeField] Text playerNameText;
    [SerializeField] Button readyButton; 
    [SerializeField] Image readyImage;

    [SyncVar] public bool playerReady = false;

    [SyncVar(hook = "OnMyName")] public string playerName;

    public void OnMyName(string name)
    {
        playerName = name;
        playerNameText.text = playerName;
    }

    private void OnEnable()
    {
        _instace = this;
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        if (LobbyManager._singelton != null) LobbyManager._singelton.OnPlayerNumberModified(1);

        LobbyPlayerList._instance.AddPlayer(this);

        if (isLocalPlayer)
            SetupLocalPlayer();
        else
            SetupOtherPlayer();
                
        OnMyName(playerName);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        SetupLocalPlayer();
    }

    private void SetupOtherPlayer()
    {
        SyncPlayerName(playerName);
        playerNameText.text = playerName;
        readyButton.transform.GetChild(0).GetComponent<Text>().text = "...";
        readyButton.interactable = false;

        if (!playerReady)
            OnClientReady(false);
        else
            OnClientReady(true);
    }

    void SetupLocalPlayer()
    {
        SetLocalPlayerGreen();

        CmdNameChange(PlayerInfo.playerName);
        playerNameText.text = playerName;

        readyButton.transform.GetChild(0).GetComponent<Text>().text = "Ready";
        readyButton.interactable = true;

        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(OnReadyClicked);
    }

    private void SetLocalPlayerGreen()
    {
        Image image = gameObject.GetComponent<Image>();
        var tempColor = Color.green;
        tempColor.a = 0.3f;
        image.color = tempColor;
    }

    public void SyncPlayerName(string name)
    {
        playerName = name;
    }

    public void ToggleReadyButton(bool enabled)
    {
        readyButton.gameObject.SetActive(enabled);
    }

    public void OnReadyClicked()
    {
        SendReadyToBeginMessage();
    }

    public override void OnClientReady(bool readyState)
    {
        if(readyState)
        {
            playerReady = readyState;
            SetPlayerReady();
        }
        else
        {
            if (LobbyManager._singelton._playerNumber >= LobbyManager._singelton.minPlayers)
                readyButton.gameObject.SetActive(true);

            readyButton.transform.GetChild(0).GetComponent<Text>().text = isLocalPlayer ? "Ready" : "...";
            readyButton.interactable = isLocalPlayer;
            readyImage.gameObject.SetActive(false);
        }
    }

    void SetPlayerReady()
    {
        readyButton.gameObject.SetActive(false);
        readyImage.gameObject.SetActive(true);
    }

    [Command]
    public void CmdNameChange(string name)
    {
        playerName = name;
    }

    [ClientRpc]
    public void RpcUpdateCountdown(int countdown)
    {
        LobbyManager._singelton.countdownPanel.UIText.text = "Match Starting in " + countdown;
        LobbyManager._singelton.countdownPanel.gameObject.SetActive(countdown != 0);
    }

    public void OnDestroy()
    {
        LobbyPlayerList._instance.RemovePlayer(this);
        if (LobbyManager._singelton != null) LobbyManager._singelton.OnPlayerNumberModified(-1);
    }
}

#pragma warning restore 0649 
