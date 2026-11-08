# -*- coding: utf-8 -*-
"""Diagram przypadków użycia gry Ostatni Peron."""
import os
import math
from uml import Svg, PALETTE, poly, label_bg, wrap, LINE, TEXT

OUT = os.path.join(os.path.dirname(__file__), "..", "use case")
W, H = 1180, 870
svg = Svg()

# granica systemu
svg.add('<rect x="250" y="30" width="680" height="710" rx="16" fill="#FCFCFA" stroke="#B4B2A9" stroke-width="1.4"/>')
svg.text(590, 62, "Ostatni Peron (gra)", 21, "SemiBold", TEXT)

RX, RY = 135, 38
uc = {
    "UC1": (430, 140, "Rozpocznij noc", "teal"),
    "UC0": (760, 140, "Wczytaj zapis gry", "gray"),
    "UC2": (430, 280, "Sprawdź dokumenty", "purple"),
    "UC4": (760, 350, "Podejmij decyzję", "purple"),
    "UC3": (430, 420, "Przesłuchaj pasażera", "purple"),
    "UC5": (760, 560, "Zakończ noc i zapisz postęp", "orange"),
    "UC6": (430, 640, "Przeglądaj Kodeks i osiągnięcia", "pink"),
}


def edge_point(cx, cy, tx, ty, rx=RX, ry=RY):
    a = math.atan2(ty - cy, tx - cx)
    return cx + rx * math.cos(a), cy + ry * math.sin(a)


def actor(x, y, name):
    svg.add('<g fill="none" stroke="#444441" stroke-width="2">'
            '<circle cx="%d" cy="%d" r="17"/>'
            '<line x1="%d" y1="%d" x2="%d" y2="%d"/>'
            '<line x1="%d" y1="%d" x2="%d" y2="%d"/>'
            '<line x1="%d" y1="%d" x2="%d" y2="%d"/>'
            '<line x1="%d" y1="%d" x2="%d" y2="%d"/></g>'
            % (x, y - 50, x, y - 33, x, y + 15, x - 30, y - 14, x + 30, y - 14,
               x, y + 15, x - 24, y + 55, x, y + 15, x + 24, y + 55))
    svg.text(x, y + 80, name, 19, "SemiBold", TEXT)


def assoc(ax, ay, key):
    cx, cy = uc[key][0], uc[key][1]
    ex, ey = edge_point(cx, cy, ax, ay)
    svg.add('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="#888780" stroke-width="1.6"/>' % (ax, ay, ex, ey))


def dep(a, b, kind, lab_pos=None):
    ax, ay = uc[a][0], uc[a][1]
    bx, by = uc[b][0], uc[b][1]
    p1 = edge_point(ax, ay, bx, by)
    p2 = edge_point(bx, by, ax, ay)
    poly(svg, [p1, p2], dashed=True, color="#6B6A65")
    mx, my = lab_pos or ((p1[0] + p2[0]) / 2, (p1[1] + p2[1]) / 2 - 14)
    label_bg(svg, mx, my, "«%s»" % kind, 16, "#5F5E5A")


GX, GY = 120, 380
PX, PY = 1060, 400
for k in ("UC1", "UC2", "UC3", "UC4", "UC6"):
    assoc(GX + 32, GY - 14, k)
for k in ("UC3", "UC4"):
    assoc(PX - 32, PY - 14, k)

dep("UC1", "UC0", "include")
dep("UC2", "UC4", "extend")
dep("UC3", "UC4", "extend")
dep("UC5", "UC4", "extend", (808, 445))
dep("UC5", "UC6", "extend")

for key, (cx, cy, name, col) in uc.items():
    fill, stroke, tcol = PALETTE[col]
    svg.add('<ellipse cx="%d" cy="%d" rx="%d" ry="%d" fill="%s" stroke="%s" stroke-width="1.5"/>'
            % (cx, cy, RX, RY, fill, stroke))
    svg.mtext(cx, cy, wrap(name, 18, RX * 1.6, "SemiBold"), 18, "SemiBold", tcol)

actor(GX, GY, "Gracz (konduktor)")
actor(PX, PY, "Pasażer")

# legenda
svg.add('<line x1="250" y1="785" x2="320" y2="785" stroke="#888780" stroke-width="1.6"/>')
svg.text(340, 785, "Asocjacja aktor – przypadek użycia", 17, "Regular", TEXT, "start")
poly(svg, [(250, 825), (320, 825)], dashed=True, color="#6B6A65")
svg.text(340, 825, "Zależność «include» / «extend»", 17, "Regular", TEXT, "start")

svg.save(os.path.join(OUT, "diagram przypadkow uzycia.png"), W, H)
print("ok")
