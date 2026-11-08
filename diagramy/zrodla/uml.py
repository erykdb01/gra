# -*- coding: utf-8 -*-
"""Mały generator diagramów UML (SVG -> PNG) w stylu materiałów z projektu zaliczeniowego.

Rysuje: diagramy aktywności z torami, diagramy sekwencji, diagram przypadków użycia
oraz legendy. Diagramy klas i ERD są w plikach .mmd (Mermaid).
"""
import os
import subprocess
from xml.sax.saxutils import escape
from PIL import ImageFont

FONT_DIR = "/usr/share/fonts/opentype/inter/"
FONT_FAMILY = "Inter, 'Segoe UI', Arial, sans-serif"
_fonts = {}


def font(size, weight="Regular"):
    key = (size, weight)
    if key not in _fonts:
        _fonts[key] = ImageFont.truetype(FONT_DIR + "Inter-%s.otf" % weight, size)
    return _fonts[key]


def tw(text, size, weight="Regular"):
    return font(size, weight).getlength(text)


def wrap(text, size, maxw, weight="Regular"):
    lines = []
    for para in text.split("\n"):
        cur = ""
        for word in para.split(" "):
            t = (cur + " " + word).strip()
            if tw(t, size, weight) <= maxw or not cur:
                cur = t
            else:
                lines.append(cur)
                cur = word
        lines.append(cur)
    return lines


# kolory torów / uczestników: (wypełnienie, obrys, tekst)
PALETTE = {
    "blue":   ("#E6F1FB", "#185FA5", "#0C447C"),
    "purple": ("#EEEDFE", "#534AB7", "#3C3489"),
    "green":  ("#EAF3DE", "#3B6D11", "#27500A"),
    "orange": ("#FAEEDA", "#854F0B", "#633806"),
    "teal":   ("#E1F5EE", "#0F6E56", "#085041"),
    "pink":   ("#FBEAF0", "#993556", "#72243E"),
    "gray":   ("#F1EFE8", "#5F5E5A", "#444441"),
}
LINE = "#7A7975"
TEXT = "#2C2C2A"
MUTED = "#5F5E5A"


class Svg:
    def __init__(self):
        self.parts = []

    def add(self, s):
        self.parts.append(s)

    def text(self, x, y, s, size=17, weight="Regular", color=TEXT, anchor="middle"):
        w = {"Regular": 400, "Medium": 500, "SemiBold": 600, "Bold": 700}[weight]
        self.add('<text x="%.1f" y="%.1f" font-family="%s" font-size="%d" font-weight="%d" fill="%s" '
                 'text-anchor="%s" dy="0.36em">%s</text>'
                 % (x, y, FONT_FAMILY, size, w, color, anchor, escape(s)))

    def mtext(self, x, y, lines, size=17, weight="Regular", color=TEXT, lh=None, anchor="middle"):
        lh = lh or size * 1.3
        y0 = y - (len(lines) - 1) * lh / 2
        for i, l in enumerate(lines):
            self.text(x, y0 + i * lh, l, size, weight, color, anchor)

    def save(self, path, w, h, zoom=2):
        svg = ('<svg xmlns="http://www.w3.org/2000/svg" width="%d" height="%d" viewBox="0 0 %d %d">'
               '<defs>'
               '<marker id="arr" viewBox="0 0 12 12" refX="11" refY="6" markerWidth="12" markerHeight="12" '
               'markerUnits="userSpaceOnUse" orient="auto"><path d="M1,1 L11,6 L1,11" fill="none" stroke="%s" '
               'stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></marker>'
               '<marker id="arrf" viewBox="0 0 12 12" refX="11" refY="6" markerWidth="12" markerHeight="12" '
               'markerUnits="userSpaceOnUse" orient="auto"><path d="M1,1 L11,6 L1,11 z" fill="%s"/></marker>'
               '</defs><rect width="100%%" height="100%%" fill="#FFFFFF"/>%s</svg>'
               % (w, h, w, h, LINE, "#444441", "".join(self.parts)))
        os.makedirs(os.path.dirname(path), exist_ok=True)
        svg_path = path[:-4] + ".svg"
        with open(svg_path, "w", encoding="utf-8") as f:
            f.write(svg)
        subprocess.run(["rsvg-convert", "-z", str(zoom), "-o", path, svg_path], check=True)
        os.remove(svg_path)


def poly(svg, pts, dashed=False, marker="arr", color=LINE, width=2):
    d = "M" + " L".join("%.1f,%.1f" % p for p in pts)
    svg.add('<path d="%s" fill="none" stroke="%s" stroke-width="%s" %s %s stroke-linejoin="round"/>'
            % (d, color, width, 'stroke-dasharray="7 6"' if dashed else "",
               'marker-end="url(#%s)"' % marker if marker else ""))


