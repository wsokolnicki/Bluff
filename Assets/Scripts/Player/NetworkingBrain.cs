using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable 618

public class NetworkingBrain : NetworkBehaviour
{
    public static NetworkingBrain _instantiate;

    private void Awake()
    { _instantiate = this; }

    private void Start()
    {
        if (isServer)
        { Gameplay._instance.randomSeed = Random.Range(1, 99999); }
    }

    //currentPlayerIndex Update in Gameplay class, when next player activated
    [Command]
    public void CmdCurrnetPlayerIndexUpdate()
    {
        Gameplay._instance.IncreaseCurrentPlayerIndexValue();
    }

    //Updating players, by changing currentPlayer state to true or false
    [Command]
    public void CmdSetCurrentPlayerToFalse(int currPlIndx)
    {
        Gameplay._instance.playerArray[currPlIndx].GetComponent<Player>().currentPlayer = false;
    }
    [Command]
    public void CmdSetCurrentPlayerToTrue(int currPlIndx)
    {
        Gameplay._instance.playerArray[currPlIndx].GetComponent<Player>().currentPlayer = true;
    }

    [Command]
    public void CmdUpdateChosenVariant(int x, int y, int z)
    { Gameplay._instance.RpcUpdateChosenVariant(x, y, z); }

    [Command]
    public void CmdPlayerReadySync(bool ready)
    {
        GetComponent<Player>().RpcPlayerReadySync(ready);
    }

    [Command]
    public void CmdCheckButtonActivated()
    {
        Gameplay._instance.RpcCheckButtonSync();
    }

    [Command]
    public void CmdSetRandomSeed()
    {
        Gameplay._instance.randomSeed = Random.Range(1, 99999);
    }

    [ClientRpc]
    public void RpcRestartGame()
    {
        StartCoroutine(Gameplay._instance.RestartGameAction());
    }

    [Command]
    public void CmdNextRound()
    {
        StartCoroutine(Gameplay._instance.NextRound());
    }

    [Command]
    public void CmdLastPLayerIndexUpdate(int playerIndex)
    {
        Gameplay._instance.LastPlayerIndex = playerIndex;
    }
}

