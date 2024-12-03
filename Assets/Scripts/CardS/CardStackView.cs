using UnityEngine;

public class CardStackView : MonoBehaviour
{
    private CardStack deck = null;
    private int lastCount = 0;
    [HideInInspector] public bool FaceUp = false;

    public GameObject CardPrefab = null;
    [SerializeField] private float deckCardsOffset = 0.0025f;

    void Start()
    {
        deck = GetComponent<CardStack>();
        ShowCards();
        lastCount = deck.CardCount;
    }

    void Update()
    {
        if (lastCount != deck.CardCount)
        {
            lastCount = deck.CardCount;
            ShowCards();
        }
    }

    public void ShowCards()
    {
        int _cardCount = 0;

        if (deck.HasCards)
        {
            foreach (int i in deck.GetCards())
            {
                float _co = deckCardsOffset * _cardCount;
                Vector3 _temp = transform.position + new Vector3(_co, 0f);
                AddCard(i, _temp, _cardCount);
                _cardCount++;
            }
        }
    }

    void AddCard(int _cardIndex, Vector3 _position, int _positionalIndex)
    {
        GameObject _cardCopy = Instantiate(CardPrefab) as GameObject;

        CardModel _cardModel = _cardCopy.GetComponent<CardModel>();
        _cardModel.CardIndex = _cardIndex;
        _cardModel.CardSuit = NamingCards.AddingSuitsValuesToCards(_cardIndex);
        _cardModel.CardValue = NamingCards.AddingValuesToCards(_cardIndex);

        if (deck.IsGameDeck)
        {
            _cardCopy.transform.SetParent(GameObject.FindGameObjectWithTag("Deck").transform);
            _cardCopy.name = NamingCards.CardNaming(_cardModel.CardValue, _cardModel.CardSuit);
        }

        _cardModel.ToggleFace(false);
        _cardCopy.transform.position = _position;
        _cardCopy.GetComponent<CardModel>().CardSpriteRenderer.sortingOrder = _positionalIndex + 1;
    }
}