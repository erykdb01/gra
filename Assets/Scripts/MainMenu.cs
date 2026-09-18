using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Graj()
{
    SceneManager.LoadScene("Peron");
}

    public void Opcje()
    {
        Debug.Log("Kliknięto OPCJE");
    }

    public void Wyjdz()
    {
        Debug.Log("Wyjście z gry");
        Application.Quit();
    }
}