using UnityEngine;
using UnityEngine.Networking;

public class EscapePanel : MonoBehaviour
{
    public GameObject EscapePanelObject = null;
    [HideInInspector] public bool EscapePanelActive = false;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EscapePanelObject.gameObject.SetActive(!EscapePanelActive);
            EscapePanelActive = !EscapePanelActive;
        }
    }

    public void ReturnButton()
    {
        EscapePanelObject.SetActive(EscapePanelActive);
        EscapePanelActive = !EscapePanelActive;
    }

    public void MainMenuButton()
    {
        LobbyManager._singelton.StopClient();
        NetworkServer.DisconnectAll();
        Destroy(FindObjectOfType<LobbyManager>().gameObject);
        LevelManager.OnMainMenuButtonClicked();
    }
}
