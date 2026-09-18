using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<Traveler> dzisiejsziPodrozni;

    public TMP_Text imieNazwiskoText;
    public TMP_Text dataUrodzeniaText;
    public TMP_Text dataWaznosciText;
    public TMP_Text numerDokumentuText;

    private int currentIndex = 0;

    void Start()
    {
        PokazPodroznego();
    }

    void PokazPodroznego()
    {
        Traveler t = dzisiejsziPodrozni[currentIndex];

        imieNazwiskoText.text = t.paszport.imieNazwisko;
        dataUrodzeniaText.text = "Data urodzenia: " + t.paszport.dataUrodzenia;
        dataWaznosciText.text = "Ważny do: " + t.paszport.dataWaznosci;
        numerDokumentuText.text = "Nr: " + t.paszport.numerDokumentu;
    }

    public void Przepusc()
    {
        OcenDecyzje(true);
    }

    public void Odmow()
    {
        OcenDecyzje(false);
    }

    void OcenDecyzje(bool decyzjaGracza)
    {
        Traveler t = dzisiejsziPodrozni[currentIndex];
        bool poprawnie = decyzjaGracza == t.shouldBeApproved;

        if (poprawnie)
            Debug.Log("Dobra decyzja!");
        else
            Debug.Log("Błędna decyzja!");

        currentIndex++;

        if (currentIndex < dzisiejsziPodrozni.Count)
        {
            PokazPodroznego();
        }
        else
        {
            Debug.Log("Koniec dnia!");
        }
    }
}