﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
#pragma warning disable 618
#pragma warning disable 0649

public class Gameplay : NetworkBehaviour
{

    [HideInInspector] public static Gameplay _instance;
    [Header("Cache")]
    [Header("-Classes")]
    [SerializeField] CardStack dealer;
    [Header("-Game Objects")]
    [SerializeField] GameObject deck;
    [SerializeField] GameObject checkedCardSingle;
    [SerializeField] GameObject checkedCardsDoubleUP;
    [SerializeField] GameObject checkedCardsDoubleDOWN;
    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject arrowPrefab;
    public GameObject pressSpace;
    public GameObject checkButton;
    //public GameObject buttonParent;
    [Header("-UI")]
    [SerializeField] Text lastChosenValueText;
    [SerializeField] Text winner;
    [SerializeField] Text cardsNo;
    [Header("-Variables")]
    [SerializeField] float delayBetweenCardsHandling;
    [SerializeField] float timeForHandlingCards = 2;
    [HideInInspector] public bool playersReady = false;
    [HideInInspector] public int maxCardsLosingCondition;


    [Header("Lists")]
    public List<GameObject> playerArray;
    public List<GameObject> cardsInGame;
    [HideInInspector] public List<int> listOfPlayersThatLost = new List<int>(); //temp public
    List<CardModel> checkedCardsList = new List<CardModel>();

    [HideInInspector] public GameObject localPlayer;
    private GameObject playerLostRound;
    [HideInInspector] public int numberOfPlayers; //public is temporary - delete if temptext class deleted
    [HideInInspector] public int noOfCardsInDeck;

    NetworkingBrain networkLocalPlayer;
    int tottalAmountOfCardsInGameThisRound = 0;

    [Header("SyncVar Variables")]
    [SyncVar(hook = "OnCurrentPlayerIndexChange")] public int currentPlayerIndex;
    [SyncVar(hook = "RandomSeedChange")] [HideInInspector] public int randomSeed = 0;
    [SyncVar] public bool roundEnd = false;
    public ChosenVariant chosenVariant;
    [SyncVar(hook = "OnLastPlayerIndexChange")] public int lastPlayerIndex;

    //temp
    public bool deckCreated = false;

    public struct ChosenVariant
    {
        public int actionValue;
        public int firstCardValue;
        public int secondCardValue;

        public ChosenVariant(int aV, int fCV, int sCV)
        {
            actionValue = aV;
            firstCardValue = fCV;
            secondCardValue = sCV;
        }

        public string GetVariablesString()
        {
            return (actionValue.ToString() + " " + firstCardValue.ToString() + " " + secondCardValue.ToString());
        }

        public int[] GetVariables()
        {
            int[] variables = { actionValue, firstCardValue, secondCardValue };
            return variables;
        }
    }

    public int LastPlayerIndex
    {
        set { lastPlayerIndex = value; }
        get { return lastPlayerIndex; }
    }
    //-------==== Hook functions ====---------
    void OnCurrentPlayerIndexChange(int currPlIndx)
    {
        currentPlayerIndex = currPlIndx;
    }
    void OnLastPlayerIndexChange(int lastPlIndx)
    {
        lastPlayerIndex = lastPlIndx;
    }
    void RandomSeedChange(int seed)
    {
        randomSeed = seed;
    }

    public class ChosenVariantSyncClass : SyncListStruct<ChosenVariant> { }
    public ChosenVariantSyncClass chosenVariantList = new ChosenVariantSyncClass();

    [ClientRpc]
    public void RpcUpdateChosenVariant(int aV, int fCV, int sCV)
    {
        chosenVariant = new ChosenVariant(aV, fCV, sCV);
        ButtonScript._inst.DisableTopButtonIfChosenVariantGrater(aV, fCV, sCV);
        if (aV == 0)
            checkButton.gameObject.SetActive(false);
    }
    //------------------------------------------

    private void Awake()
    { _instance = this; }

    private void Start()
    { StartCoroutine(CreatingPlayersAndStartingTheGame()); }

