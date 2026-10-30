# Baza danych OSTATNI PERON

Plik `ostatni_peron.db` (SQLite) odwzorowuje Twój diagram ERD jeden do jednego — te same nazwy tabel i kolumn, część A (treści gry) i część B (stan zapisu gracza). Jest wypełniony aktualną zawartością gry wyciągniętą z kodu C#. Gra na razie dalej działa dokładnie tak jak wcześniej — nic w kodzie się nie zmieniło, baza leży obok jako nowa, osobna warstwa.

## Gdzie co jest

- `Assets/StreamingAssets/Data/ostatni_peron.db` — gotowa baza, w miejscu skąd Unity w przyszłości mogłoby ją wczytywać w runtime (`Application.streamingAssetsPath`).
- `Assets/StreamingAssets/Data/schema.sql` — czysty schemat (same `CREATE TABLE`), gdybyś chciał zbudować bazę od zera albo prześledzić strukturę bez otwierania pliku `.db`.
- `Claude outputs/BAZA_DANYCH/ostatni_peron.db` i `schema.sql` — te same pliki, dla wygody (nie trzeba grzebać w Assets).
- `Claude outputs/BAZA_DANYCH/content_extract.json` — surowy wyciąg z kodu przed konwersją do SQL, gdybyś chciał coś zweryfikować źródłowo.

Bazę otwierasz dowolnym narzędziem do SQLite, np. darmowy **DB Browser for SQLite** (sqlitebrowser.org) — podgląd tabel, edycja na żywo, eksport do CSV.

## Co jest w środku (część A — treści gry)

| Tabela | Wierszy | Co to |
|---|---|---|
| akt | 7 | 7 aktów fabuły |
| noc | 12 | wszystkie 12 nocy: tytuł, zasada nocy, briefing, outro |
| flaga | 34 | 30 prawdziwych flag fabularnych + 4 "pseudoflagi" (warunki liczone w locie, np. "pomogłeś ≥4 osobom") |
| pasazer | 21 | 7 powracających postaci fabularnych + konduktor jako 13. pasażer + 13 archetypów wyglądu dla losowych pasażerów |
| pasazer_w_nocy | 29 | zaskryptowane pojawienia konkretnych postaci w konkretnych nocach/slotach |
| typ_dokumentu | 9 | bilet, dowód, baza kolejowa, rejestr, manifest itd., z nocą odblokowania |
| dokument_pasazera | 5 | twarde nadpisania dokumentów dla konkretnych postaci (np. numer dokumentu konduktora) |
| pytanie_dialogowe | 121 | 81 pytań ogólnych wg typu prawdy pasażera + 33 scenariuszowe + 7 pytań gracza o siebie |
| wydarzenie | 18 | 15 losowych + 3 scenariuszowe zdarzenia nocne |
| wybor_wydarzenia | 13 | opcje wyboru w zdarzeniach |
| wpis_kodeksu | 68 | cały kodeks/leksykon gry |
| osiagniecie | 31 | w tym 6 sekretnych |
| zakonczenie | 5 | wszystkie zakończenia, w tym sekretne "Peron czternasty" |
| warunek_zakonczenia | 14 | jakie flagi/wartości odblokowują które zakończenie |
| rejestr | 0 | patrz "Luki" niżej |

Część B (zapis, zapis_flaga, zapis_osiagniecie, zapis_kodeks, zapis_wybor, decyzja, zapis_umiejetnosc) jest **pusta** — gra jeszcze do niej nie zapisuje. Struktura czeka gotowa na etap, w którym zdecydujesz się realnie podłączyć system zapisu do tej bazy.

Każda tabela treści ma też kilka kolumn **ponad diagram** — dodałem je, bo wyciągnięta z kodu zawartość była zbyt dobra, żeby ją wyrzucać (np. wiek i zawód pasażera, priorytet pytania dialogowego, numer bitu dokumentu). Są opisane komentarzami w `schema.sql` i niczego z oryginalnego diagramu nie zastępują — tylko dokładają.

## Dwie uczciwe rozbieżności z diagramem

1. **`flaga_odblokowania` w `osiagniecie` i `wpis_kodeksu`.** Diagram zakłada, że odblokowanie zależy od jednej flagi. W kodzie tak nie jest — kodeks i osiągnięcia odblokowują się przez bezpośrednie wywołania (`Codex.Unlock(...)`, `AchievementSystem.OnShiftComplete()` itd.), nie przez sprawdzenie flagi. Kolumna `flaga_odblokowania` zostaje więc pusta (zgodna z diagramem, FK do `flaga`), a prawdziwy warunek trafia do dodatkowej kolumny `warunek_odblokowania` jako opis. Zrobiłem tak zamiast zmyślać fałszywe flagi, których w kodzie nie ma.
2. **4 "pseudoflagi"** (`pseudoflaga_SelfAdmitted`, `pseudoflaga_HelpedCountAtLeast4`, `pseudoflaga_ConscienceGteDiscipline`, `pseudoflaga_SEndingConditionsMet`) w warunkach zakończeń. To są warunki liczone na bieżąco ze statystyk zapisu (np. sumienie ≥ dyscyplina), a nie osobno trzymane flagi. Dodałem je do tabeli `flaga` jako wpisy z notatką, że są liczone, a nie zapisywane — dzięki temu klucze obce w `warunek_zakonczenia` się zgadzają i nic nie trzeba było zerować.

## Luki (czego nie wymyśliłem)

- **`rejestr`** — pusta. W obecnym kodzie nie ma osobnej "kartoteki osób" odrębnej od listy pasażerów (to, co w grze nazywa się Rejestrem, jest dokumentem w ręku gracza, nie bazą danych osób). Tabela czeka gotowa, gdyby taki system powstał.
- Statyczny tekst ukraiński (`_uk`) jest wszędzie pusty — w grze UA powstaje w runtime z osobnej tabeli `UkStrings` kluczowanej po polskim tekście, a nie jako literalny tekst w tych plikach. Gdybyś chciał, mogę w kolejnym kroku dociągnąć tłumaczenia UA wprost do tej bazy.
- `data_urodzenia` / `nr_dowodu` dla pasażerów fabularnych są puste — w grze generowane proceduralnie z wieku przy starcie (nie są stałe w kodzie).
- Pełna lista 12 drobniejszych luk jest w `content_extract.json` (pole `gaps`), gdybyś chciał zajrzeć w szczegóły.

## Sprawdzone

`PRAGMA foreign_key_check` na gotowej bazie: **0 problemów** — wszystkie klucze obce się zgadzają.

## Następny krok (nie zrobiony teraz, bo wybrałeś bezpieczną opcję)

Gra na razie nie czyta z tej bazy — PassengerGenerator, DialogueBank, EventLibrary, DocumentSystem, Codex i system zapisu działają tak jak wcześniej, na danych w C#. Podłączenie bazy do runtime (żeby gra faktycznie czytała/pisała z `ostatni_peron.db` przez SQLite w Unity) to osobny, większy etap — daj znać, jeśli chcesz, żebym go zrobił.
