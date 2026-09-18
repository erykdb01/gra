using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Peron");
    }

    public void Options()
    {
        Debug.Log("Options clicked");
    }

    public void Quit()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
}
