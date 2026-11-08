# Dokumentacja projektu

**Ostatni Peron / The Last Platform**
Gra narracyjna o kontroli dokumentów: analiza, projekt i model danych

**Wersja:** 1.0
**Zakres dokumentu:** opis projektu, wymagania, przypadki użycia, modele UML (aktywności, sekwencji, klas), model danych (ERD), architektura i uruchomienie
**Autorzy:** Illia (iansky), Eryk (erykd), Emilia
**Repozytorium:** https://github.com/erykdb01/gra

---

## 1. Opis projektu

### 1.1 Cel systemu

Ostatni Peron to jednoosobowa gra komputerowa na Windows z gatunku horroru psychologicznego, w stylu *Papers, Please*. Gracz jest konduktorem nocnego pociągu Linii 13. Każdej nocy sprawdza dokumenty 13 pasażerów, rozmawia z nimi i decyduje, czy ich wpuścić, czy odmówić. Gra ocenia decyzje według zasady danej nocy, prowadzi statystyki konduktora, rozwija jego umiejętności i zapisuje postęp kampanii.

### 1.2 Zakres

System obejmuje:

- dwa tryby gry: kampanię fabularną (12 nocy w 7 aktach, 5 zakończeń) i nieskończoną Nocną zmianę z poziomami trudności,
- generowanie pasażerów z jedną z 8 ukrytych prawd oraz ich dokumentów i dialogów,
- tryb inspekcji 9 rodzajów dokumentów (z terminalem bazy danych),
- przesłuchanie pasażera (4–6 pytań, kłamstwa i półprawdy),
- ocenę decyzji, statystyki konduktora, 5 umiejętności, stan pociągu i wydarzenia losowe,
- Kodeks (encyklopedię w grze), osiągnięcia, ustawienia i trzy języki (PL, EN, UA),
- zapis stanu gry w pliku JSON oraz bazę danych SQLite z treścią gry.

Poza zakresem są: tryb wieloosobowy, zapis w chmurze, wersje na konsole i telefony, edytor poziomów oraz zakupy w grze.

### 1.3 Użytkownicy

- **Gracz (konduktor):** wybiera tryb gry, sprawdza dokumenty, przesłuchuje pasażerów, podejmuje decyzje, przegląda Kodeks i osiągnięcia.
- **Pasażer (aktor drugorzędny):** postać sterowana przez system. Odpowiada na pytania, prosi o wpuszczenie, a po decyzji wsiada do pociągu albo odchodzi.

### 1.4 Technologie

| Element | Technologia |
|---|---|
| Silnik | Unity 6 (6000.6.0f1), Universal Render Pipeline 2D |
| Język | C# |
| Interfejs | Unity UI + TextMeshPro, Input System |
| Grafika i dźwięk | generowane w kodzie (pixel art, synteza dźwięku), bez zewnętrznych plików |
| Zapis gry | plik JSON (`ostatni_peron_save.json`) w `Application.persistentDataPath` |
| Baza danych treści | SQLite 3 (`Assets/StreamingAssets/Data/ostatni_peron.db`, schemat `schema.sql`) |
| Platforma | Windows 64-bit (Desktop) |
| Kontrola wersji | Git + GitHub |

## 2. Wymagania funkcjonalne

Wymagania wynikają wprost z przypadków użycia. Kolumna „Aktor” wskazuje, kto inicjuje daną funkcję.

| ID | Wymaganie | Aktor | Przypadek użycia |
|---|---|---|---|
| WF-01 | System umożliwia wybór trybu: nowa kampania, kontynuacja kampanii lub Nocna zmiana. | Gracz | Rozpocznij noc |
| WF-02 | System wczytuje zapis gry; jeśli plik jest uszkodzony, korzysta z kopii `.bak`, a gdy jej brak, tworzy nowy zapis. | System | Wczytaj zapis gry |
| WF-03 | W kampanii system wczytuje konfigurację bieżącej nocy (zasada, pasażerowie fabularni, wydarzenia, dostępne dokumenty). | System | Rozpocznij noc |
| WF-04 | W Nocnej zmianie system buduje noc proceduralnie, a trudność rośnie z każdym poziomem. | System | Rozpocznij noc |
| WF-05 | Przed zmianą system pokazuje briefing i zasadę nocy, a gracz rozpoczyna zmianę przyciskiem. | Gracz | Rozpocznij noc |
| WF-06 | System generuje 13 pasażerów na noc; każdy ma jedną z 8 ukrytych prawd i komplet dokumentów. | System | Rozpocznij noc |
| WF-07 | Gracz może otworzyć tryb inspekcji i przeglądać dokumenty odblokowane danej nocy. | Gracz | Sprawdź dokumenty |
| WF-08 | Gracz może powiększać, obracać i porównywać dokumenty oraz oznaczać podejrzane pola. | Gracz | Sprawdź dokumenty |
| WF-09 | Gracz może sprawdzić podpis, zdjęcie i mikrotekst; odczyt ukrytego tekstu wymaga odpowiedniego poziomu umiejętności Dokumenty. | Gracz | Sprawdź dokumenty |
| WF-10 | Gracz może wysłać zapytanie do terminala bazy danych o osobę z dokumentów. | Gracz | Sprawdź dokumenty |
| WF-11 | System rejestruje wykrytą anomalię, przyznaje XP, dodaje stres i odblokowuje powiązany wpis Kodeksu. | System | Sprawdź dokumenty |
| WF-12 | System proponuje od 4 do 6 pytań; liczba zależy od poziomu umiejętności Przesłuchanie. | System | Przesłuchaj pasażera |
| WF-13 | Pytania wymagające Autorytetu są dostępne dopiero od wskazanego poziomu tej umiejętności. | System | Przesłuchaj pasażera |
| WF-14 | Pasażer odpowiada prawdą, półprawdą, kłamstwem lub wymija się od odpowiedzi. | Pasażer | Przesłuchaj pasażera |
| WF-15 | Od poziomu 3 umiejętności Przesłuchanie system dopisuje oznakę nieszczerości (np. „unika wzroku”, od poziomu 5 „kłamie”). | System | Przesłuchaj pasażera |
| WF-16 | Zadanie pytania zabiera 3 minuty czasu w grze i może ustawić flagę fabularną lub odblokować wpis Kodeksu. | System | Przesłuchaj pasażera |
| WF-17 | Gracz decyduje przyciskiem WPUŚĆ albo ODMÓW; w Nocnej zmianie od poziomu 3 decyzję ogranicza czas. | Gracz | Podejmij decyzję |
| WF-18 | System porównuje decyzję z zasadą nocy; ważny rozkaz Dyrekcji ma pierwszeństwo, a fałszywy lub brakujący bilet oznacza odmowę. | System | Podejmij decyzję |
| WF-19 | Poprawna decyzja zwiększa dyscyplinę, reputację i XP; błędna dodaje stres, obniża reputację i uszkadza pociąg. | System | Podejmij decyzję |
| WF-20 | System przerywa noc, gdy przekroczono limit błędów, stres osiągnął 100 lub integralność pociągu spadła do 0. | System | Podejmij decyzję |
| WF-21 | Po ostatnim pasażerze system pokazuje ekran wyników zmiany (poprawne decyzje, błędy, seria, XP, wynik). | System | Zakończ noc i zapisz postęp |
| WF-22 | System zapisuje postęp kampanii lub rekordy Nocnej zmiany w sposób atomowy (plik tymczasowy i kopia `.bak`). | System | Zakończ noc i zapisz postęp |
| WF-23 | W ostatniej nocy kampanii system wylicza jedno z 5 zakończeń na podstawie flag fabularnych i statystyk. | System | Zakończ noc i zapisz postęp |
| WF-24 | Gracz może przeglądać Kodeks (zablokowane wpisy jako „???”) i listę osiągnięć. | Gracz | Przeglądaj Kodeks i osiągnięcia |
| WF-25 | Gracz może powtórzyć nieudaną noc ze stanu z jej początku. | Gracz | Rozpocznij noc |

