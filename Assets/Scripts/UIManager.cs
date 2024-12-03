using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager _inst = null;

    public GameObject InGameSelectionMenu = null;
    public GameObject CkeckButton = null;
    public GameObject EscapeMenu = null;
    public GameObject WinScreen = null;
    public GameObject ContinueScreen = null;

    private Vector3 selectionMenuStartPosition = Vector3.zero;

    private const float SELECTION_MENU_OFFSET = 500f;

    private void Awake()
    {
        _inst = this;
    }
    private void Start()
    {
        selectionMenuStartPosition = InGameSelectionMenu.transform.position;
    }

    public void InGameSelectionMenuManager(bool _currentPlayer)
    {
        if (!_currentPlayer)
        {
            InGameSelectionMenu.transform.position = new Vector3(selectionMenuStartPosition.x - SELECTION_MENU_OFFSET, selectionMenuStartPosition.y, selectionMenuStartPosition.z);
        }
        else
        {
            InGameSelectionMenu.transform.position = selectionMenuStartPosition;
        }
    }
}
