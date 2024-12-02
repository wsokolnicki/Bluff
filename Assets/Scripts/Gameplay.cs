using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Gameplay : NetworkBehaviour
{

    [HideInInspector] public static Gameplay _instance = null;
    private CardStackView cardSView = null;
    [Header("Cache")]
    [Header("-Classes")]
    [SerializeField] CardStack dealer = null;
    [Header("-Game Objects")]
    [SerializeField] private GameObject deck = null;
    [SerializeField] private GameObject checkedCardSingle= null;
    [SerializeField] private GameObject checkedCardsDoubleUP = null;
    [SerializeField] private GameObject checkedCardsDoubleDOWN = null;
    [SerializeField] private GameObject winScreen = null;
    [SerializeField] private GameObject arrowPrefab = null;
    public GameObject PressSpace = null;
    public GameObject CheckButtonObject = null;
    //public GameObject buttonParent;
    [Header("-UI")]
    [SerializeField] private Text lastChosenValueText = null;
    [SerializeField] private Text winner = null;
    [SerializeField] private Text cardsNo = null;
    [Header("-Variables")]
    [SerializeField] private float delayBetweenCardsHandling = 0f;
    [SerializeField] private float timeForHandlingCards = 2f;
    [HideInInspector] public bool PlayersReady = false;
    [HideInInspector] public int MaxCardsLosingCondition = 0;


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
        ButtonScript._inst.DisableTopButtonIfChosenVariantGreater(aV, fCV, sCV);
        if (aV == 0)
        {
            CheckButtonObject.gameObject.SetActive(false);
        }
    }
    //------------------------------------------

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        StartCoroutine(CreatingPlayersAndStartingTheGame());
        cardSView = FindObjectOfType<CardStackView>();
    }

    private void Update()
    {
        lastChosenValueText.text = NamingCards.SelectedValue(chosenVariant.actionValue, chosenVariant.firstCardValue, chosenVariant.secondCardValue);

        cardsNo.text = tottalAmountOfCardsInGameThisRound.ToString();

        if (isServer)
        {// Loop that checks all the players if they are ready for next round
            for (int i = 0; i < playerArray.Count; i++)
            {
                if (!playerArray[i].GetComponent<Player>().IsplayerReady || listOfPlayersThatLost.Count == (playerArray.Count - 1))
                    break;
                else
                {
                    if (i == playerArray.Count - 1)
                        PlayersReady = true;
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
            player.GetComponent<CardStack>().PlayerIndex = i;
            if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                localPlayer = player;
                player.GetComponent<CardStack>().MainPlayer = true;
                x = player.GetComponent<CardStack>().PlayerIndex;
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
            playerArray[x].name = playerArray[x].GetComponent<Player>().PlayerName;
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

        playerArray[currentPlayerIndex].GetComponent<Player>().CurrentPlayer = true;
        CheckButtonObject.gameObject.SetActive(false);
        MaxCardsLosingCondition = HowManyCardsLosingCondition(numberOfPlayers);
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
            if (plr.PlayerLost)
                continue;
            plr.CurrentNoOfCardsInHand = 0;
            tottalAmountOfCardsInGameThisRound += plr.NoOfCardsInHand;
        }

        int topCardIndex = 1;
        delayBetweenCardsHandling =
            ((timeForHandlingCards / tottalAmountOfCardsInGameThisRound) > 0.5f) ? 0.5f : timeForHandlingCards / tottalAmountOfCardsInGameThisRound;

        while (cardsInGame.Count < tottalAmountOfCardsInGameThisRound)
        {
            for (int x = 0; x < numberOfPlayers; x++)
            {
                if (playerArray[x].GetComponent<Player>().CurrentNoOfCardsInHand <
                    playerArray[x].GetComponent<Player>().NoOfCardsInHand &&
                    !playerArray[x].GetComponent<Player>().PlayerLost)
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

        if (player.GetComponent<CardStack>().MainPlayer || localPlayer.GetComponent<Player>().PlayerLost)
        {
            currentCard.GetComponent<CardModel>().ToggleFace(true);
            currentCard.GetComponent<Draggable>().PlayersChild = true;
        }
        player.GetComponent<Player>().CurrentNoOfCardsInHand++;
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
        playerArray[currentPlayerIndex].GetComponent<Player>().CurrentPlayer = false;
        IncreaseCurrentPlayerIndexValue();
        playerArray[currentPlayerIndex].GetComponent<Player>().CurrentPlayer = true;
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
        CheckButtonObject.SetActive(false);
        roundEnd = true;
        foreach (GameObject card in cardsInGame)
        {
            CardModel cardModel = card.GetComponent<CardModel>();
            if (cardModel.FaceUp)
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
            card.GetComponent<Draggable>().PlayersChild = false;

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
        cardModel.PlayerPosition = card.transform.parent.parent.parent.position;

        Color _transparency = new Vector4(1f,1f,1f,0.8f);
        cardModel.GetComponent<SpriteRenderer>().color = _transparency;
        RectTransform _cardTransform = cardModel.GetComponent<RectTransform>();
        _cardTransform.position = new Vector3(_cardTransform.position.x, _cardTransform.position.y + 0.1f, _cardTransform.position.z);

        GameObject cardCopy = Instantiate(cardSView.CardPrefab) as GameObject;
        CardModel _cardPreview = cardCopy.GetComponent<CardModel>();

        _cardPreview.CardIndex = cardModel.CardIndex;
        _cardPreview.CardSuit = cardModel.CardSuit;
        _cardPreview.CardValue = cardModel.CardValue;
        _cardPreview.ToggleFace(true);
        _cardPreview.CardCopyInPlayerHand = cardModel.transform;

        _cardPreview.transform.SetParent(checkedCardSingle.transform);
        checkedCardsList.Add(_cardPreview);

        //Sets cards in order from 9 to Ace if straight
        if (_cardPreview.CardValue - 1 > checkedCardsList.Count)
            cardCopy.transform.SetSiblingIndex(checkedCardsList.Count);
        else
            cardCopy.transform.SetSiblingIndex(cardModel.CardValue - 1);

        //CardModel cardModel = card.GetComponent<CardModel>();
        ////cardModel.playerName.text = card.transform.parent.parent.parent.GetComponent<Player>().playerName;
        //cardModel.playerPosition = card.transform.parent.parent.parent.position;
        //card.transform.SetParent(checkedCardSingle.transform);
        //checkedCardsList.Add(cardModel);

        ////Sets cards in order from 9 to Ace if straight
        //if (cardModel.cardValue - 1 > checkedCardsList.Count)
        //    card.transform.SetSiblingIndex(checkedCardsList.Count);
        //else
        //    card.transform.SetSiblingIndex(cardModel.cardValue - 1);
    }
    void ChangeCardsParentTwoCards(GameObject card, int first, int second)
    {
        CardModel cardModel = card.GetComponent<CardModel>();
        //cardModel.playerName.text = card.transform.parent.parent.parent.GetComponent<Player>().playerName;
        cardModel.PlayerPosition = card.transform.parent.parent.parent.position;
        if (cardModel.CardValue == first)
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
    //IEnumerator AddingArrows()
    //{
    //    //float x = 0;
    //    //float offset = 0.65f;
    //    int arrowSize = 7;
    //    float shortCoefficient = /*0.65f;*/ 0.8f;
    //    float arrowWidth = 0.3f;

    //    //GameObject[] cards = new GameObject[checkedCardSingle.transform.childCount];
    //    //for(int i=0; i<checkedCardSingle.transform.childCount; i++)
    //    //{
    //    //    cards[i] = checkedCardSingle.transform.GetChild(i).gameObject;
    //    //}
    //    yield return new WaitUntil(() => /*cards.Length == checkedCardSingle.transform.childCount*/
    //    checkedCardsList.Count == checkedCardSingle.transform.childCount ||
    //    checkedCardsList.Count == (checkedCardsDoubleUP.transform.childCount + checkedCardsDoubleDOWN.transform.childCount));

    //    foreach (/*GameObject*/CardModel card in /*cards*/checkedCardsList)
    //    {
    //        //card.playerName.gameObject.SetActive(true);

    //        //float position = Camera.main.WorldToViewportPoint(card.transform.position).y;
    //        //if (position >= 0.5f)
    //        //    card.playerName.gameObject.transform.Translate(new Vector3(0, offset, 0));
    //        //else
    //        //    card.playerName.gameObject.transform.Translate(new Vector3(0, -offset, 0));
    //        //CardModel cardmodel = card.GetComponent<CardModel>();
    //        Vector3 vectorBetweenCardAndPlayer = (card.playerPosition - card.transform.position).normalized;
    //        var magnitude = (card.playerPosition - card.transform.position).magnitude;
    //        var angle = Vector3.Angle(transform.up, vectorBetweenCardAndPlayer);
    //        int signAngle = (card.playerPosition.x > card.transform.position.x) ? -1 : 1;
    //        var arrowGO = Instantiate(arrowPrefab, card.playerPosition, Quaternion.identity);

    //        int signTranslate = (angle > 90) ? -1 : 1;

    //        //arrowGO.transform.Translate(new Vector3(0, signTranslate * offset, 0));
    //        //if (angle == 90)
    //        //{
    //        //    arrowGO.transform.Translate(new Vector3(0, signTranslate *x, 0));
    //        //    x += 0.1f;
    //        //}

    //        arrowGO.transform.eulerAngles = new Vector3(0, 0, signAngle * angle);
    //        arrowGO.transform.localScale = new Vector3(arrowWidth, -magnitude / arrowSize * shortCoefficient, 1);
    //        //arrowGO.GetComponent<SpriteRenderer>().sortingOrder = 7;

    //    }
    //}

    IEnumerator AddingArrows()
    {
        int arrowSize = 7;
        float shortCoefficient = 0.8f;
        float arrowWidth = 0.3f;

        yield return new WaitUntil(() => /*cards.Length == checkedCardSingle.transform.childCount*/
        checkedCardsList.Count == checkedCardSingle.transform.childCount ||
        checkedCardsList.Count == (checkedCardsDoubleUP.transform.childCount + checkedCardsDoubleDOWN.transform.childCount));

        foreach (CardModel card in checkedCardsList)
        {
            Vector3 vectorBetweenCardAndPlayer = (card.CardCopyInPlayerHand.position - card.transform.position).normalized;
            var magnitude = (card.CardCopyInPlayerHand.position - card.transform.position).magnitude;
            var angle = Vector3.Angle(transform.up, vectorBetweenCardAndPlayer);
            int signAngle = (card.CardCopyInPlayerHand.position.x > card.transform.position.x) ? -1 : 1;
            var arrowGO = Instantiate(arrowPrefab, card.CardCopyInPlayerHand.position, Quaternion.identity);

            int signTranslate = (angle > 90) ? -1 : 1;

            arrowGO.transform.eulerAngles = new Vector3(0, 0, signAngle * angle);
            arrowGO.transform.localScale = new Vector3(arrowWidth, -magnitude / arrowSize * shortCoefficient, 1);
        }
    }
    private void ShowingCardWithoutRepetitions(List<int> alreadyChecked, GameObject card, CardModel cardModel)
    {
        if (alreadyChecked.Count == 0)
        {
            ChangeCardsParent(card);
            alreadyChecked.Add(cardModel.CardValue);
        }
        else
        {
            for (int y = 0; y < alreadyChecked.Count; y++)
            {
                if (IsDuplicated(cardModel.CardValue, alreadyChecked))
                    break;
                else
                {
                    ChangeCardsParent(card);
                    alreadyChecked.Add(cardModel.CardValue);
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
            if (card.CardValue == fCV)
                numberOfFirstCards++;
            if (card.CardValue == sCV)
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
        player.GetComponent<Player>().NoOfCardsInHand++;
        if (CheckIfPlayerLost(player))          
            DesactivatePlayerWhenLost(player);
        else
            player.GetComponent<Player>().ParticlePlusOne.SetActive(true);

        if (listOfPlayersThatLost.Count != numberOfPlayers - 1 && !localPlayer.GetComponent<Player>().PlayerLost)
        {
            PressSpace.SetActive(true);
            FindObjectOfType<SpaceMovement>().GetComponent<SpaceMovement>().MoveSpace = true;
        }
    }
 

    //-------Next round initialization
    public IEnumerator NextRound()
    {
        randomSeed = 0;

        yield return new WaitUntil(() => PlayersReady);

        if (playerArray[currentPlayerIndex].GetComponent<Player>().PlayerLost)
            NextPlayer();

        foreach (GameObject player in playerArray)
        {
            Player plr = player.GetComponent<Player>();
            if(!plr.PlayerLost)
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

        CheckButtonObject.gameObject.SetActive(false);
        roundEnd = false;
        PlayersReady = false;
        playerLostRound.GetComponent<Player>().ParticlePlusOne.SetActive(false);
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
        dealer.Cards = new List<int>();
        if (dealer.IsGameDeck)
            StartCoroutine(dealer.CreateDeck());

        cardsInGame = new List<GameObject>();

        StartCoroutine(HandlingCards());
    }

    //-------Deactivate player that lost
    bool CheckIfPlayerLost(GameObject player)
    {
        if (player.GetComponent<Player>().NoOfCardsInHand > MaxCardsLosingCondition)
            return true;
        else return false;
    }
    void DesactivatePlayerWhenLost(GameObject player)
    {
        Player plr = player.GetComponent<Player>();
        player.GetComponent<SpriteRenderer>().color = Color.red;
        player.transform.GetChild(0).gameObject.SetActive(true);
        plr.PlayerLost = true;
        plr.IsplayerReady = true;
        listOfPlayersThatLost.Add(player.GetComponent<CardStack>().PlayerIndex);
        if (listOfPlayersThatLost.Count == numberOfPlayers - 1)
        {
            foreach(GameObject pl in playerArray)
            {
                Player plyr = pl.GetComponent<Player>();

                if (plyr.PlayerLost)
                    continue;
                else
                    winner.text = plyr.PlayerName;
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
