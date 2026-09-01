# tools — skripte za generiranje sadržaja

Sva grafika i sav zvuk u igri generirani su ovim skriptama. U projektu nema
preuzetog sadržaja, pa nema ni pitanja licencije. Jedina iznimka je font
`LiberationSans SDF`, koji dolazi ugrađen s Unityjem kroz TextMeshPro i objavljen
je pod licencijom SIL Open Font License.

## Preduvjeti

```
pip install pillow numpy matplotlib
```

## Skripte

| Skripta | Što generira | Kamo u projektu |
|---|---|---|
| `generate_sprites.py` | Slavko, čuvar, sanduci, kokos, napitci, srca | `Assets/Art/` |
| `make_art.py` | pozadine u tri sloja i pločice tla za 4 teme | `Assets/Art/` |
| `make_audio.py` | 5 glazbenih petlji i 4 zvučna efekta | `Assets/Audio/` |
| `make_diagrams.py` | dijagrami za dokumentaciju | `Dokumentacija/` |
| `make_chase_chart.py` | graf potjere za dokumentaciju | `Dokumentacija/` |

## Pokretanje

```
python make_art.py
python make_audio.py
```

Svaka skripta zapisuje rezultat u podmapu `tools/izlaz/`. Datoteke se odande
ručno kopiraju u `Assets/`, kako ponovno pokretanje skripte ne bi slučajno
pregazilo sadržaj koji je Unity već uvezao i kojemu je pridružio `.meta`
datoteke s GUID-ovima.

## Kako su izrađeni

**Slike** se crtaju bibliotekom Pillow, geometrijskim primitivima — pravokutnici,
elipse, poligoni. Brda i oblaci u pozadinama opisani su sinusnim funkcijama čiji
period stane cijeli broj puta u širinu slike, pa se slika bešavno ponavlja po
vodoravnoj osi. Elementi koji prelaze rub crtaju se i u pomaknutim kopijama.
Ispravnost spoja provjerava se usporedbom prvog i posljednjeg stupca slike.

**Zvuk** se sintetizira bibliotekom NumPy, uzorak po uzorak, bez vanjskih
snimaka: pravokutni val nosi melodiju, trokutasti bas, a kratki naleti šuma
udaraljke. Izlaz je 16-bitni mono WAV na 44,1 kHz. Svaka je skladba građena od
osam taktova, a rez zapisa postavljen je točno na kraj posljednjeg takta, pa
petlja nema čujnog spoja.

## Napomena o mjerilu

100 piksela odgovara jednoj Unity jedinici. Pozadine su 1000 × 1400 piksela,
odnosno 10 × 14 jedinica, što odgovara vidnom polju kamere (ortografska veličina
7). Ako se promijeni udaljenost kamere, pozadine treba ponovno generirati s
izmijenjenim vrijednostima `BG_W` i `BG_H` u `make_art.py`.
