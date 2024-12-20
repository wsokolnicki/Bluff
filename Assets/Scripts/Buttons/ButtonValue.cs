﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ButtonValue : MonoBehaviour
{
    [SerializeField] private int actionNumber = 0;
    [SerializeField] private int firstCardNumber = 0;
    [SerializeField] private int secondCardNumber = 0;
    public bool Active = true;

    private void OnEnable()
    {
        DisableMainButtonIfChosenVariantGrater(
            GameplayManager._instance.chosenVariant.actionValue,
            GameplayManager._instance.chosenVariant.firstCardValue,
            GameplayManager._instance.chosenVariant.secondCardValue);
    }

    private void Update()
    {
        if (actionNumber == 0 || secondCardNumber > 6)
        {
            return;
        }

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(ReturnValue);
    }

    public void ReturnValue()
    {
        GameObject player = FindObjectOfType<GameplayManager>().GetComponent<GameplayManager>().localPlayer;

        if (!player.GetComponent<NetworkIdentity>().isServer)
        {
            player.GetComponent<NetworkingBrain>().CmdUpdateChosenVariant(actionNumber, firstCardNumber, secondCardNumber);
        }
        else
        {
            ButtonScript._inst.DisableTopButtonIfChosenVariantGreater(actionNumber, firstCardNumber, secondCardNumber);
            GameplayManager._instance.RpcUpdateChosenVariant
            (actionNumber, firstCardNumber, secondCardNumber);
        }

        GameplayManager._instance.NextPlayer();
    }

    public void DisableMainButtonIfChosenVariantGrater(int action, int firstCard, int secondCard)
    {   // Buttons reset, when new round
        if (action == 0 && firstCard == 0 && secondCard == 0)
        {
            gameObject.GetComponent<Image>().enabled = true;
            gameObject.GetComponent<Button>().interactable = true;
            Active = true;
        }
        if (action != this.actionNumber)
        {
            return;
        }

        else if (action == 3)
        {
            if (gameObject.tag.Equals("HasChildren"))
            {
                if ((this.firstCardNumber < firstCard && this.firstCardNumber < secondCard) || this.firstCardNumber == firstCard && secondCard == 6)
                {
                    TurnOn_OffTheButton();
                }
            }

            else if (gameObject.tag.Equals("Children"))
            {
                if (this.secondCardNumber < secondCard)
                {
                    if (this.secondCardNumber < firstCard || this.firstCardNumber < secondCard)
                    {
                        TurnOn_OffTheButton();
                    }

                    if (this.firstCardNumber == firstCard && this.secondCardNumber <= secondCard)
                    {
                        TurnOn_OffTheButton();
                    }

                    if (this.secondCardNumber == firstCard && this.firstCardNumber == secondCard)
                    {
                        TurnOn_OffTheButton();
                    }
                }
            }
        }
        else if (action == 8)
        {
            if (gameObject.tag.Equals("HasChildren"))
            {
                if (this.firstCardNumber < firstCard)
                {
                    TurnOn_OffTheButton();
                }
            }
            else if (gameObject.tag.Equals("Children"))
            {
                if (secondCard >= this.secondCardNumber && this.firstCardNumber == firstCard)
                {
                    TurnOn_OffTheButton();
                }
            }
        }
        else if (this.firstCardNumber <= firstCard)
        {
            TurnOn_OffTheButton();
        }
    }
    private void TurnOn_OffTheButton()
    {
        gameObject.GetComponent<Image>().enabled = false;
        gameObject.GetComponent<Button>().interactable = false;
        Active = false;
    }
}