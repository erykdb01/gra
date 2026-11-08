# -*- coding: utf-8 -*-
"""Diagramy aktywności dla pięciu przypadków użycia gry Ostatni Peron."""
import os
from uml import activity, legend

OUT = os.path.join(os.path.dirname(__file__), "..", "activity")
G, P, S, D = "Gracz (konduktor)", "Pasażer", "System gry", "Dane gry"

# 1. Rozpocznij noc --------------------------------------------------------------
activity(os.path.join(OUT, "rozpocznij noc.png"),
    [(G, "blue", 1), (S, "green", 2), (D, "orange", 1)],
    {
        "s":  ("start", G, 0, 0, ""),
        "a1": ("act", G, 0, 1, "Wybierz tryb w menu głównym"),
        "a2": ("act", S, 0, 2, "Wczytaj stan gry"),
        "a3": ("act", D, 0, 2, "Odczytaj plik zapisu"),
        "d1": ("dec", S, 0, 3, "Tryb kampanii?"),
        "a4": ("act", S, 1, 4, "Wczytaj noc z kampanii"),
        "a5": ("act", D, 0, 4, "Pobierz dane nocy"),
        "a6": ("act", S, 0, 4, "Zbuduj noc proceduralną"),
        "a7": ("act", S, 0, 5, "Pokaż briefing i zasadę nocy"),
        "a8": ("act", G, 0, 6, "Przeczytaj briefing i rozpocznij zmianę"),
        "a9": ("act", S, 0, 7, "Zaplanuj wydarzenia nocy"),
        "a10": ("act", S, 0, 8, "Wprowadź pociąg i pierwszego pasażera"),
        "e":  ("end", G, 0, 8, ""),
    },
    [
        ("s", "a1", None), ("a1", "a2", None), ("a2", "a3", None), ("a3", "d1", None),
        ("d1", "a4", "Tak"), ("d1", "a6", "Nie (Nocna zmiana)"), ("a4", "a5", None),
        ("a5", "a7", None, "side"), ("a6", "a7", None, "side"),
        ("a7", "a8", None), ("a8", "a9", None), ("a9", "a10", None), ("a10", "e", None),
    ])

# 2. Sprawdź dokumenty --------------------------------------------------------------
activity(os.path.join(OUT, "sprawdz dokumenty.png"),
    [(G, "blue", 1), (S, "green", 2), (D, "orange", 1)],
    {
        "s":  ("start", G, 0, 0, ""),
        "a1": ("act", G, 0, 1, "Otwórz tryb inspekcji"),
        "a2": ("act", S, 0, 1, "Pokaż dokumenty dostępne tej nocy"),
        "a3": ("act", G, 0, 2, "Wybierz dokument i sposób sprawdzenia"),
        "d1": ("dec", S, 0, 3, "Zapytanie do bazy?"),
        "a4": ("act", S, 0, 4, "Porównaj pole z wzorcem (podpis, zdjęcie, data, skan)"),
        "a5": ("act", S, 1, 4, "Wyślij zapytanie z terminala"),
        "a6": ("act", D, 0, 4, "Wyszukaj osobę w rejestrze"),
        "d2": ("dec", S, 0, 5, "Znaleziono anomalię?"),
        "a7": ("act", S, 1, 6, "Zapisz anomalię, przyznaj XP i dodaj stres"),
        "a8": ("act", S, 0, 6, "Pokaż komunikat bez zastrzeżeń"),
        "a9": ("act", G, 0, 7, "Odczytaj wynik sprawdzenia"),
        "d3": ("dec", G, 0, 8, "Sprawdzić coś jeszcze?"),
        "a10": ("act", G, 0, 9, "Zamknij inspekcję"),
        "e":  ("end", G, 0, 10, ""),
    },
    [
        ("s", "a1", None), ("a1", "a2", None), ("a2", "a3", None, "vh"), ("a3", "d1", None, "side"),
        ("d1", "a4", "Nie"), ("d1", "a5", "Tak"), ("a5", "a6", None),
        ("a4", "d2", None), ("a6", "d2", None),
        ("d2", "a8", "Nie"), ("d2", "a7", "Tak"),
        ("a8", "a9", None, "vh"), ("a7", "a9", None, "vh"),
        ("a9", "d3", None), ("d3", "a10", "Nie"), ("d3", "a3", "Tak", "loop"), ("a10", "e", None),
    ])

# 3. Przesłuchaj pasażera ---------------------------------------------------------
activity(os.path.join(OUT, "przesluchaj pasazera.png"),
    [(G, "blue", 1), (P, "purple", 1), (S, "green", 2)],
    {
        "s":  ("start", G, 0, 0, ""),
        "a1": ("act", S, 0, 1, "Pokaż 4–6 pytań zależnie od umiejętności"),
        "a2": ("act", G, 0, 2, "Wybierz pytanie"),
        "a3": ("act", P, 0, 3, "Odpowiedz (prawda, półprawda, kłamstwo lub unik)"),
        "a4": ("act", S, 0, 4, "Przyznaj XP i zastosuj skutki (stres, flaga, Kodeks)"),
        "d1": ("dec", S, 0, 5, "Odpowiedź ujawnia anomalię?"),
        "a5": ("act", S, 1, 6, "Zarejestruj anomalię"),
        "d2": ("dec", S, 0, 7, "Przesłuchanie ≥ 3 i odpowiedź nieszczera?"),
        "a6": ("act", S, 1, 8, "Dopisz oznakę (kłamie, waha się, unika)"),
        "a7": ("act", G, 0, 9, "Przeczytaj odpowiedź"),
        "d3": ("dec", G, 0, 10, "Zadać kolejne pytanie?"),
        "e":  ("end", G, 0, 11, ""),
    },
    [
        ("s", "a1", None, "vh"), ("a1", "a2", None, "vh"), ("a2", "a3", None, "hv"), ("a3", "a4", None, "hv"),
        ("a4", "d1", None), ("d1", "a5", "Tak"), ("d1", "d2", "Nie"), ("a5", "d2", None),
        ("d2", "a6", "Tak"), ("d2", "a7", "Nie", "hv"), ("a6", "a7", None, "vh"),
        ("a7", "d3", None), ("d3", "e", "Nie"), ("d3", "a2", "Tak", "loop"),
    ])