    private void Update()
    {
        lastChosenValueText.text = NamingCards.SelectedValue
            (chosenVariant.actionValue, chosenVariant.firstCardValue, chosenVariant.secondCardValue);

        cardsNo.text = tottalAmountOfCardsInGameThisRound.ToString();

        if (isServer)
        {// Loop that checks all the players if they are ready for next round
            for (int i = 0; i < playerArray.Count; i++)
            {
                if (!playerArray[i].GetComponent<Player>().playerReady || listOfPlayersThatLost.Count == (playerArray.Count - 1))
                    break;
                else
                {
                    if (i == playerArray.Count - 1)
                        playersReady = true;
                }
            }
        }
    }

    //--------Setting players in a right places and initiation of a game
    private IEnumerator CreatingPlayersAndStartingTheGame()
    {
        yield return new WaitUntil(() => playerArray.Count == LobbyManager._singelton._playerNumber);

        numberOfPlayers = playerArray.Count;

        Vector3 center = Vector3.zero;
        int i = 0;
        int x = 0;

        foreach (GameObject player in playerArray)
        {
            player.GetComponent<CardStack>().playerIndex = i;
            if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                localPlayer = player;
                player.GetComponent<CardStack>().mainPlayer = true;
                x = player.GetComponent<CardStack>().playerIndex;
            }
            i++;
        }
        int z = 0;

        do
        {
            float ang = z * (360 / numberOfPlayers);     //angle value that grows each time loop reapeats
            var position = Ellipse(ang, center, 3.5f, 5f);
            playerArray[x].transform.position = position;
            playerArray[x].transform.SetParent(GameObject.Find("Players").transform);
            //playerArray[x].GetComponent<Player>().SetPlayerNameLocationOnBoard(ang);
            playerArray[x].GetComponent<Player>().SetTextAlignment();
            playerArray[x].name = playerArray[x].GetComponent<Player>().playerName;
            x++;
            z++;
            if (x == numberOfPlayers)
                x = 0;
        }
        while (z < numberOfPlayers);

