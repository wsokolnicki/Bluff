using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    public static ButtonScript _inst;

    public int ActionIndex = 0;
    private Vector3 startPosition = Vector3.zero;
    private float offsetX = 500f;
    private Vector3 offset = Vector3.zero;
    private GameObject buttonParent = null;
    public bool active = true;

    private void Awake()
    { 
        _inst = this; 
    }

    private void Start()
    {
        buttonParent = transform.root.transform.GetChild(1).gameObject;
        startPosition = buttonParent.transform.position;
        offset = new Vector3(offsetX, 0, 0);
    }

    public void Show_HideAllActionButtons(bool _currentPlayer)
    {
        if (!_currentPlayer)
        {
            buttonParent.transform.position = startPosition - offset;
        }
        else
        {
            buttonParent.transform.position = startPosition;
        }
    }

    public void DisableTopButtonIfChosenVariantGreater(int aV, int fCV, int sCV)
    {
        foreach (ButtonScript _mainButton in FindObjectsOfType<ButtonScript>())
        {
            Button _button = _mainButton.GetComponent<Button>();

            if (aV == 0)
            {
                _button.interactable = true;
                active = true;
            }
            else if (_mainButton.ActionIndex < aV)
            {
                _button.interactable = false;
                active = false;
            }
            else if (_mainButton.ActionIndex == aV)
            {   //Turn off the button, when top value is chosen in a ...
                if ((fCV % 6 == 0 && sCV == 0) ||                     // single value return (high card, three of a kind etc
                    ((aV == 7 || aV == 11) && fCV == 4) ||                // suit return (flush, royal flush)
                    ((fCV == 6 && sCV == 5) || (aV == 3 && fCV == 5 && sCV == 6))) // two pairs or full house
                {
                    _button.interactable = false;
                    active = false;
                }
            }
        }
    }
}
