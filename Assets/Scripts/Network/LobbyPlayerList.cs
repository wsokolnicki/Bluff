using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerList : MonoBehaviour
{
    public static LobbyPlayerList _instance;

    [SerializeField] private LobbyManager lobbyManager = null;
    [SerializeField] private RectTransform playerListRectTransform = null;

    protected VerticalLayoutGroup _layout;
    /*protected*/public List<LobbyPlayerInfo> _players = new List<LobbyPlayerInfo>();

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
