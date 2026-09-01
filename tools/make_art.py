"""Generira flat-style tileable tlo i parallax pozadine za Run, Slavko, Run!
100 px = 1 Unity jedinica. Kamera vidi y od -6 do 8 (14 jedinica, ortho size 7).
Linija tla je na y = -0.76  ->  876 px od vrha slike visoke 1400 px.
"""
import math, os
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'izlaz', 'art')
BG_W, BG_H = 1000, 1400          # 10 x 14 jedinica (kamera: ortho 7)
GROUND_ROW = 876                  # y = -0.76; kamera vidi y od -6 do 8
TILE_W, TILE_H = 200, 100         # 2 x 1 jedinica


def lerp(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(3))


def vgrad(draw, x0, y0, x1, y1, top, bot):
    for y in range(y0, y1):
        t = (y - y0) / max(1, (y1 - y0 - 1))
        draw.line([(x0, y), (x1, y)], fill=lerp(top, bot, t))


def wrap_hills(d, w, base_y, amp, period, color, phase=0.0, seed_mix=1.0):
    """Bešavni brežuljci: period mora stati cijeli broj puta u širinu."""
    cycles = max(1, round(w / period))
    pts = []
    for x in range(w + 1):
        a = 2 * math.pi * cycles * x / w
        y = base_y - amp * (0.5 + 0.5 * math.sin(a + phase))
        y -= amp * 0.35 * seed_mix * (0.5 + 0.5 * math.sin(2 * a + phase * 1.7))
        pts.append((x, y))
    d.polygon(pts + [(w, BG_H), (0, BG_H)], fill=color)


def rects_wrapped(d, w, items, color):
    """Crta pravokutnike i njihove kopije preko ruba, da tiling bude bešavan."""
    for (x, y, bw, bh) in items:
        for off in (-w, 0, w):
            d.rectangle([x + off, y, x + off + bw, y + bh], fill=color)


# ---------------------------------------------------------------- pozadine
def bg_far(theme):
    img = Image.new('RGBA', (BG_W, BG_H))
    d = ImageDraw.Draw(img)
    P = THEMES[theme]
    vgrad(d, 0, 0, BG_W, GROUND_ROW, P['sky_top'], P['sky_bot'])

    # oblaci — nebo je sad visoko pa bi inace bilo prazno
    if theme in ('Zoo', 'City'):
        cl = Image.new('RGBA', (BG_W, BG_H), (0, 0, 0, 0))
        dc = ImageDraw.Draw(cl)
        for (cx, cy, sc, al) in [(180, 150, 1.15, 190), (620, 90, 0.85, 150),
                                  (880, 230, 1.0, 165)]:
            for offx in (-BG_W, 0, BG_W):
                for (ox, oy, r) in [(-64, 8, 40), (-18, -12, 52), (34, 4, 44), (84, 14, 33)]:
                    d0 = (cx + offx + ox * sc, cy + oy * sc)
                    dc.ellipse([d0[0] - r * sc, d0[1] - r * sc,
                                d0[0] + r * sc, d0[1] + r * sc], fill=(255, 255, 255, al))
        img.alpha_composite(cl)

    if theme == 'Zoo':
        wrap_hills(d, BG_W, GROUND_ROW + 6, 150, 500, P['far1'], 0.0)
        wrap_hills(d, BG_W, GROUND_ROW + 6, 95, 250, P['far2'], 1.1, 0.6)
    elif theme == 'City':
        # bešavna silueta nebodera
        xs = [0, 70, 150, 210, 300, 360, 430, 520, 600, 660, 740, 830, 900, 960]
        hs = [230, 320, 180, 400, 260, 350, 200, 300, 380, 220, 330, 190, 280, 230]
        items = [(xs[i], GROUND_ROW - hs[i], (xs[i + 1] - xs[i]) - 8 if i + 1 < len(xs) else 40, hs[i])
                 for i in range(len(xs))]
        rects_wrapped(d, BG_W, items, P['far1'])
        for (x, y, bw, bh) in items:           # prozori
            for wy in range(y + 20, y + bh - 15, 34):
                for wx in range(x + 12, x + bw - 10, 26):
                    d.rectangle([wx, wy, wx + 11, wy + 16], fill=P['far2'])
    elif theme == 'Forest':
        wrap_hills(d, BG_W, GROUND_ROW + 6, 170, 1000, P['far1'], 0.4)
        # puni pojas šume sa zupčastim vrhom — bez rupa kroz koje se vidi nebo
        n = 25
        pts = []
        for i in range(n + 1):
            x = i * BG_W / n
            h = 150 + 55 * math.sin(i * 1.7) + 30 * math.sin(i * 0.6)
            pts += [(x - BG_W / (2 * n), GROUND_ROW - h * 0.45), (x, GROUND_ROW - h)]
        d.polygon(pts + [(BG_W, GROUND_ROW), (0, GROUND_ROW)], fill=P['far2'])
    else:  # Factory
        items = [(60, GROUND_ROW - 330, 46, 330), (250, GROUND_ROW - 420, 52, 420),
                 (470, GROUND_ROW - 300, 44, 300), (700, GROUND_ROW - 390, 50, 390),
                 (880, GROUND_ROW - 270, 42, 270)]
        rects_wrapped(d, BG_W, items, P['far1'])
        hall = [(0, GROUND_ROW - 120, 1000, 120)]
        rects_wrapped(d, BG_W, hall, P['far2'])

    # --- podzemlje: slojevi i kamenje, da 5+ jedinica ispod tla ne bude prazna ploha
    d.rectangle([0, GROUND_ROW, BG_W, BG_H], fill=P['earth'])
    e = P['earth']
    dark = tuple(max(0, ch - 14) for ch in e)
    darker = tuple(max(0, ch - 26) for ch in e)

    # valoviti slojevi
    for (off, col, amp, per, ph) in [(150, dark, 26, 500, 0.0), (330, darker, 34, 1000, 1.3)]:
        cycles = max(1, round(BG_W / per))     # cijeli broj ciklusa => bešavan spoj
        pts = []
        for x in range(BG_W + 1):
            a = 2 * math.pi * cycles * x / BG_W
            pts.append((x, GROUND_ROW + off + amp * math.sin(a + ph)))
        d.polygon(pts + [(BG_W, BG_H), (0, BG_H)], fill=col)

    # kamenje razbacano po dubini (bešavno preko ruba)
    stones = [(70, 90, 26), (300, 60, 18), (520, 130, 30), (760, 80, 21),
              (150, 250, 23), (430, 300, 32), (680, 240, 19), (900, 320, 27),
              (240, 430, 25), (600, 460, 21), (860, 410, 30), (60, 380, 17)]
    for (sx, sy, r) in stones:
        for offx in (-BG_W, 0, BG_W):
            cx, cy = sx + offx, GROUND_ROW + sy
            if cy > BG_H:
                continue
            d.ellipse([cx - r, cy - r * 0.72, cx + r, cy + r * 0.72], fill=darker)
            d.ellipse([cx - r * 0.6, cy - r * 0.6, cx + r * 0.2, cy - r * 0.05], fill=dark)
    return img


