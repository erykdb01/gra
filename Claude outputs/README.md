# OSTATNI PERON / THE LAST PLATFORM

**PL** | [EN](#english)

Pixel-artowy horror / mystery / narrative management. Unity 6 (6000.6.0f1, URP 2D, TextMeshPro, Input System). Po polsku i po angielsku. Cała grafika i cały dźwięk są generowane w kodzie.

## O czym jest gra

Jesteś konduktorem nocnego pociągu **Linia 13**. Każdej nocy na peronie stoi **13 osób**. Sprawdzasz dokumenty, rozmawiasz, szukasz anomalii i decydujesz: **WPUŚĆ** albo **ODMÓW**. Nie każdy jest tym, za kogo się podaje. Gra nigdy nie mówi, kim naprawdę jest pasażer: żywym, zmarłym, nieistniejącym, zmienionym, pętlą, echem, impostorem albo czymś, czego system nie potrafi nazwać.

## Tryby

- **STORY MODE** - 12 nocy w 7 aktach (Zmiana, Złe nazwiska, Martwy peron, Linia 13, Konduktor, Ostatni pasażer, Ostatni peron). Każda noc ma własną zasadę, briefing, nowych pasażerów, wydarzenia i dokumenty. Postacie wracają (Anna, Marek, Lena, inspektor Kruk, Piotr, Człowiek w Szarym Płaszczu, człowiek bez dokumentów), a ich los zależy od twoich decyzji. Pięć zakończeń: A (rozkazy), B (złamany system), C (prawda), D (część Linii 13) i sekretne.
- **NIGHT SHIFT** - nieskończona zmiana generowana proceduralnie. Z każdym poziomem więcej fałszerstw, anomalii, dokumentów i wydarzeń, mniej czasu. Rekord, najlepszy poziom, najdłuższa seria.
- **CONTINUE** wznawia kampanię z zapisu.

## Rozgrywka

- **9 dokumentów** odblokowywanych wraz z nocami: bilet, dowód, baza kolejowa, rejestr, manifest, karta medyczna, akt zgonu, historia podróży, rozkazy.
- **Tryb inspekcji** (przycisk DOKUMENTY albo klik w dokument): przełączanie kart, porównywanie dwóch dokumentów, obracanie, powiększanie, oznaczanie podejrzanych pól, skan mikrotekstu, sprawdzanie podpisu i zdjęcia oraz **terminal bazy danych**.
- **Rozmowa:** 4-6 pytań (zależnie od umiejętności), pytania zależne od dokumentów, kłamstwa, półprawdy i wymijające odpowiedzi.
- **5 umiejętności** (poziomy 1-10, XP): Obserwacja, Dokumenty, Przesłuchanie, Opanowanie, Autorytet.
- **Statystyki:** Dyscyplina, Sumienie, Stres, Reputacja, Autorytet, Integralność pociągu, Wynik zmiany. Pełny panel pod **TAB**.
- **Stres:** drżenie ekranu, migotanie latarni, zniekształcony tekst, szepty, oddech.
- **Pociąg jako bohater:** integralność, temperatura, prąd, ciśnienie, sygnał. Gaśnie, migocze, otwiera drzwi, pokazuje zły numer linii.
- **18 wydarzeń** (losowych i fabularnych): awarie, pukanie z wagonu 13, zegar na 03:13, 14 pasażerów, zdjęcie konduktora na cudzym dowodzie, teczka konduktora, terminal Rejestru.
- **Horror psychologiczny i rzadki:** martwi przestają oddychać, nieistniejący nie mają cienia, światło się zmienia. Bez jumpscare'ów co chwilę.
- **Ostatni pasażer:** "PASAŻERÓW POZOSTAŁO: 1". Na peron wchodzi konduktor, status: NIEZNANY.
- Ekran wyników zmiany z animowanymi licznikami, **Kodeks** (8 kategorii), **28 osiągnięć** (w tym sekrety), pauza (Kontynuuj, Kodeks, Ustawienia, Powtórz noc, Menu).
- Ustawienia: głośność główna/muzyki/efektów, pełny ekran, rozdzielczość, język, trzęsienie ekranu, efekty grozy, tempo tekstu. Zapis JSON: `ostatni_peron_save.json` w folderze `persistentDataPath`.

## Sterowanie

Mysz: przeciąganie dokumentów, klikanie pytań i przycisków. **TAB** status, **ESC** pauza / zamknięcie panelu, **SPACJA** przyspiesza pisanie tekstu.

## Struktura kodu (Assets/Scripts)

`Core/` zapis, stan gry, zdarzenia | `Story/` noce, postacie, flagi, zakończenia, tryb nieskończony | `Documents/` system dokumentów i inspektor | `Dialogue/` | `Horror/` stres i anomalie | `Train/` | `Progression/` umiejętności, statystyki, kodeks, osiągnięcia | `Events/` | `UI/` | `World/` oświetlenie i pixel-art | pliki w katalogu głównym to stare skrypty rozbudowane w miejscu.

## Jak uruchomić i zbudować

Otwórz projekt w Unity 6, scena `MainMenu`, Play. Build: menu **Ostatni Peron > Build Windows (Desktop)** (sceny MainMenu i Peron, wynik na Pulpicie).

## Autorzy

Illia (iansky), Eryk (erykd), Emilia

---

## English

Pixel-art horror / mystery / narrative management game. Unity 6 (6000.6.0f1, URP 2D, TextMeshPro, Input System). Polish and English. All graphics and audio are generated in code.

### About

You are the conductor of the night train **Line 13**. Every night **13 people** stand on the platform. You check documents, talk, hunt for anomalies and decide: **ADMIT** or **DENY**. Not everyone is who they claim to be. The game never tells you what a passenger truly is: living, dead, non-existent, altered, a loop, an echo, an impostor, or something the system cannot name.

### Modes

- **STORY MODE** - 12 nights in 7 acts. Every night has its own rule, briefing, new passengers, events and documents. Recurring characters (Anna, Marek, Lena, Inspector Kruk, Piotr, the Man in Grey, the man without papers) react to your choices. Five endings: A (orders), B (broken system), C (truth), D (part of Line 13) and a secret one.
- **NIGHT SHIFT** - an endless, procedurally generated shift. Each level brings more forgeries, anomalies, documents and events, and less time. High score, best level, best streak.
- **CONTINUE** resumes the campaign from the save.

### Gameplay

9 documents unlocked over the nights; an **Inspect mode** (compare, rotate, zoom, mark suspicious fields, scan microtext, check signature and photo, database terminal); 4-6 dialogue questions with lies, half-truths and evasions; 5 skills (Observation, Documents, Interrogation, Composure, Authority); Discipline, Conscience, Stress, Reputation, Authority, Train Integrity and Shift Score (full panel on **TAB**); a train with integrity, temperature, electricity, pressure and signal; 18 events; rare psychological horror; the last passenger is you. Shift results with animated counters, an 8-category Codex, 28 achievements, a pause menu, full settings and a JSON save.

### Controls

Mouse to drag documents and click. **TAB** status, **ESC** pause / close panel, **SPACE** skips the typewriter.

### Build

Menu **Ostatni Peron > Build Windows (Desktop)**.

### Authors

Illia (iansky), Eryk (erykd), Emilia
