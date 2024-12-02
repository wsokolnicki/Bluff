using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPrefab = null;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            Instantiate(lobbyPrefab);
        }
    }

    public static void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene(0);
    }

    public void OnPlayClick()
    {
        SceneManager.LoadScene(1);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}
#pragma warning restore 0649
