using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 0649

public class CardStack : MonoBehaviour
{
    //cache
    CardStackView dealer;
    [Header("Necessary for Dealer only")] [SerializeField] Gameplay gameplay;

    public List<int> cards;
    public int playerIndex;

    //state
    public bool isGameDeck;
    public bool mainPlayer;

    void Start()
    {
        dealer = GetComponent<CardStackView>();

        cards = new List<int>();
        if (isGameDeck)
            StartCoroutine(CreateDeck());
    }

    public bool HasCards
    {
        get
        { return cards != null && cards.Count > 0; }
    }

    public int CardCount
    {
        get
        {
            if (cards == null)
                return 0;
            else
                return cards.Count;
        }
    }

    public IEnumerable<int> GetCards()
    {
        foreach (int i in cards)
        { yield return i; }
    }

    public IEnumerator CreateDeck()
    {
        yield return new WaitUntil(() => gameplay.randomSeed != 0);

        Random.InitState(System.Environment.TickCount);
        Random.InitState(Gameplay._instance.randomSeed);
        cards.Clear();

        for (int i = 0; i < 24; i++)
        { cards.Add(i); }

        int n = cards.Count;
        Gameplay._instance.noOfCardsInDeck = n;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            int temp = cards[k];
            cards[k] = cards[n];
            cards[n] = temp;
        }
    }
}

#pragma warning restore 0649

