using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamingCards : MonoBehaviour
{
    //enum Action
    //{
    //    High_Card, One_Pair, Two_Pairs, Three_OfAKind, Small_Straight, Big_Straight, Flush,
    //    Full_House, Four_OfAKind, Small_RoyalFlush, Big_RoyalFlush
    //}
    static string[] actionNames =
        {"None", "High Card:", "One Pair:", "Two Pairs:", "Three Of A Kind:", "Small Straight:", "Big Straight:", "Flush:",
            "Full House:", "Four Of A Kind:", "Small Royal Flush:", "Big Royal Flush:"};
    enum Card
    { Nine, Ten, Jack, Queen, King, Ace }
    enum Suit
    { Diamond, Clubs, Hearts, Spades }

    public static int AddingSuitsValuesToCards(int cardIndex)
    {
        if (cardIndex < 6)
            return 1;
        else if (cardIndex < 12)
            return 2;
        else if ( cardIndex < 18)
            return 3;
        else  return 4;
    }

    public static int AddingValuesToCards(int cardIndex)
    {
        if (cardIndex % 6 == 0)
            return 1; 
        else if ((cardIndex - 1) % 6 == 0)
            return 2; 
        else if ((cardIndex - 2) % 6 == 0)
            return 3; 
        else if ((cardIndex - 3) % 6 == 0)
            return 4; 
        else if ((cardIndex - 4) % 6 == 0)
            return 5; 
        else
            return 6; 
    }

    public static string CardNaming(int cardValue, int cardSuit)
    {
        Card cardName = (Card)cardValue -1;
        Suit cardColour = (Suit)cardSuit -1;

        return (cardName + " of " + cardColour);
    }

    public static string SelectedValue(int actionNumber, int firstCardNumber, int secondCardNumber)
    {
        //Action action;
        Card firstCard;
        Card secondCard;
        Suit colour;

        //action = (Action)actionNumber -1;
        firstCard = (Card)firstCardNumber - 1;
        secondCard = (Card)secondCardNumber - 1;
        colour = (Suit)firstCardNumber - 1; 

        if (actionNumber == 3)
            return (actionNames[actionNumber] + " " + firstCard + "s and " + secondCard + "s");
        else if (actionNumber == 8)
            return (actionNames[actionNumber] + " " + firstCard + "s over " + secondCard +"s");
        else if (actionNumber == 0 || actionNumber == 5 || actionNumber == 6)
            return (actionNames[actionNumber]);
        else if (actionNumber == 7 || actionNumber == 10 || actionNumber == 11)
            return (actionNames[actionNumber] + " " + colour);
        else
            return (actionNames[actionNumber] + " " + firstCard);
    }

    public static int ReturnActionNumber()
    {
        return 0;
    }
}
