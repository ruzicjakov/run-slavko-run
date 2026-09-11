"""Dijagram arhitekture i dijagram razlika medu razinama.
Veze se crtaju pravokutno (vodoravno/okomito) kroz slobodne prolaze,
jer dijagonale kroz kutije cine sliku necitljivom.
"""
import math
import os
from PIL import Image, ImageDraw, ImageFont

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'izlaz', 'dijagrami')
os.makedirs(OUT, exist_ok=True)
FONTS = ['/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf']
BOLDF = ['/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf']


def F(sz, bold=False):
    for p in (BOLDF if bold else FONTS):
        try:
            return ImageFont.truetype(p, sz)
        except OSError:
            pass
    return ImageFont.load_default()


INK = (28, 33, 44)
MUTED = (108, 116, 132)
LINE = (128, 138, 156)


def ctext(d, cx, cy, t, f, fill=INK, bg=None):
    w = d.textlength(t, font=f)
    a = f.getbbox('Hg')
    h = a[3] - a[1]
    if bg:
        d.rectangle([cx - w / 2 - 6, cy - h / 2 - 5, cx + w / 2 + 6, cy + h / 2 + 5], fill=bg)
    d.text((cx - w / 2, cy - h / 2 - a[1]), t, font=f, fill=fill)


def box(d, x, y, w, h, label, sub=None, fill=(255, 255, 255),
        border=(96, 106, 126), r=10, lab=19, subsz=14):
    d.rounded_rectangle([x, y, x + w, y + h], radius=r, fill=fill, outline=border, width=2)
    if sub:
        ctext(d, x + w / 2, y + h / 2 - 11, label, F(lab, True))
        ctext(d, x + w / 2, y + h / 2 + 12, sub, F(subsz), MUTED)
    else:
        ctext(d, x + w / 2, y + h / 2, label, F(lab, True))


def head(d, x, y, ang, color, size=9):
    d.polygon([(x, y),
               (x - size * math.cos(ang - 0.42), y - size * math.sin(ang - 0.42)),
               (x - size * math.cos(ang + 0.42), y - size * math.sin(ang + 0.42))],
              fill=color)


def route(d, pts, color=LINE, w=2, dashed=False):
    """Pravokutna veza kroz zadane tocke, sa strelicom na kraju."""
    for i in range(len(pts) - 1):
        (x1, y1), (x2, y2) = pts[i], pts[i + 1]
        if dashed:
            n = max(int(math.hypot(x2 - x1, y2 - y1) / 9), 1)
            for k in range(n):
                if k % 2:
                    continue
                a, b = k / n, min((k + 1) / n, 1)
                d.line([x1 + (x2 - x1) * a, y1 + (y2 - y1) * a,
                        x1 + (x2 - x1) * b, y1 + (y2 - y1) * b], fill=color, width=w)
        else:
            d.line([x1, y1, x2, y2], fill=color, width=w)
    (px, py), (qx, qy) = pts[-2], pts[-1]
    head(d, qx, qy, math.atan2(qy - py, qx - px), color)


# ==================== 1. ARHITEKTURA ====================
W, H = 1560, 1040
BG = (250, 251, 253)
img = Image.new('RGB', (W, H), BG)
d = ImageDraw.Draw(img)

GREEN, BLUE, GOLD, RED, PURPLE = ((232, 245, 233), (227, 240, 252),
                                  (253, 246, 224), (253, 233, 233), (240, 236, 250))

ctext(d, W / 2, 34, 'Glavni razredi i njihovi odnosi', F(27, True))

L, R = 120, W - 30          # kutije zive izmedu ovih granica
GA, GB = 50, 84             # slobodni prolazi uz lijevi rub

bands = [(96, 210, 'Upravitelji (jedinstveni primjerci, prežive promjenu scene)'),
         (290, 404, 'Korisničko sučelje'),
         (516, 630, 'Igrač i protivnik'),
         (712, 946, 'Okolina, prepreke i predmeti')]
for (y0, y1, title) in bands:
    d.rounded_rectangle([L - 14, y0, R, y1], radius=14, outline=(212, 218, 228), width=2)
    d.text((L - 2, y0 + 7), title, font=F(14, True), fill=MUTED)

