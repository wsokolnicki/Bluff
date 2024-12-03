using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    //cache
    [SerializeField] private Text playerNameText = null;
    [SerializeField] private GameObject playerNameGO = null;
    [SerializeField] private GameObject playersTurn = null;
    public GameObject ParticlePlusOne = null;

    public int NoOfCardsInHand = 1;
    public int CurrentNoOfCardsInHand = 0;
    //state
    [SyncVar(hook = "OnStateChange")]
    public bool CurrentPlayer = false;

    [SyncVar(hook = "OnChangeName")]
    public string PlayerName = "Player";

    [SyncVar]
    public int CurrentPlayerIndex = 0;

    [SyncVar]
    public bool IsplayerReady = false;

    [SyncVar]
    public bool PlayerLost = false;

    // --------==== Hook Functions && Network ====-----------
    void OnChangeName(string name)
    {
        PlayerName = name;
        playerNameText.text = PlayerName;
    }

    void OnStateChange(bool isCurrentPlayer)
    {
        CurrentPlayer = isCurrentPlayer;

        if (CurrentPlayer)
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
            TurnCheckButtonOn_Off(CurrentPlayer);
    }

    [ClientRpc]
    public void RpcPlayerReadySync(bool ready)
    {
        IsplayerReady = ready;
        transform.Find("Player Ready").gameObject.SetActive(ready);
    }
    //---------------------------------------------

    public override void OnStartClient()
    {
        OnChangeName(PlayerName);
        OnStateChange(CurrentPlayer);
        base.OnStartClient();
    }

    private void Start()
    {
        GameplayManager._instance.playerArray.Add(gameObject);
        playerNameText.text = PlayerName;
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
            UIManager._inst.InGameSelectionMenuManager(CurrentPlayer);

            if (!GameplayManager._instance.roundEnd || PlayerLost)
                return;
            else
            {
                if (Input.GetKeyUp(KeyCode.Space) || (Input.GetKeyUp(KeyCode.Mouse1)))
                {
                    GameObject space = GameplayManager._instance.UI_ContinueGame;
                    IsplayerReady = true;
                    space.SetActive(false);
                    space.GetComponent<SpaceMovement>().transform.position = space.GetComponent<SpaceMovement>().StartPosition;
                    if (!isServer)
                        GetComponent<NetworkingBrain>().CmdPlayerReadySync(IsplayerReady);
                    else
                        RpcPlayerReadySync(IsplayerReady);
                }
            }
        }
    }

    public void TurnCheckButtonOn_Off(bool isCurrentPlayer)
    {
        GameplayManager._instance.UI_CheckButton.SetActive(isCurrentPlayer);
    }

    public void TurnAllPlayerReadyFalse()
    {
        if (!isServer)
            GetComponent<NetworkingBrain>().CmdPlayerReadySync(false);
        else
            RpcPlayerReadySync(false);
    }
}