## 3. Wymagania niefunkcjonalne

| ID | Kategoria | Wymaganie |
|---|---|---|
| WN-01 | Wydajność | Gra działa płynnie, co najmniej 60 klatek na sekundę w rozdzielczości 1920×1080 na komputerze z Windows 10/11 64-bit i zintegrowaną kartą graficzną. |
| WN-02 | Wydajność | Przejście z menu do pierwszej nocy trwa poniżej 5 sekund. |
| WN-03 | Wydajność | Reakcja interfejsu na kliknięcie (stempel, pytanie, zapytanie do bazy) trwa poniżej 100 ms. |
| WN-04 | Niezawodność | Zapis jest atomowy: plik zapisywany jest najpierw pod nazwą tymczasową, a poprzednia wersja zostaje jako `.bak`, więc awaria w trakcie zapisu nie niszczy postępu. |
| WN-05 | Niezawodność | Uszkodzony lub niepełny plik zapisu jest naprawiany przy wczytaniu (brakujące pola uzupełniane, wartości przycinane do dozwolonych zakresów). |
| WN-06 | Użyteczność | Gra jest w pełni dostępna w 3 językach (polski, angielski, ukraiński); język można zmienić w menu i w ustawieniach. |
| WN-07 | Użyteczność | Efekty grozy i trzęsienie ekranu można wyłączyć w ustawieniach. |
| WN-08 | Użyteczność | Całą grę można obsłużyć myszą; skróty: TAB (status), ESC (pauza), SPACJA (pominięcie pisania tekstu). |
| WN-09 | Przenośność | Gotowy build to folder z plikiem `.exe`, który działa bez instalacji. |
| WN-10 | Utrzymanie | Treść gry (noce, pasażerowie, wydarzenia) jest oddzielona od logiki i zapisana danymi; teksty przechodzą przez jedną funkcję lokalizacji `Loc.T()`. |
| WN-11 | Rozmiar | Projekt nie zawiera zewnętrznych plików graficznych ani dźwiękowych; cała grafika i dźwięk są generowane w kodzie. |
| WN-12 | Spójność danych | Identyfikatory flag, wpisów Kodeksu, osiągnięć i zakończeń są unikalne w całej grze. |

## 4. Słownik pojęć

| Pojęcie | Znaczenie |
|---|---|
| Konduktor | Postać gracza: pracownik peronu Linii 13, który sprawdza pasażerów. |
| Pasażer | Osoba w kolejce do pociągu; ma tożsamość, dokumenty, dialog i ukrytą prawdę. |
| Ukryta prawda | Kim pasażer naprawdę jest: Żywy, Martwy, Nieistniejący, Zmieniony, Pętla, Impostor, Echo lub Nieznany. Gra nigdy nie mówi tego wprost. |
| Noc (zmiana) | Jedna runda gry: briefing, 13 pasażerów, ekran wyników. |
| Zasada nocy | Reguła, kogo wolno wpuścić danej nocy (np. „wpuszczaj tylko żywych”). |
| Akt | Grupa nocy w kampanii (7 aktów, 12 nocy). |
| Nocna zmiana | Nieskończony tryb proceduralny z rosnącym poziomem trudności. |
| Dokument | Jeden z 9 rodzajów: bilet, dowód osobisty, baza kolejowa, rejestr, manifest, karta medyczna, akt zgonu, historia podróży, rozkazy. |
| Tryb inspekcji | Widok, w którym gracz dokładnie bada dokumenty i korzysta z terminala bazy danych. |
| Anomalia | Wykryta niezgodność, np. fałszywy podpis, niemożliwa data, wpis o śmierci. |
| Rozkaz Dyrekcji | Dokument, który (jeśli jest ważny) nakazuje wpuścić pasażera niezależnie od zasady nocy. |
| Dyscyplina / Sumienie | Statystyki konduktora (0–10), często sobie przeciwstawne. |
| Stres | Napięcie konduktora (0–100). Osiągnięcie 100 kończy noc porażką. |
| Reputacja / Autorytet | Reputacja (0–100) i wyliczany z niej oraz z umiejętności Autorytet. |
| Integralność pociągu | Stan pociągu (0–100); spada po błędach. 0 kończy noc porażką. |
| Umiejętność | Jedna z 5: Obserwacja, Dokumenty, Przesłuchanie, Opanowanie, Autorytet (poziomy 1–10). |
| Flaga fabularna | Zapamiętana decyzja gracza, od której zależą dalsze wydarzenia i zakończenie. |
| Kodeks | Encyklopedia w grze; wpisy odblokowują się w trakcie rozgrywki. |
| Zakończenie | Jedno z 5 zakończeń kampanii (A, B, C, D i sekretne). |

## 5. Założenia i ograniczenia

### 5.1 Założenia

- Każda noc ma 13 pasażerów. W ostatniej nocy kampanii trzynastym pasażerem jest sam konduktor.
- Kampania ma 12 nocy w 7 aktach. Limit błędów wynosi od 3 do 5 na noc, a w nocach 11 i 12 nie ma limitu (nikt nie ocenia decyzji).
- Zegar nocy startuje o 22:00. Każdy obsłużony pasażer to +25 minut, każde pytanie +3 minuty.
- W nocach z aktów 1–3 (i na poziomach 1–2 Nocnej zmiany) gra pokazuje po decyzji, kim był pasażer. Później komunikaty są niejednoznaczne.
- Stres mieści się w zakresie 0–100. Na początku nocy spada o 20, przy poprawnej decyzji o 3.
- Błędne wpuszczenie pasażera, który nie jest żywy, obniża integralność pociągu o 9, inny błąd o 5, a poprawna decyzja ją podnosi o 2.
- Umiejętności mają poziomy 1–10. Próg XP poziomu *n* wynosi 50·(n−1)·n/2.
- Autorytet = poziom umiejętności Autorytet × 8 + reputacja / 5 (zakres 0–100).
- Liczba pytań w rozmowie = 4 + poziom Przesłuchania / 3 (od 4 do 6).
- Nocna zmiana: limit czasu na pasażera od poziomu 3 wynosi max(18, 74 − 4·poziom) sekund. Poprawna decyzja daje 100 + 15·poziom + 10·min(seria, 10) punktów, błędna odejmuje 120.

### 5.2 Ograniczenia

- Gra jest jednoosobowa i ma jeden slot zapisu kampanii.
- Platformą docelową jest Windows 64-bit.
- Baza SQLite przechowuje treść gry (noce, pasażerów, pytania, wydarzenia, Kodeks, osiągnięcia, zakończenia). Tabele stanu zapisu (`zapis*`, `decyzja`) są przygotowane w schemacie, ale obecna wersja gry zapisuje postęp do pliku JSON.

## 6. Przypadki użycia

### 6.1 Diagram przypadków użycia

![Diagram przypadków użycia](../diagramy/use%20case/diagram%20przypadkow%20uzycia.png)

### 6.2 Zestawienie

