# -*- coding: utf-8 -*-
"""Generuje diagram ERD (Mermaid) wprost z pliku schema.sql bazy ostatni_peron.db."""
import re
import sys
import os

SCHEMA = sys.argv[1] if len(sys.argv) > 1 else "schema.sql"
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "erd.mmd")

TYPES = {"INTEGER": "int", "TEXT": "varchar", "REAL": "decimal"}
# czasownik relacji: (tabela_z_FK, kolumna) -> etykieta
LABELS = {
    ("noc", "id_aktu"): "obejmuje",
    ("pasazer_w_nocy", "id_nocy"): "planuje",
    ("pasazer_w_nocy", "id_pasazera"): "występuje",
    ("typ_dokumentu", "odblokowanie_noc"): "odblokowuje",
    ("dokument_pasazera", "id_pasazera"): "posiada",
    ("dokument_pasazera", "typ"): "określa typ",
    ("rejestr", "id_pasazera"): "opisuje",
    ("pytanie_dialogowe", "id_pasazera"): "dotyczy",
    ("pytanie_dialogowe", "ustawia_flage"): "ustawia",
    ("pytanie_dialogowe", "kodeks_po_odpowiedzi"): "odblokowuje",
    ("wydarzenie", "id_nocy"): "zdarza się",
    ("wybor_wydarzenia", "id_wydarzenia"): "ma wybory",
    ("wybor_wydarzenia", "ustawia_flage"): "ustawia",
    ("wpis_kodeksu", "flaga_odblokowania"): "odblokowuje",
    ("osiagniecie", "flaga_odblokowania"): "odblokowuje",
    ("warunek_zakonczenia", "id_zakonczenia"): "wymaga",
    ("warunek_zakonczenia", "id_flagi"): "sprawdza",
    ("zapis", "biezaca_noc"): "wskazuje",
    ("zapis_flaga", "id_zapisu"): "ma flagi",
    ("zapis_flaga", "id_flagi"): "dotyczy",
    ("zapis_osiagniecie", "id_zapisu"): "zdobyto",
    ("zapis_osiagniecie", "id_osiagniecia"): "dotyczy",
    ("zapis_kodeks", "id_zapisu"): "odkryto",
    ("zapis_kodeks", "id_wpisu"): "dotyczy",
    ("zapis_wybor", "id_zapisu"): "wybrano",
    ("zapis_wybor", "id_wyboru"): "dotyczy",
    ("decyzja", "id_zapisu"): "rejestruje",
    ("decyzja", "id_nocy"): "podjęta w",
    ("zapis_umiejetnosc", "id_zapisu"): "rozwija",
}

sql = open(SCHEMA, encoding="utf-8").read()
sql = re.sub(r"--[^\n]*", "", sql)
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
        cols.append((col, TYPES[typ], "NOT NULL" in rest or "PRIMARY KEY" in rest))
        if "PRIMARY KEY" in rest:
            pk.append(col)
        fm = re.search(r"REFERENCES (\w+)\((\w+)\)", rest)
        if fm:
            fks[col] = fm.group(1)
    tables.append((name, cols, pk, fks))

HEADER = ["---", "config:", "  layout: elk", "  theme: base", "  themeVariables:",
          "    fontFamily: Inter, Segoe UI, Arial", "    primaryColor: \"#FAFAF8\"",
          "    primaryBorderColor: \"#9C9A92\"", "    lineColor: \"#6B6A65\"",
          "    tertiaryColor: \"#FFFFFF\"", "---", "erDiagram"]


def build(include, stub=()):
    """include: tabele w całości; stub: tabele pokazane tylko z kluczem głównym."""
    lines = list(HEADER)
    for name, cols, pk, fks in tables:
        if name not in include and name not in stub:
            continue
        lines.append("  %s {" % name)
        for col, typ, notnull in cols:
            if name in stub and col not in pk:
                continue
            keys = []
            if col in pk:
                keys.append("PK")
            if col in fks:
                keys.append("FK")
            lines.append("    %s %s%s" % (typ, col, (" " + ", ".join(keys)) if keys else ""))
        lines.append("  }")
    rels = []
    for name, cols, pk, fks in tables:
        if name not in include:
            continue
        notnull = {c: nn for c, t, nn in cols}
        for col, ref in fks.items():
            if ref not in include and ref not in stub:
                continue
            one = "||" if (notnull.get(col) or col in pk) else "|o"
            lab = LABELS.get((name, col), "odwołuje")
            rels.append("  %s %s--o{ %s : \"%s\"" % (ref, one, name, lab))
    return "\n".join(lines + rels) + "\n", len(rels)


ALL = [t[0] for t in tables]
SAVE = [t for t in ALL if t.startswith("zapis") or t == "decyzja"]
CONTENT = [t for t in ALL if t not in SAVE]
d = os.path.dirname(OUT)
for fname, inc, stub in (("erd.mmd", ALL, ()), ("erd_tresci.mmd", CONTENT, ()),
                         ("erd_zapisu.mmd", SAVE, ("noc", "flaga", "osiagniecie", "wpis_kodeksu", "wybor_wydarzenia"))):
    txt, n = build(inc, stub)
    open(os.path.join(d, fname), "w", encoding="utf-8").write(txt)
    print(fname, len(inc), "tabel,", n, "relacji")
