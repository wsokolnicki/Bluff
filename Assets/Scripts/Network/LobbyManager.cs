using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class LobbyManager : NetworkLobbyManager
{
    public static LobbyManager _singelton;

    protected ulong _currentMatchID = 0;
    protected bool _disconnectServer = false;
    [HideInInspector] public bool _isMatchmaking = false;

    [HideInInspector] public int _playerNumber = 0;

    protected RectTransform currentPanel = null;
    [SerializeField] private float prematchCountdown = 3.0f;

    [Space]
    [Header("UI References")]
    [SerializeField] private RectTransform mainMenuPanel = null;
    [SerializeField] private RectTransform roomPanel = null;
    [SerializeField] private Button mainMenuButton = null;
    public LobbyCountdownPanel CountdownPanel = null;

    public delegate void BackButtonDelegate();
    public BackButtonDelegate backButtonDelegate;
    public void GoBackButton()
    {
        backButtonDelegate();
    }

    protected LobbyHook _lobbyHooks;

    private void Start()
    {
        _singelton = this;
        _lobbyHooks = GetComponent<LobbyHook>();
        currentPanel = mainMenuPanel;
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        if (SceneManager.GetSceneAt(0).name == lobbyScene)
        {
            ChangeTo(roomPanel);
            if (_isMatchmaking)
            {
                if (conn.playerControllers[0].unetView.isServer)
                    backButtonDelegate = StopHostClbk;
                else
                    backButtonDelegate = StopClientClbk;
            }
            else
            {
                if (conn.playerControllers[0].unetView.isServer)
                    backButtonDelegate = StopHostClbk;
                else
                    backButtonDelegate = StopClientClbk;
            }
        }
        else
        {
            ChangeTo(null);
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        _currentMatchID = (System.UInt64)matchInfo.networkId;
    }

    public override void OnDestroyMatch(bool success, string extendedInfo)
    {
        base.OnDestroyMatch(success, extendedInfo);
        if(_disconnectServer)
        {
            StopMatchMaker();
            StopHost();
        }
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        ChangeTo(roomPanel);
    }

    public void StopHostClbk()
    {
        if (_isMatchmaking)
        {
            matchMaker.DestroyMatch((NetworkID)_currentMatchID, 0, OnDestroyMatch);
            _disconnectServer = true;
        }
        else
            StopHost();

        ChangeTo(mainMenuPanel);
    }

    public void StopClientClbk()
    {
        StopClient();
        if (_isMatchmaking)
            StopMatchMaker();
        ChangeTo(mainMenuPanel);
    }

    public void OnPlayerNumberModified(int count)
    {
        //if(GetComponent<NetworkIdentity>().isServer)
        _playerNumber += count;
    }

    public void ChangeTo(RectTransform newPanel)
    {
        if (currentPanel != null)
            currentPanel.gameObject.SetActive(false);

        if (newPanel != null)
            newPanel.gameObject.SetActive(true);

        currentPanel = newPanel;
    }

    public void AddLocalPlayer()
    {
        TryToAddPlayer();
    }

    public void RemovePlayer(LobbyPlayerInfo player)
    {
        player.RemovePlayer();
    }

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject obj = (GameObject)Instantiate(lobbyPlayerPrefab.gameObject);

        LobbyPlayerInfo newPlayer = obj.GetComponent<LobbyPlayerInfo>();
        newPlayer.ToggleReadyButton(numPlayers + 1 >= minPlayers);

        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            LobbyPlayerInfo p = lobbySlots[i] as LobbyPlayerInfo;

            if (p != null && !p.PlayerReady)
                p.ToggleReadyButton(numPlayers + 1 >= minPlayers);
        }
        return obj;
    }

    public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
    {
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            LobbyPlayerInfo p = lobbySlots[i] as LobbyPlayerInfo;

            if (p != null)
                p.ToggleReadyButton(numPlayers + 1 >= minPlayers);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        if (!NetworkServer.active)
            ChangeTo(roomPanel);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        ChangeTo(mainMenuPanel);
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        if (_lobbyHooks)
            _lobbyHooks.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);

        return true;
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        foreach (var lobbys in lobbySlots)
        {
            if (lobbys == null) continue;
            var controllerId = lobbys.GetComponent<NetworkIdentity>().playerControllerId;
            Transform startPos = GetStartPosition();
            GameObject gamePlayer;
            if (startPos != null)
            {
                gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, startPos.position, startPos.rotation);
                gamePlayer.transform.SetParent(GameObject.FindGameObjectWithTag("PlayersInGame").transform);
            }
            else
            {
                gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
                gamePlayer.transform.SetParent(GameObject.FindGameObjectWithTag("PlayersInGame").transform);
            }
            OnLobbyServerSceneLoadedForPlayer(lobbys.gameObject, gamePlayer);
            NetworkServer.ReplacePlayerForConnection(lobbys.GetComponent<NetworkIdentity>().connectionToClient, gamePlayer, controllerId);
        }
    }

    //Countdown
    public override void OnLobbyServerPlayersReady()
    {
        bool allready = true;
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            if (lobbySlots[i] != null)
                allready &= lobbySlots[i].readyToBegin;
        }

        if (allready)
            StartCoroutine(ServerCountdownCoroutine());
    }

    public IEnumerator ServerCountdownCoroutine()
    {
        float remainingTime = prematchCountdown;
        int floorTime = Mathf.FloorToInt(remainingTime);

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.FloorToInt(remainingTime);

            if (newFloorTime != floorTime)
            {
                floorTime = newFloorTime;

                for (int i = 0; i < lobbySlots.Length; ++i)
                {
                    if (lobbySlots[i] != null)
                        (lobbySlots[i] as LobbyPlayerInfo).RpcUpdateCountdown(floorTime);
                }
            }
        }

        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            if (lobbySlots[i] != null)
                (lobbySlots[i] as LobbyPlayerInfo).RpcUpdateCountdown(0);
        }

        ServerChangeScene(playScene);
    }
}