| Przypadek użycia | Aktor główny | Zależności |
|---|---|---|
| Rozpocznij noc | Gracz | «include» Wczytaj zapis gry |
| Wczytaj zapis gry | (System) | włączany przez Rozpocznij noc |
| Sprawdź dokumenty | Gracz | «extend» Podejmij decyzję |
| Przesłuchaj pasażera | Gracz (Pasażer jako aktor drugorzędny) | «extend» Podejmij decyzję |
| Podejmij decyzję | Gracz (Pasażer jako aktor drugorzędny) | rozszerzany przez Sprawdź dokumenty, Przesłuchaj pasażera i Zakończ noc |
| Zakończ noc i zapisz postęp | Gracz (wywoływany po ostatnim pasażerze) | «extend» Podejmij decyzję; «extend» Przeglądaj Kodeks i osiągnięcia |
| Przeglądaj Kodeks i osiągnięcia | Gracz | rozszerzany przez Zakończ noc i zapisz postęp |

Szczegółowe opisy przypadków użycia są również w pliku [`diagramy/use case/przypadki_uzycia.md`](../diagramy/use%20case/przypadki_uzycia.md).

### 6.3 Rozpocznij noc

**Aktor główny:** Gracz
**Cel:** Rozpoczęcie nocnej zmiany w wybranym trybie, z właściwą zasadą, pasażerami i wydarzeniami.

**Warunki wstępne**

- Gra jest uruchomiona, a gracz jest w menu głównym (lub na ekranie wyników poprzedniej nocy)
- Wybrany jest język gry

**Warunki końcowe (sukces)**

- Wczytana jest konfiguracja nocy (zasada, limit błędów, dostępne dokumenty, wydarzenia)
- Pociąg stoi na peronie, a do okienka podchodzi pierwszy pasażer

**Główny przebieg zdarzeń**

1. Gracz wybiera w menu tryb: STORY MODE, CONTINUE albo NIGHT SHIFT
2. System wczytuje zapis gry («include» Wczytaj zapis gry)
3. W kampanii system ustala numer bieżącej nocy i wczytuje jej konfigurację
4. System odblokowuje wpisy Kodeksu przypisane do nocy
5. System pokazuje briefing: tytuł nocy, akt, opis i zasadę
6. Gracz czyta briefing i klika ROZPOCZNIJ ZMIANĘ
7. System losuje wydarzenia nocy i przypisuje je do pozycji w kolejce
8. Pociąg wjeżdża na peron, a system generuje pierwszego pasażera

**Przebiegi alternatywne**

- 1a. Nowa kampania (STORY MODE) przy istniejącym zapisie wymaga potwierdzenia drugim kliknięciem; system zeruje postęp kampanii, zachowując Kodeks, osiągnięcia i ustawienia
- 3a. Nocna zmiana: system buduje noc proceduralnie dla bieżącego poziomu (więcej fałszerstw, anomalii i wydarzeń, od poziomu 3 limit czasu)
- 3b. Powtórka nocy po porażce: system przywraca statystyki, flagi i stan pociągu z początku nocy (stres maks. 60, integralność min. 40)

**Przebiegi wyjątkowe**

- E1. Brak zapisu kampanii przy CONTINUE: przycisk jest nieaktywny, gracz może tylko zacząć nową kampanię
- E2. Uszkodzony plik zapisu: system wczytuje kopię `.bak`, a gdy jej brak, tworzy nowy zapis

**Powiązania z innymi przypadkami użycia**

- «include» Wczytaj zapis gry
- Poprzedza Sprawdź dokumenty, Przesłuchaj pasażera i Podejmij decyzję

**Reguły biznesowe**

- Każda noc ma 13 pasażerów
- Zegar nocy startuje o 22:00
- Na początku nocy stres konduktora spada o 20

### 6.4 Sprawdź dokumenty

**Aktor główny:** Gracz
**Cel:** Wykrycie niezgodności w dokumentach pasażera przed podjęciem decyzji.

**Warunki wstępne**

- Przy okienku stoi pasażer, a jego dokumenty leżą na biurku
- Decyzja dla tego pasażera nie została jeszcze podjęta

**Warunki końcowe (sukces)**

- Gracz zna wynik sprawdzenia wybranych pól
- Wykryte anomalie są zapisane, a gracz dostał za nie XP

**Główny przebieg zdarzeń**

1. Gracz otwiera tryb inspekcji (przycisk INSPECT lub kliknięcie dokumentu)
2. System pokazuje dokumenty odblokowane tej nocy
3. Gracz wybiera dokument i sposób sprawdzenia (podpis, zdjęcie, data, skan mikrotekstu, zapytanie do bazy)
4. System porównuje pole z wzorcem albo wysyła zapytanie do bazy kolejowej
5. System informuje o wyniku sprawdzenia
6. Jeśli wykryto anomalię, system ją rejestruje, przyznaje XP, dodaje stres i odblokowuje wpis Kodeksu
7. Gracz powtarza kroki 3–6 albo zamyka inspekcję

**Przebiegi alternatywne**

- 3a. Gracz oznacza podejrzane pole na dokumencie (liczba oznaczeń zależy od umiejętności Obserwacja)
- 3b. Gracz porównuje dwa dokumenty obok siebie albo obraca dokument na drugą stronę
- 6a. Anomalia była już wykryta u tego pasażera: system nie przyznaje XP drugi raz

**Przebiegi wyjątkowe**

- E1. Mikrotekst wymaga wyższego poziomu umiejętności Dokumenty: system pokazuje, że tekst jest nieczytelny
- E2. Dokument nie jest odblokowany tej nocy: zakładka jest niedostępna

**Powiązania z innymi przypadkami użycia**

- «extend» Podejmij decyzję: sprawdzenie jest opcjonalnym krokiem przed decyzją
- Wyniki sprawdzenia mogą odblokować pytania kontekstowe w Przesłuchaj pasażera

**Reguły biznesowe**

- Zapytanie do bazy zabiera czas w grze
- Groźne anomalie (wpis o śmierci, brak w księgach, nieznana osoba) dodają 4 punkty stresu

### 6.5 Przesłuchaj pasażera

**Aktor główny:** Gracz
**Aktor drugorzędny:** Pasażer (odpowiada na pytania)
**Cel:** Uzyskanie od pasażera informacji, które potwierdzą lub podważą jego dokumenty.

**Warunki wstępne**

- Przy okienku stoi pasażer, rozmowa jest otwarta
- Zostały niezadane pytania

**Warunki końcowe (sukces)**

- Gracz zna odpowiedź pasażera (z ewentualną oznaką nieszczerości)
- Skutki pytania (stres, flaga, Kodeks, anomalia) są zastosowane

**Główny przebieg zdarzeń**

1. System pokazuje od 4 do 6 pytań, zależnie od umiejętności Przesłuchanie
2. Gracz wybiera pytanie
3. Pasażer odpowiada: prawdą, półprawdą, kłamstwem albo wymija się
4. System przyznaje XP i stosuje skutki pytania (stres, flaga fabularna, wpis Kodeksu)
5. Jeśli odpowiedź ujawnia anomalię, system ją rejestruje
6. Jeśli konduktor ma wystarczający poziom Przesłuchania, system dopisuje oznakę nieszczerości
7. Gracz czyta odpowiedź i może zadać kolejne pytanie

**Przebiegi alternatywne**

- 1a. Pytania kontekstowe pojawiają się tylko wtedy, gdy dokumenty dają do nich powód
- 1b. Pytania wymagające Autorytetu są ukryte, dopóki konduktor nie ma odpowiedniego poziomu
- 3a. Pasażer sam zaczyna mówić albo prosi o wpuszczenie (pasażer współczujący)

**Przebiegi wyjątkowe**

- E1. Stres osiąga 100 po usłyszeniu odpowiedzi: noc kończy się porażką (ZAŁAMANIE)
- E2. Wszystkie pytania zostały zadane: lista pytań jest pusta, gracz przechodzi do decyzji

**Powiązania z innymi przypadkami użycia**

