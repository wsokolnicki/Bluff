using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
#pragma warning disable 0649

public class LobbyPlayerList : MonoBehaviour
{
    public static LobbyPlayerList _instance;

    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] RectTransform playerListRectTransform;

    protected VerticalLayoutGroup _layout;
    protected List<LobbyPlayerInfo> _players = new List<LobbyPlayerInfo>();

    private void OnEnable()
    {
        _instance = this;
        _layout = playerListRectTransform.GetComponent<VerticalLayoutGroup>();
    }

    public void AddPlayer(LobbyPlayerInfo player)
    {
        if (_players.Contains(player))
            return;

        _players.Add(player);

        player.transform.SetParent(playerListRectTransform, false);
    }

    public void RemovePlayer(LobbyPlayerInfo player)
    {
        _players.Remove(player);
    }

    public int ReturnNumberOfPlayersInLobby()
    {
        return _players.Count;
    }
}

#pragma warning restore 0649