def label_bg(svg, x, y, s, size=15, color=MUTED, anchor="middle"):
    w = tw(s, size) + 10
    x0 = x - w / 2 if anchor == "middle" else (x - 5 if anchor == "start" else x - w + 5)
    svg.add('<rect x="%.1f" y="%.1f" width="%.1f" height="%d" fill="#FFFFFF"/>' % (x0, y - 11, w, 22))
    svg.text(x, y, s, size, "Regular", color, anchor)


# ---------------------------------------------------------------- aktywność

def activity(path, lanes, nodes, edges, colw=250, rowh=112, title=None):
    """lanes: [(nazwa, kolor, liczba_podkolumn)]
    nodes: {id: (rodzaj, tor, podkolumna, wiersz, tekst)}; rodzaj: start/end/act/dec
    edges: [(z, do, etykieta, trasa)]; trasa: None/'v'/'h'/'vh'/'hv'/'loop'/'side'"""
    head = 64
    top = 30
    lane_x = []
    x = 30
    for name, col, subs in lanes:
        lane_x.append(x)
        x += subs * colw
    width = x + 30
    lane_idx = {l[0]: i for i, l in enumerate(lanes)}
    maxrow = max(n[3] for n in nodes.values())
    height = top + head + 30 + (maxrow + 1) * rowh + 30
    svg = Svg()
    BW, BH = colw - 44, 74
    DW, DH = colw - 44, 84

    # tory
    for i, (name, col, subs) in enumerate(lanes):
        fill, stroke, tcol = PALETTE[col]
        x0 = lane_x[i]
        w = subs * colw
        svg.add('<rect x="%d" y="%d" width="%d" height="%d" fill="#FFFFFF" stroke="#D3D1C7" stroke-width="1.2"/>'
                % (x0, top, w, height - top - 30))
        svg.add('<rect x="%d" y="%d" width="%d" height="%d" fill="%s" stroke="%s" stroke-width="1.2"/>'
                % (x0, top, w, head, fill, stroke))
        svg.text(x0 + w / 2, top + head / 2, name, 21, "SemiBold", tcol)

    geo = {}
    for nid, (kind, lane, sub, row, text) in nodes.items():
        li = lane_idx[lane]
        cx = lane_x[li] + (sub + 0.5) * colw
        cy = top + head + 30 + row * rowh + rowh / 2
        if kind == "start":
            geo[nid] = (cx, cy, 17, 17)
        elif kind == "end":
            geo[nid] = (cx, cy, 20, 20)
        elif kind == "dec":
            geo[nid] = (cx, cy, DW / 2, DH / 2)
        else:
            geo[nid] = (cx, cy, BW / 2, BH / 2)

    def port(nid, side):
        cx, cy, hw, hh = geo[nid]
        return {"top": (cx, cy - hh), "bottom": (cx, cy + hh), "left": (cx - hw, cy), "right": (cx + hw, cy)}[side]

    # krawędzie (najpierw, żeby węzły były nad nimi)
    labels = []
    for e in edges:
        a, b, lab = e[0], e[1], e[2]
        route = e[3] if len(e) > 3 else None
        ax, ay, ahw, ahh = geo[a]
        bx, by, bhw, bhh = geo[b]
        if route == "loop":
            # pętla powrotna po lewej stronie toru
            li = lane_idx[nodes[a][1]]
            lx = lane_x[li] + 14
            p1 = port(a, "left")
            p2 = port(b, "left")
            pts = [p1, (lx, p1[1]), (lx, p2[1]), p2]
            lp = (lx + 8, (p1[1] + p2[1]) / 2, "start")
        elif route == "loopr":
            li = lane_idx[nodes[a][1]]
            lx = lane_x[li] + lanes[li][2] * colw - 14
            p1 = port(a, "right")
            p2 = port(b, "right")
            pts = [p1, (lx, p1[1]), (lx, p2[1]), p2]
            lp = (lx - 8, (p1[1] + p2[1]) / 2, "end")
        elif abs(ax - bx) < 1 and route in (None, "v"):
            p1, p2 = (port(a, "bottom"), port(b, "top")) if by > ay else (port(a, "top"), port(b, "bottom"))
            pts = [p1, p2]
            lp = (ax + 12, p1[1] + 24, "start")
        elif abs(ay - by) < 1 or route == "h":
            if bx > ax:
                p1, p2 = port(a, "right"), port(b, "left")
            else:
                p1, p2 = port(a, "left"), port(b, "right")
            pts = [p1, (p2[0], p1[1])]
            lp = ((p1[0] + p2[0]) / 2, p1[1] - 16, "middle")
        elif route == "hv" or (route is None and nodes[a][0] == "dec"):
            # z boku węzła poziomo, potem w dół do góry celu
            p1 = port(a, "right" if bx > ax else "left")
            p2 = port(b, "top" if by > ay else "bottom")
            pts = [p1, (p2[0], p1[1]), p2]
            lp = (p1[0] + (24 if bx > ax else -24), p1[1] - 16, "start" if bx > ax else "end")
        elif route == "vh":
            # w dół, potem poziomo w bok celu
            p1 = port(a, "bottom" if by > ay else "top")
            p2 = port(b, "left" if bx > ax else "right")
            pts = [p1, (p1[0], p2[1]), p2]
            lp = (p1[0] + 12, p1[1] + 24, "start")
        elif route == "side":
            # z boku, w dół do połowy, poziomo, w dół (do góry celu)
            p1 = port(a, "bottom")
            p2 = port(b, "top")
            midy = p2[1] - 26
            pts = [p1, (p1[0], midy), (p2[0], midy), p2]
            lp = (p1[0] + 12, p1[1] + 22, "start")
        else:
            p1 = port(a, "bottom")
            p2 = port(b, "top")
            midy = p2[1] - 26
            pts = [p1, (p1[0], midy), (p2[0], midy), p2]
            lp = (p1[0] + 12, p1[1] + 22, "start")
        poly(svg, pts)
        if lab:
            labels.append((lp, lab))
    for (x, y, anc), lab in labels:
        label_bg(svg, x, y, lab, 16, MUTED, anc)

    for nid, (kind, lane, sub, row, text) in nodes.items():
        cx, cy, hw, hh = geo[nid]
        fill, stroke, tcol = PALETTE[lanes[lane_idx[lane]][1]]
        if kind == "start":
            svg.add('<circle cx="%.1f" cy="%.1f" r="17" fill="#111111"/>' % (cx, cy))
        elif kind == "end":
            svg.add('<circle cx="%.1f" cy="%.1f" r="20" fill="#FFFFFF" stroke="#BDBBB3" stroke-width="1.5"/>'
                    '<circle cx="%.1f" cy="%.1f" r="13" fill="#111111"/>' % (cx, cy, cx, cy))
        elif kind == "dec":
            svg.add('<polygon points="%.1f,%.1f %.1f,%.1f %.1f,%.1f %.1f,%.1f" fill="#F1EFE8" stroke="#6B6A65" stroke-width="1.3"/>'
                    % (cx, cy - hh, cx + hw, cy, cx, cy + hh, cx - hw, cy))
            svg.mtext(cx, cy, wrap(text, 15, hw * 0.95), 15, "Regular", "#3D3D3A")
        else:
            svg.add('<rect x="%.1f" y="%.1f" width="%.1f" height="%.1f" rx="10" fill="%s" stroke="%s" stroke-width="1.3"/>'
                    % (cx - hw, cy - hh, hw * 2, hh * 2, fill, stroke))
            svg.mtext(cx, cy, wrap(text, 17, hw * 2 - 22), 17, "Regular", tcol)

    svg.save(path, width, height)


