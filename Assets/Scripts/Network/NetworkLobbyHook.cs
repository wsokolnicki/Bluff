using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
#pragma warning disable 618

public class NetworkLobbyHook : LobbyHook
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer,
        GameObject gamePlayer)
    {
        LobbyPlayerInfo lobby = lobbyPlayer.GetComponent<LobbyPlayerInfo>();
        Player player = gamePlayer.GetComponent<Player>();

        player.playerName = lobby.playerName;
    }
}
