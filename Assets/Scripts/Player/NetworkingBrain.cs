using UnityEngine;
using UnityEngine.Networking;

public class NetworkingBrain : NetworkBehaviour
{
    public static NetworkingBrain _instantiate;

    private void Awake()
    { 
        _instantiate = this; 
    }

    private void Start()
    {
        if (isServer)
        {
            GameplayManager._instance.randomSeed = Random.Range(1, 99999); 
        }
    }

    //currentPlayerIndex Update in Gameplay class, when next player activated
    [Command]
    public void CmdCurrnetPlayerIndexUpdate()
    {
        GameplayManager._instance.IncreaseCurrentPlayerIndexValue();
    }

    //Updating players, by changing currentPlayer state to true or false
    [Command]
    public void CmdSetCurrentPlayerToFalse(int currPlIndx)
    {
        GameplayManager._instance.playerArray[currPlIndx].GetComponent<Player>().CurrentPlayer = false;
    }
    [Command]
    public void CmdSetCurrentPlayerToTrue(int currPlIndx)
    {
        GameplayManager._instance.playerArray[currPlIndx].GetComponent<Player>().CurrentPlayer = true;
    }

    [Command]
    public void CmdUpdateChosenVariant(int x, int y, int z)
    { GameplayManager._instance.RpcUpdateChosenVariant(x, y, z); }

    [Command]
    public void CmdPlayerReadySync(bool ready)
    {
        GetComponent<Player>().RpcPlayerReadySync(ready);
    }

    [Command]
    public void CmdCheckButtonActivated()
    {
        GameplayManager._instance.RpcCheckButtonSync();
    }

    [Command]
    public void CmdSetRandomSeed()
    {
        GameplayManager._instance.randomSeed = Random.Range(1, 99999);
    }

    [ClientRpc]
    public void RpcRestartGame()
    {
        StartCoroutine(GameplayManager._instance.RestartGameAction());
    }

    [Command]
    public void CmdNextRound()
    {
        StartCoroutine(GameplayManager._instance.NextRound());
    }

    [Command]
    public void CmdLastPLayerIndexUpdate(int playerIndex)
    {
        GameplayManager._instance.LastPlayerIndex = playerIndex;
    }
}