# ---------------------------------------------------------------- sekwencja

def sequence(path, parts, msgs, colw=290, step=64):
    """parts: [(id, etykieta, kolor)]; msgs: [(rodzaj, z, do, etykieta)] rodzaj: call/ret/self/note"""
    top = 30
    ph = 74
    pw = colw - 50
    xs = {}
    for i, (pid, lab, col) in enumerate(parts):
        xs[pid] = 40 + colw / 2 + i * colw
    width = int(40 * 2 + colw * len(parts))
    y = top + ph + 50
    acts = {pid: [] for pid, _, _ in parts}   # otwarte aktywacje: lista [y_start, depth]
    bars = []
    arrows = []
    for kind, a, b, lab in msgs:
        if kind == "call":
            arrows.append(("call", a, b, lab, y))
            depth = len(acts[b])
            acts[b].append([y, depth])
            if not acts[a]:
                acts[a].append([y, 0])
            y += step
        elif kind == "ret":
            arrows.append(("ret", a, b, lab, y))
            if acts[a]:
                s, d = acts[a].pop()
                bars.append((a, s, y, d))
            y += step
        elif kind == "self":
            arrows.append(("self", a, a, lab, y))
            depth = len(acts[a])
            bars.append((a, y + 18, y + 44, depth))
            y += step + 34
    end_y = y + 10
    for pid, stack in acts.items():
        while stack:
            s, d = stack.pop()
            bars.append((pid, s, end_y - 20, d))
    height = int(end_y + 50)
    svg = Svg()
    for pid, lab, col in parts:
        fill, stroke, tcol = PALETTE[col]
        x = xs[pid]
        svg.add('<line x1="%.1f" y1="%d" x2="%.1f" y2="%d" stroke="#C9C7BE" stroke-width="1.3" stroke-dasharray="6 6"/>'
                % (x, top + ph, x, height - 30))
        svg.add('<rect x="%.1f" y="%d" width="%d" height="%d" rx="12" fill="%s" stroke="%s" stroke-width="1.3"/>'
                % (x - pw / 2, top, pw, ph, fill, stroke))
        size = 21 if tw(lab, 21, "SemiBold") < pw - 20 else 18
        svg.text(x, top + ph / 2, lab, size, "SemiBold", tcol)

    def bar_x(pid, depth):
        return xs[pid] + depth * 9

    for pid, s, e, d in sorted(bars, key=lambda t: t[3]):
        fill, stroke, _ = PALETTE[[p[2] for p in parts if p[0] == pid][0]]
        svg.add('<rect x="%.1f" y="%.1f" width="18" height="%.1f" fill="%s" stroke="%s" stroke-width="1.1"/>'
                % (bar_x(pid, d) - 9, s, max(e - s, 14), "#E6F1FB" if fill == "#FFFFFF" else fill, stroke))
    for kind, a, b, lab, yy in arrows:
        if kind == "self":
            x = xs[a] + 9
            pts = [(x, yy), (x + 46, yy), (x + 46, yy + 30), (x + 18, yy + 30)]
            poly(svg, pts, marker="arr", color="#555552")
            svg.text(x + 56, yy + 4, lab, 17, "Regular", "#444441", "start")
            continue
        xa, xb = xs[a], xs[b]
        off = 9 if xb > xa else -9
        x1 = xa + off
        x2 = xb - off - (0 if kind == "ret" else 0)
        poly(svg, [(x1, yy), (x2, yy)], dashed=(kind == "ret"), marker="arr", color="#555552")
        svg.text((x1 + x2) / 2, yy - 18, lab, 17, "Regular", "#444441")
    svg.save(path, width, height)


