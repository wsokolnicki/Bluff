using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempText : MonoBehaviour
{
    [SerializeField] Text currPlIndx;
    //[SerializeField] Text player1;
    //[SerializeField] Text player2;
    //[SerializeField] Text player3;
    //[SerializeField] Text player4;
    //[SerializeField] Text boolCurrPl1;
    //[SerializeField] Text boolCurrPl2;
    //[SerializeField] Text boolCurrPl3;
    //[SerializeField] Text boolCurrPl4;
    [SerializeField] Text seed;
    [SerializeField] GameplayManager gmplmng = null;

    //Text[] players;
    //Text[] bools;

    //bool x = false;

    //private void Start()
    //{
    //    Invoke("Dupa", 0.2f);
    //}

    // void Dupa()
    // {
    // players = new Text[4] { player1, player2, player3, player4 };
    // bools = new Text[4] { boolCurrPl1, boolCurrPl2, boolCurrPl3, boolCurrPl4 };

    // x = true;
    // }

    private void Update()
    {
        //if (!x)
        //    return;

        //currPlIndx.text = GameplayManager._instance.listOfPlayersThatLost.Count.ToString();

        //for (int i = 0; i < Gameplay._instance.numberOfPlayers; i++)
        //{
        //    players[i].text = Gameplay._instance.playerArray[i].GetComponent<Player>().playerName;
        //    bools[i].text = Gameplay._instance.playerArray[i].GetComponent<Player>().currentPlayer.ToString();
        //}
        //int dupa = GameplayManager._instance.lastPlayerIndex + 1;
        //seed.text = dupa.ToString();

        seed.text = gmplmng.playerLostRound == null ? "Null" : gmplmng.playerLostRound.name.ToString();
    }
}