- «extend» Podejmij decyzję: rozmowa jest opcjonalnym krokiem przed decyzją
- Korzysta z wyników Sprawdź dokumenty (pytania kontekstowe)

**Reguły biznesowe**

- Liczba pytań = 4 + poziom Przesłuchania / 3 (od 4 do 6)
- Każde pytanie zabiera 3 minuty czasu w grze
- Oznaka kłamstwa: od poziomu 3 („unika wzroku”), od poziomu 5 („kłamie”)

### 6.6 Podejmij decyzję

**Aktor główny:** Gracz
**Aktor drugorzędny:** Pasażer (wsiada do pociągu albo odchodzi)
**Cel:** Wpuszczenie albo odmowa wpuszczenia pasażera i rozliczenie skutków decyzji.

**Warunki wstępne**

- Przy okienku stoi pasażer, przyciski decyzji są aktywne

**Warunki końcowe (sukces)**

- Na dokumencie jest stempel (zielony WPUŚĆ lub czerwony ODMÓW)
- Statystyki konduktora, stan pociągu, flagi fabularne i wynik nocy są zaktualizowane
- Pasażer wsiadł do pociągu albo odszedł z peronu

**Główny przebieg zdarzeń**

1. Gracz klika WPUŚĆ albo ODMÓW
2. System przybija stempel
3. System porównuje decyzję z zasadą nocy
4. Decyzja poprawna: system zwiększa dyscyplinę, reputację i XP oraz obniża stres
5. System ustawia flagi fabularne pasażera i aktualizuje stan pociągu
6. System sprawdza osiągnięcia i warunki porażki
7. System pokazuje komunikat zwrotny
8. Pasażer wsiada do pociągu albo odchodzi z peronu

**Przebiegi alternatywne**

- 1a. Minął limit czasu (Nocna zmiana od poziomu 3): system traktuje to jak odmowę i dodaje 4 punkty stresu
- 4a. Decyzja błędna: system dodaje stres, obniża dyscyplinę i reputację, uszkadza pociąg i przerywa serię
- 4b. Noc bez oceny (noce 11–12): liczy się tylko sumienie, żadna decyzja nie jest błędem
- 7a. W nocach z aktów 1–3 komunikat zdradza, kim był pasażer; później jest niejednoznaczny

**Przebiegi wyjątkowe**

- E1. Przekroczony limit błędów: noc kończy się porażką (ZWOLNIONY)
- E2. Stres osiągnął 100: porażka (ZAŁAMANIE)
- E3. Integralność pociągu spadła do 0: porażka (POCIĄG ZNISZCZONY)

**Powiązania z innymi przypadkami użycia**

- Rozszerzany przez Sprawdź dokumenty i Przesłuchaj pasażera
- Po ostatnim pasażerze rozszerzany przez Zakończ noc i zapisz postęp

**Reguły biznesowe**

- Ważny rozkaz Dyrekcji nakazuje wpuścić pasażera niezależnie od zasady nocy
- Pasażer bez biletu lub z fałszywym biletem powinien dostać odmowę
- Błędne wpuszczenie kogoś, kto nie jest żywy: integralność −9; inny błąd: −5; poprawna decyzja: +2

### 6.7 Zakończ noc i zapisz postęp

**Aktor główny:** Gracz (przypadek uruchamia się po decyzji o ostatnim pasażerze)
**Cel:** Podsumowanie zmiany i trwałe zapisanie postępu.

**Warunki wstępne**

- Obsłużono wszystkich 13 pasażerów i noc nie zakończyła się porażką

**Warunki końcowe (sukces)**

- Postęp kampanii (lub rekordy Nocnej zmiany) jest zapisany w pliku
- Gracz widzi ekran wyników i wybrał, co dalej

**Główny przebieg zdarzeń**

1. Pociąg odjeżdża z peronu
2. System podsumowuje zmianę: pasażerowie, poprawne decyzje, błędy, seria, dyscyplina, sumienie, stres, XP, wynik
3. System sprawdza i odblokowuje osiągnięcia
4. System zapisuje postęp kampanii: statystyki, flagi, numer następnej nocy i stan pociągu
5. System zapisuje plik atomowo (plik `.tmp`, kopia `.bak`)
6. System pokazuje ekran wyników
7. Gracz wybiera NASTĘPNA NOC albo MENU GŁÓWNE

**Przebiegi alternatywne**

- 4a. Nocna zmiana: system zapisuje rekord punktów, najlepszy poziom i najdłuższą serię
- 2a. Ostatnia noc kampanii: po trzynastym pasażerze (samym konduktorze) system wylicza zakończenie A, B, C, D lub sekretne i pokazuje je

**Przebiegi wyjątkowe**

- E1. Błąd zapisu na dysk: system zapisuje ostrzeżenie w logu, a poprzedni zapis pozostaje nienaruszony

**Powiązania z innymi przypadkami użycia**

- «extend» Podejmij decyzję: uruchamia się po decyzji o ostatnim pasażerze
- «extend» Przeglądaj Kodeks i osiągnięcia: nowe wpisy i osiągnięcia z nocy trafiają do Kodeksu

**Reguły biznesowe**

- Zmiana jest „bez błędu”, gdy nie ma pomyłek i jest co najmniej 13 poprawnych decyzji
- Zapis zawsze zostawia kopię poprzedniej wersji (`.bak`)

### 6.8 Pozostałe przypadki użycia (skrót)

**Wczytaj zapis gry** (włączany przez Rozpocznij noc). System czyta plik `ostatni_peron_save.json`. Jeśli plik jest uszkodzony, czyta kopię `.bak`, a gdy jej brak, tworzy nowy zapis. Następnie uzupełnia brakujące pola i przycina wartości do dozwolonych zakresów.

**Przeglądaj Kodeks i osiągnięcia** (aktor: Gracz). Gracz otwiera z menu lub pauzy Kodeks (8 kategorii) albo listę osiągnięć. Zablokowane wpisy i sekretne osiągnięcia są pokazane jako „???”.


## 7. Modele dynamiczne

### 7.1 Diagramy aktywności

Diagramy aktywności w układzie torów (swimlanes) pokazują, kto wykonuje który krok procesu. Dla każdego przypadku użycia opracowano osobny diagram:

- Rozpocznij noc – tory: Gracz, System gry, Dane gry
- Sprawdź dokumenty – tory: Gracz, System gry, Dane gry
- Przesłuchaj pasażera – tory: Gracz, Pasażer, System gry
- Podejmij decyzję – tory: Gracz, Pasażer, System gry
- Zakończ noc i zapisz postęp – tory: Gracz, System gry, Dane gry

Legenda symboli:

![Legenda diagramu aktywności](../diagramy/activity/legenda%20diagramu%20aktywnosci.png)

**Rozpocznij noc**

![Rozpocznij noc](../diagramy/activity/rozpocznij%20noc.png)

**Sprawdź dokumenty**

![Sprawdź dokumenty](../diagramy/activity/sprawdz%20dokumenty.png)

**Przesłuchaj pasażera**

![Przesłuchaj pasażera](../diagramy/activity/przesluchaj%20pasazera.png)

**Podejmij decyzję**

![Podejmij decyzję](../diagramy/activity/podejmij%20decyzje.png)

**Zakończ noc i zapisz postęp**

![Zakończ noc](../diagramy/activity/zakoncz%20noc.png)

### 7.2 Diagramy sekwencji

Diagramy sekwencji pokazują kolejność komunikatów między uczestnikami w głównym przebiegu każdego przypadku użycia. Czas płynie od góry do dołu; linia ciągła oznacza wywołanie, linia przerywana odpowiedź. Nazwy komunikatów to nazwy metod z kodu C# gry.

