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

<!--USE_CASES-->

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

<!--ERD_TABLES-->

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