        GameStart();
    }
    private Vector3 Ellipse(float ang, Vector3 center, float radiusA, float radiusB)
    {
        float angle = ang;
        Vector3 position;
        position.x = center.x - radiusB * Mathf.Sin(angle * Mathf.Deg2Rad);
        position.y = center.y - radiusA * Mathf.Cos(angle * Mathf.Deg2Rad);
        position.z = center.z;

        return position;
    }


    //--------Starting a game - setting randomly first(active) player and handling cards
    void GameStart()
    {
        networkLocalPlayer = localPlayer.GetComponent<NetworkingBrain>();

        if (localPlayer.GetComponent<NetworkIdentity>().isServer)
            currentPlayerIndex = Random.Range(0, playerArray.Count);

        playerArray[currentPlayerIndex].GetComponent<Player>().currentPlayer = true;
        checkButton.gameObject.SetActive(false);
        maxCardsLosingCondition = HowManyCardsLosingCondition(numberOfPlayers);
        StartCoroutine(HandlingCards());
    }
    IEnumerator HandlingCards()
    {
        //Wait until deck is created (necessary due to delay between server and clients - server generates Random.seed)
        yield return new WaitUntil(() => deck.transform.childCount == noOfCardsInDeck);
        deckCreated = true;
        tottalAmountOfCardsInGameThisRound = 0;
        foreach (GameObject player in playerArray)
        {
            Player plr = player.GetComponent<Player>();
            if (plr.playerLost)
                continue;
            plr.currentNoOfCardsInHand = 0;
            tottalAmountOfCardsInGameThisRound += plr.noOfCardsInHand;
        }

        int topCardIndex = 1;
        delayBetweenCardsHandling =
            ((timeForHandlingCards / tottalAmountOfCardsInGameThisRound) > 0.5f) ? 0.5f : timeForHandlingCards / tottalAmountOfCardsInGameThisRound;

        while (cardsInGame.Count < tottalAmountOfCardsInGameThisRound)
        {
            for (int x = 0; x < numberOfPlayers; x++)
            {
                if (playerArray[x].GetComponent<Player>().currentNoOfCardsInHand <
                    playerArray[x].GetComponent<Player>().noOfCardsInHand &&
                    !playerArray[x].GetComponent<Player>().playerLost)
                {
                    AddCardToPlayerHand(topCardIndex, playerArray[x]);
                    topCardIndex++;
                    yield return new WaitForSeconds(delayBetweenCardsHandling);
                }
            }
        }
    }
    void AddCardToPlayerHand(int topCardInTheDeckIndex, GameObject player)
    {
        //It takes existing card (gameObject) and moves it to players hand. Then adds to cardsInGame List
        GameObject currentCard = deck.transform.GetChild(noOfCardsInDeck - topCardInTheDeckIndex).gameObject;

        currentCard.GetComponent<CardModel>().PrepareCardForHandling(player, delayBetweenCardsHandling);

        if (player.GetComponent<CardStack>().mainPlayer || localPlayer.GetComponent<Player>().playerLost)
        {
            currentCard.GetComponent<CardModel>().ToggleFace(true);
            currentCard.GetComponent<Draggable>().playersChild = true;
        }
        player.GetComponent<Player>().currentNoOfCardsInHand++;
        cardsInGame.Add(currentCard);
    }

    //--------Next player functions - jumping from active one to the next player 
    public void NextPlayer()
    {
        if (GetComponent<NetworkIdentity>().isServer)
            NextPlayerIfServer();
        else
            StartCoroutine(NextPlayerIfNotServer());
    }
    void NextPlayerIfServer()
    {
        lastPlayerIndex = currentPlayerIndex;
        playerArray[currentPlayerIndex].GetComponent<Player>().currentPlayer = false;
        IncreaseCurrentPlayerIndexValue();
        playerArray[currentPlayerIndex].GetComponent<Player>().currentPlayer = true;
    }

    IEnumerator NextPlayerIfNotServer()
    {
        int tempCurrPlIndx = currentPlayerIndex;
        networkLocalPlayer.CmdLastPLayerIndexUpdate(currentPlayerIndex);
        networkLocalPlayer.CmdSetCurrentPlayerToFalse(currentPlayerIndex);
        networkLocalPlayer.CmdCurrnetPlayerIndexUpdate();
        yield return new WaitUntil(() => tempCurrPlIndx != currentPlayerIndex);
        networkLocalPlayer.CmdSetCurrentPlayerToTrue(currentPlayerIndex);
    }
    bool HasPlayerAlreadyLost(int currentPlayerIndex)
    {
        foreach (int i in listOfPlayersThatLost)
        {
            if (i == currentPlayerIndex)
                return true;
        }
        return false;
    }
    public void IncreaseCurrentPlayerIndexValue()
    {
        currentPlayerIndex = (currentPlayerIndex == numberOfPlayers - 1) ? 0 : currentPlayerIndex + 1;
        while (HasPlayerAlreadyLost(currentPlayerIndex))
            currentPlayerIndex = (currentPlayerIndex == numberOfPlayers - 1) ? 0 : currentPlayerIndex + 1;
    }

    //-------Check Button function - Checks if the chosen poker value is in game - moves the right cards to center
    public void CheckButton()
    {
        if (localPlayer.GetComponent<NetworkIdentity>().isServer)
        {
            RpcCheckButtonSync();
            StartCoroutine(NextRound());
        }
        else
        {
            networkLocalPlayer.CmdCheckButtonActivated();
            networkLocalPlayer.CmdNextRound();
        }
    }
    [ClientRpc]
    public void RpcCheckButtonSync()
    {
        ButtonScript._inst.Show_HideAllActionButtons(false);
        checkButton.SetActive(false);
        roundEnd = true;
        foreach (GameObject card in cardsInGame)
        {
            CardModel cardModel = card.GetComponent<CardModel>();
            if (cardModel.faceUp)
                continue;
            StartCoroutine(cardModel.FlipACard());
        }
        StartCoroutine(MovePlayingCardsToCenter
                (chosenVariant.actionValue, chosenVariant.firstCardValue, chosenVariant.secondCardValue));
    }
    IEnumerator MovePlayingCardsToCenter(int action, int firstCard, int secondCard)
    {
        List<int> alreadyChecked = new List<int>();
        foreach (GameObject card in cardsInGame)
        {
            CardModel cardModel = card.GetComponent<CardModel>();
            card.GetComponent<Draggable>().playersChild = false;

            yield return new WaitUntil(() => !cardModel.flipAnimation);

            if (secondCard != 0)
            { // Two Pairs and Full House - Action requires to return two card values
                if (cardModel.CardValue == firstCard || cardModel.CardValue == secondCard)
                    ChangeCardsParentTwoCards(card, firstCard, secondCard);
            } // High Card, One Pair etc. - Action requires to return one card value
            else if (action == 1 || action == 2 || action == 4 || action == 9)
            {
                if (cardModel.CardValue == firstCard)
                    ChangeCardsParent(card);
            }
            else if (action == 7)
            { //Flush - Action requires to return cards in the same suit
                if (cardModel.CardSuit == firstCard)
                    ChangeCardsParent(card);
            }
            else if (action == 5)
            { //Small Straight - Action requires to return cards from 9 to King
                if (cardModel.CardValue != 6)
                    ShowingCardWithoutRepetitions(alreadyChecked, card, cardModel);
            }
            else if (action == 6)
            { //Big Straight - Action requires to return cards from 10 to Ace
                if (cardModel.CardValue != 1)
                    ShowingCardWithoutRepetitions(alreadyChecked, card, cardModel);
            }
            else if (action == 10)
            { //Small Startigh Flush - Action requires to return cards from 9 to King in the same suit
                if (cardModel.CardSuit == firstCard && cardModel.CardValue != 6)
                    ChangeCardsParent(card);
            }
            else if (action == 11)
            {//Big Startigh Flush - Action requires to return cards from 10 to Ace in the same suit
                if (cardModel.CardSuit == firstCard && cardModel.CardValue != 1)
                    ChangeCardsParent(card);
            }
        }
        StartCoroutine(AddingArrows());
        AddingCardToLoser(action, firstCard, secondCard);
    }
    void ChangeCardsParent(GameObject card)
    {
        CardModel cardModel = card.GetComponent<CardModel>();
        //cardModel.playerName.text = card.transform.parent.parent.parent.GetComponent<Player>().playerName;
        cardModel.playerPosition = card.transform.parent.parent.parent.position;
        card.transform.SetParent(checkedCardSingle.transform);
        checkedCardsList.Add(cardModel);

        //Sets cards in order from 9 to Ace if straight
        if (cardModel.cardValue - 1 > checkedCardsList.Count)
            card.transform.SetSiblingIndex(checkedCardsList.Count);
        else
            card.transform.SetSiblingIndex(cardModel.cardValue - 1);
    }
    void ChangeCardsParentTwoCards(GameObject card, int first, int second)
    {
        CardModel cardModel = card.GetComponent<CardModel>();
        //cardModel.playerName.text = card.transform.parent.parent.parent.GetComponent<Player>().playerName;
        cardModel.playerPosition = card.transform.parent.parent.parent.position;
        if (cardModel.cardValue == first)
        {
            card.transform.SetParent(checkedCardsDoubleUP.transform);
            checkedCardsList.Add(cardModel);
        }
        else
        {
            card.transform.SetParent(checkedCardsDoubleDOWN.transform);
            checkedCardsList.Add(cardModel);
        }
    }
    IEnumerator AddingArrows()
    {
        //float x = 0;
        //float offset = 0.65f;
        int arrowSize = 7;
        float shortCoefficient = /*0.65f;*/ 0.8f;
        float arrowWidth = 0.3f;

        //GameObject[] cards = new GameObject[checkedCardSingle.transform.childCount];
        //for(int i=0; i<checkedCardSingle.transform.childCount; i++)
        //{
        //    cards[i] = checkedCardSingle.transform.GetChild(i).gameObject;
        //}
        yield return new WaitUntil(() => /*cards.Length == checkedCardSingle.transform.childCount*/
        checkedCardsList.Count == checkedCardSingle.transform.childCount ||
        checkedCardsList.Count == (checkedCardsDoubleUP.transform.childCount + checkedCardsDoubleDOWN.transform.childCount));

        foreach (/*GameObject*/CardModel card in /*cards*/checkedCardsList)
        {
            //card.playerName.gameObject.SetActive(true);

            //float position = Camera.main.WorldToViewportPoint(card.transform.position).y;
            //if (position >= 0.5f)
            //    card.playerName.gameObject.transform.Translate(new Vector3(0, offset, 0));
            //else
            //    card.playerName.gameObject.transform.Translate(new Vector3(0, -offset, 0));
            //CardModel cardmodel = card.GetComponent<CardModel>();
            Vector3 vectorBetweenCardAndPlayer = (card.playerPosition - card.transform.position).normalized;
            var magnitude = (card.playerPosition - card.transform.position).magnitude;
            var angle = Vector3.Angle(transform.up, vectorBetweenCardAndPlayer);
            int signAngle = (card.playerPosition.x > card.transform.position.x) ? -1 : 1;
            var arrowGO = Instantiate(arrowPrefab, card.playerPosition, Quaternion.identity);

            int signTranslate = (angle > 90) ? -1 : 1;

            //arrowGO.transform.Translate(new Vector3(0, signTranslate * offset, 0));
            //if (angle == 90)
            //{
            //    arrowGO.transform.Translate(new Vector3(0, signTranslate *x, 0));
            //    x += 0.1f;
            //}

            arrowGO.transform.eulerAngles = new Vector3(0, 0, signAngle * angle);
            arrowGO.transform.localScale = new Vector3(arrowWidth, -magnitude / arrowSize * shortCoefficient, 1);
            //arrowGO.GetComponent<SpriteRenderer>().sortingOrder = 7;

        }
    }
    private void ShowingCardWithoutRepetitions(List<int> alreadyChecked, GameObject card, CardModel cardModel)
    {
        if (alreadyChecked.Count == 0)
        {
            ChangeCardsParent(card);
            alreadyChecked.Add(cardModel.cardValue);
        }
        else
        {
            for (int y = 0; y < alreadyChecked.Count; y++)
            {
                if (IsDuplicated(cardModel.cardValue, alreadyChecked))
                    break;
                else
                {
                    ChangeCardsParent(card);
                    alreadyChecked.Add(cardModel.cardValue);
                }
            }
        }
    }
    static bool IsDuplicated(int temp, List<int> alreadyChecked)
    {
        foreach (int number in alreadyChecked)
        {
            if (temp == number)
                return true;
        }
        return false;
    }

    void AddingCardToLoser(int aV, int fCV, int sCV)
    {
        //CardModel[] checkedCards = GameObject.Find("Checked Cards").gameObject.GetComponentsInChildren<CardModel>();
        int numberOfFirstCards = 0;
        int numberOfSecondCards = 0;

        foreach (CardModel card in /*checkedCards*/checkedCardsList)
        {
            if (card.cardValue == fCV)
                numberOfFirstCards++;
            if (card.cardValue == sCV)
                numberOfSecondCards++;
        }

        if (aV == 3)
            IncreaceCardsNoInHand_V1(numberOfFirstCards >= 2 && numberOfSecondCards >= 2);
        else if (aV == 8)
            IncreaceCardsNoInHand_V1(numberOfFirstCards >= 3 && numberOfSecondCards >= 2);
        else
            IncreaceCardsNoInHand_V1(/*checkedCards.Length*/checkedCardsList.Count >= HowManyCardsDependingOnAction(aV));
    }
    void IncreaceCardsNoInHand_V1(bool isTrue)
    {
        if (isTrue)
            IncreaceCardsNoInHand_V2(playerArray[currentPlayerIndex]);
        else
            IncreaceCardsNoInHand_V2(playerArray[lastPlayerIndex]);
    }
    void IncreaceCardsNoInHand_V2(GameObject player)
    {
        playerLostRound = player;
        player.GetComponent<Player>().noOfCardsInHand++;
        if (CheckIfPlayerLost(player))          
            DesactivatePlayerWhenLost(player);
        else
            player.GetComponent<Player>().particlePlusOne.SetActive(true);

        if (listOfPlayersThatLost.Count != numberOfPlayers - 1 && !localPlayer.GetComponent<Player>().playerLost)
        {
            pressSpace.SetActive(true);
            FindObjectOfType<SpaceMovement>().GetComponent<SpaceMovement>().moveSpace = true;
        }
    }
 

    //-------Next round initialization
    public IEnumerator NextRound()
    {
        randomSeed = 0;

        yield return new WaitUntil(() => playersReady);

        if (playerArray[currentPlayerIndex].GetComponent<Player>().playerLost)
            NextPlayer();

        foreach (GameObject player in playerArray)
        {
            Player plr = player.GetComponent<Player>();
            if(!plr.playerLost)
                plr.TurnAllPlayerReadyFalse();
        }
        RestartGame();
    }
    public void RestartGame()
    {
        if (localPlayer.GetComponent<NetworkIdentity>().isServer)
        {   // Setting new seed value neccesary for creating new deck of cards
            randomSeed = Random.Range(1, 99999);
            // Setting Chones variant value to default (0,0,0)
            localPlayer.GetComponent<NetworkingBrain>().CmdUpdateChosenVariant
                (0, 0, 0);
            NetworkingBrain._instantiate.RpcRestartGame();
        }
    }
    [ClientRpc]
    public void RpcRestartGame()
    {
        RestartGameAction();
    }
    bool CheckIfReadyToStartNewRound()
    {
        int[] variablesNecesaryToStartNewRound = { 0, 0, 0 };
        int arrayLength = variablesNecesaryToStartNewRound.Length;
        int x = 0;
        for (int i =0; i< arrayLength; i++)
        {
            if (chosenVariant.GetVariables()[i] == variablesNecesaryToStartNewRound[i])
                x++;
        }
        if (x < arrayLength)
        {
            chosenVariant = new ChosenVariant(0, 0, 0);
            return true;
        }

        return (x == arrayLength) ? true : false;
    }
    public IEnumerator RestartGameAction()
    {
        yield return new WaitUntil(() => CheckIfReadyToStartNewRound());

        checkButton.gameObject.SetActive(false);
        roundEnd = false;
        playersReady = false;
        playerLostRound.GetComponent<Player>().particlePlusOne.SetActive(false);
        checkedCardsList = new List<CardModel>();
        //playerLostRound = new GameObject();

        //Destroying all arrows
        GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");
        for (int i = 0; i < arrows.Length; i++)
        { Destroy(arrows[i]); }

        //Destroying all cards
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        for (int i = 0; i < cards.Length; i++)
        { Destroy(cards[i]); }

        // Creating new deck of cards
        dealer.cards = new List<int>();
        if (dealer.isGameDeck)
            StartCoroutine(dealer.CreateDeck());

        cardsInGame = new List<GameObject>();

        StartCoroutine(HandlingCards());
    }

    //-------Deactivate player that lost
    bool CheckIfPlayerLost(GameObject player)
    {
        if (player.GetComponent<Player>().noOfCardsInHand > maxCardsLosingCondition)
            return true;
        else return false;
    }
    void DesactivatePlayerWhenLost(GameObject player)
    {
        Player plr = player.GetComponent<Player>();
        player.GetComponent<SpriteRenderer>().color = Color.red;
        player.transform.GetChild(0).gameObject.SetActive(true);
        plr.playerLost = true;
        plr.playerReady = true;
        listOfPlayersThatLost.Add(player.GetComponent<CardStack>().playerIndex);
        if (listOfPlayersThatLost.Count == numberOfPlayers - 1)
        {
            foreach(GameObject pl in playerArray)
            {
                Player plyr = pl.GetComponent<Player>();

                if (plyr.playerLost)
                    continue;
                else
                    winner.text = plyr.playerName;
            }
            
            winScreen.SetActive(true);
            StartCoroutine(EndGame());
            return;
        }   
    }

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(7f);
        LobbyManager._singelton.StopHost();
        LobbyManager._singelton.StopClient();
        Destroy(FindObjectOfType<LobbyManager>().gameObject);
        LevelManager.OnMainMenuButtonClicked();
    }

    int HowManyCardsDependingOnAction(int action)
    {
        if (action == 1)
            return 1;   // High Card
        else if (action == 2)
            return 2;   // One Pair
        else if (action == 4)
            return 3;   // Three of a kind
        else if (action == 3 || action == 9)
            return 4;   // Two pairs and Four of a kind
        else return 5;  // Everything else
    }

    int HowManyCardsLosingCondition(int numberOfPlayers)
    {
        if (numberOfPlayers < 5)
            return 5;
        else if (numberOfPlayers < 7)
            return 4;
        else
            return 3;
    }
}

#pragma warning restore 0649

