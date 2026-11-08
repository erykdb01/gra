# -*- coding: utf-8 -*-
"""Diagramy sekwencji (główny przebieg każdego przypadku użycia). Nazwy metod pochodzą z kodu C#."""
import os
from uml import sequence, legend

OUT = os.path.join(os.path.dirname(__file__), "..", "sequence")

# 1. Rozpocznij noc
sequence(os.path.join(OUT, "sequence diagram rozpocznij noc.png"),
    [("G", "Gracz", "blue"), ("MM", ":MainMenu", "purple"), ("GM", ":GameManager", "green"),
     ("SM", ":StoryManager", "teal"), ("SS", ":SaveSystem", "orange")],
    [
        ("call", "G", "MM", "wybierzTryb(\"STORY MODE\")"),
        ("call", "MM", "GM", "BeginStory(resume) + LoadScene(\"Peron\")"),
        ("call", "GM", "SM", "Begin()"),
        ("call", "SM", "SS", "Data"),
        ("ret", "SS", "SM", "StoryProgress"),
        ("ret", "SM", "GM", "nightIndex"),
        ("call", "GM", "SM", "Night(nightIndex)"),
        ("ret", "SM", "GM", "DayConfig"),
        ("call", "GM", "G", "pokażBriefing(tytuł, zasada)"),
        ("ret", "G", "GM", "ROZPOCZNIJ ZMIANĘ"),
        ("self", "GM", "GM", "zaplanujWydarzenia()"),
        ("ret", "GM", "MM", "noc rozpoczęta"),
    ])

# 2. Sprawdź dokumenty
sequence(os.path.join(OUT, "sequence diagram sprawdz dokumenty.png"),
    [("G", "Gracz", "blue"), ("GM", ":GameManager", "green"), ("DI", ":DocumentInspector", "purple"),
     ("DS", ":DocumentSystem", "teal"), ("AS", ":AnomalySystem", "orange")],
    [
        ("call", "G", "GM", "OpenInspector(type)"),
        ("call", "GM", "DI", "Open(onClosed, type)"),
        ("ret", "DI", "G", "widok dokumentów"),
        ("call", "G", "DI", "Query(q)"),
        ("call", "DI", "DS", "Query(passenger, q, out anomaly)"),
        ("ret", "DS", "DI", "wynik, anomaly"),
        ("call", "DI", "AS", "Discover(anomaly, Documents)"),
        ("ret", "AS", "DI", "true (nowa anomalia)"),
        ("ret", "DI", "G", "komunikat ANOMALIA"),
        ("call", "G", "DI", "Close()"),
        ("ret", "DI", "GM", "onClosed()"),
    ])

# 3. Przesłuchaj pasażera
sequence(os.path.join(OUT, "sequence diagram przesluchaj pasazera.png"),
    [("G", "Gracz", "blue"), ("DP", ":DialoguePanel", "purple"), ("GM", ":GameManager", "green"),
     ("DM", ":DialogueManager", "teal"), ("AS", ":AnomalySystem", "orange")],
    [
        ("call", "G", "DP", "wybierzPytanie(q)"),
        ("call", "DP", "GM", "OnAsk(q)"),
        ("call", "GM", "DM", "Ask(q)"),
        ("self", "DM", "DM", "Give(Interrogation, xp)"),
        ("call", "DM", "AS", "Discover(q.anomaly, Interrogation)"),
        ("ret", "AS", "DM", "true"),
        ("ret", "DM", "GM", "odpowiedź + oznaka kłamstwa"),
        ("call", "GM", "DM", "Offered()"),
        ("ret", "DM", "GM", "List<QA>"),
        ("call", "GM", "DP", "ShowAnswer(q, answer, pytania)"),
        ("ret", "DP", "G", "odpowiedź pasażera"),
    ])

# 4. Podejmij decyzję
sequence(os.path.join(OUT, "sequence diagram podejmij decyzje.png"),
    [("G", "Gracz", "blue"), ("GM", ":GameManager", "green"), ("D", ":Days", "teal"),
     ("PS", ":PlayerStats", "purple"), ("TS", ":TrainSystem", "orange"), ("AC", ":AchievementSystem", "pink")],
    [
        ("call", "G", "GM", "Admit() / Deny()"),
        ("self", "GM", "GM", "Resolve(admit)"),
        ("call", "GM", "D", "ShouldAdmit(passenger, day)"),
        ("ret", "D", "GM", "should"),
        ("call", "GM", "PS", "AddDiscipline(d), AddStress(raw)"),
        ("ret", "PS", "GM", "stres po zmianie"),
        ("call", "GM", "TS", "OnDecision(p, admit, correct)"),
        ("ret", "TS", "GM", "integralność pociągu"),
        ("call", "GM", "AC", "OnDecision(p, admit, correct)"),
        ("ret", "AC", "GM", "osiągnięcia"),
        ("self", "GM", "GM", "CheckFailure()"),
        ("ret", "GM", "G", "komunikat zwrotny"),
    ], colw=270)

# 5. Zakończ noc
sequence(os.path.join(OUT, "sequence diagram zakoncz noc.png"),
    [("G", "Gracz", "blue"), ("GM", ":GameManager", "green"), ("AC", ":AchievementSystem", "pink"),
     ("SM", ":StoryManager", "teal"), ("SS", ":SaveSystem", "orange"), ("RU", ":ShiftResultsUI", "purple")],
    [
        ("call", "G", "GM", "decyzja o ostatnim pasażerze"),
        ("self", "GM", "GM", "FinishNight()"),
        ("call", "GM", "AC", "OnShiftComplete(report, night, story)"),
        ("ret", "AC", "GM", "odblokowane osiągnięcia"),
        ("call", "GM", "SM", "Commit(nextNight, integrity)"),
        ("call", "SM", "SS", "Save()"),
        ("ret", "SS", "SM", "zapisano (.json + .bak)"),
        ("ret", "SM", "GM", "postęp zapisany"),
        ("call", "GM", "RU", "Show(report, head, outro)"),
        ("ret", "RU", "G", "ekran wyników"),
        ("call", "G", "RU", "NASTĘPNA NOC"),
        ("ret", "RU", "GM", "Choice = 0"),
    ], colw=270)

legend(os.path.join(OUT, "legenda diagramu sekwencji.png"), [
    ("lifeobj", "Uczestnik: aktor lub obiekt (:Klasa = instancja klasy)", "green", ":Obiekt"),
    ("lifeline", "Linia życia: czas płynie z góry na dół"),
    ("bar", "Pas aktywacji: obiekt wykonuje operację"),
    ("call", "Komunikat synchroniczny: wywołanie metody"),
    ("ret", "Komunikat zwrotny: wynik lub odpowiedź"),
    ("selfcall", "Wywołanie własnej metody obiektu"),
], width=1100)
print("ok")
