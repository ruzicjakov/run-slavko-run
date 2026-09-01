"""Dijagrami za zavrsni rad: tok scena i anatomija segmenta razine."""
from PIL import Image, ImageDraw, ImageFont

import os
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'izlaz')
os.makedirs(OUT, exist_ok=True)

FONTS = ['/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',
         '/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf']
BOLD = ['/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',
        '/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf']


def F(sz, bold=False):
    for p in (BOLD if bold else FONTS):
        try:
            return ImageFont.truetype(p, sz)
        except OSError:
            pass
    return ImageFont.load_default()


INK = (28, 33, 44)
MUTED = (108, 116, 132)
LINE = (170, 178, 192)


def ctext(d, cx, cy, t, f, fill=INK):
    w = d.textlength(t, font=f)
    a = f.getbbox('Hg')
    d.text((cx - w / 2, cy - (a[3] - a[1]) / 2 - a[1]), t, font=f, fill=fill)


def box(d, x, y, w, h, label, sub=None, fill=(255, 255, 255), border=(90, 100, 120), r=10):
    d.rounded_rectangle([x, y, x + w, y + h], radius=r, fill=fill, outline=border, width=2)
    if sub:
        ctext(d, x + w / 2, y + h / 2 - 12, label, F(19, True))
        ctext(d, x + w / 2, y + h / 2 + 13, sub, F(15), MUTED)
    else:
        ctext(d, x + w / 2, y + h / 2, label, F(19, True))


def arrow(d, x1, y1, x2, y2, color=(90, 100, 120), w=2, head=9):
    d.line([x1, y1, x2, y2], fill=color, width=w)
    import math
    a = math.atan2(y2 - y1, x2 - x1)
    d.polygon([(x2, y2),
               (x2 - head * math.cos(a - 0.42), y2 - head * math.sin(a - 0.42)),
               (x2 - head * math.cos(a + 0.42), y2 - head * math.sin(a + 0.42))], fill=color)


# ==================== 1. TOK SCENA ====================
W, H = 1500, 640
img = Image.new('RGB', (W, H), (250, 251, 253))
d = ImageDraw.Draw(img)

GREEN, BLUE, GOLD, RED = (232, 245, 233), (227, 240, 252), (253, 246, 224), (253, 233, 233)

box(d, 40, 250, 210, 92, 'MainMenu', 'Igraj / Nastavi / Izadi', GREEN)

lv = [('Razina 1', 'Zoo'), ('Razina 2', 'Grad'), ('Razina 3', 'Suma'), ('Razina 4', 'Tvornica')]
xs = [310, 545, 780, 1015]
for (x, (t, s)) in zip(xs, lv):
    box(d, x, 250, 190, 92, t, s, BLUE)

box(d, 1250, 250, 210, 92, 'Pobjeda', 'konfeti + naslov', GOLD)

arrow(d, 250, 296, 305, 296)
for i in range(3):
    arrow(d, xs[i] + 190, 296, xs[i + 1] - 5, 296)
arrow(d, xs[3] + 190, 296, 1245, 296)

# oznake iznad strelica
ctext(d, 278, 274, 'start', F(13), MUTED)
for i in range(3):
    ctext(d, (xs[i] + 190 + xs[i + 1]) / 2, 274, 'cilj', F(13), MUTED)
ctext(d, (xs[3] + 190 + 1250) / 2, 274, 'cilj', F(13), MUTED)

# Game Over ispod
box(d, 545, 440, 190, 78, 'Game Over', 'Retry / Izbornik', RED)
arrow(d, 640, 342, 640, 435, (198, 100, 100))
ctext(d, 700, 390, '0 zivota', F(13), (170, 90, 90))
arrow(d, 545, 479, 250, 479, (198, 100, 100))
arrow(d, 250, 479, 145, 342, (198, 100, 100))
ctext(d, 380, 462, 'Glavni izbornik', F(13), (170, 90, 90))

# Pauza iznad
box(d, 780, 100, 190, 78, 'Pauza (Esc)', 'Nastavi / Ponovi', (243, 240, 252))
arrow(d, 875, 250, 875, 182, (130, 118, 180))
arrow(d, 780, 139, 145, 139, (130, 118, 180))
arrow(d, 145, 139, 145, 245, (130, 118, 180))
ctext(d, 460, 122, 'Glavni izbornik', F(13), (120, 110, 165))

# napomena o spremanju
d.rounded_rectangle([310, 560, 1205, 610], radius=8, fill=(244, 246, 250), outline=LINE)
ctext(d, 757, 585,
      'Nakon svake predene razine GameManager sprema indeks u PlayerPrefs  '
      '->  gumb "Nastavi" u izborniku', F(16), MUTED)

ctext(d, W / 2, 40, 'Tok scena i stanja igre', F(26, True))
img.save(OUT + '/dijagram_tok.png')
print('dijagram_tok.png', img.size)


# ==================== 2. ANATOMIJA SEGMENTA ====================
_F0 = F
def F(sz, bold=False):
    return _F0(int(sz * 1.55), bold)

W, H = 1500, 700
img = Image.new('RGB', (W, H), (250, 251, 253))
d = ImageDraw.Draw(img)

ctext(d, W / 2, 40, 'Anatomija segmenta razine s jamom (28 jedinica)', F(26, True))

