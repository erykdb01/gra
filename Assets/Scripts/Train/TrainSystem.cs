using UnityEngine;

// The train as a character: integrity, temperature, electricity, pressure and signal.
// It reacts to decisions, anomalies and events and drives the train's look and sound.
public class TrainSystem : MonoBehaviour
{
    public float Integrity = 100f;
    public float Temperature = 50f;
    public float Electricity = 85f;
    public float Pressure = 40f;
    public float Signal = 90f;

    TrainView view;
    float nextSignFlicker, nextPowerFlicker;
    readonly bool[] warned = new bool[5];

    public bool Destroyed { get { return Integrity <= 0f; } }

    public static TrainSystem Create(GameObject host, TrainView view)
    {
        TrainSystem t = host.AddComponent<TrainSystem>();
        t.view = view;
        return t;
    }

    public void ResetNight()
    {
        for (int i = 0; i < warned.Length; i++) warned[i] = false;
        Electricity = Mathf.Max(Electricity, 70f);
        Signal = Mathf.Max(Signal, 60f);
        Temperature = 50f;
    }

    public void LoadIntegrity(int v) { Integrity = Mathf.Clamp(v, 1, 100); }

    // Called after every decision.
    public void OnDecision(PassengerData p, bool admitted, bool correct)
    {
        if (!correct)
        {
            Integrity -= (admitted && p.truth != PassengerTruth.Alive) ? 9f : 5f;
            Pressure += 8f;
            if (view != null) view.Bump(1f);
        }
        else
        {
            Integrity += 2f;
            Pressure -= 4f;
        }

        if (admitted)
        {
            switch (p.truth)
            {
                case PassengerTruth.Dead:        Temperature -= 10f; break;
                case PassengerTruth.NonExistent: Signal -= 12f; Electricity -= 6f; break;
                case PassengerTruth.Unknown:     Signal -= 15f; Temperature -= 5f; break;
                case PassengerTruth.Echo:        Temperature -= 6f; Signal -= 6f; break;
                case PassengerTruth.Loop:        Signal -= 8f; break;
                case PassengerTruth.Impostor:    Pressure += 5f; break;
            }
        }
        else
        {
            Signal += 3f;
        }

        Electricity += 2f;
        Pressure = Mathf.Lerp(Pressure, 40f + PlayerStats.Stress01 * 45f, 0.3f);
        Temperature = Mathf.Lerp(Temperature, 50f, 0.12f);
        Signal = Mathf.Lerp(Signal, 90f, 0.05f);
        if (Pressure > 85f) Integrity -= 2f;
        Clamp();
        CheckWarnings();
    }

    public void AddElectricity(float d) { Electricity += d; Clamp(); CheckWarnings(); }
    public void AddSignal(float d) { Signal += d; Clamp(); CheckWarnings(); }
    public void AddIntegrity(float d) { Integrity += d; Clamp(); }
    public void AddTemperature(float d) { Temperature += d; Clamp(); CheckWarnings(); }

    void Clamp()
    {
        Integrity = Mathf.Clamp(Integrity, 0f, 100f);
        Temperature = Mathf.Clamp(Temperature, 0f, 100f);
        Electricity = Mathf.Clamp(Electricity, 0f, 100f);
        Pressure = Mathf.Clamp(Pressure, 0f, 100f);
        Signal = Mathf.Clamp(Signal, 0f, 100f);
    }

    void CheckWarnings()
    {
        Warn(0, Integrity < 30f, Loc.T("Kadłub trzeszczy. Integralność poniżej 30%.", "The hull creaks. Integrity below 30%."));
        Warn(1, Temperature < 25f, Loc.T("W wagonach robi się mroźno.", "The cars grow freezing cold."));
        Warn(2, Electricity < 25f, Loc.T("Napięcie w sieci spada. Światła słabną.", "Mains voltage is dropping. The lights weaken."));
        Warn(3, Pressure > 85f, Loc.T("Ciśnienie w kotle przekracza normę.", "Boiler pressure exceeds the limit."));
        Warn(4, Signal < 30f, Loc.T("Sygnał ginie. Radio zaczyna szumieć.", "The signal is fading. The radio begins to hiss."));
    }

    void Warn(int i, bool condition, string text)
    {
        if (condition && !warned[i])
        {
            warned[i] = true;
            ToastUI.Show(Loc.T("POCIĄG", "TRAIN"), text, new Color(0.95f, 0.6f, 0.3f));
            if (SoundFX.I != null) SoundFX.I.Stinger(0.3f);
        }
        else if (!condition && warned[i])
        {
            warned[i] = false;
        }
    }

    void Update()
    {
        if (view == null) return;

        // low signal: the line number is sometimes wrong
        if (Signal < 30f && Time.time > nextSignFlicker)
        {
            nextSignFlicker = Time.time + Random.Range(2.5f, 6f);
            StartCoroutine(WrongLine(Random.value < 0.5f ? "31" : (Random.value < 0.5f ? "1 3" : "??")));
        }

        // low electricity: the lights flicker now and then
        if (Electricity < 25f && Time.time > nextPowerFlicker)
        {
            nextPowerFlicker = Time.time + Random.Range(3f, 7f);
            view.FlickerFor(Random.Range(0.6f, 1.6f));
            if (SoundFX.I != null) SoundFX.I.Click();
        }
    }

    System.Collections.IEnumerator WrongLine(string text)
    {
        view.SetLineNumber(text);
        yield return new WaitForSeconds(Random.Range(1.2f, 2.5f));
        view.SetLineNumber("13");
    }

    public System.Collections.IEnumerator ShowWrongLine(string text, float seconds)
    {
        view.SetLineNumber(text);
        yield return new WaitForSeconds(seconds);
        view.SetLineNumber("13");
    }

    // Returns the value 0..1 for the HUD.
    public float Get01(int index)
    {
        switch (index)
        {
            case 0: return Integrity / 100f;
            case 1: return Temperature / 100f;
            case 2: return Electricity / 100f;
            case 3: return Pressure / 100f;
            default: return Signal / 100f;
        }
    }
}
