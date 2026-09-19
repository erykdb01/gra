# OSTATNI PERON

2D pixel-art game in the style of *Papers, Please* / *Death and Taxes*.
You are a conductor on the night train **Linia 13**. Before each passenger boards, you check their documents and decide: admit or deny.

**Status:** early prototype (version of 19 September 2026, 01:52). No final graphics or audio yet.

## Requirements

- Unity 6 (6000.6.0f1)
- Universal Render Pipeline (2D)
- TextMeshPro, Input System

## How to run

1. Open the project folder in Unity Hub.
2. Open the `MainMenu` scene (Assets/Scenes) and press Play.
3. Both scenes (`MainMenu` at index 0, `Peron` at index 1) must be in the Build Profiles scene list.

## Gameplay (current prototype)

A shift is 10 passengers. For each one you see four panels on the desk: **ticket**, **ID card**, **railway database** and **registry**. The sources may contain small discrepancies in name, birth date or status.

- **WPUŚĆ** - admit the passenger
- **ODMÓW** - deny the passenger

Hidden truth about each passenger (never shown up front):

| Truth | Chance | Meaning |
|---|---|---|
| Alive | 60% | all documents agree |
| Dead | 25% | registry says the person died |
| NonExistent | 15% | birth date differs by a year, parents died before the birth |

Rule of day 1: admit only living passengers. After every decision you get feedback (right / wrong and who the passenger really was). After 10 passengers the shift ends and the MENU button appears.

## Project structure

```
Assets/
  Scenes/    MainMenu, Peron
  Scripts/
    GameManager.cs          game logic, decisions, score
    UIController.cs         fills the panels, score and messages
    PassengerData.cs        data: PassengerTruth, Record, PassengerData
    PassengerGenerator.cs   random passenger generator
    MainMenu.cs             Play / Options / Quit
```

In-game texts are in Polish; code, comments and object names are in English.

## Roadmap

- [ ] Drag-and-drop documents and stamps
- [ ] Dialogue with passengers
- [ ] Day cycle with changing rules
- [ ] Train reacting to mistakes
- [ ] Twist ending (the passenger is you)
- [ ] Pixel-art graphics and audio
- [ ] Steam release

## Git

Work happens on the `dev/iansky` branch. `Library/`, `Temp/`, `Logs/`, `obj/` and `UserSettings/` are ignored via `.gitignore`.
