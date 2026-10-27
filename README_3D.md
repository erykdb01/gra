# OSTATNI PERON 3D (gałąź `feature/3d`)

**PL** | [UA](#українська)

Wersja 3D gry *Ostatni Peron*. Gałąź jest eksperymentem: wersja 2D (gałąź `dev/iansky-v2`) zostaje nietknięta i nadal działa.

## Cel

Przenieść całą oprawę wizualną z 2D (pikselowa grafika) do 3D, nie ruszając logiki gry. Cała logika jest już oddzielona od grafiki, więc zostaje bez zmian: pasażerowie i ich ukryte prawdy, dokumenty, anomalie, dialogi, fabuła, osiągnięcia, zapis gry oraz trzy języki (PL / EN / UA).

## Co zostaje, co się zmienia

| Zostaje | Do przepisania w 3D |
|---|---|
| `PassengerGenerator`, `DocumentSystem`, `DialogueBank`, `DayConfig`, `StoryCast`, `Loc` i tłumaczenia | `CharacterArt` / `CharacterView` -> modele 3D pasażerów |
| Zapis gry, osiągnięcia, kodeks | `StageView` / `TrainView` -> scena 3D stacji, peron, pociąg |
| Interfejs (menu, HUD, ustawienia) - pozostaje jako nakładka 2D | Biurko z dokumentami, pieczątki PUŚĆ / ODMÓW jako obiekty 3D |
| | Oświetlenie, mgła, deszcz, efekty grozy w 3D |

## Plan (etapy)

1. **Scena i kamera** - peron, pociąg, okienko kontrolne, kamera z pierwszej osoby za biurkiem, oświetlenie i mgła.
2. **Modele pasażerów** - 13 wyglądów low-poly (w tym dziecko, konduktor, człowiek w szarym płaszczu) generowanych w kodzie, proste animacje (oddech, skinienie, odejście).
3. **Biurko i dokumenty** - dokumenty jako obiekty na biurku, przeciąganie, obracanie, powiększanie; pieczątki.
4. **Efekty i klimat** - migotanie lamp, brak cienia u nieistniejących, zimny oddech u zmarłych, dźwięk przestrzenny.
5. **Dopracowanie** - wydajność, ustawienia jakości grafiki, README i build.

## Styl

Stylizowane low-poly w duchu PS1 (niska rozdzielczość tekstur, twarde cienie, gęsta mgła). Dobrze pasuje do horroru i nie wymaga zewnętrznych modeli: wszystko jest generowane w kodzie, tak jak grafika i dźwięk w wersji 2D.

## Założenia i ograniczenia

- Unity 6, ten sam projekt; wersja 3D wchodzi jako osobna gałąź i osobne sceny.
- Brak zewnętrznych assetów (modele, tekstury generowane proceduralnie).
- Etapy oddawane pojedynczo, każdy do sprawdzenia w Unity zanim zacznie się następny.

## Praca z gałęzią

```
git checkout dev/iansky-v2
git pull
git checkout -b feature/3d
git push -u origin feature/3d
```

Powrót do wersji 2D: `git checkout dev/iansky-v2`.

---

<a name="українська"></a>

# ОСТАННІЙ ПЕРОН 3D (гілка `feature/3d`)

3D-версія гри. Це експеримент: 2D-версія (гілка `dev/iansky-v2`) залишається без змін.

**Мета:** перенести всю візуальну частину з 2D у 3D, не чіпаючи логіки: пасажири, документи, діалоги, сюжет, досягнення, збереження й три мови працюють як раніше.

**Етапи:** 1) сцена й камера, 2) low-poly моделі пасажирів, 3) стіл із документами та печатками, 4) ефекти й атмосфера, 5) оптимізація та збірка.

**Стиль:** стилізоване low-poly в дусі PS1, усе генерується кодом, без зовнішніх ассетів.
