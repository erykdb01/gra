-- ============================================================================
-- OSTATNI PERON / THE LAST PLATFORM -- baza danych tresci i stanu zapisu
-- Wygenerowano wg diagramu ERD dostarczonego przez autora gry.
-- Silnik: SQLite 3.
--
-- Kolumny oznaczone w komentarzu "-- dodatkowa" wykraczaja poza oryginalny
-- diagram: dodano je, bo wyekstrahowana zawartosc gry je zawierala i szkoda
-- by ich bylo nie zapisac (np. wiek pasazera, zrodlowy identyfikator z kodu).
-- Wszystkie kolumny z diagramu sa zachowane pod tymi samymi nazwami.
-- ============================================================================

PRAGMA foreign_keys = ON;

-- ============================================================================
-- CZESC A: TRESCI GRY (dane projektowe / statyczne)
-- ============================================================================

CREATE TABLE akt (
    id          INTEGER PRIMARY KEY,
    tytul_pl    TEXT NOT NULL,
    tytul_en    TEXT,
    tytul_uk    TEXT
);

CREATE TABLE noc (
    id              INTEGER PRIMARY KEY,
    id_aktu         INTEGER REFERENCES akt(id),
    tytul_pl        TEXT NOT NULL,
    tytul_en        TEXT,
    tytul_uk        TEXT,
    zasada_pl       TEXT,
    zasada_en       TEXT,
    zasada_uk       TEXT,
    briefing_pl     TEXT,
    briefing_en     TEXT,
    briefing_uk     TEXT,
    data_w_grze     TEXT,       -- dodatkowa: fikcyjna data w uniwersum gry
    outro_pl        TEXT,       -- dodatkowa: tekst konczacy noc
    outro_en        TEXT,       -- dodatkowa
    maks_bledow     INTEGER     -- dodatkowa: ile bledow gracz moze popelnic
);

CREATE TABLE flaga (
    id                  TEXT PRIMARY KEY,
    opis                TEXT,
    wartosc_domyslna    INTEGER NOT NULL DEFAULT 0,
    zrodlo              TEXT,   -- dodatkowa: plik/klasa C# w ktorej zyje flaga
    notatka             TEXT    -- dodatkowa: kiedy/jak flaga jest ustawiana
);

CREATE TABLE pasazer (
    id                      TEXT PRIMARY KEY,
    imie_nazwisko           TEXT,
    data_urodzenia          TEXT,
    nr_dowodu               TEXT,
    prawda                  TEXT,       -- rodzaj "prawdy" pasazera (Alive/Dead/Non-Existent/Altered/Loop/Impostor/Echo/Unknown...)
    postac_powracajaca      INTEGER DEFAULT 0,
    wiek                    INTEGER,    -- dodatkowa
    plec                    TEXT,       -- dodatkowa
    zawod_pl                TEXT,       -- dodatkowa
    zawod_en                TEXT,       -- dodatkowa
    pochodzenie_pl          TEXT,       -- dodatkowa
    pochodzenie_en          TEXT,       -- dodatkowa
    kodeks_id               TEXT,       -- dodatkowa: powiazany wpis kodeksu
    notatka                 TEXT        -- dodatkowa
);

CREATE TABLE pasazer_w_nocy (
    id_nocy         INTEGER NOT NULL REFERENCES noc(id),
    slot            INTEGER NOT NULL,
    id_pasazera     TEXT NOT NULL REFERENCES pasazer(id),
    id_scenariusza  TEXT,   -- dodatkowa: konkretny ScriptDef z StoryCast (ta sama osoba, inna noc/prawda)
    PRIMARY KEY (id_nocy, slot)
);

CREATE TABLE typ_dokumentu (
    kod                 TEXT PRIMARY KEY,
    nazwa_pl            TEXT,
    nazwa_en            TEXT,
    nazwa_uk            TEXT,
    odblokowanie_noc    INTEGER REFERENCES noc(id),
    bit                 INTEGER     -- dodatkowa: pozycja w masce bitowej dokumentow
);

CREATE TABLE dokument_pasazera (
    id              TEXT PRIMARY KEY,
    id_pasazera     TEXT REFERENCES pasazer(id),
    typ             TEXT REFERENCES typ_dokumentu(kod),
    wskazowka_pl    TEXT,
    wskazowka_en    TEXT,
    wskazowka_uk    TEXT,
    zrodlo          TEXT        -- dodatkowa: fragment kodu zrodlowego
);

