using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
#pragma warning disable 618
#pragma warning disable 0649

public class LobbyServerInfo : MonoBehaviour
{
    [SerializeField] Text roomInfoText;
    [SerializeField] Text slotInfo;
    [SerializeField] Button joinButton;

    public void Populate(MatchInfoSnapshot match, LobbyManager lobbyManager)
    {
        roomInfoText.text = match.name;
        slotInfo.text = match.currentSize.ToString() + "/" + match.maxSize.ToString();

        NetworkID networkID = match.networkId;

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => { JoinMatch(networkID, lobbyManager, match.name); });
    }

    void JoinMatch(NetworkID networkID, LobbyManager lobbyManager, string matchName)
    {
        lobbyManager.matchMaker.JoinMatch(networkID, "", "", "", 0, 0, lobbyManager.OnMatchJoined);
        lobbyManager._isMatchmaking = true;
    }
}

#pragma warning restore 0649