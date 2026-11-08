# -*- coding: utf-8 -*-
"""Tworzy sekcję 9.1 i 9.2 dokumentacji (opis tabel i relacji) na podstawie schema.sql."""
import re
import sys

SCHEMA = sys.argv[1]
sql_raw = open(SCHEMA, encoding="utf-8").read()

TABLE_DESC = {
    "akt": "Akty kampanii (7 aktów).",
    "noc": "Noce kampanii: zasada dnia, briefing, zakończenie nocy i limit błędów (12 nocy).",
    "flaga": "Flagi fabularne, które zapamiętują decyzje gracza.",
    "pasazer": "Pasażerowie zapisani w treści gry (postacie fabularne i scenariuszowe).",
    "pasazer_w_nocy": "Przypisanie pasażera do konkretnej pozycji w kolejce danej nocy (tabela pośrednia noc–pasażer).",
    "typ_dokumentu": "Rodzaje dokumentów (9) i noc, w której gracz je odblokowuje.",
    "dokument_pasazera": "Dokumenty konkretnych pasażerów wraz ze wskazówkami.",
    "rejestr": "Kartoteka osób (struktura przygotowana na przyszłość, obecnie pusta).",
    "pytanie_dialogowe": "Pytania i przygotowane odpowiedzi pasażerów wraz ze skutkami zadania pytania.",
    "wydarzenie": "Wydarzenia losowe i fabularne (18).",
    "wybor_wydarzenia": "Opcje wyboru w wydarzeniach i ich skutki.",
    "wpis_kodeksu": "Wpisy Kodeksu (encyklopedii w grze) w 8 kategoriach.",
    "osiagniecie": "Osiągnięcia, w tym sekretne.",
    "zakonczenie": "Zakończenia kampanii (5).",
    "warunek_zakonczenia": "Warunki (wartości flag) potrzebne do danego zakończenia.",
    "zapis": "Stan zapisu gracza: tryb, bieżąca noc, statystyki i rekordy.",
    "zapis_flaga": "Flagi ustawione w danym zapisie (tabela pośrednia zapis–flaga).",
    "zapis_osiagniecie": "Osiągnięcia zdobyte w danym zapisie (tabela pośrednia).",
    "zapis_kodeks": "Wpisy Kodeksu odkryte w danym zapisie (tabela pośrednia).",
    "zapis_wybor": "Wybory dokonane w wydarzeniach (tabela pośrednia).",
    "decyzja": "Historia decyzji gracza: werdykt dla każdego pasażera.",
    "zapis_umiejetnosc": "Poziom i XP każdej z 5 umiejętności w danym zapisie.",
}
COL_DESC = {
    "id": "Klucz główny", "slot": "Pozycja w kolejce nocy (0–12)", "kod": "Kod typu dokumentu",
    "imie_nazwisko": "Imię i nazwisko", "data_urodzenia": "Data urodzenia", "nr_dowodu": "Numer dowodu",
    "prawda": "Ukryta prawda (Alive, Dead, NonExistent, Altered, Loop, Impostor, Echo, Unknown)",
    "postac_powracajaca": "1 = postać fabularna wracająca w kolejnych nocach", "wiek": "Wiek",
    "plec": "Płeć", "kodeks_id": "Powiązany wpis Kodeksu", "notatka": "Notatka autora",
    "status": "Status osoby w rejestrze", "data_zgonu": "Data zgonu", "opis": "Opis",
    "wartosc_domyslna": "Wartość początkowa flagi", "zrodlo": "Plik lub klasa C#, z której pochodzi wpis",
    "rodzaj": "Rodzaj (losowe / fabularne)", "kategoria": "Kategoria Kodeksu", "sekretne": "1 = osiągnięcie ukryte",
    "warunek_odblokowania": "Opisowy warunek z kodu", "wymagana_wartosc": "Wymagana wartość flagi",
    "szansa_klamstwa": "Prawdopodobieństwo kłamstwa (0–1)", "wymaga_dokumentu": "1 = pytanie kontekstowe",
    "zmiana_stresu": "Zmiana stresu konduktora", "priorytet": "Kolejność proponowania pytania",
    "rodzaj_odpowiedzi": "Truth / Half / Lie / Evasive", "anomalia_po_odpowiedzi": "Anomalia ujawniana odpowiedzią",
    "min_autorytet": "Wymagany poziom Autorytetu", "zrodlo_funkcji": "Metoda w DialogueBank.cs",
    "data_w_grze": "Data w świecie gry", "maks_bledow": "Limit błędów w nocy (-1 = bez limitu)",
    "bit": "Bit w masce dokumentów (DayConfig.docMask)", "tryb": "Story / Endless",
    "dyscyplina": "Dyscyplina (0–10)", "sumienie": "Sumienie (0–10)", "stres": "Stres (0–100)",
    "reputacja": "Reputacja (0–100)", "integralnosc_pociagu": "Integralność pociągu (0–100)",
    "rekord_punktow": "Rekord punktów (Nocna zmiana)", "najlepszy_poziom": "Najlepszy poziom",
    "najdluzsza_seria": "Najdłuższa seria poprawnych decyzji", "zaktualizowano": "Data ostatniego zapisu",
    "wartosc": "Wartość flagi", "data_zdobycia": "Data zdobycia", "werdykt": "Wpuść / Odmów",
    "poprawna": "1 = decyzja zgodna z zasadą", "zlamana_zasada": "Złamana zasada (jeśli była)",
    "umiejetnosc": "Nazwa umiejętności", "poziom": "Poziom (1–10)", "xp": "Zdobyte doświadczenie",
    "id_osoby": "Klucz główny osoby", "id_scenariusza": "Skrypt postaci w StoryCast",
}
BASE = {"tytul": "Tytuł", "zasada": "Zasada nocy", "briefing": "Briefing", "outro": "Tekst kończący noc",
        "nazwa": "Nazwa", "tekst": "Tekst", "opis": "Opis", "wskazowka": "Wskazówka", "pytanie": "Pytanie",
        "odpowiedz": "Odpowiedź", "zawod": "Zawód", "pochodzenie": "Pochodzenie", "notatka": "Notatka"}
