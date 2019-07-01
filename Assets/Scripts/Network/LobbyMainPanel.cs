using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyMainPanel : MonoBehaviour
{
    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] RectTransform lobbyServerList;

    public void OnMainMenuButtonClick()
    {
        LevelManager.OnMainMenuButtonClicked();
        Destroy(lobbyManager.gameObject, 0.1f);
    }

    public void OnClickCreateMatchMakingGame()
    {
        lobbyManager.StartMatchMaker();
        lobbyManager.matchMaker.CreateMatch(
            (PlayerInfo.playerName + "'s Room"),
            (uint)lobbyManager.maxPlayers,
            true,
            "", "", "",
            0, 0,
            lobbyManager.OnMatchCreate);

        lobbyManager.backButtonDelegate = lobbyManager.StopHostClbk;
        lobbyManager._isMatchmaking = true;
    }

    public void OnClickOpenServersList()
    {
        lobbyManager.StartMatchMaker();
        lobbyManager.backButtonDelegate = lobbyManager.StopClientClbk;
        lobbyManager.ChangeTo(lobbyServerList);
    }
}
