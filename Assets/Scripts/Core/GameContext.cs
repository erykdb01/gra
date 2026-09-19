using System;
using UnityEngine;

// Everything the events and managers need to reach, gathered in one place (filled in by GameManager).
public class GameContext
{
    public Canvas canvas;
    public MonoBehaviour runner;            // runs coroutines
    public UIController ui;
    public TrainView trainView;
    public TrainSystem trainSys;
    public StageView stage;
    public LightingController lighting;
    public HorrorManager horror;
    public HudPanel hud;
    public DialoguePanel dialogue;
    public Overlay overlay;
    public ClockUI clock;
    public DocumentInspector inspector;
    public DayConfig day;
    public PassengerData current;           // the passenger at the booth (null between passengers)

    // hooks into GameManager
    public Action RefreshPortrait;          // redraw the photo on the desk ID
    public Action RefreshDesk;              // rewrite the desk documents
    public Action<int, float> ShowRemaining; // shows a fake "passengers remaining" number for some seconds
    public Action<bool> StopClock;          // freezes the clock at 03:13
    public Action RefreshHud;
    public bool StoryMode;
    public int nightNumber;                 // 1-based
}
