using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class EscapePanel : MonoBehaviour
{
    public GameObject escapePanel;
    [HideInInspector] public bool escapePanelActive = false;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            escapePanel.gameObject.SetActive(!escapePanelActive);
            escapePanelActive = !escapePanelActive;
        }
    }

    public void ReturnButton()
    {
        escapePanel.SetActive(escapePanelActive);
        escapePanelActive = !escapePanelActive;
    }

    public void MainMenuButton()
    {
        LobbyManager._singelton.StopClient();
        NetworkServer.DisconnectAll();
        Destroy(FindObjectOfType<LobbyManager>().gameObject);
        LevelManager.OnMainMenuButtonClicked();
    }
}
