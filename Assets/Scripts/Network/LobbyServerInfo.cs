using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class LobbyServerInfo : MonoBehaviour
{
    [SerializeField] private Text roomInfoText = null;
    [SerializeField] private Text slotInfo = null;
    [SerializeField] private Button joinButton = null;

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