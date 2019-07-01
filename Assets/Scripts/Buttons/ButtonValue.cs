using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ButtonValue : MonoBehaviour
{
    [SerializeField] int actionNumber = 0;
    [SerializeField] int firstCardNumber = 0;
    [SerializeField] int secondCardNumber = 0;

    private void OnEnable()
    {
        DisableMainButtonIfChosenVariantGrater(
            Gameplay._instance.chosenVariant.actionValue,
            Gameplay._instance.chosenVariant.firstCardValue,
            Gameplay._instance.chosenVariant.secondCardValue);
    }

    private void Update()
    {
        if (actionNumber == 0 || secondCardNumber > 6)
            return;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(ReturnValue);
    }

    public void ReturnValue()
    {
        GameObject player = FindObjectOfType<Gameplay>().GetComponent<Gameplay>().localPlayer;

        if (!player.GetComponent<NetworkIdentity>().isServer)
            player.GetComponent<NetworkingBrain>().CmdUpdateChosenVariant
                (actionNumber, firstCardNumber, secondCardNumber);
        else
        {
            ButtonScript._inst.DisableTopButtonIfChosenVariantGrater(actionNumber, firstCardNumber, secondCardNumber);
            Gameplay._instance.RpcUpdateChosenVariant
            (actionNumber, firstCardNumber, secondCardNumber);
        }

        Gameplay._instance.NextPlayer();
    }

    public void DisableMainButtonIfChosenVariantGrater(int action, int firstCard, int secondCard)
    {   // Buttons reset, when new round
        if (action == 0 && firstCard == 0 && secondCard == 0)
        {
            gameObject.GetComponent<Image>().enabled = true;
            gameObject.GetComponent<Button>().interactable = true;
        }
        if (action != this.actionNumber)
            return;

        else if (action == 3)
        {
            if (gameObject.tag.Equals("HasChildren"))
            {
                if ((this.firstCardNumber < firstCard && this.firstCardNumber < secondCard)
                    || this.firstCardNumber == firstCard && secondCard == 6)
                    TurnOn_OffTheButton();
            }

            else if (gameObject.tag.Equals("Children"))
            {
                if(this.secondCardNumber < secondCard)
                {
                    if(this.secondCardNumber < firstCard || this.firstCardNumber < secondCard)
                        TurnOn_OffTheButton();
                }

                if (this.firstCardNumber == firstCard && this.secondCardNumber <= secondCard)
                    TurnOn_OffTheButton();

                if (this.secondCardNumber == firstCard && this.firstCardNumber == secondCard)
                    TurnOn_OffTheButton();
            }
        }
        else if (action == 8)
        {
            if (gameObject.tag.Equals("HasChildren"))
            {
                if (this.firstCardNumber < firstCard)
                    TurnOn_OffTheButton();
            }
            else if (gameObject.tag.Equals("Children"))
            {
                if (secondCard >= this.secondCardNumber && this.firstCardNumber == firstCard)
                    TurnOn_OffTheButton();
            }
        }
        else if (this.firstCardNumber <= firstCard)
            TurnOn_OffTheButton();
    }
    private void TurnOn_OffTheButton()
    {
        gameObject.GetComponent<Image>().enabled = false;
        gameObject.GetComponent<Button>().interactable = false;
    }
}