| Przypadek użycia | Uczestnicy | Kluczowe komunikaty |
|---|---|---|
| Rozpocznij noc | Gracz, :MainMenu, :GameManager, :StoryManager, :SaveSystem | `Begin()`, `Night(nightIndex)`, `pokażBriefing()` |
| Sprawdź dokumenty | Gracz, :GameManager, :DocumentInspector, :DocumentSystem, :AnomalySystem | `Open(onClosed, type)`, `Query(passenger, q, out anomaly)`, `Discover(anomaly, Documents)` |
| Przesłuchaj pasażera | Gracz, :DialoguePanel, :GameManager, :DialogueManager, :AnomalySystem | `OnAsk(q)`, `Ask(q)`, `Offered()`, `ShowAnswer()` |
| Podejmij decyzję | Gracz, :GameManager, :Days, :PlayerStats, :TrainSystem, :AchievementSystem | `Resolve(admit)`, `ShouldAdmit(p, day)`, `OnDecision(p, admit, correct)`, `CheckFailure()` |
| Zakończ noc i zapisz postęp | Gracz, :GameManager, :AchievementSystem, :StoryManager, :SaveSystem, :ShiftResultsUI | `FinishNight()`, `OnShiftComplete()`, `Commit()`, `Save()`, `Show(report)` |

Legenda symboli:

![Legenda diagramu sekwencji](../diagramy/sequence/legenda%20diagramu%20sekwencji.png)

**Rozpocznij noc**

![Sekwencja: Rozpocznij noc](../diagramy/sequence/sequence%20diagram%20rozpocznij%20noc.png)

**Sprawdź dokumenty**

![Sekwencja: Sprawdź dokumenty](../diagramy/sequence/sequence%20diagram%20sprawdz%20dokumenty.png)

**Przesłuchaj pasażera**

![Sekwencja: Przesłuchaj pasażera](../diagramy/sequence/sequence%20diagram%20przesluchaj%20pasazera.png)

**Podejmij decyzję**

![Sekwencja: Podejmij decyzję](../diagramy/sequence/sequence%20diagram%20podejmij%20decyzje.png)

**Zakończ noc i zapisz postęp**

![Sekwencja: Zakończ noc](../diagramy/sequence/sequence%20diagram%20zakoncz%20noc.png)

## 8. Model strukturalny (diagram klas)

Diagram pokazuje najważniejsze klasy gry (model projektowy, nie wszystkie 60 skryptów). Klasy oznaczone «static» to klasy statyczne C#. Pominięto klasy czysto widokowe (np. `StageView`, `HudPanel`, `PixelArt`).

![Diagram klas](../diagramy/class/diagram%20klas.png)

### 8.1 Klasy

| Klasa | Plik | Atrybuty | Metody |
|---|---|---|---|
| GameManager | `Scripts/GameManager.cs` | nightIndex: int, day: DayConfig, current: PassengerData, report: ShiftReport, streak: int | Admit(), Deny(), PlayNight(), HandlePassenger(index), Resolve(admit), CheckFailure(), FinishNight() |
| DayConfig | `Scripts/DayConfig.cs` | act, rulePl, passengers, maxMistakes, allow: bool[8], docMask, timeLimit, finalDay | Has(type): bool |
| PassengerData | `Scripts/PassengerData.cs` | truth: PassengerTruth, fullName, birthDate, ticketNo, forgedTicket, orderState, storyId | DisplayName: string |
| DocumentData | `Scripts/Documents/DocumentData.cs` | type: DocumentType, number, stamp, available | – |
| DocLine | `Scripts/Documents/DocumentData.cs` | label, value, anomaly, hidden, minSkill | – |
| QA | `Scripts/Documents/DocumentData.cs` | question, answer, kind, priority, minAuthority, used | – |
| PassengerGenerator «static» | `Scripts/PassengerGenerator.cs` | – | ForSlot(day, index), Generate(truth, day, script) |
| DocumentSystem «static» | `Scripts/Documents/DocumentSystem.cs` | – | Build(p, day), Query(p, q, out anomaly) |
| DocumentInspector | `Scripts/Documents/DocumentInspector.cs` | passenger: PassengerData | Open(onClosed, start), Close(), Scan(), CheckSignature(), CheckPhoto(), Query(q) |
| DialogueManager | `Scripts/Dialogue/DialogueManager.cs` | passenger, Asked: int | Begin(p), Offered(): List&lt;QA&gt;, Ask(q): string |
| AnomalySystem «static» | `Scripts/Horror/AnomalySystem.cs` | FoundThisShift: int | Discover(id, via): bool |
| PlayerStats «static» | `Scripts/Progression/PlayerStats.cs` | Conscience, Discipline, Stress, Reputation | AddStress(raw), AddDiscipline(d), AddReputation(d) |
| TrainSystem | `Scripts/Train/TrainSystem.cs` | Integrity, Temperature, Electricity, Pressure, Signal | OnDecision(p, admitted, correct), Destroyed: bool |
| StoryManager «static» | `Scripts/Story/StoryManager.cs` | – | Begin(), Night(index), Commit(nextNight, integrity), CalculateEnding(admitted) |
| SaveSystem «static» | `Scripts/Core/SaveSystem.cs` | data: SaveData | Load(), Save(), ResetStory() |
| SaveData | `Scripts/Core/SaveData.cs` | version, codex, achievements, endings | – |
| StoryProgress | `Scripts/Core/SaveData.cs` | night, conscience, discipline, stress, flags, skillXp | – |
| ShiftReport | `Scripts/Core/ShiftReport.cs` | passengers, correct, mistakes, score, bestStreak | Perfect: bool |

### 8.2 Relacje między klasami

| Klasy | Rodzaj | Liczebność | Znaczenie |
|---|---|---|---|
| GameManager – DayConfig | asocjacja | 1 : 1 | Noc rozgrywana jest według jednej konfiguracji. |
| GameManager – PassengerData | asocjacja | 1 : 0..1 | Przy okienku stoi co najwyżej jeden pasażer. |
| GameManager – ShiftReport | kompozycja | 1 : 1 | Raport powstaje dla każdej nocy i nie istnieje bez niej. |
| GameManager – DocumentInspector | kompozycja | 1 : 1 | Inspektor jest tworzony przez GameManager przy starcie sceny. |
| GameManager – DialogueManager | kompozycja | 1 : 1 | Menedżer rozmowy należy do GameManager. |
| GameManager – TrainSystem | kompozycja | 1 : 1 | Pociąg jest tworzony i sterowany przez GameManager. |
| GameManager – PlayerStats | asocjacja | 1 : 1 | Decyzje zmieniają statystyki konduktora. |
| PassengerData – DocumentData | kompozycja | 1 : 1..* | Dokumenty należą do pasażera. |
| DocumentData – DocLine | kompozycja | 1 : 1..* | Dokument składa się z linii (pól). |
| PassengerData – QA | kompozycja | 1 : 0..* | Pasażer ma przygotowane pytania i odpowiedzi. |
| PassengerGenerator – PassengerData | asocjacja | 1 : 0..* | Generator tworzy pasażerów. |
| DocumentSystem – DocumentData | asocjacja | 1 : 0..* | DocumentSystem buduje dokumenty pasażera. |
| DocumentInspector – PassengerData | asocjacja | 1 : 0..1 | Inspektor sprawdza bieżącego pasażera. |
| DialogueManager – PassengerData | asocjacja | 1 : 0..1 | Rozmowa dotyczy bieżącego pasażera. |
| DocumentInspector / DialogueManager – AnomalySystem | asocjacja | 1 : 1 | Wykryte anomalie są zgłaszane do AnomalySystem. |
| StoryManager – DayConfig | asocjacja | 1 : 1..* | StoryManager dostarcza konfiguracje 12 nocy. |
| StoryManager – SaveSystem | asocjacja | 1 : 1 | Postęp kampanii zapisywany jest przez SaveSystem. |
| SaveSystem – SaveData | kompozycja | 1 : 1 | SaveSystem przechowuje jedną strukturę zapisu. |
| SaveData – StoryProgress | kompozycja | 1 : 1 | Postęp kampanii jest częścią zapisu. |

