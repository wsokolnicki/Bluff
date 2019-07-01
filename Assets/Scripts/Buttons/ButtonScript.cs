using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    public static ButtonScript _inst;

    public int actionIndex;
    Vector3 startPosition;
    float offsetX = 200;
    Vector3 offset;
    GameObject buttonParent;

    private void Awake()
    { _inst = this; }

    private void Start()
    {
        buttonParent = transform.root.transform.GetChild(1).gameObject;
        startPosition = buttonParent.transform.position;
        offset = new Vector3(offsetX, 0, 0);
    }

    public void Show_HideAllActionButtons(bool currentPlayer)
    {
        if (!currentPlayer)
            buttonParent.transform.position = startPosition - offset;
        else
            buttonParent.transform.position = startPosition;
    }

    public void DisableTopButtonIfChosenVariantGrater(int aV, int fCV, int sCV)
    {
        foreach (ButtonScript mainButton in FindObjectsOfType<ButtonScript>())
        {
            if (aV == 0)
                mainButton.GetComponent<Button>().interactable = true;

            else if (mainButton.actionIndex < aV)
                mainButton.GetComponent<Button>().interactable = false;

            else if (mainButton.actionIndex == aV)
            {   //Turn off the button, when top value is chosen in a ...
                if ((fCV % 6 == 0 && sCV == 0) ||                     // single value return (high card, three of a kind etc
                    ((aV == 7 || aV == 11) && fCV == 4) ||                // suit return (flush, royal flush)
                    ((fCV == 6 && sCV == 5) || (aV == 3 && fCV == 5 && sCV == 6))) // two pairs or full house
                    mainButton.GetComponent<Button>().interactable = false;
            }
        }
    }
}
