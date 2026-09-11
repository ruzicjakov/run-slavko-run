"""Dimenzioniranje siroke jame s dvije tocke vjesanja (razina 3 - Suma).

Trazi se sirina jame i razmak tocaka takvi da:
  - jedan zamah s prve tocke NE prelazi jamu (inace druga tocka nema smisla),
  - lanac dva zamaha ju prelazi, i to s razumnim prozorom za pogresku.

Model prati PlayerController korak po korak, istim vremenskim korakom kao Unity.
"""
import math

DT = 0.02
G = 3 * 9.81                 # gravityScale 3
JUMP = 9.0
GROUND_TOP = -0.76
CAP_H, CAP_W = 1.6513135, 1.181855
CAP_OFF_Y = 0.017562509
HALF_AXIS = (CAP_H - CAP_W) / 2.0        # 0.2347
CAP_R = CAP_W / 2.0                      # 0.5909
STAND_Y = GROUND_TOP + CAP_H / 2.0 - CAP_OFF_Y

HP_Y = 3.0
ZONE_R = 1.8
MIN_ROPE, MAX_ROPE = 1.2, 3.0
SWING_F = 14.0
REL_BOOST, REL_UP = 1.15, 7.0
MAX_HANG = 2.0


def in_zone(px, py, hx):
    """Preklapa li kapsula igraca krug zone hvatanja oko (hx, HP_Y)."""
    cx, cy = px + 0.008981526, py + CAP_OFF_Y
    dy = abs(HP_Y - cy) - HALF_AXIS
    if dy < 0:
        dy = 0.0
    d = math.hypot(hx - cx, dy)
    return d <= ZONE_R + CAP_R


def swing(px, py, vx, vy, hx, release_t):
    """Njihanje na tocki (hx, HP_Y); vraca stanje u trenutku otpustanja."""
    L = min(max(math.hypot(px - hx, py - HP_Y), MIN_ROPE), MAX_ROPE)
    t = 0.0
    while t < release_t:
        vx += SWING_F * DT
        vy -= G * DT
        px += vx * DT
        py += vy * DT
        # kruta veza: vrati na krug i ukloni radijalnu komponentu brzine
        dx, dy = px - hx, py - HP_Y
        d = math.hypot(dx, dy) or 1e-9
        px, py = hx + dx / d * L, HP_Y + dy / d * L
        nx, ny = dx / d, dy / d
        rad = vx * nx + vy * ny
        vx -= rad * nx
        vy -= rad * ny
        t += DT
    return px, py, vx, vy


def launch(vx, vy, run):
    return max(abs(vx), run) * REL_BOOST, max(vy, REL_UP)


def fly(px, py, vx, vy, until_x=None, hx2=None, t_max=3.0):
    """Slobodan let. Vraca (sletio_na_tlo_x, presao_kroz_zonu2 stanje)."""
    t = 0.0
    grab2 = None
    while t < t_max:
        vy -= G * DT
        px += vx * DT
        py += vy * DT
        t += DT
        if hx2 is not None and grab2 is None and in_zone(px, py, hx2):
            grab2 = (px, py, vx, vy, t)
        if py <= STAND_Y and vy < 0:
            return px, py, t, grab2
    return px, py, t, grab2


def jump_to_hp1(run, edge_x, hx1):
    """Skok s ruba jame; vraca stanje u trenutku ulaska u zonu prve tocke."""
    px, py, vx, vy = edge_x, STAND_Y, run, JUMP
    t = 0.0
    while t < 1.2:
        vy -= G * DT
        px += vx * DT
        py += vy * DT
        t += DT
        if in_zone(px, py, hx1):
            return px, py, vx, vy, t
    return None


def best_single(run, edge_x, hx1):
    """Najdalji doskok koji se moze postici JEDNIM zamahom s prve tocke."""
    st = jump_to_hp1(run, edge_x, hx1)
    if st is None:
        return None
    px0, py0, vx0, vy0, _ = st
    best = -1e9
    for i in range(1, int(MAX_HANG / DT)):
        px, py, vx, vy = swing(px0, py0, vx0, vy0, hx1, i * DT)
        lx, ly, _, _ = fly(px, py, *launch(vx, vy, run))
        best = max(best, lx)
    return best


def best_chain(run, edge_x, hx1, hx2):
    """Najdalji doskok lancem dva zamaha + koliko trenutaka otpustanja vodi do lanca."""
    st = jump_to_hp1(run, edge_x, hx1)
    if st is None:
        return None, 0, 0
    px0, py0, vx0, vy0, _ = st
    best = -1e9
    ok_release = 0
    total = 0
    for i in range(1, int(MAX_HANG / DT)):
        total += 1
        px, py, vx, vy = swing(px0, py0, vx0, vy0, hx1, i * DT)
        lvx, lvy = launch(vx, vy, run)
        _, _, _, g2 = fly(px, py, lvx, lvy, hx2=hx2)
        if g2 is None:
            continue
        ok_release += 1
        gx, gy, gvx, gvy, _ = g2
        for j in range(1, int(MAX_HANG / DT)):
            px2, py2, vx2, vy2 = swing(gx, gy, gvx, gvy, hx2, j * DT)
            lx, _, _, _ = fly(px2, py2, *launch(vx2, vy2, run))
            best = max(best, lx)
    return best, ok_release, total


print('%-6s %-6s %-7s %-9s %-9s %-9s %s' %
      ('brzina', 'jama', 'HP1', 'HP2', 'jedan', 'lanac', 'prozor za 1. otpustanje'))
print('-' * 82)
EDGE = 0.0
for run in (7.0,):
    for pit in (9.0, 10.0, 11.0, 12.0):
        for d1 in (1.5,):
            for d2 in (5.0, 5.5, 6.0, 6.5, 7.0):
                hx1, hx2 = EDGE + d1, EDGE + d2
                if hx2 >= pit - 0.5:
                    continue
                one = best_single(run, EDGE, hx1)
                ch, ok, tot = best_chain(run, EDGE, hx1, hx2)
                if one is None:
                    print('  skok ne dohvaca HP1')
                    continue
                verdict = ''
                if one < pit and ch is not None and ch > pit + 0.5:
                    verdict = '  <-- TRAZENO'
                print('%-6.1f %-6.1f %-7.1f %-9.1f %-9.2f %-9.2f %d/%d (%.0f%%)%s' %
                      (run, pit, hx1, hx2, one, ch, ok, tot, 100.0 * ok / tot, verdict))