PassengerData, DocumentData i QA są osobnymi klasami, a nie polami GameManager, bo każdy pasażer ma własny zestaw dokumentów i pytań, generowany na nowo w każdej nocy. Logika (GameManager, DocumentSystem, DialogueManager) jest oddzielona od danych (PassengerData, DayConfig, SaveData), co ułatwia zapis i testowanie.

## 9. Model danych (ERD)

Baza `ostatni_peron.db` (SQLite 3) ma 22 tabele w dwóch częściach: **A. treści gry** (15 tabel, dane statyczne) i **B. stan zapisu gracza** (7 tabel, przygotowane na zapis w bazie). Pełny diagram oraz dwa diagramy częściowe dla czytelności:

![Legenda diagramu ERD](../diagramy/erd%20-%20schemat%20bazy%20danych/legenda%20diagramu%20erd.png)

**Pełny schemat bazy**

![Diagram ERD](../diagramy/erd%20-%20schemat%20bazy%20danych/diagram%20bazy%20danych.png)

**Część A: treści gry**

![ERD: treści gry](../diagramy/erd%20-%20schemat%20bazy%20danych/diagram%20bazy%20danych%20-%20tresci%20gry.png)

**Część B: zapis gracza** (tabele z części A pokazane tylko z kluczem głównym)

![ERD: zapis gracza](../diagramy/erd%20-%20schemat%20bazy%20danych/diagram%20bazy%20danych%20-%20zapis%20gracza.png)

Liczba rekordów w bazie (stan repozytorium): akt 7, noc 12, flaga 34, pasazer 21, pasazer_w_nocy 29, typ_dokumentu 9, dokument_pasazera 5, pytanie_dialogowe 121, wydarzenie 18, wybor_wydarzenia 13, wpis_kodeksu 68, osiagniecie 31, zakonczenie 5, warunek_zakonczenia 14; tabele `rejestr` oraz części B są puste.

### 9.1 Tabele

#### akt

Akty kampanii (7 aktów).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | int | PK | Klucz główny |
| tytul_pl | varchar |  | Tytuł po polsku |
| tytul_en | varchar |  | Tytuł po angielsku |
| tytul_uk | varchar |  | Tytuł po ukraińsku |

#### noc

Noce kampanii: zasada dnia, briefing, zakończenie nocy i limit błędów (12 nocy).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | int | PK | Klucz główny |
| id_aktu | int | FK | Odwołanie do akt |
| tytul_pl | varchar |  | Tytuł po polsku |
| tytul_en | varchar |  | Tytuł po angielsku |
| tytul_uk | varchar |  | Tytuł po ukraińsku |
| zasada_pl | varchar |  | Zasada nocy po polsku |
| zasada_en | varchar |  | Zasada nocy po angielsku |
| zasada_uk | varchar |  | Zasada nocy po ukraińsku |
| briefing_pl | varchar |  | Briefing po polsku |
| briefing_en | varchar |  | Briefing po angielsku |
| briefing_uk | varchar |  | Briefing po ukraińsku |
| data_w_grze | varchar |  | Data w świecie gry |
| outro_pl | varchar |  | Tekst kończący noc po polsku |
| outro_en | varchar |  | Tekst kończący noc po angielsku |
| maks_bledow | int |  | Limit błędów w nocy (-1 = bez limitu) |

#### flaga

Flagi fabularne, które zapamiętują decyzje gracza.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | varchar | PK | Klucz główny |
| opis | varchar |  | Opis |
| wartosc_domyslna | int |  | Wartość początkowa flagi |
| zrodlo | varchar |  | Plik lub klasa C#, z której pochodzi wpis |
| notatka | varchar |  | Notatka autora |

#### pasazer

Pasażerowie zapisani w treści gry (postacie fabularne i scenariuszowe).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | varchar | PK | Klucz główny |
| imie_nazwisko | varchar |  | Imię i nazwisko |
| data_urodzenia | varchar |  | Data urodzenia |
| nr_dowodu | varchar |  | Numer dowodu |
| prawda | varchar |  | Ukryta prawda (Alive, Dead, NonExistent, Altered, Loop, Impostor, Echo, Unknown) |
| postac_powracajaca | int |  | 1 = postać fabularna wracająca w kolejnych nocach |
| wiek | int |  | Wiek |
| plec | varchar |  | Płeć |
| zawod_pl | varchar |  | Zawód po polsku |
| zawod_en | varchar |  | Zawód po angielsku |
| pochodzenie_pl | varchar |  | Pochodzenie po polsku |
| pochodzenie_en | varchar |  | Pochodzenie po angielsku |
| kodeks_id | varchar |  | Powiązany wpis Kodeksu |
| notatka | varchar |  | Notatka autora |

#### pasazer_w_nocy

Przypisanie pasażera do konkretnej pozycji w kolejce danej nocy (tabela pośrednia noc–pasażer).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id_nocy | int | PK, FK | Odwołanie do noc |
| slot | int | PK | Pozycja w kolejce nocy (0–12) |
| id_pasazera | varchar | FK | Odwołanie do pasazer |
| id_scenariusza | varchar |  | Skrypt postaci w StoryCast |

#### typ_dokumentu

Rodzaje dokumentów (9) i noc, w której gracz je odblokowuje.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| kod | varchar | PK | Kod typu dokumentu |
| nazwa_pl | varchar |  | Nazwa po polsku |
| nazwa_en | varchar |  | Nazwa po angielsku |
| nazwa_uk | varchar |  | Nazwa po ukraińsku |
| odblokowanie_noc | int | FK | Odwołanie do noc |
| bit | int |  | Bit w masce dokumentów (DayConfig.docMask) |

#### dokument_pasazera

Dokumenty konkretnych pasażerów wraz ze wskazówkami.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | varchar | PK | Klucz główny |
| id_pasazera | varchar | FK | Odwołanie do pasazer |
| typ | varchar | FK | Odwołanie do typ_dokumentu |
| wskazowka_pl | varchar |  | Wskazówka po polsku |
| wskazowka_en | varchar |  | Wskazówka po angielsku |
| wskazowka_uk | varchar |  | Wskazówka po ukraińsku |
| zrodlo | varchar |  | Plik lub klasa C#, z której pochodzi wpis |

#### rejestr

Kartoteka osób (struktura przygotowana na przyszłość, obecnie pusta).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id_osoby | varchar | PK | Klucz główny osoby |
| id_pasazera | varchar | FK | Odwołanie do pasazer |
| status | varchar |  | Status osoby w rejestrze |
| data_zgonu | varchar |  | Data zgonu |
| notatka_pl | varchar |  | Notatka po polsku |
| notatka_en | varchar |  | Notatka po angielsku |

#### pytanie_dialogowe

