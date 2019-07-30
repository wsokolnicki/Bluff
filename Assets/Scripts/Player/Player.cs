using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
#pragma warning disable 618
#pragma warning disable 0649

public class Player : NetworkBehaviour
{
    //cache
    [SerializeField] Text playerNameText;
    [SerializeField] GameObject playerNameGO;
    [SerializeField] GameObject playersTurn;
    public GameObject particlePlusOne;

    public int noOfCardsInHand = 1;
    public int currentNoOfCardsInHand;
    //state
    [SyncVar(hook = "OnStateChange")]
    public bool currentPlayer = false;

    [SyncVar(hook = "OnChangeName")]
    public string playerName = "Player";

    [SyncVar]
    public int currentPlayerIndex;

    [SyncVar]
    public bool playerReady = false;

    [SyncVar]
    public bool playerLost = false;

    // --------==== Hook Functions && Network ====-----------
    void OnChangeName(string name)
    {
        playerName = name;
        playerNameText.text = playerName;
    }

    void OnStateChange(bool isCurrentPlayer)
    {
        currentPlayer = isCurrentPlayer;

        if (currentPlayer)
        {
            GetComponent<SpriteRenderer>().color = Color.green;
            playersTurn.SetActive(true);
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            playersTurn.SetActive(false);
        }

        if (isLocalPlayer)
            TurnCheckButtonOn_Off(currentPlayer);
    }

    [ClientRpc]
    public void RpcPlayerReadySync(bool ready)
    {
        playerReady = ready;
        transform.Find("Player Ready").gameObject.SetActive(ready);
    }
    //---------------------------------------------

    public override void OnStartClient()
    {
        OnChangeName(playerName);
        OnStateChange(currentPlayer);
        base.OnStartClient();
    }

    private void Start()
    {
        Gameplay._instance.playerArray.Add(gameObject);
        playerNameText.text = playerName;
    }

    public void SetPlayerNameLocationOnBoard(float angle)
    {
        float nameRadiusX = 0.5f;
        float nameRadiusY = 1.3f;
        var position = playersNameEllipse(angle, transform.position, nameRadiusY, nameRadiusX);
        playerNameGO.transform.position = position;
        //SetTextAlignment(playerNameGO);
    }
    public void SetTextAlignment(/*GameObject name*/)
    {
        float offset = 1.3f;
        //if((Camera.main.WorldToViewportPoint(name.transform.position).x == 0.5f))
        //    playerNameGO.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        //else if(Camera.main.WorldToViewportPoint(name.transform.position).x < 0.5f)
        //    playerNameGO.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        //else
        //    playerNameGO.GetComponent<Text>().alignment = TextAnchor.MiddleRight;   
        if ((Camera.main.WorldToViewportPoint(transform.position).y >= 0.5f))
            playerNameGO.transform.Translate(0, -offset, 0);
        else
            playerNameGO.transform.Translate(0, offset, 0);
    }
    Vector3 playersNameEllipse(float ang, Vector3 center, float radiusA, float radiusB)
    {
        float angle = ang;
        Vector3 position;
        position.x = center.x + radiusB * Mathf.Sin(angle * Mathf.Deg2Rad);
        position.y = center.y + radiusA * Mathf.Cos(angle * Mathf.Deg2Rad);
        position.z = center.z;

        return position;
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            ButtonScript._inst.Show_HideAllActionButtons(currentPlayer);

            if (!Gameplay._instance.roundEnd || playerLost)
                return;
            else
            {
                if (Input.GetKeyUp(KeyCode.Space) || (Input.GetKeyUp(KeyCode.Mouse1)))
                {
                    GameObject space = Gameplay._instance.pressSpace;
                    playerReady = true;
                    space.SetActive(false);
                    space.GetComponent<SpaceMovement>().transform.position = space.GetComponent<SpaceMovement>().startPosition;
                    if (!isServer)
                        GetComponent<NetworkingBrain>().CmdPlayerReadySync(playerReady);
                    else
                        RpcPlayerReadySync(playerReady);
                }
            }
        }
    }

    public void TurnCheckButtonOn_Off(bool isCurrentPlayer)
    {
        Gameplay._instance.checkButton.SetActive(isCurrentPlayer);
    }

    public void TurnAllPlayerReadyFalse()
    {
        if (!isServer)
            GetComponent<NetworkingBrain>().CmdPlayerReadySync(false);
        else
            RpcPlayerReadySync(false);
    }
}

#pragma warning restore 0649
