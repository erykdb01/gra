# -*- coding: utf-8 -*-
"""Składa dokumentację: wstawia opisy przypadków użycia i tabele ERD do szablonu,
tworzy osobny plik z przypadkami użycia i eksportuje oba do PDF (pandoc + LibreOffice).

Użycie: python3 zbuduj_dokumentacje.py ścieżka/do/schema.sql
"""
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
schema = sys.argv[1]

uc = open(os.path.join(HERE, "przypadki_uzycia_tresc.md"), encoding="utf-8").read()
erd = subprocess.run([sys.executable, os.path.join(HERE, "tabele_erd_md.py"), schema],
                     capture_output=True, text=True, check=True).stdout
tpl = open(os.path.join(HERE, "szablon_dokumentacji.md"), encoding="utf-8").read()
doc = tpl.replace("<!--USE_CASES-->", uc).replace("<!--ERD_TABLES-->", erd)
doc_path = os.path.join(ROOT, "dokumentacja", "Dokumentacja_projektu.md")
open(doc_path, "w", encoding="utf-8").write(doc)

# osobny plik z przypadkami użycia (numeracja 1..n)
def renum(m):
    return "## %d. " % (int(m.group(1)) - 2)
uc_alone = re.sub(r"^### 6\.(\d) ", renum, uc, flags=re.M)
uc_alone = ("# Szczegółowe przypadki użycia\n\n**Ostatni Peron** (gra narracyjna o kontroli dokumentów)\n\n"
            "![Diagram przypadków użycia](diagram%20przypadkow%20uzycia.png)\n\n" + uc_alone)
uc_path = os.path.join(ROOT, "diagramy", "use case", "przypadki_uzycia.md")
open(uc_path, "w", encoding="utf-8").write(uc_alone)


CHROME = "/opt/pw-browsers/chromium-1243/chrome-linux64/chrome"


def to_pdf(md_path):
    """Markdown -> HTML (pandoc, styl.css) -> PDF (Chrome headless)."""
    folder = os.path.dirname(md_path)
    base = os.path.splitext(os.path.basename(md_path))[0]
    html = os.path.join(folder, "_" + base + ".html")
    subprocess.run(["pandoc", md_path, "-s", "-o", html, "--metadata", "title=" + base.replace("_", " "),
                    "-V", "pagetitle=" + base, "--css", os.path.join(HERE, "styl.css"), "--embed-resources",
                    "--resource-path", folder, "-V", "title="], check=True)
    pdf = os.path.join(folder, base + ".pdf")
    subprocess.run([CHROME, "--headless", "--no-sandbox", "--disable-gpu", "--no-pdf-header-footer",
                    "--print-to-pdf=" + pdf, "file://" + html], check=True, capture_output=True)
    os.remove(html)
    print("PDF:", pdf)


to_pdf(doc_path)
to_pdf(uc_path)
