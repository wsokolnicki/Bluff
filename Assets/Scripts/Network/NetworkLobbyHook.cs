using UnityEngine;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayerInfo lobby = lobbyPlayer.GetComponent<LobbyPlayerInfo>();
        Player player = gamePlayer.GetComponent<Player>();

        player.PlayerName = lobby.PlayerName;
    }
}
