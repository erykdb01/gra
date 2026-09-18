using UnityEngine;
using UnityEngine.SceneManagement;

// "Mózg" gry: trzyma bieżącego pasażera, przyjmuje decyzje, liczy wynik.
public class GameManager : MonoBehaviour
{
    public UIController ui;                 // przeciągnij obiekt UI
    public int passengersPerShift = 10;
    public GameObject menuButton;           // przycisk MENU, widoczny dopiero po końcu zmiany

    PassengerData current;
    int served;
    int correct;
    int mistakes;
    bool finished;

    void Start()
    {
        if (menuButton != null) menuButton.SetActive(false);
        NextPassenger();
    }

    void NextPassenger()
    {
        if (served >= passengersPerShift)
        {
            finished = true;
            ui.ShowShiftEnd(correct, mistakes);
            if (menuButton != null) menuButton.SetActive(true);
            return;
        }

        current = PassengerGenerator.Generate(PassengerGenerator.RandomTruth());
        ui.ShowPassenger(current);
        ui.ShowScore(correct, mistakes, served, passengersPerShift);
    }

    // admit = true -> WPUŚĆ, false -> ODMÓW.
    public void Decide(bool admit)
    {
        if (finished) return;

        // Zasada dnia 1: wpuszczamy tylko żywych.
        bool shouldAdmit = current.truth == PassengerTruth.Alive;
        bool ok = admit == shouldAdmit;

        if (ok) correct++;
        else mistakes++;

        Debug.Log($"Decision: admit={admit}, truth={current.truth}, hint={current.debugHint}");

        ui.ShowFeedback(ok, current.truth);
        served++;
        NextPassenger();
    }

    // Funkcje do podpięcia pod przyciski (On Click).
    public void Admit() => Decide(true);
    public void Deny()  => Decide(false);

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