CREATE TABLE rejestr (
    id_osoby        TEXT PRIMARY KEY,
    id_pasazera     TEXT REFERENCES pasazer(id),
    status          TEXT,
    data_zgonu      TEXT,
    notatka_pl      TEXT,
    notatka_en      TEXT
);
-- Tabela jest pusta przy generowaniu: w obecnym kodzie gry nie istnieje
-- osobny "rejestr osob" odrebny od listy pasazerow. Struktura jest gotowa
-- na przyszlosc (np. kartoteka zaginionych/zmarlych).

CREATE TABLE pytanie_dialogowe (
    id                      INTEGER PRIMARY KEY,
    kod                     TEXT,       -- dodatkowa: oryginalny identyfikator tekstowy z kodu
    id_pasazera             TEXT REFERENCES pasazer(id),   -- NULL dla pytan ogolnych (wg typu prawdy)
    prawda                  TEXT,       -- dodatkowa: dla pytan generycznych -- do jakiego typu prawdy pasazera naleza
    pytanie_pl              TEXT,
    pytanie_en              TEXT,
    pytanie_uk              TEXT,
    odpowiedz_pl             TEXT,
    odpowiedz_en            TEXT,
    odpowiedz_uk            TEXT,
    szansa_klamstwa         REAL,
    wymaga_dokumentu        INTEGER DEFAULT 0,
    zmiana_stresu           INTEGER DEFAULT 0,
    ustawia_flage           TEXT REFERENCES flaga(id),
    priorytet               INTEGER,    -- dodatkowa: kolejnosc wyboru pytania przez DialogueBank
    rodzaj_odpowiedzi       TEXT,       -- dodatkowa: np. Evasive/Half/Lie/Truth
    kodeks_po_odpowiedzi    TEXT REFERENCES wpis_kodeksu(id),  -- dodatkowa
    anomalia_po_odpowiedzi  TEXT,       -- dodatkowa
    min_autorytet           INTEGER,    -- dodatkowa
    zrodlo_funkcji          TEXT        -- dodatkowa: metoda w DialogueBank.cs
);

CREATE TABLE wydarzenie (
    id          TEXT PRIMARY KEY,
    rodzaj      TEXT,
    id_nocy     INTEGER REFERENCES noc(id),
    nazwa_pl    TEXT,
    nazwa_en    TEXT,
    nazwa_uk    TEXT,
    tekst_pl    TEXT,
    tekst_en    TEXT,
    tekst_uk    TEXT
);

CREATE TABLE wybor_wydarzenia (
    id              TEXT PRIMARY KEY,
    id_wydarzenia   TEXT REFERENCES wydarzenie(id),
    tekst_pl        TEXT,
    tekst_en        TEXT,
    tekst_uk        TEXT,
    zmiana_stresu   INTEGER DEFAULT 0,
    ustawia_flage   TEXT REFERENCES flaga(id)
);

-- UWAGA: w obecnym kodzie gry wpisy kodeksu NIE sa odblokowywane przez
-- sprawdzenie wartosci jednej flagi (jak zaklada diagram), tylko przez
-- bezposrednie wywolanie Codex.Unlock(id) w wielu miejscach (poczatek nocy,
-- spotkanie pasazera, zdarzenie losowe, anomalia dokumentu...). Kolumna
-- flaga_odblokowania zostaje wiec pusta (zgodnie z diagramem to FK do
-- flaga), a rzeczywisty warunek trafia do dodatkowej kolumny tekstowej.
CREATE TABLE wpis_kodeksu (
    id                      TEXT PRIMARY KEY,
    kategoria               TEXT,
    tytul_pl                TEXT,
    tytul_en                TEXT,
    tytul_uk                TEXT,
    tekst_pl                TEXT,
    tekst_en                TEXT,
    tekst_uk                TEXT,
    flaga_odblokowania      TEXT REFERENCES flaga(id),
    warunek_odblokowania    TEXT    -- dodatkowa: opisowy warunek z kodu (patrz uwaga wyzej)
);

-- UWAGA: jak wyzej -- osiagniecia sa odblokowywane wywolaniami w
-- AchievementSystem (OnShiftComplete/OnAnomaly/OnDecision/...), nie przez
-- pojedyncza flage. flaga_odblokowania zostaje puste, warunek opisowy w
-- dodatkowej kolumnie.
CREATE TABLE osiagniecie (
    id                      TEXT PRIMARY KEY,
    nazwa_pl                TEXT,
    nazwa_en                TEXT,
    nazwa_uk                TEXT,
    opis_pl                 TEXT,
    opis_en                 TEXT,
    opis_uk                 TEXT,
    sekretne                INTEGER DEFAULT 0,
    flaga_odblokowania      TEXT REFERENCES flaga(id),
    warunek_odblokowania    TEXT    -- dodatkowa: opisowy warunek z kodu (patrz uwaga wyzej)
);