# 4. Podejmij decyzję ----------------------------------------------------------------
activity(os.path.join(OUT, "podejmij decyzje.png"),
    [(G, "blue", 1), (P, "purple", 1), (S, "green", 2)],
    {
        "s":  ("start", G, 0, 0, ""),
        "a1": ("act", G, 0, 1, "Kliknij WPUŚĆ lub ODMÓW"),
        "a2": ("act", S, 0, 2, "Przybij stempel"),
        "a3": ("act", S, 0, 3, "Porównaj decyzję z zasadą nocy"),
        "d1": ("dec", S, 0, 4, "Decyzja poprawna?"),
        "a4": ("act", S, 0, 5, "Dodaj dyscyplinę, reputację i XP"),
        "a5": ("act", S, 1, 5, "Dodaj stres, obniż reputację, uszkodź pociąg"),
        "d2": ("dec", S, 1, 6, "Limit błędów przekroczony?"),
        "a6": ("act", S, 1, 7, "Przerwij noc (ZWOLNIONY)"),
        "e1": ("end", S, 1, 8, ""),
        "a7": ("act", S, 0, 8, "Ustaw flagi fabularne i stan pociągu"),
        "a8": ("act", S, 0, 9, "Pokaż komunikat zwrotny"),
        "a9": ("act", P, 0, 9, "Wsiądź do pociągu albo odejdź z peronu"),
        "e2": ("end", G, 0, 9, ""),
    },
    [
        ("s", "a1", None), ("a1", "a2", None, "hv"), ("a2", "a3", None), ("a3", "d1", None),
        ("d1", "a4", "Tak"), ("d1", "a5", "Nie"), ("a5", "d2", None), ("d2", "a6", "Tak"),
        ("d2", "a7", "Nie", "hv"), ("a6", "e1", None), ("a4", "a7", None),
        ("a7", "a8", None), ("a8", "a9", None), ("a9", "e2", None),
    ])

# 5. Zakończ noc i zapisz postęp ----------------------------------------------------
activity(os.path.join(OUT, "zakoncz noc.png"),
    [(G, "blue", 2), (S, "green", 2), (D, "orange", 1)],
    {
        "s":  ("start", G, 0, 0, ""),
        "a1": ("act", G, 0, 1, "Obsłuż ostatniego pasażera nocy"),
        "a2": ("act", S, 0, 2, "Odprowadź pociąg z peronu"),
        "a3": ("act", S, 0, 3, "Podsumuj zmianę (raport nocy)"),
        "a4": ("act", S, 0, 4, "Sprawdź i odblokuj osiągnięcia"),
        "d1": ("dec", S, 0, 5, "Tryb kampanii?"),
        "a5": ("act", S, 0, 6, "Zapisz postęp kampanii"),
        "a6": ("act", S, 1, 6, "Zapisz rekordy Nocnej zmiany"),
        "a7": ("act", D, 0, 7, "Zapisz plik atomowo (.tmp, kopia .bak)"),
        "a8": ("act", S, 0, 8, "Pokaż ekran wyników"),
        "d2": ("dec", G, 0, 9, "Następna noc?"),
        "a9": ("act", G, 0, 10, "Rozpocznij kolejną noc"),
        "a10": ("act", G, 1, 10, "Wróć do menu głównego"),
        "e1": ("end", G, 0, 11, ""),
        "e2": ("end", G, 1, 11, ""),
    },
    [
        ("s", "a1", None), ("a1", "a2", None, "hv"), ("a2", "a3", None), ("a3", "a4", None),
        ("a4", "d1", None), ("d1", "a5", "Tak"), ("d1", "a6", "Nie"),
        ("a5", "a7", None, "side"), ("a6", "a7", None, "side"),
        ("a7", "a8", None, "vh"), ("a8", "d2", None),
        ("d2", "a9", "Tak"), ("d2", "a10", "Nie"), ("a9", "e1", None), ("a10", "e2", None),
    ])

legend(os.path.join(OUT, "legenda diagramu aktywnosci.png"), [
    ("start", "Węzeł początkowy: start procesu"),
    ("end", "Węzeł końcowy: koniec procesu (może być kilka)"),
    ("act", "Czynność: pojedynczy krok procesu"),
    ("dec", "Punkt decyzyjny: warunek rozgałęziający"),
    ("arrow", "Strzałka: kierunek przepływu sterowania"),
    ("lane", "Nagłówek toru (swimlane): kto wykonuje krok", "blue"),
    ("label", "Etykieta na strzałce: wynik warunku z rombu"),
    ("lane", "Kolor pola: przypisanie do konkretnego toru", "purple"),
])
print("ok")