# --- upravitelji ---
box(d, L, 128, 236, 74, 'GameManager', 'tijek igre, spremanje', GREEN, lab=18)
box(d, L + 262, 128, 226, 74, 'AudioManager', 'glazba i učinci', GREEN, lab=18)
box(d, L + 514, 128, 200, 74, 'LevelMusic', 'podloga scene', GREEN, lab=18)
box(d, L + 740, 128, 214, 74, 'LevelManager', 'parametri razine', GREEN, lab=18)
box(d, L + 980, 128, 280, 74, 'MainMenuController', 'izbornik, uvod, zasluge', GREEN, lab=18)

# --- sucelje ---
box(d, L, 322, 236, 74, 'UIManager', 'srca, trake, zasloni', BLUE, lab=18)
box(d, L + 346, 322, 200, 74, 'PauseMenu', 'Esc', BLUE, lab=18)
box(d, L + 656, 322, 214, 74, 'VictoryAnimation', 'konfeti', BLUE, lab=16)
box(d, L + 980, 322, 280, 74, 'CameraFollow', 'praćenje igrača', BLUE, lab=18)

# --- igrac ---
box(d, L + 60, 542, 300, 74, 'PlayerController', 'trčanje, skok, njihanje', GOLD, lab=20)
box(d, L + 480, 542, 280, 74, 'PlayerHealth', 'životi, posrtanje', GOLD, lab=20)
box(d, L + 900, 542, 260, 74, 'GuardAI', 'potjera, nalet', RED, lab=20)

# --- okolina ---
r1 = 748
box(d, L, r1, 190, 66, 'Obstacle', 'prepreka', PURPLE, lab=17, subsz=13)
box(d, L + 206, r1, 214, 66, 'BreakableObstacle', 'lomljiva', PURPLE, lab=15, subsz=13)
box(d, L + 436, r1, 200, 66, 'MovingObstacle', 'njiše se', PURPLE, lab=16, subsz=13)
box(d, L + 652, r1, 190, 66, 'HangPoint', 'točka vješanja', PURPLE, lab=17, subsz=13)
box(d, L + 858, r1, 196, 66, 'ConveyorBelt', 'pokretna traka', PURPLE, lab=16, subsz=13)
box(d, L + 1070, r1, 190, 66, 'PowerUpBase', 'Kokos, Ape, Boost', PURPLE, lab=15, subsz=12)

r2 = 838
box(d, L, r2, 190, 66, 'PitDeath', 'dno jame', PURPLE, lab=17, subsz=13)
box(d, L + 206, r2, 214, 66, 'FinishLine', 'cilj razine', PURPLE, lab=17, subsz=13)
box(d, L + 436, r2, 200, 66, 'Checkpoint', 'kontrolna točka', PURPLE, lab=16, subsz=13)
box(d, L + 652, r2, 190, 66, 'ParallaxBackground', 'dubina', PURPLE, lab=13, subsz=13)

# ---------------- veze ----------------
GM_Y, UI_Y = 165, 359
PH_Y = 579
PC_C, PH_C = L + 210, L + 620

# PlayerHealth -> UIManager (gornji prolaz, pa uz lijevi rub)
route(d, [(L + 520, 540), (L + 520, 462), (GB, 462), (GB, UI_Y), (L - 2, UI_Y)])
ctext(d, L + 130, 462, 'UpdateLives()', F(15, True), (60, 116, 180), bg=BG)

# PlayerHealth -> GameManager
route(d, [(L + 700, 540), (L + 700, 494), (GA, 494), (GA, GM_Y), (L - 2, GM_Y)])
ctext(d, L + 130, 494, 'GameOver()', F(15, True), (76, 132, 92), bg=BG)

# GuardAI -> PlayerHealth
route(d, [(L + 898, PH_Y), (L + 762, PH_Y)], color=(186, 120, 120))
ctext(d, L + 830, PH_Y - 24, 'TakeHit()', F(15, True), (170, 90, 90), bg=BG)

# okolina -> igrac (skupne veze kroz donji prolaz)
route(d, [(PC_C, r1 - 6), (PC_C, 620)])
ctext(d, PC_C + 186, 672, 'HangPoint, ConveyorBelt i predmeti mijenjaju kretanje',
      F(15, True), (110, 100, 150), bg=BG)
route(d, [(PH_C, r1 - 6), (PH_C, 620)])
ctext(d, PH_C + 110, 700, 'prepreke: Stumble()', F(15, True), (110, 100, 150), bg=BG)

# UIManager -> GameManager
route(d, [(L + 190, 320), (L + 190, 204)])

