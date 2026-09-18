using UnityEngine;
using UnityEngine.SceneManagement;

// Game "brain": holds the current passenger, takes decisions, keeps score.
public class GameManager : MonoBehaviour
{
    public UIController ui;                 // drag the UI object here
    public int passengersPerShift = 10;
    public GameObject menuButton;           // MENU button, only visible after the shift ends

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

    // admit = true -> ADMIT, false -> DENY.
    public void Decide(bool admit)
    {
        if (finished) return;

        // Day 1 rule: only living passengers are admitted.
        bool shouldAdmit = current.truth == PassengerTruth.Alive;
        bool ok = admit == shouldAdmit;

        if (ok) correct++;
        else mistakes++;

        Debug.Log($"Decision: admit={admit}, truth={current.truth}, hint={current.debugHint}");

        ui.ShowFeedback(ok, current.truth);
        served++;
        NextPassenger();
    }

    // Methods to hook up to the buttons (On Click).
    public void Admit() => Decide(true);
    public void Deny()  => Decide(false);

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
