using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameplayManager : NetworkBehaviour
{
    [HideInInspector] public static GameplayManager _instance = null;
    private CardStackView cardSView = null;
    [Header("Classes")]
    [SerializeField] private CardStack dealer = null;
    [SerializeField] private UIManager uIManager = null;
    [Header("Game Objects")]
    [SerializeField] private GameObject deckObject = null;
    [SerializeField] private GameObject checkedCardSingle= null;
    [SerializeField] private GameObject checkedCardsDoubleUP = null;
    [SerializeField] private GameObject checkedCardsDoubleDOWN = null;
    [SerializeField] private GameObject arrowPrefab = null;
    private GameObject arrowsParent = null;
    //public GameObject buttonParent;
    [Header("UI")]
    public GameObject UI_ContinueGame = null;
    public GameObject UI_CheckButton = null;
    [SerializeField] private GameObject UI_WinScreen = null;
    [SerializeField] private Text lastChosenValueText = null;
    [SerializeField] private Text winnerText = null;
    [SerializeField] private Text cardsNoText = null;
    [Header("Variables")]
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
    public bool DeckCreated = false;

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
            return ($"{actionValue.ToString()} {firstCardValue.ToString()} {secondCardValue.ToString()}");
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
            UI_CheckButton.gameObject.SetActive(false);
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
        cardsNoText.text = tottalAmountOfCardsInGameThisRound.ToString();

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

        Vector3 _center = Vector3.zero;
        int playerIndex = 0;
        int localPlayerIndex = 0;

        foreach (GameObject _player in playerArray)
        {
            CardStack _playerCardStack = _player.GetComponent<CardStack>();

            _playerCardStack.PlayerIndex = playerIndex;
            if (_player.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                localPlayer = _player;
                _playerCardStack.MainPlayer = true;
                localPlayerIndex = _playerCardStack.PlayerIndex;
            }
            playerIndex++;
        }
        int i = 0;

        do
        {
            float _ang = i * (360f / numberOfPlayers);     //angle value that grows each time loop reapeats
            Vector3 _position = Ellipse(_ang, _center, 3.5f, 5f);
            playerArray[localPlayerIndex].transform.position = _position;
            playerArray[localPlayerIndex].transform.SetParent(GameObject.FindGameObjectWithTag("PlayersInGame").transform);
            //playerArray[x].GetComponent<Player>().SetPlayerNameLocationOnBoard(ang);
            playerArray[localPlayerIndex].GetComponent<Player>().SetTextAlignment();
            playerArray[localPlayerIndex].name = playerArray[localPlayerIndex].GetComponent<Player>().PlayerName;
            localPlayerIndex++;
            i++;
            if (localPlayerIndex == numberOfPlayers)
                localPlayerIndex = 0;
        }
        while (i < numberOfPlayers);

        GameStart();
    }
    private Vector3 Ellipse(float ang, Vector3 center, float radiusA, float radiusB)
    {
        float _angle = ang;
        Vector3 _position = Vector3.zero;
        _position.x = center.x - radiusB * Mathf.Sin(_angle * Mathf.Deg2Rad);
        _position.y = center.y - radiusA * Mathf.Cos(_angle * Mathf.Deg2Rad);
        _position.z = center.z;

        return _position;
    }

    //--------Starting a game - setting randomly first(active) player and handling cards
    void GameStart()
    {
        networkLocalPlayer = localPlayer.GetComponent<NetworkingBrain>();

        if (localPlayer.GetComponent<NetworkIdentity>().isServer)
        {
            currentPlayerIndex = Random.Range(0, playerArray.Count);
        }

        playerArray[currentPlayerIndex].GetComponent<Player>().CurrentPlayer = true;
        UI_CheckButton.gameObject.SetActive(false);
        MaxCardsLosingCondition = HowManyCardsLosingCondition(numberOfPlayers);
        StartCoroutine(HandlingCards());
    }
    IEnumerator HandlingCards()
    {
        //Wait until deck is created (necessary due to delay between server and clients - server generates Random.seed)
        yield return new WaitUntil(() => deckObject.transform.childCount == noOfCardsInDeck);
        DeckCreated = true;
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
        delayBetweenCardsHandling = ((timeForHandlingCards / tottalAmountOfCardsInGameThisRound) > 0.5f) ? 0.5f : timeForHandlingCards / tottalAmountOfCardsInGameThisRound;

        while (cardsInGame.Count < tottalAmountOfCardsInGameThisRound)
        {
            for (int x = 0; x < numberOfPlayers; x++)
            {
                if (playerArray[x].GetComponent<Player>().CurrentNoOfCardsInHand < playerArray[x].GetComponent<Player>().NoOfCardsInHand && !playerArray[x].GetComponent<Player>().PlayerLost)
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
        GameObject currentCard = deckObject.transform.GetChild(noOfCardsInDeck - topCardInTheDeckIndex).gameObject;

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
        UIManager._inst.InGameSelectionMenuManager(false);
        UI_CheckButton.SetActive(false);
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
        List<int> _alreadyChecked = new List<int>();
        foreach (GameObject card in cardsInGame)
        {
            CardModel _cardModel = card.GetComponent<CardModel>();
            card.GetComponent<Draggable>().PlayersChild = false;

            yield return new WaitUntil(() => !_cardModel.flipAnimation);

            if (secondCard != 0)
            { // Two Pairs and Full House - Action requires to return two card values
                if (_cardModel.CardValue == firstCard || _cardModel.CardValue == secondCard)
                    ChangeCardsParentTwoCards(_cardModel, firstCard, secondCard);
            } // High Card, One Pair etc. - Action requires to return one card value
            else if (action == 1 || action == 2 || action == 4 || action == 9)
            {
                if (_cardModel.CardValue == firstCard)
                    ChangeCardsParent(_cardModel, _alreadyChecked);
            }
            else if (action == 7)
            { //Flush - Action requires to return cards in the same suit
                if (_cardModel.CardSuit == firstCard)
                    ChangeCardsParent(_cardModel, _alreadyChecked);
            }
            else if (action == 5)
            { //Small Straight - Action requires to return cards from 9 to King
                if (_cardModel.CardValue != 6)
                    ChangeCardsParent(_cardModel, _alreadyChecked, true);
            }
            else if (action == 6)
            { //Big Straight - Action requires to return cards from 10 to Ace
                if (_cardModel.CardValue != 1)
                    ChangeCardsParent(_cardModel, _alreadyChecked, true);
            }
            else if (action == 10)
            { //Small Startigh Flush - Action requires to return cards from 9 to King in the same suit
                if (_cardModel.CardSuit == firstCard && _cardModel.CardValue != 6)
                    ChangeCardsParent(_cardModel, _alreadyChecked);
            }
            else if (action == 11)
            {//Big Startigh Flush - Action requires to return cards from 10 to Ace in the same suit
                if (_cardModel.CardSuit == firstCard && _cardModel.CardValue != 1)
                    ChangeCardsParent(_cardModel, _alreadyChecked);
            }
        }
        StartCoroutine(AddArrows());
        AddCardToLoser(action, firstCard, secondCard);
    }
    void ChangeCardsParent(CardModel cardModel, List<int> alreadyChecked, bool withoutRepetition = false)
    {
        CardModel _cardPreview = null;

        if (withoutRepetition == true)
        {
            if (alreadyChecked.Count == 0)
            {
                CreateCardPreview(cardModel, out _cardPreview);
                _cardPreview.transform.SetParent(checkedCardSingle.transform);
                alreadyChecked.Add(_cardPreview.CardValue);
                checkedCardsList.Add(_cardPreview);
            }
            else
            {
                for (int y = 0; y < alreadyChecked.Count; y++)
                {
                    _cardPreview = cardModel;
                    if (IsDuplicated(_cardPreview.CardValue, alreadyChecked))
                        break;
                    else
                    {
                        CreateCardPreview(cardModel, out _cardPreview);
                        _cardPreview.transform.SetParent(checkedCardSingle.transform);

                        //Sets cards in order from 9 to Ace if straight
                        for (int i = 0; i < checkedCardSingle.transform.childCount - 1; i++)
                        {
                            if (_cardPreview.CardValue < checkedCardSingle.transform.GetChild(i).GetComponent<CardModel>().CardValue)
                            {
                                _cardPreview.transform.SetSiblingIndex(checkedCardSingle.transform.GetChild(i).GetSiblingIndex());
                                break;
                            }
                            else
                                continue;
                        }

                        alreadyChecked.Add(_cardPreview.CardValue);
                        checkedCardsList.Add(_cardPreview);
                    }
                }
            }
        }
        else
        {
            CreateCardPreview(cardModel, out _cardPreview);
            _cardPreview.transform.SetParent(checkedCardSingle.transform);
            checkedCardsList.Add(_cardPreview);
        }
    }
    void ChangeCardsParentTwoCards(CardModel cardModel, int first, int second)
    {
        CardModel _cardPreview = null;
        CreateCardPreview(cardModel, out _cardPreview);

        if (cardModel.CardValue == first)
        {
            _cardPreview.transform.SetParent(checkedCardsDoubleUP.transform);
            checkedCardsList.Add(_cardPreview);
        }
        else
        {
            _cardPreview.transform.SetParent(checkedCardsDoubleDOWN.transform);
            checkedCardsList.Add(_cardPreview);
        }
    }

    private void CreateCardPreview (CardModel card, out CardModel cardPreview)
    {
        card.CardSpriteRenderer.color = new Vector4(1f, 1f, 1f, 0.8f);
        RectTransform _cardTransform = card.GetComponent<RectTransform>();
        _cardTransform.position = new Vector3(_cardTransform.position.x, _cardTransform.position.y + 0.1f, _cardTransform.position.z);

        GameObject _cardCopy = Instantiate(cardSView.CardPrefab) as GameObject;
        CardModel _cardPreview = _cardCopy.GetComponent<CardModel>();

        _cardPreview.CardIndex = card.CardIndex;
        _cardPreview.CardSuit = card.CardSuit;
        _cardPreview.CardValue = card.CardValue;
        _cardPreview.ToggleFace(true);
        _cardPreview.CardCopyInPlayerHand = card.transform;

        _cardCopy.name = NamingCards.CardNaming(_cardPreview.CardValue, _cardPreview.CardSuit);

        cardPreview = _cardPreview;
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

    IEnumerator AddArrows()
    {
        int arrowSize = 7;
        float shortCoefficient = 0.8f;
        float arrowWidth = 0.3f;
        arrowsParent = Instantiate(gameObject);
        arrowsParent.name = "ArrowsParent";

        yield return new WaitUntil(() => /*cards.Length == checkedCardSingle.transform.childCount*/
        checkedCardsList.Count == checkedCardSingle.transform.childCount ||
        checkedCardsList.Count == (checkedCardsDoubleUP.transform.childCount + checkedCardsDoubleDOWN.transform.childCount));

        foreach (CardModel card in checkedCardsList)
        {
            Vector3 vectorBetweenCardAndPlayer = (card.CardCopyInPlayerHand.position - card.transform.position).normalized;
            var magnitude = (card.CardCopyInPlayerHand.position - card.transform.position).magnitude;
            var angle = Vector3.Angle(transform.up, vectorBetweenCardAndPlayer);
            int signAngle = (card.CardCopyInPlayerHand.position.x > card.transform.position.x) ? -1 : 1;
            var arrowGO = Instantiate(arrowPrefab, card.CardCopyInPlayerHand.position, Quaternion.identity, arrowsParent.transform);

            int signTranslate = (angle > 90) ? -1 : 1;

            arrowGO.transform.eulerAngles = new Vector3(0, 0, signAngle * angle);
            arrowGO.transform.localScale = new Vector3(arrowWidth, -magnitude / arrowSize * shortCoefficient, 1);
        }
    }

    void AddCardToLoser(int aV, int fCV, int sCV)
    {
        int numberOfFirstCards = 0;
        int numberOfSecondCards = 0;

        foreach (CardModel card in checkedCardsList)
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
            IncreaceCardsNoInHand_V1(checkedCardsList.Count >= HowManyCardsDependingOnAction(aV));
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
            UI_ContinueGame.SetActive(true);
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
            localPlayer.GetComponent<NetworkingBrain>().CmdUpdateChosenVariant(0, 0, 0);
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
        int[] _variablesNecesaryToStartNewRound = { 0, 0, 0 };
        int _arrayLength = _variablesNecesaryToStartNewRound.Length;
        int x = 0;
        for (int i =0; i< _arrayLength; i++)
        {
            if (chosenVariant.GetVariables()[i] == _variablesNecesaryToStartNewRound[i])
                x++;
        }
        if (x < _arrayLength)
        {
            chosenVariant = new ChosenVariant(0, 0, 0);
            return true;
        }

        return (x == _arrayLength) ? true : false;
    }
    public IEnumerator RestartGameAction()
    {
        yield return new WaitUntil(() => CheckIfReadyToStartNewRound());

        UI_CheckButton.gameObject.SetActive(false);
        roundEnd = false;
        PlayersReady = false;
        playerLostRound.GetComponent<Player>().ParticlePlusOne.SetActive(false);
        checkedCardsList = new List<CardModel>();

        //Destroying all arrows
        Destroy(arrowsParent);
        //GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");
        //for (int i = 0; i < arrows.Length; i++)
        //{ Destroy(arrows[i]); }

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
                    winnerText.text = plyr.PlayerName;
            }
            
            UI_WinScreen.SetActive(true);
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
