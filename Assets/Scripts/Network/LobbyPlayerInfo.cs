using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LobbyPlayerInfo : NetworkLobbyPlayer
{
    public static LobbyPlayerInfo _instace;

    [SerializeField] private Text playerNameText = null;
    [SerializeField] private Button readyButton = null; 
    [SerializeField] private Image readyImage = null;
    [SerializeField] private InputField noOfCards = null;

    [SyncVar] public bool PlayerReady = false;

    private bool isHost = false;
    [SyncVar(hook = "OnMyName")] public string PlayerName = "";

    public void OnMyName(string name)
    {
        PlayerName = name;
        playerNameText.text = PlayerName;
    }

    private void OnEnable()
    {
        _instace = this;
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        if (LobbyManager._singelton != null)
        {
            LobbyManager._singelton.OnPlayerNumberModified(1);
        }

        LobbyPlayerList._instance.AddPlayer(this);

        if (isLocalPlayer)
            SetupLocalPlayer();
        else
            SetupOtherPlayer();
                
        OnMyName(PlayerName);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        SetupLocalPlayer();
    }

    private void SetupOtherPlayer()
    {
        SyncPlayerName(PlayerName);

        //noOfCards.interactable = isHost;

        playerNameText.text = PlayerName;
        readyButton.transform.GetChild(0).GetComponent<Text>().text = "...";
        readyButton.interactable = false;

        if (!PlayerReady)
            OnClientReady(false);
        else
            OnClientReady(true);
    }

    void SetupLocalPlayer()
    {
        //if (LobbyManager._singelton._playerNumber == 1)
        //{
        //    isHost = true;
        //    noOfCards.interactable = true;
        //}
        //else
        //    noOfCards.interactable = false;

        SetLocalPlayerGreen();

        CmdNameChange(PlayerInfo.playerName);
        playerNameText.text = PlayerName;

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
        PlayerName = name;
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
            PlayerReady = readyState;
            SetPlayerReady();
        }
        else
        {
            if (LobbyManager._singelton._playerNumber >= LobbyManager._singelton.minPlayers)
            {
                readyButton.gameObject.SetActive(true);
            }

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
        PlayerName = name;
    }

    [ClientRpc]
    public void RpcUpdateCountdown(int countdown)
    {
        LobbyManager._singelton.CountdownPanel.UIText.text = "Match Starting in " + countdown;
        LobbyManager._singelton.CountdownPanel.gameObject.SetActive(countdown != 0);
    }

    public void OnDestroy()
    {
        LobbyPlayerList._instance.RemovePlayer(this);
        if (LobbyManager._singelton != null) LobbyManager._singelton.OnPlayerNumberModified(-1);
    }
}