CREATE TABLE zakonczenie (
    id          TEXT PRIMARY KEY,
    tytul_pl    TEXT,
    tytul_en    TEXT,
    tytul_uk    TEXT,
    opis_pl     TEXT
);

CREATE TABLE warunek_zakonczenia (
    id                  TEXT PRIMARY KEY,
    id_zakonczenia      TEXT REFERENCES zakonczenie(id),
    id_flagi            TEXT REFERENCES flaga(id),
    wymagana_wartosc    INTEGER
);

-- ============================================================================
-- CZESC B: STAN ZAPISU GRACZA (puste przy generowaniu -- gra nie zapisuje
-- jeszcze do tej bazy; tabele czekaja na podpiecie w kolejnym etapie)
-- ============================================================================

CREATE TABLE zapis (
    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
    tryb                    TEXT,
    biezaca_noc             INTEGER REFERENCES noc(id),
    dyscyplina              INTEGER DEFAULT 0,
    sumienie                INTEGER DEFAULT 0,
    stres                   INTEGER DEFAULT 0,
    reputacja               INTEGER DEFAULT 0,
    integralnosc_pociagu    INTEGER DEFAULT 100,
    rekord_punktow          INTEGER DEFAULT 0,
    najlepszy_poziom        INTEGER DEFAULT 0,
    najdluzsza_seria        INTEGER DEFAULT 0,
    zaktualizowano          TEXT
);

CREATE TABLE zapis_flaga (
    id_zapisu   INTEGER NOT NULL REFERENCES zapis(id),
    id_flagi    TEXT NOT NULL REFERENCES flaga(id),
    wartosc     INTEGER DEFAULT 0,
    PRIMARY KEY (id_zapisu, id_flagi)
);

CREATE TABLE zapis_osiagniecie (
    id_zapisu           INTEGER NOT NULL REFERENCES zapis(id),
    id_osiagniecia      TEXT NOT NULL REFERENCES osiagniecie(id),
    data_zdobycia       TEXT,
    PRIMARY KEY (id_zapisu, id_osiagniecia)
);

CREATE TABLE zapis_kodeks (
    id_zapisu   INTEGER NOT NULL REFERENCES zapis(id),
    id_wpisu    TEXT NOT NULL REFERENCES wpis_kodeksu(id),
    PRIMARY KEY (id_zapisu, id_wpisu)
);

CREATE TABLE zapis_wybor (
    id_zapisu   INTEGER NOT NULL REFERENCES zapis(id),
    id_wyboru   TEXT NOT NULL REFERENCES wybor_wydarzenia(id),
    PRIMARY KEY (id_zapisu, id_wyboru)
);

CREATE TABLE decyzja (
    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    id_zapisu           INTEGER REFERENCES zapis(id),
    id_nocy             INTEGER REFERENCES noc(id),
    slot                INTEGER,
    werdykt             TEXT,
    poprawna            INTEGER,
    zlamana_zasada      TEXT,
    zmiana_stresu       INTEGER
);

CREATE TABLE zapis_umiejetnosc (
    id_zapisu       INTEGER NOT NULL REFERENCES zapis(id),
    umiejetnosc     TEXT NOT NULL,
    poziom          INTEGER DEFAULT 1,
    xp              INTEGER DEFAULT 0,
    PRIMARY KEY (id_zapisu, umiejetnosc)
);

-- ============================================================================
-- Indeksy pomocnicze
-- ============================================================================
CREATE INDEX idx_noc_akt ON noc(id_aktu);
CREATE INDEX idx_pwn_pasazer ON pasazer_w_nocy(id_pasazera);
CREATE INDEX idx_dokpas_pasazer ON dokument_pasazera(id_pasazera);
CREATE INDEX idx_pytanie_pasazer ON pytanie_dialogowe(id_pasazera);
CREATE INDEX idx_pytanie_prawda ON pytanie_dialogowe(prawda);
CREATE INDEX idx_wydarzenie_noc ON wydarzenie(id_nocy);
CREATE INDEX idx_wybor_wydarzenie ON wybor_wydarzenia(id_wydarzenia);
CREATE INDEX idx_warunek_zakonczenie ON warunek_zakonczenia(id_zakonczenia);
CREATE INDEX idx_decyzja_zapis ON decyzja(id_zapisu);
