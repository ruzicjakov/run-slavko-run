"""Grafika uvedena u drugoj inacici: viseca prepreka, pokretna traka,
ciljna vrata i uvodna slika.

Stil prati ostatak igre: plosne boje, tanki obrub, bez sjencanja.
100 px = 1 Unity jedinica.
"""
import math
import os
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'izlaz', 'newart')
os.makedirs(OUT, exist_ok=True)


def canvas(w, h):
    return Image.new('RGBA', (w, h), (0, 0, 0, 0))


def save(img, name):
    img.save(os.path.join(OUT, name))
    print('  %-24s %s' % (name, img.size))


# ------------------------------------------------------- viseca prepreka
# Prepreka koja visi na visini skoka: dok Slavko trci, prolazi ispod nje,
# ali ako skoci, udari u nju. Zato mora izgledati kao nesto sto visi
# ODOZGO — greda sa znakom, ne predmet koji stoji na tlu.
def make_hanging_sign():
    W, H = 120, 200
    img = canvas(W, H)
    d = ImageDraw.Draw(img)
    steel = (108, 116, 130, 255)
    steel_d = (72, 78, 90, 255)
    sign = (214, 78, 62, 255)
    sign_d = (150, 46, 36, 255)
    band = (242, 240, 232, 255)

    # nosac koji ide s vrha kadra (dojam da visi s necega iznad)
    d.rectangle([W // 2 - 7, 0, W // 2 + 7, 74], fill=steel, outline=steel_d, width=3)
    for y in range(10, 70, 16):
        d.line([W // 2 - 7, y, W // 2 + 7, y + 8], fill=steel_d, width=2)

    # ploca znaka
    d.rounded_rectangle([10, 74, W - 10, H - 16], radius=12,
                        fill=sign, outline=sign_d, width=4)
    # dijagonalne pruge upozorenja
    for i in range(-H, W + H, 30):
        d.polygon([(i, 74), (i + 14, 74), (i + 14 - 120, H - 16), (i - 120, H - 16)],
                  fill=band)
    d.rounded_rectangle([10, 74, W - 10, H - 16], radius=12,
                        outline=sign_d, width=4)
    # maska: ostavi samo unutrasnjost ploce
    mask = canvas(W, H)
    ImageDraw.Draw(mask).rounded_rectangle([10, 74, W - 10, H - 16], radius=12,
                                           fill=(255, 255, 255, 255))
    plate = Image.composite(img, canvas(W, H), mask.split()[3])
    out = canvas(W, H)
    d2 = ImageDraw.Draw(out)
    d2.rectangle([W // 2 - 7, 0, W // 2 + 7, 78], fill=steel, outline=steel_d, width=3)
    out.alpha_composite(plate)
    d2 = ImageDraw.Draw(out)
    d2.rounded_rectangle([10, 74, W - 10, H - 16], radius=12, outline=sign_d, width=4)
    save(out, 'hanging_sign.png')


# ------------------------------------------------------- pokretna traka
# Bešavno se ponavlja po vodoravnoj osi (SpriteRenderer Draw Mode = Tiled).
def make_belt():
    W, H = 200, 60
    img = canvas(W, H)
    d = ImageDraw.Draw(img)
    body = (78, 84, 96, 255)
    dark = (52, 57, 66, 255)
    tread = (128, 136, 150, 255)
    edge = (168, 150, 60, 255)

    d.rectangle([0, 10, W, H - 10], fill=body)
    # strelice u smjeru gibanja; period 40 px stane cijeli broj puta u 200
    for x in range(0, W, 40):
        d.polygon([(x + 8, 20), (x + 26, H // 2), (x + 8, H - 20)], fill=tread)
    # rubne trake gore i dolje
    d.rectangle([0, 0, W, 10], fill=edge)
    d.rectangle([0, H - 10, W, H], fill=edge)
    # krajevi su ukljucivi u PIL-u, pa se crta x..x+9 da period bude tocno 20 px
    for x in range(0, W, 20):
        d.rectangle([x, 0, x + 9, 10], fill=dark)
        d.rectangle([x + 10, H - 10, x + 19, H], fill=dark)
    save(img, 'belt.png')


# ------------------------------------------------------- ciljna vrata
# Vidljiv znak da razina zavrsava upravo ovdje.
def make_finish_gate():
    W, H = 300, 380
    img = canvas(W, H)
    d = ImageDraw.Draw(img)
    post = (96, 102, 116, 255)
    post_d = (62, 67, 78, 255)
    arch = (72, 148, 92, 255)
    arch_d = (46, 104, 62, 255)

    # dva stupa
    for x0 in (14, W - 62):
        d.rectangle([x0, 90, x0 + 48, H - 6], fill=post, outline=post_d, width=4)
    # gornja greda
    d.rounded_rectangle([6, 26, W - 6, 104], radius=10, fill=arch, outline=arch_d, width=5)
    # sahovnica na gredi
    sq = 22
    for i in range((W - 24) // sq + 1):
        for j in range(2):
            if (i + j) % 2 == 0:
                x = 12 + i * sq
                y = 44 + j * sq
                d.rectangle([x, y, min(x + sq, W - 14), y + sq], fill=(246, 246, 240, 255))
    d.rounded_rectangle([6, 26, W - 6, 104], radius=10, outline=arch_d, width=5)

    # natpis CILJ
    tw = 150
    d.rounded_rectangle([(W - tw) // 2, 118, (W + tw) // 2, 168], radius=8,
                        fill=(246, 246, 240, 255), outline=arch_d, width=4)
    # slova crtana rucno da ne ovisimo o fontu
    letters = {
        'C': [(0, 0, 1, 0), (0, 0, 0, 2), (0, 2, 1, 2)],
        'I': [(0.5, 0, 0.5, 2)],
        'L': [(0, 0, 0, 2), (0, 2, 1, 2)],
        'J': [(1, 0, 1, 2), (0, 2, 1, 2), (0, 1.4, 0, 2)],
    }
    lx = (W - tw) // 2 + 22
    for ch in 'CILJ':
        for (x1, y1, x2, y2) in letters[ch]:
            d.line([lx + x1 * 20, 130 + y1 * 12, lx + x2 * 20, 130 + y2 * 12],
                   fill=arch_d, width=6)
        lx += 32
    save(img, 'finish_gate.png')


# ------------------------------------------------------- uvodna slika
# Slavko iza resetaka: kontekst price prije prve razine.
def make_intro():
    W, H = 960, 420
    img = Image.new('RGBA', (W, H), (36, 44, 58, 255))
    d = ImageDraw.Draw(img)

    # zid kaveza
    d.rectangle([0, 0, W, H], fill=(52, 62, 78, 255))
    for i in range(0, W, 60):
        d.rectangle([i, 0, i + 30, H], fill=(46, 55, 70, 255))

    # noc iza resetaka
    d.rectangle([120, 60, W - 120, H - 60], fill=(28, 40, 64, 255))
    d.ellipse([W - 260, 96, W - 190, 166], fill=(238, 236, 200, 255))
    for (cx, cy, r) in [(220, 120, 4), (320, 96, 3), (430, 140, 3),
                        (540, 104, 4), (640, 150, 3), (700, 110, 3)]:
        d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(226, 230, 245, 255))

    # tlo kaveza
    d.rectangle([120, H - 150, W - 120, H - 60], fill=(74, 62, 48, 255))

    # Slavko (isti oblik kao sprite, pojednostavljen)
    bx, by = 430, H - 250
    body, belly, out = (139, 90, 43, 255), (222, 184, 135, 255), (60, 35, 15, 255)
    d.ellipse([bx, by + 46, bx + 96, by + 172], fill=body, outline=out, width=4)
    d.ellipse([bx + 20, by + 78, bx + 76, by + 152], fill=belly)
    d.ellipse([bx + 14, by - 8, bx + 82, by + 60], fill=body, outline=out, width=4)
    d.ellipse([bx + 2, by + 4, bx + 34, by + 36], fill=body, outline=out, width=4)
    d.ellipse([bx + 62, by + 4, bx + 94, by + 36], fill=body, outline=out, width=4)
    d.ellipse([bx + 26, by + 14, bx + 70, by + 52], fill=belly)
    d.ellipse([bx + 34, by + 22, bx + 44, by + 34], fill=(20, 20, 20, 255))
    d.ellipse([bx + 54, by + 22, bx + 64, by + 34], fill=(20, 20, 20, 255))

    # resetke ispred
    for x in range(150, W - 120, 74):
        d.rectangle([x, 40, x + 16, H - 40], fill=(150, 158, 172, 255),
                    outline=(96, 104, 118, 255), width=3)
    d.rectangle([120, 40, W - 120, 62], fill=(150, 158, 172, 255),
                outline=(96, 104, 118, 255), width=3)
    d.rectangle([120, H - 62, W - 120, H - 40], fill=(150, 158, 172, 255),
                outline=(96, 104, 118, 255), width=3)

    save(img, 'intro_scene.png')


print('nova grafika:')
make_hanging_sign()
make_belt()
make_finish_gate()
make_intro()

# Provjera bešavnosti trake. Uzorak se ponavlja svakih 40 px (strelice) i 20 px
# (rubne pruge), pa je spoj ispravan ako je sirina djeljiva s oba perioda i ako
# stupac 0 izgleda jednako kao stupac 40 — tada se slika nastavlja sama na sebe.
b = Image.open(os.path.join(OUT, 'belt.png')).convert('RGBA')
W, H = b.size
per_ok = (W % 40 == 0) and (W % 20 == 0)
col = lambda x: [b.getpixel((x, y)) for y in range(H)]
phase_ok = col(0) == col(40) and col(19) == col(59)
print()
print('traka: sirina', W, 'djeljiva s periodima:', per_ok, ' faza se poklapa:', phase_ok)
print('bešavno po vodoravnoj osi:', per_ok and phase_ok)