def bg_mid(theme):
    img = Image.new('RGBA', (BG_W, BG_H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    P = THEMES[theme]
    if theme == 'Zoo':
        for i in range(5):                        # palme, varirane visine
            x = 100 + i * 200
            th = 175 + 55 * math.sin(i * 2.1)
            for off in (-BG_W, 0, BG_W):
                d.rectangle([x + off - 9, GROUND_ROW - th, x + off + 9, GROUND_ROW], fill=P['mid2'])
                for k in range(6):
                    a = math.pi * (0.15 + 0.7 * k / 5)
                    d.polygon([(x + off, GROUND_ROW - th),
                               (x + off + 95 * math.cos(a), GROUND_ROW - th - 62 * math.sin(a)),
                               (x + off + 30 * math.cos(a), GROUND_ROW - th + 24)], fill=P['mid1'])
    elif theme == 'City':
        items = [(90, GROUND_ROW - 200, 150, 200), (400, GROUND_ROW - 250, 170, 250),
                 (720, GROUND_ROW - 175, 160, 175)]
        rects_wrapped(d, BG_W, items, P['mid1'])
        for (x, y, bw, bh) in items:
            for wy in range(y + 18, y + bh - 14, 40):
                for wx in range(x + 16, x + bw - 14, 34):
                    for off in (-BG_W, 0, BG_W):
                        d.rectangle([wx + off, wy, wx + off + 15, wy + 22], fill=P['mid2'])
    elif theme == 'Forest':
        for i in range(6):
            x = 80 + i * 165
            th = 225 + 60 * math.sin(i * 1.9)
            for off in (-BG_W, 0, BG_W):
                d.rectangle([x + off - 13, GROUND_ROW - th, x + off + 13, GROUND_ROW], fill=P['mid2'])
                for k in range(3):
                    ry = GROUND_ROW - th - k * 55
                    rw = 105 - k * 22
                    d.polygon([(x + off - rw, ry + 60), (x + off, ry - 40),
                               (x + off + rw, ry + 60)], fill=P['mid1'])
    else:
        for i in range(4):                        # cijevi, rjeđe i varirane
            x = 120 + i * 250
            th = 150 + 55 * math.sin(i * 2.4)
            for off in (-BG_W, 0, BG_W):
                d.rectangle([x + off - 16, GROUND_ROW - th, x + off + 16, GROUND_ROW], fill=P['mid1'])
                d.rectangle([x + off - 24, GROUND_ROW - th - 15, x + off + 24, GROUND_ROW - th + 5],
                            fill=P['mid2'])
    return img


def bg_near(theme):
    img = Image.new('RGBA', (BG_W, BG_H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    P = THEMES[theme]
    if theme in ('Zoo', 'Forest'):
        for i in range(14):                       # grmlje uz tlo
            x = 40 + i * 72
            r = 34 + 14 * math.sin(i * 2.3)
            for off in (-BG_W, 0, BG_W):
                d.ellipse([x + off - r, GROUND_ROW - r, x + off + r, GROUND_ROW + r * 0.7], fill=P['near1'])
    elif theme == 'City':
        for i in range(9):                        # ograda + rasvjeta
            x = 55 + i * 112
            for off in (-BG_W, 0, BG_W):
                d.rectangle([x + off - 5, GROUND_ROW - 120, x + off + 5, GROUND_ROW], fill=P['near1'])
        d.rectangle([0, GROUND_ROW - 122, BG_W, GROUND_ROW - 112], fill=P['near1'])
    else:
        for i in range(4):                        # bačve
            x = 90 + i * 250
            for off in (-BG_W, 0, BG_W):
                d.rectangle([x + off - 26, GROUND_ROW - 74, x + off + 26, GROUND_ROW], fill=P['near1'])
                d.rectangle([x + off - 26, GROUND_ROW - 52, x + off + 26, GROUND_ROW - 44], fill=P['near2'])
    return img


# ------------------------------------------------------------------- tlo
def ground_tile(theme):
    img = Image.new('RGBA', (TILE_W, TILE_H))
    d = ImageDraw.Draw(img)
    P = THEMES[theme]
    top, body, dark = P['g_top'], P['g_body'], P['g_dark']
    d.rectangle([0, 0, TILE_W, TILE_H], fill=body)
    d.rectangle([0, 0, TILE_W, 20], fill=top)
    d.rectangle([0, 20, TILE_W, 25], fill=dark)

    if theme in ('Zoo', 'Forest'):
        for i in range(0, TILE_W, 14):            # travke
            h = 8 + int(5 * math.sin(i * 0.9))
            d.polygon([(i, 20), (i + 5, 20 - h), (i + 9, 20)], fill=top)
        for (cx, cy, r) in [(38, 58, 9), (120, 74, 7), (168, 48, 8), (78, 86, 6)]:
            d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=dark)
    elif theme == 'City':
        for x in (0, 100, 200):                   # spojnice ploča
            d.rectangle([x - 2, 0, x + 2, TILE_H], fill=dark)
        d.rectangle([0, 55, TILE_W, 59], fill=dark)
    else:
        for x in (0, 100, 200):
            d.rectangle([x - 3, 0, x + 3, TILE_H], fill=dark)
        for cx in (25, 75, 125, 175):             # zakovice
            for cy in (40, 78):
                d.ellipse([cx - 4, cy - 4, cx + 4, cy + 4], fill=dark)
    return img


THEMES = {
    'Zoo': dict(sky_top=(122, 178, 232), sky_bot=(196, 224, 236), far1=(120, 176, 116),
                far2=(96, 152, 96), earth=(104, 76, 50), mid1=(72, 148, 88), mid2=(126, 96, 62),
                near1=(66, 130, 74), near2=(52, 106, 60),
                g_top=(124, 186, 96), g_body=(146, 108, 70), g_dark=(112, 80, 52)),
    'City': dict(sky_top=(96, 116, 148), sky_bot=(178, 190, 204), far1=(86, 98, 122),
                 far2=(150, 166, 190), earth=(58, 62, 72), mid1=(66, 76, 96), mid2=(190, 200, 152),
                 near1=(50, 58, 74), near2=(40, 48, 62),
                 g_top=(120, 126, 136), g_body=(86, 90, 100), g_dark=(62, 66, 76)),
    'Forest': dict(sky_top=(96, 148, 116), sky_bot=(178, 208, 168), far1=(58, 104, 72),
                   far2=(44, 86, 60), earth=(58, 44, 30), mid1=(38, 92, 58), mid2=(84, 62, 42),
                   near1=(30, 72, 46), near2=(24, 58, 38),
                   g_top=(74, 132, 76), g_body=(88, 66, 46), g_dark=(62, 46, 32)),
    'Factory': dict(sky_top=(150, 106, 74), sky_bot=(206, 168, 130), far1=(92, 78, 72),
                    far2=(74, 64, 60), earth=(46, 46, 54), mid1=(104, 94, 88), mid2=(140, 96, 54),
                    near1=(150, 96, 48), near2=(92, 60, 34),
                    g_top=(122, 122, 128), g_body=(80, 80, 88), g_dark=(54, 54, 62)),
}

os.makedirs(OUT, exist_ok=True)
made = []
for theme in THEMES:
    for name, fn in [('bg_far', bg_far), ('bg_mid', bg_mid), ('bg_near', bg_near)]:
        p = f'{OUT}/{theme}_{name}.png'
        fn(theme).save(p)
        made.append(p)
    p = f'{OUT}/{theme}_ground.png'
    ground_tile(theme).save(p)
    made.append(p)

print(f"generirano {len(made)} slika")
for m in made:
    print(' ', os.path.basename(m), Image.open(m).size)