# LevelMusic -> AudioManager
route(d, [(L + 514, 165), (L + 490, 165)])

# MainMenuController -> GameManager
route(d, [(L + 1120, 126), (L + 1120, 108), (L + 118, 108), (L + 118, 126)])
ctext(d, L + 900, 108, 'StartGame() / ContinueGame()', F(15, True), (76, 132, 92), bg=BG)

# napomena o zavrsetku razine
ctext(d, (L + R) / 2 - 40, 922,
      'PitDeath i FinishLine završavaju razinu pozivom preko GameManager.Instance',
      F(15), MUTED)

# legenda
d.rounded_rectangle([L - 14, 970, R, 1016], radius=8, fill=(244, 246, 250),
                    outline=(214, 219, 228))
ctext(d, (L + R) / 2, 993,
      'Strelica pokazuje smjer poziva: od razreda koji poziva prema onome čija se metoda poziva',
      F(16), MUTED)

img.save(os.path.join(OUT, 'dijagram_arhitektura.png'))
print('dijagram_arhitektura.png', img.size)


# ==================== 2. RAZLIKE MEDU RAZINAMA ====================
W, H = 1560, 570
img = Image.new('RGB', (W, H), BG)
d = ImageDraw.Draw(img)
ctext(d, W / 2, 38, 'Što svaka razina donosi novo', F(27, True))

COLS = [
    ('1. Zoo', (232, 245, 233), '6,0 j/s', [
        ('skok preko prepreke', 1), ('njihanje na užetu', 1),
        ('uske i široke jame', 1), ('kokos, PowerApe, Boost', 1),
    ], 'uvodi osnove'),
    ('2. Grad', (227, 240, 252), '6,5 j/s', [
        ('viseća prepreka', 1), ('skok se mora zadržati', 0),
        ('prepreka koja se njiše', 1), ('amplituda 2 j, ciklus 2,5 s', 0),
    ], 'traži i zadržavanje skoka'),
    ('3. Šuma', (236, 245, 232), '7,0 j/s', [
        ('lančano njihanje', 1), ('jama 11 j, dvije točke', 0),
        ('brže pokretne prepreke', 1), ('amplituda 2,5 j, ciklus 2 s', 0),
    ], 'spaja sve i pooštrava'),
    ('4. Tvornica', (253, 240, 230), '7,5 j/s', [
        ('pokretne trake', 1), ('faktor 0,85 i 1,25', 0),
        ('čuvar u naletu', 1), ('8,6 j/s svakih 9 s', 0),
    ], 'prva pogreška se ne oprašta'),
]

x0, gap = 44, 18
cw = (W - 2 * x0 - 3 * gap) / 4.0
for i, (name, col, speed, items, note) in enumerate(COLS):
    x = x0 + i * (cw + gap)
    d.rounded_rectangle([x, 84, x + cw, 470], radius=14, fill=col,
                        outline=(182, 192, 206), width=2)
    ctext(d, x + cw / 2, 120, name, F(23, True))
    ctext(d, x + cw / 2, 152, 'brzina ' + speed, F(15), MUTED)
    d.line([x + 24, 178, x + cw - 24, 178], fill=(192, 200, 214), width=2)
    y = 212
    for (txt, strong) in items:
        pre = ('+ ' if i > 0 else '') if strong else ''
        ctext(d, x + cw / 2, y, pre + txt, F(17 if strong else 13, bool(strong)),
              INK if strong else MUTED)
        y += 32 if strong else 28
    d.rounded_rectangle([x + 18, 390, x + cw - 18, 446], radius=8,
                        fill=(255, 255, 255), outline=(202, 210, 222))
    ctext(d, x + cw / 2, 418, note, F(14, True), (90, 100, 120))
    if i < 3:
        route(d, [(x + cw + 2, 275), (x + cw + gap - 2, 275)], color=(150, 158, 174), w=3)

d.rounded_rectangle([44, 496, W - 44, 550], radius=10, fill=(244, 246, 250),
                    outline=(214, 219, 228))
ctext(d, W / 2, 523,
      'Svaka razina zadržava sve prethodne mehanike i dodaje barem jednu novu, '
      'a s brzinom trčanja raste i doseg skoka',
      F(16), MUTED)

img.save(os.path.join(OUT, 'dijagram_razine.png'))
print('dijagram_razine.png', img.size)