Pytania i przygotowane odpowiedzi pasażerów wraz ze skutkami zadania pytania.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | int | PK | Klucz główny |
| kod | varchar |  | Kod typu dokumentu |
| id_pasazera | varchar | FK | Odwołanie do pasazer |
| prawda | varchar |  | Ukryta prawda (Alive, Dead, NonExistent, Altered, Loop, Impostor, Echo, Unknown) |
| pytanie_pl | varchar |  | Pytanie po polsku |
| pytanie_en | varchar |  | Pytanie po angielsku |
| pytanie_uk | varchar |  | Pytanie po ukraińsku |
| odpowiedz_pl | varchar |  | Odpowiedź po polsku |
| odpowiedz_en | varchar |  | Odpowiedź po angielsku |
| odpowiedz_uk | varchar |  | Odpowiedź po ukraińsku |
| szansa_klamstwa | decimal |  | Prawdopodobieństwo kłamstwa (0–1) |
| wymaga_dokumentu | int |  | 1 = pytanie kontekstowe |
| zmiana_stresu | int |  | Zmiana stresu konduktora |
| ustawia_flage | varchar | FK | Odwołanie do flaga |
| priorytet | int |  | Kolejność proponowania pytania |
| rodzaj_odpowiedzi | varchar |  | Truth / Half / Lie / Evasive |
| kodeks_po_odpowiedzi | varchar | FK | Odwołanie do wpis_kodeksu |
| anomalia_po_odpowiedzi | varchar |  | Anomalia ujawniana odpowiedzią |
| min_autorytet | int |  | Wymagany poziom Autorytetu |
| zrodlo_funkcji | varchar |  | Metoda w DialogueBank.cs |

#### wydarzenie

Wydarzenia losowe i fabularne (18).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | varchar | PK | Klucz główny |
| rodzaj | varchar |  | Rodzaj (losowe / fabularne) |
| id_nocy | int | FK | Odwołanie do noc |
| nazwa_pl | varchar |  | Nazwa po polsku |
| nazwa_en | varchar |  | Nazwa po angielsku |
| nazwa_uk | varchar |  | Nazwa po ukraińsku |
| tekst_pl | varchar |  | Tekst po polsku |
| tekst_en | varchar |  | Tekst po angielsku |
| tekst_uk | varchar |  | Tekst po ukraińsku |

#### wybor_wydarzenia

Opcje wyboru w wydarzeniach i ich skutki.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | varchar | PK | Klucz główny |
| id_wydarzenia | varchar | FK | Odwołanie do wydarzenie |
| tekst_pl | varchar |  | Tekst po polsku |
| tekst_en | varchar |  | Tekst po angielsku |
| tekst_uk | varchar |  | Tekst po ukraińsku |
| zmiana_stresu | int |  | Zmiana stresu konduktora |
| ustawia_flage | varchar | FK | Odwołanie do flaga |

#### wpis_kodeksu

Wpisy Kodeksu (encyklopedii w grze) w 8 kategoriach.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | varchar | PK | Klucz główny |
| kategoria | varchar |  | Kategoria Kodeksu |
| tytul_pl | varchar |  | Tytuł po polsku |
| tytul_en | varchar |  | Tytuł po angielsku |
| tytul_uk | varchar |  | Tytuł po ukraińsku |
| tekst_pl | varchar |  | Tekst po polsku |
| tekst_en | varchar |  | Tekst po angielsku |
| tekst_uk | varchar |  | Tekst po ukraińsku |
| flaga_odblokowania | varchar | FK | Odwołanie do flaga |
| warunek_odblokowania | varchar |  | Opisowy warunek z kodu |

#### osiagniecie

Osiągnięcia, w tym sekretne.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | varchar | PK | Klucz główny |
| nazwa_pl | varchar |  | Nazwa po polsku |
| nazwa_en | varchar |  | Nazwa po angielsku |
| nazwa_uk | varchar |  | Nazwa po ukraińsku |
| opis_pl | varchar |  | Opis po polsku |
| opis_en | varchar |  | Opis po angielsku |
| opis_uk | varchar |  | Opis po ukraińsku |
| sekretne | int |  | 1 = osiągnięcie ukryte |
| flaga_odblokowania | varchar | FK | Odwołanie do flaga |
| warunek_odblokowania | varchar |  | Opisowy warunek z kodu |

#### zakonczenie

Zakończenia kampanii (5).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | varchar | PK | Klucz główny |
| tytul_pl | varchar |  | Tytuł po polsku |
| tytul_en | varchar |  | Tytuł po angielsku |
| tytul_uk | varchar |  | Tytuł po ukraińsku |
| opis_pl | varchar |  | Opis po polsku |

#### warunek_zakonczenia

Warunki (wartości flag) potrzebne do danego zakończenia.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | varchar | PK | Klucz główny |
| id_zakonczenia | varchar | FK | Odwołanie do zakonczenie |
| id_flagi | varchar | FK | Odwołanie do flaga |
| wymagana_wartosc | int |  | Wymagana wartość flagi |

#### zapis

Stan zapisu gracza: tryb, bieżąca noc, statystyki i rekordy.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | int | PK | Klucz główny |
| tryb | varchar |  | Story / Endless |
| biezaca_noc | int | FK | Odwołanie do noc |
| dyscyplina | int |  | Dyscyplina (0–10) |
| sumienie | int |  | Sumienie (0–10) |
| stres | int |  | Stres (0–100) |
| reputacja | int |  | Reputacja (0–100) |
| integralnosc_pociagu | int |  | Integralność pociągu (0–100) |
| rekord_punktow | int |  | Rekord punktów (Nocna zmiana) |
| najlepszy_poziom | int |  | Najlepszy poziom |
| najdluzsza_seria | int |  | Najdłuższa seria poprawnych decyzji |
| zaktualizowano | varchar |  | Data ostatniego zapisu |

#### zapis_flaga

Flagi ustawione w danym zapisie (tabela pośrednia zapis–flaga).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id_zapisu | int | PK, FK | Odwołanie do zapis |
| id_flagi | varchar | PK, FK | Odwołanie do flaga |
| wartosc | int |  | Wartość flagi |

#### zapis_osiagniecie

Osiągnięcia zdobyte w danym zapisie (tabela pośrednia).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id_zapisu | int | PK, FK | Odwołanie do zapis |
| id_osiagniecia | varchar | PK, FK | Odwołanie do osiagniecie |
| data_zdobycia | varchar |  | Data zdobycia |

#### zapis_kodeks

Wpisy Kodeksu odkryte w danym zapisie (tabela pośrednia).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id_zapisu | int | PK, FK | Odwołanie do zapis |
| id_wpisu | varchar | PK, FK | Odwołanie do wpis_kodeksu |

#### zapis_wybor

Wybory dokonane w wydarzeniach (tabela pośrednia).

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id_zapisu | int | PK, FK | Odwołanie do zapis |
| id_wyboru | varchar | PK, FK | Odwołanie do wybor_wydarzenia |

#### decyzja

Historia decyzji gracza: werdykt dla każdego pasażera.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id | int | PK | Klucz główny |
| id_zapisu | int | FK | Odwołanie do zapis |
| id_nocy | int | FK | Odwołanie do noc |
| slot | int |  | Pozycja w kolejce nocy (0–12) |
| werdykt | varchar |  | Wpuść / Odmów |
| poprawna | int |  | 1 = decyzja zgodna z zasadą |
| zlamana_zasada | varchar |  | Złamana zasada (jeśli była) |
| zmiana_stresu | int |  | Zmiana stresu konduktora |

#### zapis_umiejetnosc

Poziom i XP każdej z 5 umiejętności w danym zapisie.

| Kolumna | Typ | Klucz | Opis |
|---|---|---|---|
| id_zapisu | int | PK, FK | Odwołanie do zapis |
| umiejetnosc | varchar | PK | Nazwa umiejętności |
| poziom | int |  | Poziom (1–10) |
| xp | int |  | Zdobyte doświadczenie |

### 9.2 Relacje

