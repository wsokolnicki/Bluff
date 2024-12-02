using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class LobbyServerList : MonoBehaviour
{
    [SerializeField] private LobbyManager lobbyManager = null;
    [SerializeField] private RectTransform serverListRect = null;
    [SerializeField] private GameObject serverInfoPrefab = null;
    [SerializeField] private GameObject noServersFound = null;

    private void OnEnable()
    {
        noServersFound.SetActive(false);
        lobbyManager.matchMaker.ListMatches(0,6,"",true,0,0,OnGUIMatchList);
    }

    public void RefreshButton()
    {
        DestroyAllMatches();
        lobbyManager.matchMaker.ListMatches(0, 6, "", true, 0, 0, OnGUIMatchList);
    }

    private void DestroyAllMatches()
    {
        GameObject[] matches = GameObject.FindGameObjectsWithTag("Server");
        foreach (GameObject match in matches)
        {
            Destroy(match);
        }
    }

    public void OnGUIMatchList(bool succes, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        if (matches.Count == 0)
        {
            noServersFound.SetActive(true);
            return;
        }

        noServersFound.SetActive(false);

        for(int i=0; i<matches.Count; ++i)
        {
            GameObject server = (GameObject)Instantiate(serverInfoPrefab);
            server.GetComponent<LobbyServerInfo>().Populate(matches[i], lobbyManager);
            server.transform.SetParent(serverListRect, false);
        }
    }
}
