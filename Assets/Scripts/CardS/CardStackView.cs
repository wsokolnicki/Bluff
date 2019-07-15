using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 0649

public class CardStackView : MonoBehaviour
{
    CardStack deck;

    public GameObject cardPrefab;
    [SerializeField] float cardOffset;

    int lastCount;
    public bool faceUp = false;

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
        GameObject cardCopy = Instantiate(cardPrefab) as GameObject;

        CardModel cardModel = cardCopy.GetComponent<CardModel>();
        cardModel.cardIndex = cardIndex;
        cardModel.CardSuit = NamingCards.AddingSuitsValuesToCards(cardIndex);
        cardModel.CardValue = NamingCards.AddingValuesToCards(cardIndex);

        if (deck.isGameDeck)
        {
            cardCopy.transform.SetParent(GameObject.FindGameObjectWithTag("Deck").transform);
            cardCopy.name = NamingCards.CardNaming(cardModel.CardValue, cardModel.CardSuit);
        }

        cardModel.ToggleFace(false);

        cardCopy.transform.position = position;

        cardCopy.GetComponent<SpriteRenderer>().sortingOrder = positionalIndex + 1;
    }
}

#pragma warning restore 0649