LANG = {"pl": "po polsku", "en": "po angielsku", "uk": "po ukraińsku"}
TYPES = {"INTEGER": "int", "TEXT": "varchar", "REAL": "decimal"}

sql = re.sub(r"--[^\n]*", "", sql_raw)
tables = []
for m in re.finditer(r"CREATE TABLE (\w+)\s*\((.*?)\);", sql, re.S):
    name, body = m.group(1), m.group(2)
    cols, pk, fks = [], [], {}
    for line in [l.strip().rstrip(",") for l in body.split("\n") if l.strip()]:
        pm = re.match(r"PRIMARY KEY\s*\(([^)]*)\)", line)
        if pm:
            pk += [c.strip() for c in pm.group(1).split(",")]
            continue
        cm = re.match(r"(\w+)\s+(INTEGER|TEXT|REAL)(.*)", line)
        if not cm:
            continue
        col, typ, rest = cm.groups()
        cols.append((col, TYPES[typ], "NOT NULL" in rest))
        if "PRIMARY KEY" in rest:
            pk.append(col)
        fm = re.search(r"REFERENCES (\w+)\((\w+)\)", rest)
        if fm:
            fks[col] = fm.group(1)
    tables.append((name, cols, pk, fks))


def desc(col, fks):
    if col in fks:
        return "Odwołanie do " + fks[col]
    m = re.match(r"(\w+)_(pl|en|uk)$", col)
    if m:
        return "%s %s" % (BASE.get(m.group(1), m.group(1).capitalize()), LANG[m.group(2)])
    return COL_DESC.get(col, col.replace("_", " ").capitalize())


out = ["### 9.1 Tabele", ""]
for name, cols, pk, fks in tables:
    out += ["#### %s" % name, "", TABLE_DESC.get(name, ""), "",
            "| Kolumna | Typ | Klucz | Opis |", "|---|---|---|---|"]
    for col, typ, nn in cols:
        k = ", ".join([x for x, c in (("PK", col in pk), ("FK", col in fks)) if c])
        out.append("| %s | %s | %s | %s |" % (col, typ, k, desc(col, fks)))
    out.append("")
out += ["### 9.2 Relacje", "", "| Tabele | Liczebność | Klucz obcy |", "|---|---|---|"]
for name, cols, pk, fks in tables:
    nn = {c: n for c, t, n in cols}
    for col, ref in fks.items():
        one = "1" if (nn.get(col) or col in pk) else "0..1"
        out.append("| %s – %s | %s : 0..* | %s.%s |" % (ref, name, one, name, col))
print("\n".join(out))
