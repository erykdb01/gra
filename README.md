# OSTATNI PERON

Gra 2D w stylu pixel-art, inspirowana *Papers, Please* i *Death and Taxes*.
Jesteś konduktorem nocnego pociągu **Linia 13**. Zanim pasażer wsiądzie, sprawdzasz jego dokumenty i decydujesz: wpuścić czy odmówić.

**Status:** wczesny prototyp (wersja z 19 września 2026, 01:52). Nie ma jeszcze docelowej grafiki ani dźwięku.

## Wymagania

- Unity 6 (6000.6.0f1)
- Universal Render Pipeline (2D)
- TextMeshPro, Input System

## Jak uruchomić

1. Otwórz folder projektu w Unity Hub.
2. Otwórz scenę `MainMenu` (Assets/Scenes) i naciśnij Play.
3. Obie sceny muszą być na liście w Build Profiles: `MainMenu` (indeks 0) i `Peron` (indeks 1).

## Rozgrywka (obecny prototyp)

Zmiana to 10 pasażerów. Przy każdym widzisz cztery panele na biurku: **bilet**, **dowód**, **bazę kolejową** i **rejestr**. W tych źródłach mogą być drobne różnice w imieniu, dacie urodzenia lub statusie.

- **WPUŚĆ** – wpuszczasz pasażera
- **ODMÓW** – odmawiasz pasażerowi

Ukryta prawda o pasażerze (gra jej nie pokazuje):

| Prawda | Szansa | Znaczenie |
|---|---|---|
| Żywy | 60% | wszystkie dokumenty się zgadzają |
| Zmarły | 25% | rejestr mówi, że osoba nie żyje |
| Nieistniejący | 15% | data urodzenia różni się o rok, rodzice zmarli przed urodzeniem |

Zasada dnia 1: wpuszczaj tylko żywych. Po każdej decyzji dostajesz informację, czy było dobrze i kim naprawdę był pasażer. Po 10 pasażerach zmiana się kończy i pojawia się przycisk MENU.

## Struktura projektu

```
Assets/
  Scenes/    MainMenu, Peron
  Scripts/
    GameManager.cs          logika gry, decyzje, wynik
    UIController.cs         wypełnia panele, wynik i komunikaty
    PassengerData.cs        dane: PassengerTruth, Record, PassengerData
    PassengerGenerator.cs   generator losowych pasażerów
    MainMenu.cs             Play / Options / Quit
```

Teksty w grze są po polsku, a kod, komentarze i nazwy obiektów po angielsku.

## Plan rozwoju

- [ ] Przeciąganie dokumentów i pieczątki
- [ ] Rozmowy z pasażerami
- [ ] Cykl dni ze zmieniającymi się zasadami
- [ ] Pociąg reagujący na błędy
- [ ] Zakończenie z twistem (pasażerem jesteś ty)
- [ ] Grafika pixel-art i dźwięk
- [ ] Premiera na Steam

## Git

Praca odbywa się na gałęzi `dev/iansky`. Foldery `Library/`, `Temp/`, `Logs/`, `obj/` i `UserSettings/` są ignorowane przez `.gitignore`.
