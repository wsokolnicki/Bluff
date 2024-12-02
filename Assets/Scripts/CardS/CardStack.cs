using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour
{
    //cache
    private CardStackView dealer = null;
    [Header("Necessary for Dealer only")] [SerializeField] Gameplay gameplay = null;

    public List<int> Cards = new List<int>();
    public int PlayerIndex = 0;

    //state
    public bool IsGameDeck = false;
    public bool MainPlayer = false;

    void Start()
    {
        dealer = GetComponent<CardStackView>();
        Cards = new List<int>();

        if (IsGameDeck)
        {
            StartCoroutine(CreateDeck());
        }
    }

    public bool HasCards
    {
        get
        { return Cards != null && Cards.Count > 0; }
    }

    public int CardCount
    {
        get
        {
            if (Cards == null)
                return 0;
            else
                return Cards.Count;
        }
    }

    public IEnumerable<int> GetCards()
    {
        foreach (int i in Cards)
        { 
            yield return i; 
        }
    }

    public IEnumerator CreateDeck()
    {
        yield return new WaitUntil(() => gameplay.randomSeed != 0);

        Random.InitState(System.Environment.TickCount);
        Random.InitState(Gameplay._instance.randomSeed);
        Cards.Clear();

        for (int i = 0; i < 24; i++)
        { 
            Cards.Add(i); 
        }

        int n = Cards.Count;
        Gameplay._instance.noOfCardsInDeck = n;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            int temp = Cards[k];
            Cards[k] = Cards[n];
            Cards[n] = temp;
        }
    }
}
