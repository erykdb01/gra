# -*- coding: utf-8 -*-
"""Legenda diagramu ERD (notacja kurzej łapki)."""
import os
from uml import Svg, TEXT

OUT = os.path.join(os.path.dirname(__file__), "..", "erd - schemat bazy danych", "legenda diagramu erd.png")
svg = Svg()
C = "#555552"


def ln(x1, y1, x2, y2):
    svg.add('<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="%s" stroke-width="2"/>' % (x1, y1, x2, y2, C))


def end(x, y, kind):
    # x = punkt styku z tabelą (prawy koniec linii)
    ln(x - 140, y, x, y)
    if kind in ("one", "only"):
        ln(x - 18, y - 12, x - 18, y + 12)
        ln(x - 30, y - 12, x - 30, y + 12)
    if kind == "zero_one":
        ln(x - 18, y - 12, x - 18, y + 12)
        svg.add('<circle cx="%d" cy="%d" r="9" fill="#FFF" stroke="%s" stroke-width="2"/>' % (x - 40, y, C))
    if kind in ("many", "one_many"):
        ln(x - 24, y, x, y - 13); ln(x - 24, y, x, y + 13)
        if kind == "one_many":
            ln(x - 34, y - 12, x - 34, y + 12)
        else:
            svg.add('<circle cx="%d" cy="%d" r="9" fill="#FFF" stroke="%s" stroke-width="2"/>' % (x - 36, y, C))


rows = [
    ("pk", "PK (Primary Key): klucz główny, unikalny i niepusty"),
    ("fk", "FK (Foreign Key): klucz obcy, odwołanie do PK innej tabeli"),
    ("one", "Dokładnie jeden (1)"),
    ("zero_one", "Zero lub jeden (0..1)"),
    ("one_many", "Jeden lub więcej (1..*)"),
    ("many", "Zero lub więcej (0..*)"),
]
y = 60
for kind, desc in rows:
    if kind in ("pk", "fk"):
        svg.add('<rect x="70" y="%d" width="160" height="44" fill="#FAFAF8" stroke="#9C9A92"/>' % (y - 22))
        svg.text(100, y, kind.upper(), 19, "SemiBold", "#444441")
        svg.text(170, y, "id" if kind == "pk" else "id_nocy", 18, "Regular", TEXT)
    else:
        end(230, y, kind)
    svg.text(280, y, desc, 21, "Regular", TEXT, "start")
    y += 90
svg.save(OUT, 1100, y - 20)
print("ok")