# skala: 28 jedinica -> 1300 px
X0, SCALE = 100, 1300 / 28.0
GY = 430                      # razina tla na dijagramu


def ux(u):
    return X0 + u * SCALE


# tlo lijevo i desno, jama u sredini (uska jama: 6 jedinica, 11..17)
d.rectangle([ux(0), GY, ux(11), GY + 70], fill=(146, 108, 70))
d.rectangle([ux(0), GY, ux(11), GY + 14], fill=(124, 186, 96))
d.rectangle([ux(17), GY, ux(28), GY + 70], fill=(146, 108, 70))
d.rectangle([ux(17), GY, ux(28), GY + 14], fill=(124, 186, 96))

# jama
d.rectangle([ux(11), GY, ux(17), GY + 70], fill=(238, 240, 244))
for i in range(int(ux(11)), int(ux(17)), 14):
    d.line([i, GY + 70, i + 8, GY + 62], fill=(206, 210, 220), width=2)
d.rectangle([ux(11), GY + 118, ux(17), GY + 150], fill=(252, 226, 226), outline=(214, 130, 130))
ctext(d, ux(14), GY + 134, 'PitDeathZone (trigger)', F(14), (176, 84, 84))

# prepreka na +3
d.rectangle([ux(3) - 10, GY - 42, ux(3) + 10, GY], fill=(150, 96, 60), outline=(96, 62, 38), width=2)
ctext(d, ux(3), GY - 62, 'prepreka', F(14), MUTED)

# hangpoint na +12.8, visina 3.0 (iznad tla)
HPX, HPY = ux(12.8), GY - 200
d.ellipse([HPX - 14, HPY - 14, HPX + 14, HPY + 14], fill=(122, 84, 52), outline=(78, 52, 32), width=2)
d.ellipse([HPX - 92, HPY - 92, HPX + 92, HPY + 92], outline=(214, 160, 90), width=2)
ctext(d, HPX + 150, HPY - 60, 'zona hvatanja r = 1.8', F(14), (176, 132, 66))
ctext(d, HPX + 30, HPY - 128, 'HangPoint (y = 3.0)', F(15, True), (140, 96, 50))

# --- luk skoka S RUBA JAME: pada u jamu ---
import math
pts = []
t = 0.0
while True:
    xu = 11 + 6 * t
    yu = 9 * t - 0.5 * 29.43 * t * t
    if yu < -1.6:
        break
    pts.append((ux(xu), GY - yu * 34))
    t += 0.008
d.line(pts, fill=(70, 130, 200), width=3)
land = pts[-1]
d.line([land[0] - 12, land[1] - 12, land[0] + 12, land[1] + 12], fill=(200, 70, 70), width=3)
d.line([land[0] + 12, land[1] - 12, land[0] - 12, land[1] + 12], fill=(200, 70, 70), width=3)
ctext(d, ux(6.6), GY - 118, 'sam skok s ruba jame:', F(15, True), (60, 116, 180))
ctext(d, ux(6.6), GY - 96, 'doseg 3.67 j  <  jama 6 j', F(15, True), (60, 116, 180))
ctext(d, ux(6.6), GY - 74, 'pad u jamu', F(15, True), (60, 116, 180))

# --- putanja sa zamahom: hvat, njihaj, izbacaj, doskok na desno tlo ---
pts2 = []
for i in range(26):                       # njihaj oko tocke vjesanja
    a = math.radians(-38 + i * 2.6)
    L = 1.55
    pts2.append((ux(12.8 + L * math.sin(a)), GY - (3.0 - L * math.cos(a)) * 34))
rx, ry = 12.8 + 1.55 * math.sin(math.radians(27)), 3.0 - 1.55 * math.cos(math.radians(27))
t = 0.0
while True:                                # let nakon otpustanja
    xu = rx + 6.9 * t
    yu = ry + 7.0 * t - 0.5 * 29.43 * t * t
    if yu < 0.05:
        break
    pts2.append((ux(xu), GY - yu * 34))
    t += 0.008
d.line(pts2, fill=(214, 128, 60), width=3)
ok = pts2[-1]
d.ellipse([ok[0] - 9, ok[1] - 9, ok[0] + 9, ok[1] + 9], outline=(60, 150, 80), width=3)
ctext(d, ux(22.4), GY - 178, 'sa zamahom: dolet 4.3+ j', F(15, True), (196, 116, 52))
ctext(d, ux(22.4), GY - 156, 'prelazi jamu', F(15, True), (196, 116, 52))

# kote
def kota(x1, x2, y, label):
    d.line([x1, y, x2, y], fill=INK, width=2)
    for xx in (x1, x2):
        d.line([xx, y - 7, xx, y + 7], fill=INK, width=2)
    ctext(d, (x1 + x2) / 2, y - 18, label, F(15, True))


kota(ux(11), ux(17), GY + 190, 'jama 6 jedinica')
kota(ux(0), ux(28), GY + 232, 'segment 28 jedinica')

# legenda
d.text((ux(0), 96), 'Jama je sira od dosega skoka,', font=F(15), fill=INK)
d.text((ux(0), 126), 'pa je zamah jedini nacin prelaska.', font=F(15), fill=INK)

img.save(OUT + '/dijagram_segment.png')
print('dijagram_segment.png', img.size)