# ---------------------------------------------------------------- legendy

def legend(path, rows, width=1000):
    svg = Svg()
    y = 60
    for kind, desc, *extra in rows:
        cx = 130
        if kind == "start":
            svg.add('<circle cx="%d" cy="%d" r="19" fill="#111111"/>' % (cx, y))
        elif kind == "end":
            svg.add('<circle cx="%d" cy="%d" r="22" fill="#FFFFFF" stroke="#BDBBB3" stroke-width="1.5"/>'
                    '<circle cx="%d" cy="%d" r="14" fill="#111111"/>' % (cx, y, cx, y))
        elif kind == "act":
            svg.add('<rect x="%d" y="%d" width="120" height="54" rx="10" fill="#FAFAF8" stroke="#BDBBB3" stroke-width="1.3"/>' % (cx - 60, y - 27))
        elif kind == "dec":
            svg.add('<polygon points="%d,%d %d,%d %d,%d %d,%d" fill="#F1EFE8" stroke="#6B6A65" stroke-width="1.3"/>'
                    % (cx, y - 34, cx + 62, y, cx, y + 34, cx - 62, y))
        elif kind in ("arrow", "label"):
            poly(svg, [(cx - 60, y), (cx + 60, y)])
            if kind == "label":
                svg.text(cx - 24, y - 26, "Tak", 17, "Regular", MUTED)
        elif kind == "lane":
            fill, stroke, _ = PALETTE[extra[0]]
            svg.add('<rect x="%d" y="%d" width="120" height="46" rx="6" fill="%s" stroke="%s" stroke-width="1.3"/>' % (cx - 60, y - 23, fill, stroke))
        elif kind == "lifeobj":
            fill, stroke, tcol = PALETTE[extra[0]]
            svg.add('<rect x="%d" y="%d" width="140" height="50" rx="10" fill="%s" stroke="%s" stroke-width="1.3"/>' % (cx - 70, y - 25, fill, stroke))
            svg.text(cx, y, extra[1], 18, "SemiBold", tcol)
        elif kind == "lifeline":
            svg.add('<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#C9C7BE" stroke-width="1.5" stroke-dasharray="6 6"/>' % (cx, y - 30, cx, y + 30))
        elif kind == "bar":
            svg.add('<rect x="%d" y="%d" width="18" height="64" fill="#E6F1FB" stroke="#185FA5" stroke-width="1.1"/>' % (cx - 9, y - 32))
        elif kind == "call":
            poly(svg, [(cx - 65, y), (cx + 65, y)], color="#555552")
            svg.text(cx, y - 18, "metoda()", 16, "Regular", "#444441")
        elif kind == "ret":
            poly(svg, [(cx + 65, y), (cx - 65, y)], dashed=True, color="#555552")
            svg.text(cx, y - 18, "wynik", 16, "Regular", "#444441")
        elif kind == "selfcall":
            poly(svg, [(cx - 30, y - 16), (cx + 20, y - 16), (cx + 20, y + 14), (cx - 8, y + 14)], color="#555552")
        svg.text(250, y, desc, 21, "Regular", TEXT, "start")
        y += 96
    svg.save(path, width, y - 20)