| Tabele | Liczebność | Klucz obcy |
|---|---|---|
| akt – noc | 0..1 : 0..* | noc.id_aktu |
| noc – pasazer_w_nocy | 1 : 0..* | pasazer_w_nocy.id_nocy |
| pasazer – pasazer_w_nocy | 1 : 0..* | pasazer_w_nocy.id_pasazera |
| noc – typ_dokumentu | 0..1 : 0..* | typ_dokumentu.odblokowanie_noc |
| pasazer – dokument_pasazera | 0..1 : 0..* | dokument_pasazera.id_pasazera |
| typ_dokumentu – dokument_pasazera | 0..1 : 0..* | dokument_pasazera.typ |
| pasazer – rejestr | 0..1 : 0..* | rejestr.id_pasazera |
| pasazer – pytanie_dialogowe | 0..1 : 0..* | pytanie_dialogowe.id_pasazera |
| flaga – pytanie_dialogowe | 0..1 : 0..* | pytanie_dialogowe.ustawia_flage |
| wpis_kodeksu – pytanie_dialogowe | 0..1 : 0..* | pytanie_dialogowe.kodeks_po_odpowiedzi |
| noc – wydarzenie | 0..1 : 0..* | wydarzenie.id_nocy |
| wydarzenie – wybor_wydarzenia | 0..1 : 0..* | wybor_wydarzenia.id_wydarzenia |
| flaga – wybor_wydarzenia | 0..1 : 0..* | wybor_wydarzenia.ustawia_flage |
| flaga – wpis_kodeksu | 0..1 : 0..* | wpis_kodeksu.flaga_odblokowania |
| flaga – osiagniecie | 0..1 : 0..* | osiagniecie.flaga_odblokowania |
| zakonczenie – warunek_zakonczenia | 0..1 : 0..* | warunek_zakonczenia.id_zakonczenia |
| flaga – warunek_zakonczenia | 0..1 : 0..* | warunek_zakonczenia.id_flagi |
| noc – zapis | 0..1 : 0..* | zapis.biezaca_noc |
| zapis – zapis_flaga | 1 : 0..* | zapis_flaga.id_zapisu |
| flaga – zapis_flaga | 1 : 0..* | zapis_flaga.id_flagi |
| zapis – zapis_osiagniecie | 1 : 0..* | zapis_osiagniecie.id_zapisu |
| osiagniecie – zapis_osiagniecie | 1 : 0..* | zapis_osiagniecie.id_osiagniecia |
| zapis – zapis_kodeks | 1 : 0..* | zapis_kodeks.id_zapisu |
| wpis_kodeksu – zapis_kodeks | 1 : 0..* | zapis_kodeks.id_wpisu |
| zapis – zapis_wybor | 1 : 0..* | zapis_wybor.id_zapisu |
| wybor_wydarzenia – zapis_wybor | 1 : 0..* | zapis_wybor.id_wyboru |
| zapis – decyzja | 0..1 : 0..* | decyzja.id_zapisu |
| noc – decyzja | 0..1 : 0..* | decyzja.id_nocy |
| zapis – zapis_umiejetnosc | 1 : 0..* | zapis_umiejetnosc.id_zapisu |


### 9.3 Zasady modelu

- Każda tabela ma klucz główny. Tabele pośrednie (`pasazer_w_nocy`, `zapis_flaga`, `zapis_osiagniecie`, `zapis_kodeks`, `zapis_wybor`, `zapis_umiejetnosc`) mają klucze złożone.
- Klucz obcy zawsze leży po stronie „wiele” relacji.
- Relacje N:M (zapis–flaga, zapis–osiągnięcie, zapis–wpis Kodeksu, zapis–wybór, noc–pasażer) są rozbite tabelami pośrednimi.
- Nazwy tabel i kolumn są pisane małymi literami, bez polskich znaków, z podkreślnikiem.
- Teksty są przechowywane w trzech kolumnach językowych (`_pl`, `_en`, `_uk`), zgodnie z WN-06.
- Nie przechowujemy danych, które da się wyliczyć (np. liczby poprawnych decyzji w nocy, bo wynika z tabeli `decyzja`).
- Diagram klas i ERD różnią się celowo: diagram klas pokazuje obiekty w pamięci podczas gry, ERD pokazuje trwałe dane.

## 10. Architektura i uruchomienie

### 10.1 Sceny i struktura projektu

| Element | Opis |
|---|---|
| `Assets/Scenes/MainMenu.unity` | Menu główne (indeks 0 w buildzie): wybór trybu, Kodeks, osiągnięcia, ustawienia. |
| `Assets/Scenes/Peron.unity` | Scena rozgrywki (indeks 1): peron, okienko, biurko z dokumentami. |
| `Scripts/Core` | Zapis gry, stan gry, zdarzenia, raport zmiany. |
| `Scripts/Story` | Noce kampanii, postacie, flagi fabularne, zakończenia, tryb nieskończony. |
| `Scripts/Documents` | Dokumenty i tryb inspekcji. |
| `Scripts/Dialogue` | Rozmowy z pasażerami. |
| `Scripts/Horror` | Stres, efekty grozy i anomalie. |
| `Scripts/Train` | System pociągu. |
| `Scripts/Progression` | Umiejętności, statystyki, Kodeks, osiągnięcia. |
| `Scripts/Events` | Biblioteka wydarzeń. |
| `Scripts/UI`, `Scripts/World` | Panele interfejsu, oświetlenie, pixel art. |
| `Scripts/Editor` | Narzędzie budowania wersji Windows. |
| `Assets/StreamingAssets/Data` | Baza `ostatni_peron.db` i `schema.sql`. |

### 10.2 Uruchomienie i budowanie

1. Otwórz folder projektu w Unity Hub (Unity 6, wersja 6000.6.0f1) i poczekaj na kompilację skryptów.
2. Otwórz scenę `Assets/Scenes/MainMenu.unity` i naciśnij Play.
3. Build: menu **Ostatni Peron > Build Windows (Desktop)**. Narzędzie dodaje sceny MainMenu i Peron i tworzy plik `.exe` w wybranym folderze.

## 11. Zgodność dokumentacji z projektem

Tabela łączy przypadki użycia z miejscami w kodzie, które je realizują.

| Przypadek użycia | Klasy i metody w kodzie |
|---|---|
| Rozpocznij noc | `MainMenu`, `GameState.BeginStory()/BeginEndless()`, `GameManager.Run()`, `GameManager.PlayNight()`, `StoryManager.Begin()`, `StoryManager.Night()`, `EndlessManager.Build()` |
| Wczytaj zapis gry | `SaveSystem.Load()`, `SaveSystem.Sanitize()` |
| Sprawdź dokumenty | `GameManager.OpenInspector()`, `DocumentInspector.Open()/Scan()/CheckSignature()/CheckPhoto()/Query()`, `DocumentSystem.Query()`, `AnomalySystem.Discover()` |
| Przesłuchaj pasażera | `GameManager.OnAsk()`, `DialogueManager.Begin()/Offered()/Ask()`, `DialoguePanel.ShowAnswer()` |
| Podejmij decyzję | `GameManager.Admit()/Deny()/Resolve()/CheckFailure()`, `Days.ShouldAdmit()`, `PlayerStats`, `TrainSystem.OnDecision()`, `AchievementSystem.OnDecision()` |
| Zakończ noc i zapisz postęp | `GameManager.FinishNight()`, `AchievementSystem.OnShiftComplete()`, `StoryManager.Commit()`, `EndlessManager.SaveStats()`, `SaveSystem.Save()`, `ShiftResultsUI.Show()`, `StoryManager.CalculateEnding()` |
| Przeglądaj Kodeks i osiągnięcia | `CodexUI`, `AchievementUI`, `Codex.IsUnlocked()`, `AchievementSystem.IsUnlocked()` |
