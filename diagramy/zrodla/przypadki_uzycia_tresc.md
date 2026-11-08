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
