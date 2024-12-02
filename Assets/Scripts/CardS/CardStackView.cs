using UnityEngine;

public class CardStackView : MonoBehaviour
{
    private CardStack deck = null;

    public GameObject CardPrefab = null;
    [SerializeField] private float cardOffset = 0.0025f;

    private int lastCount = 0;
    public bool FaceUp = false;

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
        int cardCount = 0;

        if (deck.HasCards)
        {
            foreach (int i in deck.GetCards())
            {
                float co = cardOffset * cardCount;
                Vector3 temp = transform.position + new Vector3(co, 0f);
                AddCard(i, temp, cardCount);
                cardCount++;
            }
        }
    }

    void AddCard(int cardIndex, Vector3 position, int positionalIndex)
    {
        GameObject cardCopy = Instantiate(CardPrefab) as GameObject;

        CardModel cardModel = cardCopy.GetComponent<CardModel>();
        cardModel.CardIndex = cardIndex;
        cardModel.CardSuit = NamingCards.AddingSuitsValuesToCards(cardIndex);
        cardModel.CardValue = NamingCards.AddingValuesToCards(cardIndex);

        if (deck.IsGameDeck)
        {
            cardCopy.transform.SetParent(GameObject.FindGameObjectWithTag("Deck").transform);
            cardCopy.name = NamingCards.CardNaming(cardModel.CardValue, cardModel.CardSuit);
        }

        cardModel.ToggleFace(false);

        cardCopy.transform.position = position;

        cardCopy.GetComponent<SpriteRenderer>().sortingOrder = positionalIndex + 1;
    }
}